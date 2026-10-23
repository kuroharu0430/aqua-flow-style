using System.ComponentModel;
using System.Reflection;
using BlazorApp.Core.Model;

namespace BlazorApp.Service
{
    public static class ViewModelMetaBuilder
    {
        public static ViewModelMetadata Build(object vm)
        {
            var type = vm.GetType();

            var meta = new ViewModelMetadata
            {
                ViewModel = type.Name,
                DisplayName = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                              ?? type.Name
            };

            foreach (var prop in type.GetProperties())
            {
                var fieldMeta = new FieldMetadata
                {
                    Name = prop.Name,
                    DisplayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                                  ?? prop.Name,
                    Type = ConvertType(prop.PropertyType)
                };

                meta.Fields[prop.Name] = fieldMeta;
            }

            return meta;
        }

        private static string ConvertType(Type t)
        {
            switch (t)
            {
                case Type _ when t == typeof(string):
                    return "string";

                case Type _ when t == typeof(int):
                    return "int";

                case Type _ when t == typeof(bool):
                    return "bool";

                case Type _ when t == typeof(DateTime):
                    return "datetime";

                default:
                    return "object";
            }
        }
    }
}
