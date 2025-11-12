using BlazorApp.EntityFramework.Enums;
using BlazorApp.EntityFramework.Models;
using System.ComponentModel.DataAnnotations;

public class FieldTypeDefinition
{
    [Key]
    public int Id { get; set; }

    [Required]
    public AttributeType Type { get; set; }

    [Required]
    public string DefaultDisplayName { get; set; } = "";

    public int DisplayOrder { get; set; } = 0;

    public string? InputType { get; set; }

    //public bool IsPrimitive { get; set; } = true;

    //// 子とのナビゲーションプロパティ（1対多）
    //public List<LayoutFieldDefinition> LayoutFields { get; set; } = new();
}