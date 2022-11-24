use serde::Deserialize;
use serde_json;
use super::Context;
use crate::semantics::elements::{
    ElementError,
    base_ty::BaseTy,
    element::KeyElement,
    ev::Ev,
    method::{
        Method,
        MethodParamInOut,
    },
    qual::{
        Qual,
        QualKey,
    },
    ty::{
        Ty,
        TyArg,
        TyInstance,
        TyLogicalKey,
    },
};

#[derive(Deserialize)]
struct UdonSymbols {
    base_tys: Vec<BaseTySymbol>,
    tys: Vec<TySymbol>,
    methods: Vec<MethodSymbol>,
    evs: Vec<EvSymbol>,
}

#[derive(Deserialize)]
struct BaseTySymbol {
    scopes: Vec<String>,
    name: String,
    logical_name: String,
}

#[derive(Deserialize)]
struct TySymbol {
    scopes: Vec<String>,
    name: String,
    real_name: String,
    args: Vec<String>,
    parents: Vec<String>,
}

#[derive(Deserialize)]
struct MethodSymbol {
    is_static: bool,
    ty: String,
    name: String,
    param_tys: Vec<String>,
    param_in_outs: Vec<String>,
    real_name: String,
    param_real_names: Vec<String>,
}

#[derive(Deserialize)]
struct EvSymbol {
    name: String,
    param_tys: Vec<String>,
    param_in_outs: Vec<String>,
    real_name: String,
    param_real_names: Vec<String>,
}

impl<'input> Context<'input> {
    pub fn register_from_json(&self, json: String) -> Result<(), Vec<String>> {
        let Ok(symbols) = serde_json::from_str::<UdonSymbols>(json.as_str())
        else {
            return Err(vec!["Udon symbols cannot be initialized".to_owned()]);
        };

        self.register_from_base_ty_symbols(&symbols.base_tys)
            .map_err(|e| vec![e.message])?;
        self.register_from_ty_symbols(&symbols.tys)
            .map_err(|e| vec![e.message])?;
        self.register_from_method_symbols(&symbols.methods)
            .map_err(|e| vec![e.message])?;
        self.register_from_ev_symbols(&symbols.evs)
            .map_err(|e| vec![e.message])?;
        Ok(())
    }

    fn register_from_base_ty_symbols(&self, symbols: &Vec<BaseTySymbol>) -> Result<(), ElementError> {
        for sym in symbols {
            let qual = Qual::new_or_get_quals(self, sym.scopes.clone());
            BaseTy::new(
                self,
                qual,
                sym.name.clone(),
                sym.logical_name.clone(),
            )?;
        }
        Ok(())
    }

    fn register_from_ty_symbols(&self, symbols: &Vec<TySymbol>) -> Result<(), ElementError> {
        for sym in symbols {
            let qual = QualKey::new_quals(sym.scopes.clone());
            let args = sym.args.iter().map(|x| TyArg::Ty(TyLogicalKey::new(x.clone()))).collect();
            Ty::new_strict(
                self,
                BaseTy::get(self, qual, sym.name.clone())?,
                args,
                sym.real_name.clone(),
                Some(TyInstance::Single { elem_name: None, ty_name: sym.real_name.clone() }),
                sym.parents.iter().map(|x| TyLogicalKey::new(x.clone())).collect(),
            )?;
        }
        Ok(())
    }

    fn register_from_method_symbols(&self, symbols: &Vec<MethodSymbol>) -> Result<(), ElementError> {
        for sym in symbols {
            let ty = if sym.is_static {
                Ty::new_or_get_type_from_key(self, TyLogicalKey::new(sym.ty.clone()))?
            }
            else {
                TyLogicalKey::new(sym.ty.clone()).get_value(self)?
            };
            Method::new(
                self,
                ty,
                sym.name.clone(),
                sym.param_tys.iter().map(|x| TyLogicalKey::new(x.to_owned()).get_value(self)).collect::<Result<_, _>>()?,
                sym.param_in_outs.iter().map(|x|
                    match x.as_str() {
                        "IN" => Ok(MethodParamInOut::In),
                        "IN_OUT" => Ok(MethodParamInOut::InOut),
                        "OUT" => Ok(MethodParamInOut::Out),
                        _ => Err(ElementError::new("Illegal method param in/out kind".to_owned())),
                    })
                    .collect::<Result<_, _>>()?,
                sym.real_name.clone(),
                sym.param_real_names.clone(),
            )?;
        }
        Ok(())
    }

    fn register_from_ev_symbols(&self, symbols: &Vec<EvSymbol>) -> Result<(), ElementError> {
        for sym in symbols {
            Ev::new(
                self,
                sym.name.clone(),
                sym.param_tys.iter().map(|x| TyLogicalKey::new(x.to_owned()).get_value(self)).collect::<Result<_, _>>()?,
                sym.param_in_outs.iter().map(|x|
                    match x.as_str() {
                        "IN" => Ok(MethodParamInOut::In),
                        "IN_OUT" => Ok(MethodParamInOut::InOut),
                        "OUT" => Ok(MethodParamInOut::Out),
                        _ => Err(ElementError::new("Illegal method param in/out kind".to_owned())),
                    })
                    .collect::<Result<_, _>>()?,
                sym.real_name.clone(),
                sym.param_real_names.clone(),
            )?;
        }
        Ok(())
    }
}
