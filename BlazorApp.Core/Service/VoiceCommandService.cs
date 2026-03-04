namespace BlazorApp.Core.Service
{
    public class VoiceCommandService
    {
        private readonly Dictionary<VoiceIntent, Action> _commands = new();

        public void Register(VoiceIntent intent, Action action)
            => _commands[intent] = action;

        public void Execute(VoiceIntent intent)
        {
            if (_commands.TryGetValue(intent, out var action))
                action?.Invoke();
        }

        /// <summary>
        /// 受け取った音声TextをIntentに変換
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public VoiceIntent ParseIntent(string text)
        {
            text = text.ToLower();

            foreach (var kv in map)
            {
                if (text.Contains(kv.Key))
                    return kv.Value;
            }

            return VoiceIntent.None;
        }

        /// <summary>
        /// 音声命令辞書
        /// </summary>
        private static readonly Dictionary<string, VoiceIntent> map = new()
        {
            // ※英語の命令は小文字で丸める
            { "undo", VoiceIntent.Undo },
            { "とりけし", VoiceIntent.Undo },
            { "取り消し", VoiceIntent.Undo },
            { "あんどぅ", VoiceIntent.Undo },

            { "redo", VoiceIntent.Redo },
            { "やり直し", VoiceIntent.Redo },
            { "やりなおし", VoiceIntent.Redo },

            { "save", VoiceIntent.Save },
            { "保存", VoiceIntent.Save },
            { "ほぞん", VoiceIntent.Save },

            { "ok", VoiceIntent.Ok },
            { "おーけー", VoiceIntent.Ok },
            { "決定", VoiceIntent.Ok },

            { "cancel", VoiceIntent.Cancel },
            { "キャンセル", VoiceIntent.Cancel },
            { "きゃんせる", VoiceIntent.Cancel },
        };
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