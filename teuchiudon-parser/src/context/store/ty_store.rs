use crate::context::Context;
use crate::semantics::elements::{
    ElementError,
    base_ty::BaseTy,
    qual::Qual,
    ty::{
        Ty,
        TyLogicalKey,
    },
};

impl Context {
    pub fn register_default_tys(&self) -> Result<(), Vec<String>> {
        self.register_default_tys_core().map_err(|e| vec![e.message])
    }

    fn register_default_tys_core(&self) -> Result<(), ElementError> {
        let ty_names = vec![
            ("qual", "qual", None, false, Vec::new()),
            ("type", "type", None, false, Vec::new()),
            ("unit", "unit", None, true, Vec::new()),
            ("tuple", "tuple", None, false, Vec::new()),
            ("array", "array", None, false, Vec::new()),
            ("function", "function", None, false, Vec::new()),
            ("nfunction", "nfunction", None, false, Vec::new()),
            ("closure", "closure", None, false, Vec::new()),
            ("method", "method", None, false, Vec::new()),
            ("getter", "getter", None, false, Vec::new()),
            ("setter", "setter", None, false, Vec::new()),
            ("unknown", "unknown", None, true, Vec::new()),
            ("any", "any", None, true, Vec::new()),
            ("never", "never", None, true, Vec::new()),
            ("nulltype", "nulltype", Some("SystemObject"), true, Vec::new()),
            ("object", "SystemObject", Some("SystemObject"), true, Vec::new()),
            ("bool", "SystemBoolean", Some("SystemBoolean"), true, vec!["SystemObject"]),
            ("byte", "SystemByte", Some("SystemByte"), true, vec!["SystemObject"]),
            ("sbyte", "SystemSByte", Some("SystemSByte"), true, vec!["SystemObject"]),
            ("short", "SystemInt16", Some("SystemInt16"), true, vec!["SystemObject"]),
            ("ushort", "SystemUInt16", Some("SystemUInt16"), true, vec!["SystemObject"]),
            ("int", "SystemInt32", Some("SystemInt32"), true, vec!["SystemObject"]),
            ("uint", "SystemUInt32", Some("SystemUInt32"), true, vec!["SystemObject"]),
            ("long", "SystemInt64", Some("SystemInt64"), true, vec!["SystemObject"]),
            ("ulong", "SystemUInt64", Some("SystemUInt64"), true, vec!["SystemObject"]),
            ("float", "SystemSingle", Some("SystemSingle"), true, vec!["SystemObject"]),
            ("double", "SystemDouble", Some("SystemDouble"), true, vec!["SystemObject"]),
            ("decimal", "SystemDecimal", Some("SystemDecimal"), true, vec!["SystemObject"]),
            ("char", "SystemChar", Some("SystemChar"), true, vec!["SystemObject"]),
            ("string", "SystemString", Some("SystemString"), true, vec!["SystemObject"]),
            ("unityobject", "UnityEngineObject", Some("UnityEngineObject"), true, vec!["SystemObject"]),
            ("gameobject", "UnityEngineGameObject", Some("UnityEngineGameObject"), true, vec!["SystemObject", "UnityEngineObject"]),
            ("vec2", "UnityEngineVector2", Some("UnityEngineVector2"), true, vec!["SystemObject"]),
            ("vec3", "UnityEngineVector3", Some("UnityEngineVector3"), true, vec!["SystemObject"]),
            ("vec4", "UnityEngineVector4", Some("UnityEngineVector4"), true, vec!["SystemObject"]),
            ("quat", "UnityEngineQuaternion", Some("UnityEngineQuaternion"), true, vec!["SystemObject"]),
            ("color", "UnityEngineColor", Some("UnityEngineColor"), true, vec!["SystemObject"]),
            ("color32", "UnityEngineColor32", Some("UnityEngineColor32"), true, vec!["SystemObject"]),
            ("vrcurl", "VRCSDKBaseVRCUrl", Some("VRCSDKBaseVRCUrl"), true, vec!["SystemObject"]),
            ("udon", "VRCUdonUdonBehaviour", Some("VRCUdonUdonBehaviour"), true, vec!["SystemObject", "UnityEngineObject", "UnityEngineComponent", "VRCUdonCommonInterfacesIUdonEventReceiver"]),
        ];
        let top = Qual::top(self)?;
        for (name, logical_name, real_name, is_ty, parents) in ty_names {
            let base = BaseTy::new(
                self,
                top.clone(),
                name.to_owned(),
                logical_name.to_owned(),
            )?;
            if is_ty {
                Ty::new_strict(
                    self,
                    base,
                    Vec::new(),
                    logical_name.to_owned(),
                    real_name.map(|x| x.to_owned()),
                    parents.into_iter().map(|x| TyLogicalKey::new(x.to_owned())).collect(),
                )?;
            }
        }
        Ok(())
    }
}
