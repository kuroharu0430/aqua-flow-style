namespace BlazorApp.Core.Styling
{
    public class CssClassBuilder
    {
        private readonly List<string> _classes = new();

        public CssClassBuilder Add(string className)
        {
            if (!string.IsNullOrWhiteSpace(className))
                _classes.Add(className);
            return this;
        }

        public CssClassBuilder AddRange(params string[] classes)
        {
            foreach (var cls in classes)
            {
                if (!string.IsNullOrWhiteSpace(cls))
                    _classes.Add(cls);
            }
            return this;
        }

        public CssClassBuilder AddIf(string className, bool condition)
        {
            if (condition)
                _classes.Add(className);
            return this;
        }

        public CssClassBuilder Remove(string className)
        {
            _classes.Remove(className);
            return this;
        }

        public CssClassBuilder Clear()
        {
            _classes.Clear();
            return this;
        }
        public bool Contains(string className)
        {
            return _classes.Contains(className);
        }

        public override string ToString()
        {
            return string.Join(" ", _classes);
        }

        public CssClassBuilder ApplyExternalCssClass(string? externalClass)
        {
            if (string.IsNullOrWhiteSpace(externalClass))
                return this;

            var classList = externalClass.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var cls in classList)
            {
                if (!string.IsNullOrWhiteSpace(cls))
                    _classes.Add(cls.Trim());
            }

            return this;
        }

        public CssClassBuilder AddCase<TEnum>(TEnum enumValue) where TEnum : System.Enum
        {
            var className = enumValue.ToString().ToLowerInvariant(); // "Floating" → "floating"
            return Add(className);
        }
    }
}