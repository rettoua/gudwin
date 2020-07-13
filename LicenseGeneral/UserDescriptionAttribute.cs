using System;
using System.Linq;

namespace Smartline.License.Common {
    public class UserDescriptionAttribute : Attribute {
        private readonly string _description;

        private readonly bool _showOnlyAdmin;

        public UserDescriptionAttribute(string description, bool showOnlyAdmin = false) {
            _description = description;
            _showOnlyAdmin = showOnlyAdmin;
        }

        public string Description {
            get { return _description; }
        }

        public bool ShowOnlyAdmin {
            get { return _showOnlyAdmin; }
        }

        public static string GetStatusText<TValue>(object value) {
            return
                ((UserDescriptionAttribute)
                 typeof(TValue).GetMember(value.ToString())[0].GetCustomAttributes(typeof(UserDescriptionAttribute),
                                                                                    false).First()).Description;
        }

        public static bool GetMarkShowOnlyAdmin<TValue>(object value) {
            return
                ((UserDescriptionAttribute)
                 typeof(TValue).GetMember(value.ToString())[0].GetCustomAttributes(typeof(UserDescriptionAttribute),
                                                                                    false).First()).ShowOnlyAdmin;
        }
    }
}