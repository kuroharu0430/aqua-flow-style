using Microsoft.AspNetCore.Components;
using BlazorApp.Core.Service;
using BlazorApp.ViewModel;
using BlazorApp.Core.Enum;
using BlazorApp.Core.State;
using BlazorApp.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using BlazorApp.EntityFramework.Models;
using BlazorApp.Service;
using BlazorApp.Components.Dialog;

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
                .ToList();

            var sectionIds = Themes.SelectMany(t => t.LayoutSections).Select(s => s.Id).ToList();

            // ViewModelに変換するのでNoTracking
            var layouts = DbContext.UIBaseLayouts
                .Where(l => sectionIds.Contains(l.LayoutSectionId))
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
                        .Where(l => l.LayoutSectionId == section.Id)
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
                var layoutEntity = layout.ToEntity();

                switch ((layout.OriginStatus, layout.LayoutStatus))
                {
                    case (OriginStatus.FromAdd, LayoutStatus.Deleted):
                        // 新規だけど削除された → 保存しない
                        break;

                    case (OriginStatus.FromAdd, _):
                        context.Add(layoutEntity);
                        // ここでId が確定
                        context.SaveChanges();
                        // ViewModelに代入
                        layout.Id = layoutEntity.Id;
                        //layout.FieldValuesに
                        layout.FieldValues.ForEach(f => f.UILayoutBaseId = layoutEntity.Id);
                        layout.OriginStatus = OriginStatus.FromDb;
                        break;

                    case (OriginStatus.FromDb, LayoutStatus.Deleted):
                        context.Remove(layoutEntity);
                        break;

                    case (OriginStatus.FromDb, _):
                        context.Update(layoutEntity);
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
                            // ここでId が確定
                            context.SaveChanges();
                            // ViewModelに代入
                            field.Id = fieldEntity.Id;
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
            // 追従しているcontextもsave
            DbContext.SaveChanges();
        }

        private async Task CreateNewTheme()
        {
            var result = await DialogService.OpenAsync<CreateTheme>(
                "テーマ作成",
                new Dictionary<string, object> { },
                new DialogOptions { }
            );

            if (result is string themeName)
            {
                // 新しい Theme を作成
                var newTheme = new Theme
                {
                    Name = themeName,
                    LayoutSections = new List<LayoutSection>
                    {
                        {
                            // 必ず1セクション作成
                            LayoutSection.CreateDefault()
                        }
                    }
                };

                // DbContextに追加して保存
                DbContext.Themes.Add(newTheme);
                DbContext.SaveChanges();

                // メモリ上のリストにも追加
                Themes.Add(newTheme);
                int newThemeId = newTheme.Id;
                int newSectionId = newTheme.LayoutSections[0].Id;
                // Dictionaryにも反映しておく
                LayoutsBySection[newSectionId] = new List<UILayoutModelBase>();

                // 選択中テーマを新しいものに切り替え
                SelectedThemeId = newTheme.Id;

                StateHasChanged();
            }
        }

        private void CreateNewSelction()
        {

        }
    }

    public enum LayoutDragMode
    {
        Registering,
        Move,
        Resize
    }
}
