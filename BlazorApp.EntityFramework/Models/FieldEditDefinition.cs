using BlazorApp.Core.Enum;
using BlazorApp.EntityFramework.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.EntityFramework.Models
{
    public class FieldEditDefinition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ComponentType LayoutType { get; set; }

        [ForeignKey(nameof(FieldType))]
        public int FieldTypeDefinitionId { get; set; }

        [Required]
        public FieldTypeDefinition FieldType { get; set; } = null!;

        public AttributeType AttributeType => FieldType.Type;

        [Required]
        public string DisplayName { get; set; } = "";

        public bool IsRequired { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;
    }

}
