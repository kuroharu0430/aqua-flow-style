using BlazorApp.Core.Enum;
using BlazorApp.EntityFramework.Enums;
using BlazorApp.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace BlazorApp.EntityFramework.Context
{
    public class LayoutDbContext : DbContext
    {
        public LayoutDbContext(DbContextOptions<LayoutDbContext> options) : base(options) { }

        public DbSet<Theme> Themes { get; set; }
        public DbSet<LayoutSection> LayoutSections { get; set; }
        public DbSet<UIBaseLayout> UIBaseLayouts { get; set; }
        public DbSet<FieldTypeDefinition> FieldTypeDefinitions { get; set; }
        public DbSet<FieldEditDefinition> FieldEditDefinitions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 初回Migaration時にTheme, LayoutSectionがある状態を保証
            modelBuilder.Entity<Theme>().HasData(
                new Theme { Id = 1, Name = "Default" }
            );

            var layoutSection = new LayoutSection()
            {
                Id = 1,
                ThemeId = 1,
                Number = 1,
                ColumnNumber = 10,
                RowNumber = 10,
                WidthPerCell = 100,
                HeightPerCell = 100,
                ScreenWidth = 400,
                ScreenHeight = 600
            };
            modelBuilder.Entity<LayoutSection>().HasData(layoutSection);


            modelBuilder.Entity<FieldTypeDefinition>().HasData(
                new FieldTypeDefinition
                {
                    Id = 1,
                    Type = AttributeType.Text,
                    DefaultDisplayName = "表示テキスト",
                    InputType = "text",
                    DisplayOrder = 1
                },
                new FieldTypeDefinition
                {
                    Id = 2,
                    Type = AttributeType.Src,
                    DefaultDisplayName = "画像ソース",
                    InputType = "text",
                    DisplayOrder = 2
                },
                new FieldTypeDefinition
                {
                    Id = 3,
                    Type = AttributeType.Alt,
                    DefaultDisplayName = "代替テキスト",
                    InputType = "text",
                    DisplayOrder = 3
                },
                new FieldTypeDefinition
                {
                    Id = 4,
                    Type = AttributeType.Href,
                    DefaultDisplayName = "リンク先URL",
                    InputType = "text",
                    DisplayOrder = 4
                },
                new FieldTypeDefinition
                {
                    Id = 5,
                    Type = AttributeType.Target,
                    DefaultDisplayName = "リンクの開き方",
                    InputType = "select",
                    DisplayOrder = 5
                },
                new FieldTypeDefinition
                {
                    Id = 6,
                    Type = AttributeType.Title,
                    DefaultDisplayName = "ツールチップ",
                    InputType = "text",
                    DisplayOrder = 6
                }
            );

            modelBuilder.Entity<FieldEditDefinition>().HasData(
                // Button
                new FieldEditDefinition
                {
                    Id = 1,
                    LayoutType = ComponentType.Button,
                    FieldTypeDefinitionId = 1, // Text
                    DisplayName = "ボタンラベル",
                    IsRequired = true,
                    DisplayOrder = 1
                },

                // Label
                new FieldEditDefinition
                {
                    Id = 2,
                    LayoutType = ComponentType.Label,
                    FieldTypeDefinitionId = 1, // Text
                    DisplayName = "ラベル表示文字",
                    IsRequired = true,
                    DisplayOrder = 1
                },

                // Image
                new FieldEditDefinition
                {
                    Id = 3,
                    LayoutType = ComponentType.Image,
                    FieldTypeDefinitionId = 2, // Src
                    DisplayName = "画像URL",
                    IsRequired = true,
                    DisplayOrder = 1
                },
                new FieldEditDefinition
                {
                    Id = 4,
                    LayoutType = ComponentType.Image,
                    FieldTypeDefinitionId = 3, // Alt
                    DisplayName = "代替テキスト",
                    IsRequired = false,
                    DisplayOrder = 2
                },

                // Link
                new FieldEditDefinition
                {
                    Id = 5,
                    LayoutType = ComponentType.Link,
                    FieldTypeDefinitionId = 4, // Href
                    DisplayName = "リンク先URL",
                    IsRequired = true,
                    DisplayOrder = 1
                },
                new FieldEditDefinition
                {
                    Id = 6,
                    LayoutType = ComponentType.Link,
                    FieldTypeDefinitionId = 5, // Target
                    DisplayName = "リンクの開き方",
                    IsRequired = false,
                    DisplayOrder = 2
                },
                new FieldEditDefinition
                {
                    Id = 7,
                    LayoutType = ComponentType.Link,
                    FieldTypeDefinitionId = 1, // Text
                    DisplayName = "リンクテキスト",
                    IsRequired = true,
                    DisplayOrder = 3
                }
            );
        }
    }
}
