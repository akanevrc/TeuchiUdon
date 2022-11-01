
#[macro_export]
macro_rules! impl_key_value_elements {
    (
        $key_type:ty,
        $value_type:ty,
        $key_type_name:ident { $($key_field:ident),+ },
        format!($format_str:literal, $($format_field:ident),+),
        $store_name:ident
    ) => {
        impl PartialEq for $value_type {
            fn eq(&self, other: &Self) -> bool {
                self.id == other.id
            }
        }

        impl Eq for $value_type {}

        impl std::hash::Hash for $value_type {
            fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
                self.id.hash(state);
            }
        }

        impl crate::semantics::elements::element::SemanticElement for $value_type {
            fn description(&self) -> String {
                use crate::semantics::elements::element::ValueElement;
                self.to_key().description()
            }
        }

        impl crate::semantics::elements::element::ValueElement<$key_type> for $value_type {
            fn to_key(&self) -> $key_type {
                $key_type_name {
                    $($key_field: self.$key_field.clone(),)+
                }
            }
        }

        impl crate::semantics::elements::element::SemanticElement for $key_type {
            fn description(&self) -> String {
                format!($format_str, $(self.$format_field.description()),+)
            }
        }

        impl crate::semantics::elements::element::KeyElement<$value_type> for $key_type {
            fn consume_key(self, context: &crate::context::Context) -> Option<Rc<$value_type>> {
                context.$store_name.get(self)
            }

            fn consume_key_or_err(self, context: &crate::context::Context) -> Result<Rc<$value_type>, crate::semantics::elements::ElementError> {
                context.$store_name.get_unwrap(self)
            }
        }
    };
}
