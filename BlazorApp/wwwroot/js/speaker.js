window.speech = {
    startContinuousRecognition: function (dotnetObj) {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        const recog = new SpeechRecognition();

        recog.lang = "ja-JP";
        recog.interimResults = false;
        recog.maxAlternatives = 1;
        recog.continuous = true;

        recog.onresult = function (event) {
            const text = event.results[event.results.length - 1][0].transcript;
            dotnetObj.invokeMethodAsync("OnRecognized", text);
        };

        recog.onend = function () {
            recog.start();
        };

        // 音声ファイル化
        recorder.onstop = () => {
            const blob = new Blob(chunks, { type: 'audio/wav' });
            // ここで Blazor に blob を渡す
            dotnetObj.invokeMethodAsync("OnAudioCaptured", blob);
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
