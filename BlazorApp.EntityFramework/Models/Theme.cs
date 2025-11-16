using System.ComponentModel.DataAnnotations;

namespace BlazorApp.EntityFramework.Models
{
    public class Theme : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }         // 主キー
        public string Name { get; set; } = string.Empty;

        public DateTime? DeletedAt { get; set; }

        // ナビゲーション
        public List<LayoutSection> LayoutSections { get; set; } = [];
    }
}
