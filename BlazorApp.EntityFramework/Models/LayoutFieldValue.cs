using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.EntityFramework.Models
{
    // Unique制約
    [Index(nameof(UILayoutBaseId), nameof(FieldTypeDefinitionId), IsUnique = true)]
    public class LayoutFieldValue : ISoftDeletable
    {
        [Key]
        public int Id { get; set; } // サロゲートキー（主キー）

        [ForeignKey(nameof(BaseLayout))]
        public int UILayoutBaseId { get; set; } // UIBaseLayout.Id

        [ForeignKey(nameof(FieldDefinition))]
        public int FieldTypeDefinitionId { get; set; } // LayoutFieldDefinition.Id

        [Required]
        public string Value { get; set; } = string.Empty; // 実データ

        public DateTime? DeletedAt { get; set; }

        // ナビゲーションプロパティ
        public UIBaseLayout BaseLayout { get; set; } = null!;
        public FieldTypeDefinition FieldDefinition { get; set; } = null!;
    }
}

