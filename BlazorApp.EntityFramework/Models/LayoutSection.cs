using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.EntityFramework.Models
{
    public class LayoutSection : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }         // 主キー
        public int Number { get; set; }     // ページ番号や識別用の番号
        public int ColumnNumber { get; set; }
        public int RowNumber { get; set; }
        public int WidthPerCell { get; set; }
        public int HeightPerCell { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int ThemeId { get; set; }    // 外部キー
        public DateTime? DeletedAt { get; set; }
        [ForeignKey("ThemeId")]
        public Theme Theme { get; set; } = null!;    // ナビゲーションプロパティ

        public List<UIBaseLayout> Layouts { get; set; } = new(); // 子レイアウトへのナビゲーション

        public static LayoutSection CreateDefault()
        {
            return new LayoutSection
            {
                Number = 1,
                ColumnNumber = 10,
                RowNumber = 10,
                WidthPerCell = 100,
                HeightPerCell = 100,
                ScreenWidth = 400,
                ScreenHeight = 600
            };
        }
    }
}
