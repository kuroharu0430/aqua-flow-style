using BlazorApp.Core.Styling;
using BlazorApp.Core.Enum;
using BlazorApp.Core.State;

namespace BlazorApp.Core.Model
{
    public interface IStylable
    {
        StyleBuilder Style { get; set; }
        StyleBuilder WrapperStyle { get; set; }
        LayoutStatus LayoutStatus { get; set; }
    }

    public interface IFieldValuable
    {
        string Value { get; set; }
        LayoutStatus LayoutStatus { get; set; }
    }

}
