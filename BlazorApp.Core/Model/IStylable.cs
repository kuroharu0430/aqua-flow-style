using BlazorApp.Core.Styling;
using BlazorApp.Core.Enum;

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
    public interface IDisplayOptionEditable
    {
        bool IsRestoring { get; set; }
        int ColumnNumber { get; set; }
        int RowNumber { get; set; }
        int WidthPerCell { get; set; }
        int HeightPerCell { get; set; }
        LayoutStatus LayoutStatus { get; }
    }
}
