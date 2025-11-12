using BlazorApp.EntityFramework.Models;
using BlazorApp.Core.Enum;
using BlazorApp.Core.Model;
using BlazorApp.EntityFramework.Enums;

namespace BlazorApp.ViewModel
{
    public class LayoutFieldValueModel: IFieldValuable
    {
        // DBと連携する基本プロパティ
        public int Id { get; set; }
        public int UILayoutBaseId { get; set; }
        public int FieldTypeDefinitionId { get; set; }
        public string Value { get; set; } = string.Empty;

        // UI専用の状態管理プロパティ
        public LayoutStatus LayoutStatus { get; set; }
        public OriginStatus OriginStatus { get; set; }

        public AttributeType AttributeType => FieldDefinition.Type;

        // ナビゲーションプロパティ
        public UIBaseLayout BaseLayout { get; set; } = null!;
        public FieldTypeDefinition FieldDefinition { get; set; } = null!;

        // Entityへの変換メソッド
        public LayoutFieldValue ToEntity()
        {
            return new LayoutFieldValue
            {
                Id = this.Id,
                UILayoutBaseId = this.UILayoutBaseId,
                FieldTypeDefinitionId = this.FieldTypeDefinitionId,
                Value = this.Value
            };
        }

        // EntityからViewModelを生成する静的メソッド（逆変換）
        public static LayoutFieldValueModel FromEntity(LayoutFieldValue entity)
        {
            return new LayoutFieldValueModel
            {
                Id = entity.Id,
                UILayoutBaseId = entity.UILayoutBaseId,
                FieldTypeDefinitionId = entity.FieldTypeDefinitionId,
                Value = entity.Value,
                FieldDefinition = entity.FieldDefinition,
                OriginStatus = OriginStatus.FromDb,
                LayoutStatus = LayoutStatus.Unchanged
            };
        }
    }
}
