using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlazorApp.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldTypeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultDisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    InputType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldTypeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FieldEditDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LayoutType = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldTypeDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldEditDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldEditDefinitions_FieldTypeDefinitions_FieldTypeDefinitionId",
                        column: x => x.FieldTypeDefinitionId,
                        principalTable: "FieldTypeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LayoutSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    ColumnNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    WidthPerCell = table.Column<int>(type: "INTEGER", nullable: false),
                    HeightPerCell = table.Column<int>(type: "INTEGER", nullable: false),
                    ScreenWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    ScreenHeight = table.Column<int>(type: "INTEGER", nullable: false),
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LayoutSections_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UIBaseLayouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LayoutType = table.Column<int>(type: "INTEGER", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    SizeX = table.Column<int>(type: "INTEGER", nullable: false),
                    SizeY = table.Column<int>(type: "INTEGER", nullable: false),
                    StyleJson = table.Column<string>(type: "TEXT", nullable: false),
                    WrapperStyleJson = table.Column<string>(type: "TEXT", nullable: false),
                    CssJson = table.Column<string>(type: "TEXT", nullable: false),
                    PageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UIBaseLayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UIBaseLayouts_LayoutSections_PageId",
                        column: x => x.PageId,
                        principalTable: "LayoutSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LayoutFieldValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UILayoutBaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldTypeDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutFieldValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LayoutFieldValue_FieldTypeDefinitions_FieldTypeDefinitionId",
                        column: x => x.FieldTypeDefinitionId,
                        principalTable: "FieldTypeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LayoutFieldValue_UIBaseLayouts_UILayoutBaseId",
                        column: x => x.UILayoutBaseId,
                        principalTable: "UIBaseLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FieldTypeDefinitions",
                columns: new[] { "Id", "DefaultDisplayName", "DisplayOrder", "InputType", "Type" },
                values: new object[,]
                {
                    { 1, "表示テキスト", 1, "text", 1 },
                    { 2, "画像ソース", 2, "text", 2 },
                    { 3, "代替テキスト", 3, "text", 3 },
                    { 4, "リンク先URL", 4, "text", 4 },
                    { 5, "リンクの開き方", 5, "select", 5 },
                    { 6, "ツールチップ", 6, "text", 6 }
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Default" });

            migrationBuilder.InsertData(
                table: "FieldEditDefinitions",
                columns: new[] { "Id", "DisplayName", "DisplayOrder", "FieldTypeDefinitionId", "IsRequired", "LayoutType" },
                values: new object[,]
                {
                    { 1, "ボタンラベル", 1, 1, true, 0 },
                    { 2, "ラベル表示文字", 1, 1, true, 1 },
                    { 3, "画像URL", 1, 2, true, 2 },
                    { 4, "代替テキスト", 2, 3, false, 2 },
                    { 5, "リンク先URL", 1, 4, true, 3 },
                    { 6, "リンクの開き方", 2, 5, false, 3 },
                    { 7, "リンクテキスト", 3, 1, true, 3 }
                });

            migrationBuilder.InsertData(
                table: "LayoutSections",
                columns: new[] { "Id", "ColumnNumber", "HeightPerCell", "Number", "RowNumber", "ScreenHeight", "ScreenWidth", "ThemeId", "WidthPerCell" },
                values: new object[] { 1, 10, 100, 1, 10, 600, 400, 1, 100 });

            migrationBuilder.CreateIndex(
                name: "IX_FieldEditDefinitions_FieldTypeDefinitionId",
                table: "FieldEditDefinitions",
                column: "FieldTypeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutFieldValue_FieldTypeDefinitionId",
                table: "LayoutFieldValue",
                column: "FieldTypeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutFieldValue_UILayoutBaseId_FieldTypeDefinitionId",
                table: "LayoutFieldValue",
                columns: new[] { "UILayoutBaseId", "FieldTypeDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LayoutSections_ThemeId",
                table: "LayoutSections",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_UIBaseLayouts_PageId",
                table: "UIBaseLayouts",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldEditDefinitions");

            migrationBuilder.DropTable(
                name: "LayoutFieldValue");

            migrationBuilder.DropTable(
                name: "FieldTypeDefinitions");

            migrationBuilder.DropTable(
                name: "UIBaseLayouts");

            migrationBuilder.DropTable(
                name: "LayoutSections");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
