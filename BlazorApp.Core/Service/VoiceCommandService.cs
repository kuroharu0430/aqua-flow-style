namespace BlazorApp.Core.Service
{
    public class VoiceCommandService
    {
        // --- 基本コマンド ---
        public Action? Undo { get; set; }
        public Action? Redo { get; set; }
        public Action? Save { get; set; }
        public Action? Cancel { get; set; }
        public Action? Ok { get; set; }

        // --- 拡張コマンド（後で使う） ---
        public Action<string>? RawTextReceived { get; set; }

        // --- Move / Resize / Select などの自然言語コマンド ---
        public Action<int, int>? Move { get; set; }
        public Action<int, int>? Resize { get; set; }
        public Action<string>? Select { get; set; }
        public Action? Delete { get; set; }

        // --- テキストを受け取ってパースする入口 ---
        //public void Execute(string text)
        //{
        //    RawTextReceived?.Invoke(text);

        //    // ここは後で自然言語パーサーを入れる場所
        //    // 今は Listener 側で処理してるので空でOK
        //}

        public VoiceIntent ParseIntent(string text)
        {
            if (text.Contains("取り消し") || text.Contains("Undo"))
                return VoiceIntent.Undo;

            if (text.Contains("やり直し") || text.Contains("Redo"))
                return VoiceIntent.Redo;

            if (text.Contains("保存") || text.Contains("Save"))
                return VoiceIntent.Save;

            if (text.Contains("キャンセル") || text.Contains("Cancel"))
                return VoiceIntent.Cancel;

            if (text.Contains("OK") || text.Contains("オーケー"))
                return VoiceIntent.Ok;

            return VoiceIntent.None;
        }

        // TODO _surfacesをもたすのは危険
        //public void Execute(string text)
        //{
        //    RawTextReceived?.Invoke(text);

        //    var intent = ParseIntent(text);

        //    foreach (var surface in _surfaces)
        //        surface.ExecuteVoiceCommand(intent);
        //}

    }

    public enum VoiceIntent
    {
        None,
        Undo,
        Redo,
        Save,
        Ok,
        Cancel
    }
}