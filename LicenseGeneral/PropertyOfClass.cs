using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Smartline.License.Common {
    public class PropertyOfClass<TClass> {
        public static PropertyValue[] GetProperties(TClass source) {
            var result = new List<PropertyValue>();
            foreach (PropertyInfo item in source.GetType().GetProperties()) {
                result.Add(new PropertyValue {
                    Property = item.Name,
                    Title = UserDescriptionAttribute.GetStatusText<TClass>(item.Name),
                    ShowOnlyAdmin = UserDescriptionAttribute.GetMarkShowOnlyAdmin<TClass>(item.Name),
                    Value = item.GetValue(source, null)
                });
            }
            return result.ToArray();
        }

        public static void SetValues(PropertyValue[] source, ref TClass target) {
            if (target == null) { return; }

            foreach (PropertyInfo item in target.GetType().GetProperties()) {
                PropertyValue founded;
                if ((founded = source.FirstOrDefault(a => a.Property == item.Name)) != null) {
                    item.SetValue(target, founded.Value, null);
                }
            }
        }
    }
}