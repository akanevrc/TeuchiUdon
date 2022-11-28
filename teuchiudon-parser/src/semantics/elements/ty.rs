use std::{
    hash::{
        Hash,
        Hasher,
    },
    rc::Rc,
};
use crate::context::Context;
use super::{
    ElementError,
    base_ty::BaseTy,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    fn_stats::FnKey,
    method::MethodKey,
    qual::QualKey,
};

#[derive(Clone, Debug)]
pub struct Ty {
    pub id: usize,
    pub base: Rc<BaseTy>,
    pub args: Vec<TyArg>,
    pub logical_name: String,
    pub instance: Option<TyInstance>,
    pub parents: Vec<TyLogicalKey>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct TyKey {
    pub qual: QualKey,
    pub name: String,
    pub args: Vec<TyArg>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct TyLogicalKey {
    pub logical_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum TyArg {
    Qual(QualKey),
    Ty(TyLogicalKey),
    Fn(FnKey),
    Method(MethodKey),
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum TyInstance {
    Unit,
    Single { elem_name: Option<String>, ty_name: String },
    Tuple { elem_name: Option<String>, instances: Vec<TyInstance> },
}

impl PartialEq for Ty {
    fn eq(&self, other: &Self) -> bool {
        self.id == other.id
    }
}

impl Eq for Ty {}

impl Hash for Ty {
    fn hash<H: Hasher>(&self, state: &mut H) {
        self.id.hash(state);
    }
}

impl SemanticElement for Ty {
    fn description(&self) -> String {
        <Ty as ValueElement<TyKey>>::to_key(self).description()
    }

    fn logical_name(&self) -> String {
        <Ty as ValueElement<TyLogicalKey>>::to_key(self).logical_name()
    }
}

impl ValueElement<TyKey> for Ty {
    fn to_key(&self) -> TyKey {
        TyKey {
            qual: self.base.qual.to_key(),
            name: self.base.name.clone(),
            args: self.args.clone(),
        }
    }
}

impl SemanticElement for TyKey {
    fn description(&self) -> String {
        if self.args.len() == 0 {
            format!(
                "{}{}",
                self.qual.qualify_description("::"),
                self.name.description()
            )
        }
        else {
            format!(
                "{}{}<{}>",
                self.qual.qualify_description("::"),
                self.name.description(),
                self.args.iter().map(|x| x.description()).collect::<Vec<_>>().join(", "),
            )
        }
    }

    fn logical_name(&self) -> String {
        if self.args.len() == 0 {
            format!(
                "{}{}",
                self.qual.qualify_logical_name(">"),
                self.name.logical_name()
            )
        }
        else {
            format!(
                "{}{}[{}]",
                self.qual.qualify_logical_name(">"),
                self.name.logical_name(),
                self.args.iter().map(|x| x.logical_name()).collect::<Vec<_>>().join("]["),
            )
        }
    }
}

impl<'input> KeyElement<'input, Ty> for TyKey {
    fn get_value(&self, context: &Context<'input>) -> Result<Rc<Ty>, ElementError> {
        context.ty_store.get(self)
    }
}

impl ValueElement<TyLogicalKey> for Ty {
    fn to_key(&self) -> TyLogicalKey {
        TyLogicalKey {
            logical_name: self.logical_name.clone(),
        }
    }
}

impl SemanticElement for TyLogicalKey {
    fn description(&self) -> String {
        self.logical_name.description()
    }

    fn logical_name(&self) -> String {
        self.logical_name.logical_name()
    }
}

impl<'input> KeyElement<'input, Ty> for TyLogicalKey {
    fn get_value(&self, context: &Context<'input>) -> Result<Rc<Ty>, ElementError> {
        context.ty_logical_store.get(self)
    }
}

impl SemanticElement for TyArg {
    fn description(&self) -> String {
        match self {
            Self::Qual(x) => x.description(),
            Self::Ty(x) => x.description(),
            Self::Fn(x) => x.description(),
            Self::Method(x) => x.description(),
        }
    }

    fn logical_name(&self) -> String {
        match self {
            Self::Qual(x) => x.logical_name(),
            Self::Ty(x) => x.logical_name(),
            Self::Fn(x) => x.logical_name(),
            Self::Method(x) => x.logical_name(),
        }
    }
}

impl Ty {
    pub fn new_strict<'input>(
        context: &Context<'input>,
        base: Rc<BaseTy>,
        args: Vec<TyArg>,
        logical_name: String,
        instance: Option<TyInstance>,
        parents: Vec<TyLogicalKey>,
    ) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.ty_store.next_id(),
            base: base.clone(),
            args,
            logical_name,
            instance,
            parents,
        });
        let key = value.to_key();
        let logical_key: TyLogicalKey = value.to_key();
        context.ty_store.add(key, value.clone())?;
        context.ty_logical_store.add(logical_key.clone(), value.clone()).ok();
        Ok(value)
    }

