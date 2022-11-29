
#[macro_export]
macro_rules! impl_key_value_elements {
    (
        $key_ty:ident,
        $value_ty:ident$(<$($value_lifetime:lifetime),+>)?,
        $key_ty_name:ident { $($key_field:ident: self.$($field_ident:ident$($(::<$field_ty:ty>)?($($field_tt:tt)*))?).+),* },
        $store_name:ident
    ) => {
        impl$(<$($value_lifetime),+>)? PartialEq for $value_ty$(<$($value_lifetime),+>)? {
            fn eq(&self, other: &Self) -> bool {
                self.id == other.id
            }
        }

        impl$(<$($value_lifetime),+>)? Eq for $value_ty$(<$($value_lifetime),+>)? {}

        impl$(<$($value_lifetime),+>)? PartialOrd for $value_ty$(<$($value_lifetime),+>)? {
            fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
                self.id.partial_cmp(&other.id)
            }
        }

        impl$(<$($value_lifetime),+>)? Ord for $value_ty$(<$($value_lifetime),+>)? {
            fn cmp(&self, other: &Self) -> std::cmp::Ordering {
                self.id.cmp(&other.id)
            }
        }

        impl$(<$($value_lifetime),+>)? std::hash::Hash for $value_ty$(<$($value_lifetime),+>)? {
            fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
                self.id.hash(state);
            }
        }

        impl$(<$($value_lifetime),+>)? crate::semantics::elements::element::SemanticElement for $value_ty$(<$($value_lifetime),+>)? {
            fn description(&self) -> String {
                crate::semantics::elements::element::ValueElement::to_key(self).description()
            }

            fn logical_name(&self) -> String {
                crate::semantics::elements::element::ValueElement::to_key(self).logical_name()
            }
        }

        impl$(<$($value_lifetime),+>)? crate::semantics::elements::element::ValueElement<$key_ty> for $value_ty$(<$($value_lifetime),+>)? {
            fn to_key(&self) -> $key_ty {
                $key_ty_name {
                    $($key_field: self.$($field_ident$($(::<$field_ty>)?($($field_tt)*))?).+),*
                }
            }
        }

        impl<'input> crate::semantics::elements::element::KeyElement<'input, $value_ty$(<$($value_lifetime),+>)?> for $key_ty {
            fn get_value(
                &self, context: &crate::context::Context<'input>
            ) -> Result<Rc<$value_ty$(<$($value_lifetime),+>)?>, crate::semantics::elements::ElementError> {
                context.$store_name.get(self)
            }
        }
    };
}
