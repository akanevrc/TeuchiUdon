use std::{
    collections::HashMap,
    rc::Rc,
};
use crate::context::Context;
use super::{
    ElementError,
    base_ty::BaseTyKey,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    method::MethodKey,
    qual::QualKey,
    ty::{
        Ty,
        TyArg,
        TyKey,
        TyLogicalKey,
    },
};

impl Ty {
    pub fn new_or_get_qual_from_key(context: &Context, key: QualKey) -> Result<Rc<Self>, ElementError> {
        let base = BaseTyKey::from_name("qual".to_owned()).get_value(context)?;
        let arg = key.get_value(context)?;
        Self::new_or_get(context, base, vec![TyArg::Qual(arg.to_key())])
    }

    pub fn new_or_get_qual_from_names(context: &Context, quals: Vec<String>) -> Result<Rc<Self>, ElementError> {
        Self::new_or_get_qual_from_key(context, QualKey::new_quals(quals))
    }

    pub fn new_or_get_type_from_key(context: &Context, key: TyLogicalKey) -> Result<Rc<Self>, ElementError> {
        let base = BaseTyKey::from_name("type".to_owned()).get_value(context)?;
        let arg = key.get_value(context)?;
        Self::new_or_get(context, base, vec![TyArg::Ty(arg.to_key())])
    }

    pub fn new_or_get_type(context: &Context, qual: QualKey, name: String, args: Vec<TyArg>) -> Result<Rc<Self>, ElementError> {
        let base = BaseTyKey::from_name("type".to_owned()).get_value(context)?;
        let arg = Self::get(context, qual, name, args)?;
        Self::new_or_get(context, base, vec![TyArg::Ty(arg.to_key())])
    }

    pub fn new_or_get_type_from_name(context: &Context, name: String) -> Result<Rc<Self>, ElementError> {
        Self::new_or_get_type(context, QualKey::top(), name, Vec::new())
    }

    pub fn new_or_get_tuple_from_keys(context: &Context, keys: Vec<TyLogicalKey>) -> Result<Rc<Self>, ElementError> {
        let base = BaseTyKey::from_name("tuple".to_owned()).get_value(context)?;
        let args = keys.iter().map(|x| Ok(TyArg::Ty(x.get_value(context)?.to_key()))).collect::<Result<_, _>>()?;
        Self::new_or_get(context, base, args)
    }

    pub fn get_array_from_key(context: &Context, key: TyLogicalKey) -> Result<Rc<Self>, ElementError> {
        BaseTyKey::from_name("array".to_owned()).new_applied(vec![TyArg::Ty(key)]).get_value(context)
    }

    pub fn get_array_from_name(context: &Context, name: String) -> Result<Rc<Self>, ElementError> {
        Self::get_array_from_key(context, Ty::get_from_name(context, name)?.to_key())
    }

    pub fn logical_eq_with(self: &Rc<Self>, context: &Context, key: TyKey) -> bool {
        let ty = key.get_value(context);
        ty.is_ok() && self.logical_name == ty.unwrap().logical_name
    }
    
    pub fn logical_eq_with_name(self: &Rc<Self>, context: &Context, name: &str) -> bool {
        self.logical_eq_with(context, TyKey::from_name(name.to_owned()))
    }

    pub fn base_eq_with_name(self: &Rc<Self>, name: &str) -> bool {
        self.base.logical_name == name
    }

    pub fn arg_as_qual(self: &Rc<Self>) -> QualKey {
        if !self.base_eq_with_name("qual") {
            panic!("Illegal state")
        }
        if self.args.len() == 1 {
            match &self.args[0] {
                TyArg::Qual(x) =>
                    x.clone(),
                _ =>
                    panic!("Illegal state"),
            }
        }
        else {
            panic!("Illegal state")
        }
    }

    pub fn arg_as_type(self: &Rc<Self>) -> TyLogicalKey {
        if !self.base_eq_with_name("type") {
            panic!("Illegal state")
        }
        if self.args.len() == 1 {
            match &self.args[0] {
                TyArg::Ty(x) =>
                    x.clone(),
                _ =>
                    panic!("Illegal state"),
            }
        }
        else {
            panic!("Illegal state")
        }
    }

