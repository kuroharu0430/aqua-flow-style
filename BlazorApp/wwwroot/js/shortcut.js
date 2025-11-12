window.registerSurfaceShortcut = function (dotnetHelper) {
    document.addEventListener('keydown', function (event) {
        // shiftを押すと大文字になるので条件式に注意
        // Ctrl + Shift + Z → Redo
        if (event.ctrlKey && event.shiftKey && event.code === 'KeyZ') {
            event.preventDefault();
            dotnetHelper.invokeMethodAsync('HandleRedo');
            // Undoが走らないようにする
            return;
        }

        // Ctrl + Z → Undo
        if (event.ctrlKey && event.code === 'KeyZ') {
            event.preventDefault();
            dotnetHelper.invokeMethodAsync('HandleShortcut');
        }

        // Escape → Cancel / Deselect
        if (event.key === 'Escape') {
            event.preventDefault();
            dotnetHelper.invokeMethodAsync('HandleEscape');
        }

        // Delete → 削除
        if (event.key === 'Delete') {
            event.preventDefault();
            dotnetHelper.invokeMethodAsync('HandleDelete');
        }

        // Ctrl + A → 全選択
        if (event.ctrlKey && event.key.toLowerCase() === 'a') {
            event.preventDefault();
            dotnetHelper.invokeMethodAsync('HandleSelectAll');
        }

        // Ctrl + D → Duplicate（複製）
        if (event.ctrlKey && event.key.toLowerCase() === 'd') {
            event.preventDefault();
            dotnetHelper.invokeMethodAsync('HandleDuplicate');
        }
    });
};