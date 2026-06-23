using BlazorApp.Core.Model;
using BlazorApp.Core.Enum;
using BlazorApp.EntityFramework.Models;
using BlazorApp.Core.Styling;
using BlazorApp.EntityFramework.Enums;
using BlazorApp.Components;

namespace BlazorApp.ViewModel
{
    public class UILayoutModelBase : IDraggable, IStylable
    {
        /// <summary>
        /// コンストラクタ　DB読み出し時
        /// </summary>
        /// <param name="entity"></param>
        public UILayoutModelBase(UIBaseLayout entity)
        {
            Id = entity.Id;
            LayoutSectionId = entity.LayoutSectionId;
            Name = entity.Name;
            ComponentType = entity.LayoutType;
            GridBounds = new GridBounds(entity.X, entity.Y, entity.SizeX, entity.SizeY);
            OriginStatus = OriginStatus.FromDb;
            LayoutStatus = LayoutStatus.Unchanged;
            Style = entity.StyleJson.FromJson();
            WrapperStyle = entity.WrapperStyleJson.FromJson();
            FieldValues = [.. entity.FieldValues.Select(LayoutFieldValueModel.FromEntity)];
        }

        /// <summary>
        /// コンストラクタ　新規追加用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public UILayoutModelBase(string name, int x, int y, ComponentType componentType, int layoutSectionId)
        {
            Name = name;
            GridBounds = new GridBounds(x, y, 1, 1);
            ComponentType = componentType;
            OriginStatus = OriginStatus.FromAdd;
            LayoutSectionId = layoutSectionId;
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public GridBounds GridBounds { get; set; }
        public RectBounds RectBounds { get; set; } = new RectBounds(0, 0, 0, 0);

        public bool NeedsRectUpdate { get; set; } = true;

        public SelectionState SelectionState { get; set; }
        public InteractionPhase InteractionPhase { get; set; }
        public MobilityState MobilityState { get; set; }
        public LayoutStatus LayoutStatus { get; set; }

        // コンポーネント種別（Label, Image, Radio, etc）
        public virtual ComponentType ComponentType { get; }
        public OriginStatus OriginStatus { get; set; }

        public StyleBuilder Style { get; set; } = new();
        public StyleBuilder WrapperStyle { get; set; } = new();

        public List<LayoutFieldValueModel> FieldValues { get; set; } = [];

        public IEnumerable<LayoutFieldValueModel> VisibleFieldValues =>
                    FieldValues.Where(f => f.LayoutStatus != LayoutStatus.Deleted);
        public int LayoutSectionId { get; set; }

        // indexer
        public string this[AttributeType type] =>
                    VisibleFieldValues.FirstOrDefault(v => v.AttributeType == type)?.Value ?? string.Empty;

        /// <summary>
        /// ComponetTypeとViewのMapping
        /// </summary>
        public static readonly Dictionary<ComponentType, Type> ComponentMap = new()
        {
            { ComponentType.Button, typeof(ButtonLayoutView) },
            { ComponentType.Image, typeof(ImageLayoutView) },
            { ComponentType.Label, typeof(LabelLayoutView) },
            { ComponentType.Link, typeof(LinkLayoutView) },
            // 他のタイプも追加
        };

        public Type GetViewComponentType()
        {
            return ComponentMap.TryGetValue(ComponentType, out var componentType)
                ? componentType
                : typeof(ButtonLayoutView);
        }
        public UIBaseLayout ToEntity()
        {
            return new UIBaseLayout
            {
                Id = this.Id,
                Name = this.Name,
                LayoutType = this.ComponentType,
                X = this.GridBounds.X,
                Y = this.GridBounds.Y,
                SizeX = this.GridBounds.SizeX,
                SizeY = this.GridBounds.SizeY,
                StyleJson = this.Style.ToJson(),
                WrapperStyleJson = this.WrapperStyle.ToJson(),
                CssJson = string.Empty,
                LayoutSectionId = this.LayoutSectionId,
                // 状態は共有してないので別で保存
                //FieldValues = [.. this.FieldValues.Select(f => f.ToEntity())]
            };
        }
    }
}