    pub fn args_as_tuple(self: &Rc<Self>) -> Vec<TyLogicalKey> {
        if !self.base_eq_with_name("tuple") {
            panic!("Illegal state")
        }
        self.args.iter()
        .map(|x| {
            if let TyArg::Ty(t) = x {
                t.clone()
            }
            else {
                panic!("Illegal state")
            }
        })
        .collect()
    }

    pub fn arg_as_array(self: &Rc<Self>) -> TyLogicalKey {
        if !self.base_eq_with_name("array") {
            panic!("Illegal state")
        }
        if self.args.len() == 1 {
            match &self.args[0] {
                TyArg::Ty(x) =>
                    x.clone(),
                _ =>
                    panic!("Illegal state"),
            }
        }
        else {
            panic!("Illegal state")
        }
    }

    pub fn args_as_method(self: &Rc<Self>) -> Vec<MethodKey> {
        if !self.base_eq_with_name("method") {
            panic!("Illegal state")
        }
        self.args.iter()
        .map(|x| {
            if let TyArg::Method(m) = x {
                m.clone()
            }
            else {
                panic!("Illegal state")
            }
        })
        .collect()
    }

    pub fn arg_as_getter(self: &Rc<Self>) -> MethodKey {
        if !self.base_eq_with_name("getter") {
            panic!("Illegal state")
        }
        if self.args.len() == 1 {
            match &self.args[0] {
                TyArg::Method(x) =>
                    x.clone(),
                _ =>
                    panic!("Illegal state"),
            }
        }
        else {
            panic!("Illegal state")
        }
    }

    pub fn arg_as_setter(self: &Rc<Self>) -> MethodKey {
        if !self.base_eq_with_name("setter") {
            panic!("Illegal state")
        }
        if self.args.len() == 1 {
            match &self.args[0] {
                TyArg::Method(x) =>
                    x.clone(),
                _ =>
                    panic!("Illegal state"),
            }
        }
        else {
            panic!("Illegal state")
        }
    }

    pub fn assignable_from(self: &Rc<Self>, context: &Context, ty: &Rc<Self>) -> bool {
        self.assignable_from_unknown(context, ty) ||
        self.assignable_from_any(context, ty) ||
        self.assignable_from_never(context, ty) ||
        self.assignable_from_qual(context, ty) ||
        self.assignable_from_type(context, ty) ||
        self.assignable_from_unit(context, ty) ||
        self.assignable_from_tuple(context, ty) ||
        self.assignable_from_method(context, ty) ||
        self.assignable_from_getter(context, ty) ||
        self.assignable_from_setter(context, ty) ||
        self.assignable_from_dotnet_ty(context, ty)
    }

    fn assignable_from_unknown(self: &Rc<Self>, _context: &Context, ty: &Rc<Self>) -> bool {
        self.base_eq_with_name("unknown") || ty.base_eq_with_name("unknown")
    }

    fn assignable_from_any(self: &Rc<Self>, _context: &Context, _ty: &Rc<Self>) -> bool {
        self.base_eq_with_name("any")
    }

    fn assignable_from_never(self: &Rc<Self>, _context: &Context, ty: &Rc<Self>) -> bool {
        ty.base_eq_with_name("never")
    }

    fn assignable_from_qual(self: &Rc<Self>, _context: &Context, ty: &Rc<Self>) -> bool {
        self.base_eq_with_name("qual") && ty.base_eq_with_name("qual") &&
        self.arg_as_qual() == ty.arg_as_qual()
    }

    fn assignable_from_type(self: &Rc<Self>, context: &Context, ty: &Rc<Self>) -> bool {
        self.base_eq_with_name("type") && ty.base_eq_with_name("type") && {
            let self_ty = self.arg_as_type().get_value(context);
            let ty_ty = ty.arg_as_type().get_value(context);
            self_ty.is_ok() && ty_ty.is_ok() &&
            self_ty.unwrap().assignable_from(context, &ty_ty.unwrap())
        }
    }

