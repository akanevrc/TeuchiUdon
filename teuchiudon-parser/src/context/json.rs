use serde::Deserialize;
use serde_json::from_str;
use super::Context;
use crate::semantics::elements::{
    ElementError,
    base_ty::BaseTy,
    qual::QualKey,
    ty::{
        Ty,
        TyArg,
        TyLogicalKey,
    }, element::KeyElement,
};

pub fn register_from_json(context: &Context, json: String) -> Result<(), Vec<String>> {
    let Ok(symbols) = from_str::<UdonSymbols>(json.as_str())
    else {
        return Err(vec!["Udon symbols cannot be initialized".to_owned()]);
    };

    register_from_base_ty_symbols(context, &symbols.base_tys)
        .map_err(|e| vec![e.message])?;
    register_from_ty_symbols(context, &symbols.tys)
        .map_err(|e| vec![e.message])?;
    register_from_method_symbols(context, &symbols.methods)
        .map_err(|e| vec![e.message])?;
    register_from_ev_symbols(context, &symbols.evs)
        .map_err(|e| vec![e.message])?;
    Ok(())
}

fn register_from_base_ty_symbols(context: &Context, symbols: &Vec<BaseTySymbol>) -> Result<(), ElementError> {
    for sym in symbols {
        let qual = QualKey::new_quals(sym.scopes.clone()).get_value(context)?;
        BaseTy::new(
            context,
            qual,
            sym.name.clone(),
            sym.logical_name.clone(),
        )?;
    }
    Ok(())
}

fn register_from_ty_symbols(context: &Context, symbols: &Vec<TySymbol>) -> Result<(), ElementError> {
    for sym in symbols {
        let qual = QualKey::new_quals(sym.scopes.clone());
        let args = sym.args.iter().map(|x| TyArg::Ty(TyLogicalKey::new(x.clone()))).collect();
        Ty::new_strict(
            context,
            BaseTy::get(context, qual, sym.name.clone())?,
            args,
            sym.real_name.clone(),
            Some(sym.real_name.clone()),
        )?;
    }
    Ok(())
}

fn register_from_method_symbols(context: &Context, symbols: &Vec<MethodSymbol>) -> Result<(), ElementError> {
    Ok(())
}

fn register_from_ev_symbols(context: &Context, symbols: &Vec<EvSymbol>) -> Result<(), ElementError> {
    Ok(())
}

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
