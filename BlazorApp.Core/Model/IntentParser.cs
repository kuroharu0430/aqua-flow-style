namespace BlazorApp.Core.Model
{
    public static class IntentParser
    {
        public static string BuildPrompt(
            string text,
            string operationMap)
        {
            return $$"""
                    {{RolePrompt}}

                    【ユーザー発話】

                    {{text}}

                    【現在の画面状態(OperationMap)】

                    {{operationMap}}

                    {{IntentDefinition}}

                    {{ResponseExample}}

                    {{Notes}}
                    """;
        }
        private static string RolePrompt =>
            """
            あなたは音声操作用のIntent生成AIです。

            ユーザーの発話(Text)と、
            現在の画面状態(OperationMap)から
            最適なIntentを生成してください。

            あなたは画面を直接操作しません。
            Intentを生成するだけです。

            対象はOperationMapに存在する要素のみです。
            存在しない要素は操作対象にしないでください。

            """;

        private static string IntentDefinition =>
            """
            【Intentクラス定義】

            public class Intent
            {
            public IntentType Type { get; set; }

            public int TargetId { get; set; }

            public string? Value { get; set; }

            public float Confidence { get; set; }

            public string RawText { get; set; }
            }

            public enum IntentType
            {
                Set,
                Submit,
                Undo,
                Select
            }

            各項目の説明

            Type
            実行する操作種類

            TargetId
            OperationMap上の対象要素のId

            Value
            設定する値

            Confidence
            解釈の信頼度
            0～1

            RawText
            ユーザー発話そのもの

            """;

        private static string ResponseExample =>
            """
            【返却例】

            ユーザー発話:
            名前を山田にして

            返却JSON:

            {
            "Type":"Set",
            "TargetId":1,
            "Value":"山田",
            "Confidence":0.95,
            "RawText":"名前を山田にして"
            }

            """;

        private static string Notes =>
            """
            【注意事項】

            JSONのみ返却してください。

            説明文は禁止です。

            Markdownは禁止です。

            OperationMapに存在しないTargetIdは禁止です。

            推測でTargetIdを作らないでください。

            対象が判断できない場合はConfidenceを下げてください。

            """;

    }
}