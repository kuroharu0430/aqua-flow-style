//using BlazorApp.Core.Enum;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;
//using BlazorApp.EntityFramework.Enums;
//using System.Reflection.Metadata;

//public class LayoutFieldDefinition
//{
//    [Key]
//    public int Id { get; set; }

//    [Required]
//    public ComponentType LayoutType { get; set; }

//    [Required]
//    public string FieldName { get; set; } = "";

//    [ForeignKey(nameof(FieldType))]
//    public int FieldTypeId { get; set; }

//    public AttributeType AttributeType => FieldType.Type;

//    public FieldTypeDefinition FieldType { get; set; } = null!;

//    [Required]
//    public string DisplayName { get; set; } = "";

//    public bool IsRequired { get; set; } = false;

//    public int DisplayOrder { get; set; } = 0;
//}