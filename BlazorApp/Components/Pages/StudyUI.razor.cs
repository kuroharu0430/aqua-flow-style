using Microsoft.AspNetCore.Components;
using BlazorApp.Core.Service;
using BlazorApp.ViewModel;
using BlazorApp.Core.Enum;
using BlazorApp.Core.State;
using BlazorApp.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using BlazorApp.EntityFramework.Models;

namespace BlazorApp3.Client.Pages
{
    public partial class StudyUI : LayoutComponentBase
    {
        [Inject] public required LayoutDbContext DbContext { get; set; }
        [Inject] public required IDbContextFactory<LayoutDbContext> DbContextFactory { get; set; }

        public SurfaceInteractionMode SurfaceInteractionMode { get; set; } = SurfaceInteractionMode.Dragging;

        public bool IsSelectingMode
        {
            get => SurfaceInteractionMode == SurfaceInteractionMode.Selecting;
            set => SurfaceInteractionMode = value ? SurfaceInteractionMode.Selecting : SurfaceInteractionMode.Dragging;
        }

        public OverlapMode OverlapMode { get; set; } = OverlapMode.Push;

        public IEnumerable<OverlapMode> OverlapModes => Enum.GetValues(typeof(OverlapMode)).Cast<OverlapMode>();

        private Dictionary<int, List<UILayoutModelBase>> LayoutsBySection = new();

        public List<FieldTypeDefinition> FieldTypeDefinitions { get; set; } = [];
        public List<FieldEditDefinition> FieldEditDefinitions { get; set; } = [];

        public DisplayOption DisplayOption { get; set; } = new();

        private int SelectedThemeId = 1;

        private List<Theme> Themes = new();

        private int CurrentSectionNumber = 1;

        /// <summary>
        /// 現在のLayoutSection　※Theme作成時に必ず1section作成するのでnullはあり得ない
        /// </summary>
        private Theme CurrentTheme => Themes.First(t => t.Id == SelectedThemeId);

        private LayoutSection CurrentSection => CurrentTheme.LayoutSections
            .Single(s => s.Number == CurrentSectionNumber);

        protected override void OnInitialized()
        {
            // DBから初期データを読み込む
            // ViewModelに変換しないのでTracking
            Themes = DbContext.Themes
                .Include(t => t.LayoutSections)
                .ToList(); // ← Tracking OK

            var sectionIds = Themes.SelectMany(t => t.LayoutSections).Select(s => s.Id).ToList();

            // ViewModelに変換するのでNoTracking
            var layouts = DbContext.UIBaseLayouts
                .Where(l => sectionIds.Contains(l.PageId))
                .Include(l => l.FieldValues)
                .ThenInclude(f => f.FieldDefinition)
                .AsNoTracking()
                .ToList();

            // UIBaseLayoutsをViewModelに変換
            LayoutsBySection = Themes
                .SelectMany(t => t.LayoutSections)
                .ToDictionary(
                    section => section.Id,
                    section => layouts
                        .Select(layout => new UILayoutModelBase(layout))
                        .ToList()
                );

            FieldTypeDefinitions = DbContext.FieldTypeDefinitions
                .OrderBy(f => f.DisplayOrder)
                .ToList();
            FieldEditDefinitions = DbContext.FieldEditDefinitions
                .OrderBy(f => f.DisplayOrder)
                .ToList();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // TODO READY実装
            }
        }

        /// <summary>
        /// Layout追加イベント
        /// </summary>
        /// <param name="layout"></param>
        private void HandleLayoutAdded(UILayoutModelBase layout)
        {
            LayoutsBySection[CurrentSection.Id].Add(layout);
            StateHasChanged();
        }

