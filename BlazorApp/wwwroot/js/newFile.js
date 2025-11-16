window.registerSurfaceShortcut = function (dotnetHelper) {
    document.addEventListener('keydown', function (event) {
        const keys = [];

        // 修飾キーはフラグで追加
        if (event.ctrlKey) keys.push("Ctrl");
        if (event.shiftKey) keys.push("Shift");
        if (event.altKey) keys.push("Alt");
        // Mac対応なら追加
        if (event.metaKey) keys.push("Meta");

        // メインキーだけ code を push（修飾キーは除外）
        if (
            event.code !== "ControlLeft" &&
            event.code !== "ControlRight" &&
            event.code !== "ShiftLeft" &&
            event.code !== "ShiftRight" &&
            event.code !== "AltLeft" &&
            event.code !== "AltRight" &&
            // Mac対応なら追加
            event.code !== "MetaLeft" &&
            event.code !== "MetaRight"
        ) {
            keys.push(event.code);
        }

        event.preventDefault();
        dotnetHelper.invokeMethodAsync('HandleShortcut', keys);
    });
};