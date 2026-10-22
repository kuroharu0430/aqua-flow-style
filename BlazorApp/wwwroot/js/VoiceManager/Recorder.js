window.voiceRecord = {
    dotnetRef: null,
    
    initRecorder: function (dotnet) {
        this.dotnetRef = dotnet;
    },

    start: async function () {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        this.mediaRecorder = new MediaRecorder(stream, { mimeType: "audio/webm" });

     this.mediaRecorder.ondataavailable = async (e) => { 

             console.log("ondataavailable size:", e.data.size);    console.log("ondataavailable size:", e.data.size);
        if (e.data.size > 0) {
            // ArrayBuffer → Uint8Array（音声データそのもの）
            const buf = await e.data.arrayBuffer();
            const bytes = new Uint8Array(buf);

                console.log("bytes.length:", bytes.length);    console.log("bytes.length:", bytes.length);

            // Uint8Array → Base64（音声データを安全に文字列化）
            let binary = "";
            for (let i = 0; i < bytes.length; i++) {
                binary += String.fromCharCode(bytes[i]);
            }
            const base64 = btoa(binary);
            console.log("base64.length:", base64.length);  console.log("base64.length:", base64.length);
            await this.dotnetRef.invokeMethodAsync("OnChunkBase64", base64);
        }
    };
        this.mediaRecorder.start(1000); // ★ 1秒チャンク
    },

    stop: function () {
        if (this.mediaRecorder) {
            this.mediaRecorder.stop();

            this.mediaRecorder.stream.getTracks().forEach(t => t.stop());

            this.mediaRecorder = null;
        }
    }
};