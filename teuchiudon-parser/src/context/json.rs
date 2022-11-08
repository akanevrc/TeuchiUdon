use serde::Deserialize;
use serde_json::from_str;
use super::Context;
use crate::semantics::elements::{
    base_ty::BaseTy,
    qual::Qual,
    scope::Scope,
};

pub fn register_from_json(context: &Context, json: String) -> Result<(), Vec<String>> {
    let Ok(symbols) = from_str::<UdonSymbols>(json.as_str())
    else {
        return Err(vec!["Udon symbols cannot be initialized".to_owned()]);
    };

    register_from_ty_symbols(context, &symbols.tys);
    register_from_array_ty_symbols(context, &symbols.array_tys);
    register_from_generic_base_ty_symbols(context, &symbols.generic_base_tys);
    register_from_method_symbols(context, &symbols.methods);
    register_from_ev_symbols(context, &symbols.evs);
    Ok(())
}

fn register_from_ty_symbols(context: &Context, symbols: &Vec<TySymbol>) {
    for sym in symbols {
        BaseTy::new(
            context,
            Qual::new(sym.scopes.iter().map(|x| Scope::Qual(x.clone())).collect()),
            sym.name.clone(),
            sym.logical_name.clone(),
        );
    }
}

fn register_from_array_ty_symbols(context: &Context, symbols: &Vec<ArrayTySymbol>) {

}

fn register_from_generic_base_ty_symbols(context: &Context, symbols: &Vec<GenericBaseTySymbol>) {
    for sym in symbols {
        BaseTy::new(
            context,
            Qual::new(sym.scopes.iter().map(|x| Scope::Qual(x.clone())).collect()),
            sym.name.clone(),
            sym.logical_name.clone(),
        );
    }
}

fn register_from_method_symbols(context: &Context, symbols: &Vec<MethodSymbol>) {

}

fn register_from_ev_symbols(context: &Context, symbols: &Vec<EvSymbol>) {

}

#[derive(Deserialize)]
struct UdonSymbols {
    tys: Vec<TySymbol>,
    array_tys: Vec<ArrayTySymbol>,
    generic_base_tys: Vec<GenericBaseTySymbol>,
    methods: Vec<MethodSymbol>,
    evs: Vec<EvSymbol>,
}

#[derive(Deserialize)]
struct TySymbol {
    scopes: Vec<String>,
    name: String,
    logical_name: String,
    real_name: String,
    args: Vec<String>,
}

#[derive(Deserialize)]
struct ArrayTySymbol {
    real_name: String,
    element_ty: String,
}

#[derive(Deserialize)]
struct GenericBaseTySymbol {
    scopes: Vec<String>,
    name: String,
    logical_name: String,
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
