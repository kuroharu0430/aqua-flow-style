namespace BlazorApp.ViewModel
{
    public class Control
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = ""; // 音声で呼ばれる名前

        // --- UI状態 ---
        public bool Visible { get; set; } = true;
        public bool Disabled { get; set; } = false;
        public bool Selected { get; set; } = false;

        // --- CSS / Style ---
        public string CssClass { get; set; } = "";
        public string Style { get; set; } = "";
    }
}
