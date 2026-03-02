window.audioRecorder = {
    recorder: null,
    chunks: [],
    dotnetObj: null,

    start: function (dotnetObj) {
        this.dotnetObj = dotnetObj;

        navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
            this.chunks = [];
            this.recorder = new MediaRecorder(stream);

            this.recorder.ondataavailable = e => {
                this.chunks.push(e.data);
            };

            this.recorder.onstop = async () => {
                const blob = new Blob(this.chunks, { type: "audio/webm" });

                // ★ サーバーにアップロード
                await uploadAudio(blob);

                // ★ Whisper に送りたい場合はここで呼ぶ
                // await sendToWhisper(blob, this.dotnetObj);

                // ★ C# に「アップロード完了」を通知
                if (this.dotnetObj) {
                    this.dotnetObj.invokeMethodAsync("OnAudioUploaded");
                }
            };

            this.recorder.start();
        });
    },

    stop: function () {
        if (this.recorder && this.recorder.state !== "inactive") {
            this.recorder.stop();
        }
    }
};


// ------------------------------
// サーバーに音声ファイルを送る
// ------------------------------
async function uploadAudio(blob) {
    const form = new FormData();
    form.append("file", blob, "audio.webm");

    await fetch("/upload-audio", {
        method: "POST",
        body: form
    });
}