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
        ("unknown", "unknown", None, true),
        ("qual", "qual", None, false),
        ("type", "type", None, false),
        ("unit", "unit", None, true),
        ("tuple", "tuple", None, false),
        ("array", "array", None, false),
        ("function", "function", None, false),
        ("nfunction", "nfunction", None, false),
        ("closure", "closure", None, false),
        ("method", "method", None, false),
        ("getter", "getter", None, false),
        ("setter", "setter", None, false),
        ("nulltype", "nulltype", Some("SystemObject"), true),
        ("object", "SystemObject", Some("SystemObject"), true),
        ("bool", "SystemBoolean", Some("SystemBoolean"), true),
        ("byte", "SystemByte", Some("SystemByte"), true),
        ("sbyte", "SystemSByte", Some("SystemSByte"), true),
        ("short", "SystemInt16", Some("SystemInt16"), true),
        ("ushort", "SystemUInt16", Some("SystemUInt16"), true),
        ("int", "SystemInt32", Some("SystemInt32"), true),
        ("uint", "SystemUInt32", Some("SystemUInt32"), true),
        ("long", "SystemInt64", Some("SystemInt64"), true),
        ("ulong", "SystemUInt64", Some("SystemUInt64"), true),
        ("float", "SystemSingle", Some("SystemSingle"), true),
        ("double", "SystemDouble", Some("SystemDouble"), true),
        ("decimal", "SystemDecimal", Some("SystemDecimal"), true),
        ("char", "SystemChar", Some("SystemChar"), true),
        ("string", "SystemString", Some("SystemString"), true),
        ("unityobject", "UnityEngineObject", Some("UnityEngineObject"), true),
        ("gameobject", "UnityEngineGameObject", Some("UnityEngineGameObject"), true),
        ("vec2", "UnityEngineVector2", Some("UnityEngineVector2"), true),
        ("vec3", "UnityEngineVector3", Some("UnityEngineVector3"), true),
        ("vec4", "UnityEngineVector4", Some("UnityEngineVector4"), true),
        ("quat", "UnityEngineQuaternion", Some("UnityEngineQuaternion"), true),
        ("color", "UnityEngineColor", Some("UnityEngineColor"), true),
        ("color32", "UnityEngineColor32", Some("UnityEngineColor32"), true),
        ("udon", "VRCUdonUdonBehaviour", Some("VRCUdonUdonBehaviour"), true),
    ];
    let top = Qual::top(context);
    for (name, logical_name, real_name, is_ty) in ty_names {
        let base = BaseTy::new(
            context,
            top.clone(),
            name.to_owned(),
            logical_name.to_owned(),
        )?;
        if is_ty {
            Ty::new_strict(
                context,
                base,
                Vec::new(),
                logical_name.to_owned(),
                real_name.map(|x| x.to_owned()),
            )?;
        }
    }
    Ok(())
}
