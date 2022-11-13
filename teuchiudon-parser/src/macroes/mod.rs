
#[macro_export]
macro_rules! impl_key_value_elements {
    (
        $key_ty:ty,
        $value_ty:ty,
        $key_ty_name:ident { $($key_field:ident: self.$($field_ident:ident$($(::<$field_ty:ty>)?($($field_tt:tt)*))?).+),* },
        $store_name:ident
    ) => {
        impl PartialEq for $value_ty {
            fn eq(&self, other: &Self) -> bool {
                self.id == other.id
            }
        }

        impl Eq for $value_ty {}

        impl std::hash::Hash for $value_ty {
            fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
                self.id.hash(state);
            }
        }

        impl crate::semantics::elements::element::SemanticElement for $value_ty {
            fn description(&self) -> String {
                crate::semantics::elements::element::ValueElement::to_key(self).description()
            }

            fn logical_name(&self) -> String {
                crate::semantics::elements::element::ValueElement::to_key(self).logical_name()
            }
        }

        impl crate::semantics::elements::element::ValueElement<$key_ty> for $value_ty {
            fn to_key(&self) -> $key_ty {
                $key_ty_name {
                    $($key_field: self.$($field_ident$($(::<$field_ty>)?($($field_tt)*))?).+),*
                }
            }
        }

        impl crate::semantics::elements::element::KeyElement<$value_ty> for $key_ty {
            fn get_value(&self, context: &crate::context::Context) -> Result<Rc<$value_ty>, crate::semantics::elements::ElementError> {
                context.$store_name.get(self)
            }
        }
    };
}
