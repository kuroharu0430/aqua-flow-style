using System.Text.Json;

namespace BlazorApp.Core.Styling
{
    public class StyleBuilder
    {
        private readonly Dictionary<string, string> _styles = new();

        public StyleBuilder DeepCopy()
        {
            var copy = new StyleBuilder();
            copy.AddRange(this.ToDictionary());
            return copy;
        }

        public StyleBuilder Add<T>(string key, T value)
        {
            string cssValue = value switch
            {
                System.Enum e => e.ToString().ToLower(),
                string s => s,
                int i => $"{i}",
                double d => $"{d}",
                _ => value?.ToString() ?? ""
            };

            _styles[key] = cssValue;
            return this;
        }

        public StyleBuilder AddIf<T>(string key, T value, bool condition)
        {
            return condition ? Add(key, value) : this;
        }

        public StyleBuilder AddVar(string key, string cssVariableName)
        {
            _styles[key] = $"var(--{cssVariableName})";
            return this;
        }

        public StyleBuilder AddRange(Dictionary<string, string> styles)
        {
            foreach (var kv in styles)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                {
                    _styles[kv.Key] = kv.Value;
                }
            }
            return this;
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(_styles);
        }

        public StyleBuilder Toggle(string key, string value, bool isOn)
        {
            if (isOn)
                _styles[key] = value;
            else
                _styles.Remove(key);
            return this;
        }

        public StyleBuilder Replace(string key, string newValue)
        {
            _styles[key] = newValue;
            return this;
        }
        public StyleBuilder Remove(string key)
        {
            _styles.Remove(key);
            return this;
        }

        public StyleBuilder Clear()
        {
            _styles.Clear();
            return this;
        }

        public override string ToString()
        {
            return string.Join("; ", _styles.Select(kv => $"{kv.Key}:{kv.Value}"));
        }

        // StyleParameterを受けてMergeする
        public StyleBuilder ApplyExternalStyle(string? applyStyle)
        {
            if (string.IsNullOrWhiteSpace(applyStyle))
                return this;

            var pairs = applyStyle.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split(':', 2);
                if (kv.Length == 2)
                {
                    var key = kv[0].Trim();
                    var value = kv[1].Trim();
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        _styles[key] = value;
                }
            }
            return this;
        }

        public string? this[string key]
        {
            get => _styles.TryGetValue(key, out var value) ? value : null;
            set
            {
                if (string.IsNullOrWhiteSpace(key)) return;

                if (string.IsNullOrWhiteSpace(value))
                    _styles.Remove(key);
                else
                    _styles[key] = value;
            }
        }

        // よく使うStyleは直接プロパティにしてみる
        // レイアウト系
        public StyleBuilder Top(string value) => Add("top", value);
        public StyleBuilder Left(string value) => Add("left", value);
        public StyleBuilder Right(string value) => Add("right", value);
        public StyleBuilder Bottom(string value) => Add("bottom", value);
        public StyleBuilder Display(string value) => Add("display", value);
        public StyleBuilder Overflow(string value) => Add("overflow", value);
        public StyleBuilder ZIndex(string value) => Add("z-index", value);
        public StyleBuilder Width(string value) => Add("width", value);
        public StyleBuilder Height(string value) => Add("height", value);


        // Position
        public StyleBuilder Position(string value) => Add("position", value);
        public StyleBuilder Position(PositionType value) => Add("position", value.ToString().ToLower());

        // スペーシング系
        // Padding 系
        public StyleBuilder Padding(string value) => Add("padding", value);
        public StyleBuilder PaddingTop(string value) => Add("padding-top", value);
        public StyleBuilder PaddingBottom(string value) => Add("padding-bottom", value);
        public StyleBuilder PaddingLeft(string value) => Add("padding-left", value);
        public StyleBuilder PaddingRight(string value) => Add("padding-right", value);

        // Margin 系
        public StyleBuilder Margin(string value) => Add("margin", value);
        public StyleBuilder MarginTop(string value) => Add("margin-top", value);
        public StyleBuilder MarginBottom(string value) => Add("margin-bottom", value);
        public StyleBuilder MarginLeft(string value) => Add("margin-left", value);
        public StyleBuilder MarginRight(string value) => Add("margin-right", value);

        // 色・装飾系
        public StyleBuilder Color(string value) => Add("color", value);
        public StyleBuilder Border(string value) => Add("border", value);
        public StyleBuilder BackgroundColor (string value) => Add("background-color", value);
        public StyleBuilder BorderRadius(string value) => Add("border-radius", value);
        public StyleBuilder BoxShadow(string value) => Add("box-shadow", value);
        public StyleBuilder Opacity(string value) => Add("opacity", value);

        // フレックス系
        public StyleBuilder FlexDirection(string value) => Add("flex-direction", value);
        public StyleBuilder JustifyContent(string value) => Add("justify-content", value);
        public StyleBuilder AlignItems(string value) => Add("align-items", value);


        public enum PositionType
        {
            Static,
            Relative,
            Absolute,
            Fixed,
            Sticky
        }

        public enum DisplayType
        {
            Block,
            Inline,
            InlineBlock,
            Flex,
            Grid,
            None
        }
    }

    public static class StyleKeys
    {
        // スペーシング系
        public const string Padding = "padding";
        public const string PaddingTop = "padding-top";
        public const string PaddingBottom = "padding-bottom";
        public const string PaddingLeft = "padding-left";
        public const string PaddingRight = "padding-right";

        public const string Margin = "margin";
        public const string MarginTop = "margin-top";
        public const string MarginBottom = "margin-bottom";
        public const string MarginLeft = "margin-left";
        public const string MarginRight = "margin-right";

        // 色・装飾系
        public const string Color = "color";
        public const string BackgroundColor = "background-color";
        public const string Border = "border";
        public const string BorderRadius = "border-radius";
        public const string BoxShadow = "box-shadow";
        public const string Opacity = "opacity";

        // フレックス系
        public const string FlexDirection = "flex-direction";
        public const string JustifyContent = "justify-content";
        public const string AlignItems = "align-items";

        // Position 系
        public const string Position = "position";
        public const string Top = "top";
        public const string Left = "left";
        public const string Right = "right";
        public const string Bottom = "bottom";
    }

    public static class StyleBuilderExtensions
    {
        public static string ToJson(this StyleBuilder style)
        {
            return JsonSerializer.Serialize(style.ToDictionary());
        }

        public static StyleBuilder FromJson(this string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new StyleBuilder();

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return new StyleBuilder().AddRange(dict ?? new());
            }
            catch (Exception ex)
            {
                Console.WriteLine("FromJson失敗: " + ex.Message);
                return new StyleBuilder();
            }
        }


    }
}
