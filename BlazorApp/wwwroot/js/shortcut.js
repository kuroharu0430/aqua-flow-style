window.registerSurfaceShortcut = function (dotnetHelper) {
    document.addEventListener('keydown', function (event) {
        const keys = [];
        if (event.ctrlKey) keys.push("Ctrl");
        if (event.shiftKey) keys.push("Shift");
        if (event.altKey) keys.push("Alt");

        // メインキーだけ code を push
        if (
            event.code !== "ControlLeft" &&
            event.code !== "ControlRight" &&
            event.code !== "ShiftLeft" &&
            event.code !== "ShiftRight" &&
            event.code !== "AltLeft" &&
            event.code !== "AltRight"
        ) {
            keys.push(event.code);
        }

        // ショートカットや特殊キーならブラウザの標準動作を止める
        if (event.ctrlKey || event.altKey || event.shiftKey ||
            ["Escape", "Delete"].includes(event.code)) {
            event.preventDefault();
        }

        dotnetHelper.invokeMethodAsync('HandleShortcut', keys);
    });
}

