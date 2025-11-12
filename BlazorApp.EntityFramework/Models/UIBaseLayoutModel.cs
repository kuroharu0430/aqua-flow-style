using BlazorApp.Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.EntityFramework.Models
{
    public class UIBaseLayout
    {
        [Key]
        public int Id { get; set; }              // 一意の識別子
        public string Name { get; set; } = "default";        // レイアウト名
        public ComponentType LayoutType { get; set; }   // レイアウトの種類
        public int X { get; set; }               // X座標
        public int Y { get; set; }               // Y座標
        public int SizeX { get; set; }           // 幅
        public int SizeY { get; set; }          // 高さ
        public string StyleJson { get; set; } = "{}";
        public string WrapperStyleJson { get; set; } = "{}";
        public string CssJson { get; set; } = "{}";
        public List<LayoutFieldValue> FieldValues { get; set; } = new();

        public int PageId { get; set; }
        public LayoutSection Page { get; set; }

    }
}
