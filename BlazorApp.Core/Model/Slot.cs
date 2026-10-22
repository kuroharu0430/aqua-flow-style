namespace BlazorApp.Core.Model
{
    public class Slot
    {
        public string? TargetVM { get; set; }

        public string? Field { get; set; }
        public string? Type { get; set; }

        public object? Value { get; set; }
        public string? Option { get; set; }
        public Dictionary<string, object> Extra { get; set; } = new();
    }

}