    fn assignable_from_unit(self: &Rc<Self>, _context: &Context, ty: &Rc<Self>) -> bool {
        self.base_eq_with_name("unit") && ty.base_eq_with_name("unit")
    }

    fn assignable_from_tuple(self: &Rc<Self>, context: &Context, ty: &Rc<Self>) -> bool {
        self.base_eq_with_name("tuple") && ty.base_eq_with_name("tuple") && {
            let self_tys =
                self.args_as_tuple().iter()
                .map(|x| x.get_value(context))
                .collect::<Result<Vec<_>, _>>();
            let ty_tys =
                ty.args_as_tuple().iter()
                .map(|x| x.get_value(context))
                .collect::<Result<Vec<_>, _>>();
            self_tys.is_ok() && ty_tys.is_ok() && {
                let self_tys = self_tys.unwrap();
                let ty_tys = ty_tys.unwrap();
                self_tys.len() == ty_tys.len() &&
                self_tys.iter().zip(ty_tys.iter())
                .all(|(s, t)| s.assignable_from(context, t))
            }
        }
    }

    fn assignable_from_method(self: &Rc<Self>, _context: &Context, _ty: &Rc<Self>) -> bool {
        false
    }

    fn assignable_from_getter(self: &Rc<Self>, _context: &Context, _ty: &Rc<Self>) -> bool {
        false
    }

    fn assignable_from_setter(self: &Rc<Self>, _context: &Context, _ty: &Rc<Self>) -> bool {
        false
    }

    fn assignable_from_dotnet_ty(self: &Rc<Self>, _context: &Context, ty: &Rc<Self>) -> bool {
        self.is_dotnet_ty() && ty.is_dotnet_ty() &&
        ty.parents.contains(&self.to_key())
    }

    pub fn is_dotnet_ty(self: &Rc<Self>) -> bool {
        vec![
            "unknown",
            "any",
            "never",
            "qual",
            "type",
            "unit",
            "tuple",
            "function",
            "nfunction",
            "closure",
            "method",
            "getter",
            "setter",
        ]
        .iter()
        .all(|x| !self.base_eq_with_name(x))
    }

    pub fn is_syncable(self: &Rc<Self>, context: &Context) -> bool {
        vec![
            "bool",
            "byte",
            "sbyte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "char",
            "string",
            "vec2",
            "vec3",
            "vec4",
            "quat",
            "color",
            "color32",
            "vrcurl",
        ]
        .iter()
        .any(|x| self.logical_eq_with_name(context, x)) ||
        vec![
            "bool",
            "byte",
            "sbyte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "char",
            "vec2",
            "vec3",
            "vec4",
            "quat",
            "color",
            "color32",
        ]
        .iter()
        .any(|x| {
            let ty = Self::get_array_from_name(context, (*x).to_owned());
            ty.is_ok() && self.logical_eq_with(context, ty.unwrap().to_key())
        })
    }

    pub fn is_linear_syncable(self: &Rc<Self>, context: &Context) -> bool {
        vec![
            "byte",
            "sbyte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "vec2",
            "vec3",
            "quat",
            "color",
            "color32",
        ]
        .iter()
        .any(|x| self.logical_eq_with_name(context, x))
    }

    pub fn is_smooth_syncable(self: &Rc<Self>, context: &Context) -> bool {
        vec![
            "byte",
            "sbyte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "vec2",
            "vec3",
            "quat",
        ]
        .iter()
        .any(|x| self.logical_eq_with_name(context, x))
    }

    pub fn is_signed_integer(self: &Rc<Self>, context: &Context) -> bool {
        vec![
            "sbyte",
            "short",
            "int",
            "long",
        ]
        .iter()
        .any(|x| self.logical_eq_with_name(context, x))
    }

    pub fn contains_unknown(self: &Rc<Self>, context: &Context) -> bool {
        self.base_eq_with_name("unknown") ||
        self.args.iter().any(|x| if let TyArg::Ty(t) = x { t.get_value(context).is_ok() } else { false })
    }

