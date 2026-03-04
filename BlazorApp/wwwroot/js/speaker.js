window.speech = {
    startContinuousRecognition: function (dotnetObj) {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        const recog = new SpeechRecognition();

        recog.lang = "ja-JP";
        recog.interimResults = true;
        recog.maxAlternatives = 1;
        recog.continuous = true;

        let lastText = "";
        let silenceTimer = null;
        let silenceMs = 400; // ★ ここを 300〜700 で調整

        recog.onresult = function (event) {
            const text = event.results[event.results.length - 1][0].transcript;
            lastText = text; // ★ 常に最新を保持（isFinal 無視）

            // 無音タイマーをリセット
            if (silenceTimer) clearTimeout(silenceTimer);

            // 無音が続いたら確定送信
            silenceTimer = setTimeout(() => {
                if (lastText.trim() !== "") {
                    dotnetObj.invokeMethodAsync("OnRecognized", lastText);
                }
                lastText = "";
            }, silenceMs);
        };

        recog.onend = function () {
            // Chrome のバグ対策：少し遅らせて再起動
            setTimeout(() => recog.start(), 200);
        };


        recog.onerror = function (event) {
            dotnetObj.invokeMethodAsync("OnRecognized", "エラー: " + event.error);
        };

        recog.start();
    },

    // ★ Volume 対応の「喋る」機能（正しい位置）
    speak: function (text, volume) {
        const utter = new SpeechSynthesisUtterance(text);
        utter.lang = "ja-JP";
        utter.volume = volume ?? 1.0;
        utter.rate = 1.0;
        utter.pitch = 0.6;

        speechSynthesis.speak(utter);
    }
};
