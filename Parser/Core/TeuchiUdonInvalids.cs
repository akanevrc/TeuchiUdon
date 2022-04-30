using System.Linq;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonInvalids
    {
        public TeuchiUdonType InvalidType { get; private set; }
        public InvalidLabel InvalidLabel { get; private set; }
        public TeuchiUdonBlock InvalidBlock { get; private set; }
        public TeuchiUdonMethod InvalidMethod { get; private set; }
        public TeuchiUdonOutValue InvalidOutValue { get; private set; }

        private bool IsInitialized { get; set; }

        public void Init()
        {
            if (!IsInitialized)
            {
                InvalidType  = new TeuchiUdonType(TeuchiUdonQualifier.Top, "invalid", "invalid", "SystemObject", typeof(object));
                InvalidLabel = new InvalidLabel(InvalidType);
                InvalidBlock = new TeuchiUdonBlock(-1, TeuchiUdonQualifier.Top, InvalidType)
                {
                    Return   = InvalidLabel,
                    Continue = InvalidLabel,
                    Break    = InvalidLabel
                };
                InvalidMethod = new TeuchiUdonMethod
                (
                    InvalidType,
                    "_",
                    Enumerable.Empty<TeuchiUdonType>(),
                    Enumerable.Empty<TeuchiUdonType>(),
                    Enumerable.Empty<TeuchiUdonType>(),
                    Enumerable.Empty<TeuchiUdonMethodParamInOut>(),
                    "_",
                    Enumerable.Empty<string>()
                );
                InvalidOutValue = new TeuchiUdonOutValue(TeuchiUdonQualifier.Top, InvalidType, -1);
                IsInitialized = true;
            }
        }
    }
}
