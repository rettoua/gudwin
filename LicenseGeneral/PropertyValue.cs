using System;

namespace Smartline.License.Common {
    public class PropertyValue {
        private Type _type;
        private object _value;
        public string Title { get; set; }

        public string Property { get; set; }

        public object Value {
            get { return _value; }
            set {
                if (_value == null) {
                    _value = value;
                    _type = value.GetType();
                } else {
                    if (_type == typeof(Guid)) {
                        _value = Guid.Parse((string)value);
                    } else if (_type == typeof(Version))
                        _value = Version.Parse((string)value);
                    else {
                        _value = Convert.ChangeType(value, _type);
                    }
                }
            }
        }

        public bool ShowOnlyAdmin { get; set; }
    }
}