    pub fn new<'input>(
        context: &Context<'input>,
        base: Rc<BaseTy>,
        args: Vec<TyArg>
    ) -> Result<Rc<Self>, ElementError> {
        Self::new_strict(
            context,
            base.clone(),
            args.clone(),
            Self::logical_name(&base, &args),
            None,
            Vec::new(),
        )
    }

    pub fn new_or_get<'input>(
        context: &Context<'input>,
        base: Rc<BaseTy>,
        args: Vec<TyArg>
    ) -> Result<Rc<Self>, ElementError> {
        let instance = Self::instance(context, &base, &args)?;
        let value = Rc::new(Self {
            id: context.ty_store.next_id(),
            base: base.clone(),
            args: args.clone(),
            logical_name: Self::logical_name(&base, &args),
            instance,
            parents: Vec::new(),
        });

        let key: TyKey = value.to_key();
        match key.clone().get_value(context) {
            Ok(x) => return Ok(x),
            Err(_) => (),
        }

        let real_key: TyLogicalKey = value.to_key();
        match real_key.clone().get_value(context) {
            Ok(_) => panic!("Illegal state"),
            Err(_) => (),
        }

        context.ty_store.add(key, value.clone()).unwrap();
        context.ty_logical_store.add(real_key, value.clone()).ok();
        Ok(value)
    }

    fn logical_name(base: &Rc<BaseTy>, args: &Vec<TyArg>) -> String {
        if args.len() == 0 {
            format!(
                "{}{}",
                base.qual.qualify_logical_name(">"),
                base.name.logical_name()
            )
        }
        else {
            format!(
                "{}{}[{}]",
                base.qual.qualify_logical_name(">"),
                base.name.logical_name(),
                args.iter().map(|x| x.logical_name()).collect::<Vec<_>>().join("]["),
            )
        }
    }

    pub fn get<'input>(
        context: &Context<'input>,
        qual: QualKey,
        name: String,
        args: Vec<TyArg>
    ) -> Result<Rc<Self>, ElementError> {
        TyKey::new(qual, name, args).get_value(context)
    }

    pub fn get_from_name<'input>(
        context: &Context<'input>,
        name: &str
    ) -> Result<Rc<Self>, ElementError> {
        TyKey::from_name(name).get_value(context)
    }

    pub fn get_from_logical_name<'input>(
        context: &Context<'input>,
        logical_name: String
    ) -> Result<Rc<Self>, ElementError> {
        TyLogicalKey::new(logical_name).get_value(context)
    }

    fn instance<'input>(
        context: &Context<'input>,
        base: &Rc<BaseTy>,
        args: &Vec<TyArg>
    ) -> Result<Option<TyInstance>, ElementError> {
        match base.logical_name.as_str() {
            "qual" |
            "type" |
            "unknown" =>
                Ok(None),
            "unit" |
            "never" |
            "function" |
            "nfunction" |
            "closure" |
            "method" |
            "getter" |
            "setter" =>
                Ok(Some(TyInstance::Unit)),
            "tuple" => {
                    let instances =
                        args.iter().enumerate()
                        .map(|(i, x)| Self::sub_instance(context, x, i.to_string()))
                        .collect::<Result<Vec<_>, _>>()?;
                    if instances.iter().any(|x| x.is_none()) {
                        Ok(None)
                    }
                    else {
                        let instances = instances.into_iter().map(|x| x.unwrap()).collect();
                        Ok(Some(TyInstance::Tuple { elem_name: None, instances }))
                    }
                },
            "any" =>
                Ok(Some(TyInstance::Single { elem_name: None, ty_name: "SystemObject".to_owned() })),
            _ =>
                Ok(None),
        }
    }

    fn sub_instance<'input>(
        context: &Context<'input>,
        arg: &TyArg,
        elem_name: String
    ) -> Result<Option<TyInstance>, ElementError> {
        match arg {
            TyArg::Ty(x) =>
                match &x.get_value(context)?.instance {
                    Some(TyInstance::Unit) =>
                        Ok(Some(TyInstance::Unit)),
                    Some(TyInstance::Single { elem_name: _, ty_name }) =>
                        Ok(Some(TyInstance::Single { elem_name: Some(elem_name), ty_name: ty_name.clone() })),
                    Some(TyInstance::Tuple { elem_name: _, instances }) =>
                        Ok(Some(TyInstance::Tuple { elem_name: Some(elem_name), instances: instances.clone() })),
                    None =>
                        Ok(None),
                }
            _ =>
                panic!("Illegal state"),
        }
    }
}

impl TyKey {
    pub fn new(qual: QualKey, name: String, args: Vec<TyArg>) -> Self {
        Self {
            qual,
            name,
            args,
        }
    }

    pub fn from_name(name: &str) -> Self {
        Self {
            qual: QualKey::top(),
            name: name.to_owned(),
            args: Vec::new(),
        }
    }
}

impl TyLogicalKey {
    pub fn new(logical_name: String) -> Self {
        Self {
            logical_name,
        }
    }
}