    pub fn infer(self: &Rc<Self>, context: &Context, ty: &Rc<Self>) -> Result<Rc<Self>, ElementError> {
        if self.base_eq_with_name("unknown") {
            Ok(ty.clone())
        }
        else if self.assignable_from(context, ty) {
            let args = self.args.iter().zip(ty.args.iter())
            .map(|(s, t)| match (s, t) {
                (TyArg::Ty(s), TyArg::Ty(t)) => {
                    let s = s.get_value(context)?;
                    let t = t.get_value(context)?;
                    Ok(TyArg::Ty(s.infer(context, &t)?.to_key()))
                },
                _ =>
                    Ok(t.clone()),
            })
            .collect::<Result<_, _>>()?;

            if self.is_dotnet_ty() {
                self.base.get_applied(context, args)
            }
            else {
                self.base.new_or_get_applied(context, args)
            }
        }
        else {
            Err(ElementError::new(format!("Cannot be inferred type `{}` from `{}`", self.description(), ty.description())))
        }
    }

    pub fn most_compatible_method(self: &Rc<Self>, context: &Context, in_tys: Vec<TyLogicalKey>) -> Result<MethodKey, ElementError> {
        if !self.base_eq_with_name("method") {
            panic!("Illegal state")
        }

        let args = self.args_as_method();
        let methods =
            args.iter()
            .filter(|x| x.in_tys.len() == in_tys.len())
            .collect::<Vec<_>>();
        if methods.len() == 0 {
            return Err(ElementError::new("No compatible methods found".to_owned()));
        }

        let mut just_count_to_methods = HashMap::new();
        for method in methods {
            let mut compatible = true;
            let mut just_count = 0;
            for (m, i) in method.in_tys.iter().zip(in_tys.iter()) {
                let m = m.get_value(context)?;
                let i = i.get_value(context)?;
                if !m.assignable_from(context, &i) {
                    compatible = false;
                    break;
                }
                if m.logical_eq_with(context, i.to_key()) {
                    just_count += 1;
                }
            }

            if compatible {
                if !just_count_to_methods.contains_key(&just_count) {
                    just_count_to_methods.insert(just_count, Vec::new());
                }
                just_count_to_methods.get_mut(&just_count).unwrap().push(method);
            }
        }

        for i in (0..=in_tys.len()).rev() {
            if just_count_to_methods.contains_key(&i) {
                if just_count_to_methods[&i].len() == 1 {
                    return Ok(just_count_to_methods[&i][0].clone());
                }
                else {
                    return Err(ElementError::new("Too many compatible methods found".to_owned()))
                }
            }
        }

        Err(ElementError::new("No compatible methods found".to_owned()))
    }

    pub fn members(self: &Rc<Self>, context: &Context) -> Result<Vec<(Option<String>, Rc<Ty>)>, ElementError> {
        if self.base_eq_with_name("unit") {
            Ok(Vec::new())
        }
        else if self.base_eq_with_name("tuple") {
            self.args_as_tuple().iter()
            .enumerate()
            .map(|(i, x)| Ok((Some(i.to_string()), x.get_value(context)?)))
            .collect::<Result<_, _>>()
        }
        else if self.base_eq_with_name("array") {
            Ok(vec![(None, self.clone())])
        }
        else if self.real_name == None {
            Err(ElementError::new(format!("Cannot instantiate type: `{}`", self.description())))
        }
        else {
            Ok(vec![(None, self.clone())])
        }
    }

    pub fn member_count(self: &Rc<Self>) -> Result<usize, ElementError> {
        if self.base_eq_with_name("unit") {
            Ok(0)
        }
        else if self.base_eq_with_name("tuple") {
            Ok(self.args_as_tuple().len())
        }
        else if self.base_eq_with_name("array") {
            Ok(1)
        }
        else if self.real_name == None {
            Err(ElementError::new(format!("Cannot instantiate type: `{}`", self.description())))
        }
        else {
            Ok(1)
        }
    }
}
