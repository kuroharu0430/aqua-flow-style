using System.ComponentModel.DataAnnotations;

namespace BlazorApp.EntityFramework.Models
{
    public class Theme
    {
        [Key]
        public int Id { get; set; }         // 主キー
        public string Name { get; set; } = string.Empty;

        // ナビゲーション
        public List<LayoutSection> LayoutSections { get; set; } = [];
    }
}
