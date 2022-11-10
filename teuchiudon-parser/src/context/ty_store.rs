use super::Context;
use crate::semantics::elements::{
    ElementError,
    base_ty::BaseTy,
    qual::Qual,
    ty::Ty,
};

pub fn register_default_tys(context: &Context) -> Result<(), Vec<String>> {
    register_default_tys_core(context).map_err(|e| vec![e.message])
}

fn register_default_tys_core(context: &Context) -> Result<(), ElementError> {
    let ty_names = vec![
        ("unknown", "unknown"),
        ("qual", "qual"),
        ("type", "type"),
        ("unit", "unit"),
        ("tuple", "tuple"),
        ("array", "array"),
        ("function", "function"),
        ("nfunction", "nfunction"),
        ("closure", "closure"),
        ("method", "method"),
        ("getter", "getter"),
        ("setter", "setter"),
        ("nulltype", "nulltype"),
        ("object", "SystemObject"),
        ("bool", "SystemBoolean"),
        ("byte", "SystemByte"),
        ("sbyte", "SystemSByte"),
        ("short", "SystemInt16"),
        ("ushort", "SystemUInt16"),
        ("int", "SystemInt32"),
        ("uint", "SystemUInt32"),
        ("long", "SystemInt64"),
        ("ulong", "SystemUInt64"),
        ("float", "SystemSingle"),
        ("double", "SystemDouble"),
        ("decimal", "SystemDecimal"),
        ("char", "SystemChar"),
        ("string", "SystemString"),
        ("unityobject", "UnityEngineObject"),
        ("gameobject", "UnityEngineGameObject"),
        ("vec2", "UnityEngineVector2"),
        ("vec3", "UnityEngineVector3"),
        ("vec4", "UnityEngineVector4"),
        ("quat", "UnityEngineQuaternion"),
        ("color", "UnityEngineColor"),
        ("color32", "UnityEngineColor32"),
        ("udon", "VRCUdonUdonBehaviour"),
    ];
    let mut base_tys = Vec::new();
    for (name, logical_name) in ty_names {
        base_tys.push(
            BaseTy::new(
                context,
                Qual::TOP,
                name.to_owned(),
                logical_name.to_owned(),
            )?,
        )
    }
    for x in base_tys {
        Ty::new(
            context,
            x.clone(),
            Vec::new(),
            x.logical_name.clone(),
        )?;
    }
    Ok(())
}