        private void Save()
        {
            // ViewModelをcontextに追従させないようにする
            using var context = DbContextFactory.CreateDbContext();

            foreach (var layout in LayoutsBySection[CurrentSection.Id])
            {
                var entity = layout.ToEntity();

                switch ((layout.OriginStatus, layout.LayoutStatus))
                {
                    case (OriginStatus.FromAdd, LayoutStatus.Deleted):
                        // 新規だけど削除された → 保存しない
                        break;

                    case (OriginStatus.FromAdd, _):
                        context.Add(entity);                 
                        layout.OriginStatus = OriginStatus.FromDb;
                        break;

                    case (OriginStatus.FromDb, LayoutStatus.Deleted):
                        context.Remove(entity);
                        break;

                    case (OriginStatus.FromDb, _):
                        context.Update(entity);
                        break;
                }

                foreach (var field in layout.FieldValues)
                {
                    var fieldEntity = field.ToEntity();

                    switch ((field.OriginStatus, field.LayoutStatus))
                    {
                        case (OriginStatus.FromAdd, LayoutStatus.Deleted):
                            // 新規で削除された → 保存しない
                            break;

                        case (OriginStatus.FromAdd, _):
                            context.Add(fieldEntity);
                            field.OriginStatus = OriginStatus.FromDb;
                            break;

                        case (OriginStatus.FromDb, LayoutStatus.Deleted):
                            context.Remove(fieldEntity);
                            break;

                        case (OriginStatus.FromDb, _):
                            context.Update(fieldEntity);
                            break;
                    }
                }
            }
            context.SaveChanges();
            DbContext.SaveChanges();
        }

    }

    public enum LayoutDragMode
    {
        Registering,
        Move,
        Resize
    }

    //Themes = new List<Theme>
    //{
    //    new Theme { Id = 1, Name = "Dark" },
    //    new Theme { Id = 2, Name = "Light" }
    //};
    //var pages = new List<LayoutSection>();

    //int pageIdCounter = 1;
    //int layoutIdCounter = 1;

    //foreach (var theme in Themes)
    //{
    //    for (int sectionIndex = 0; sectionIndex < 3; sectionIndex++)
    //    {
    //        var page = new LayoutSection
    //        {
    //            Id = pageIdCounter++,
    //            Number = sectionIndex + 1,
    //            ThemeId = theme.Id,
    //            Theme = theme,
    //            ColumnNumber = 100,
    //            RowNumber = 100,
    //            WidthPerCell = 100,
    //            HeightPerCell = 100,
    //            ScreenWidth = 800,
    //            ScreenHeight = 600,
    //            Layouts = new List<UIBaseLayout>()
    //        };

    //        for (int i = 0; i < 1000; i++)
    //        {
    //            int x = i % 50;
    //            int y = i / 50;

    //            ComponentType type = (i % 4) switch
    //            {
    //                0 => ComponentType.Button,
    //                1 => ComponentType.Label,
    //                2 => ComponentType.Image,
    //                3 => ComponentType.Button,
    //                _ => ComponentType.Button,
    //            };

    //            string name = $"{type}_{i + 1}";

    //            var layout = new UIBaseLayout
    //            {
    //                Id = layoutIdCounter++,
    //                Name = name,
    //                X = x,
    //                Y = y,
    //                SizeX = 1,
    //                SizeY = 1,
    //                LayoutType = type,
    //                PageId = page.Id,
    //                Page = page,
    //                StyleJson = "{}",
    //                WrapperStyleJson = "{}",
    //                CssJson = "{}",
    //                FieldValues = new List<LayoutFieldValue>()
    //            };

    //            page.Layouts.Add(layout);
    //        }

    //        LayoutSections.Add(page);
    //    }

    //    foreach (var section in LayoutSections)
    //    {
    //        LayoutsBySection[section.Id] = section.Layouts
    //            .Select(layout => new UILayoutModelBase(
    //                layout.Name,
    //                layout.X,
    //                layout.Y,
    //                layout.LayoutType)
    //            {
    //                LayoutSectionId = section.Id,
    //            })
    //            .ToList();
    //    }
    //}

}
