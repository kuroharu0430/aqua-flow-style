namespace BlazorApp.Core.Model
{
    public class ViewModelMetadata
    {
        public string ViewModel { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public Dictionary<string, FieldMetadata> Fields { get; set; } = new();
    }

    public class FieldMetadata
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Type { get; set; } = "";
    }
}
