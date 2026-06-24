using BlazorApp.Core.Model.SnapShots;
using BlazorApp.Core.Enum;

namespace BlazorApp.Service
{
    public class UndoManager
    {

        private Stack<CompositeSnapshot> UndoStack = new();

        private Stack<CompositeSnapshot> RedoStack = new();

        public bool CanUndo => UndoStack.Any();
        public bool CanRedo => RedoStack.Any();

        public void Push(CompositeSnapshot snapList)
        {
            UndoStack.Push(snapList);

            // ★ 新しい操作が来た瞬間に未来を破棄
            RedoStack.Clear();
        }

        public void Undo()
        {
            if (UndoStack.TryPop(out var snapshot))
            {
                // 現在の状態を RedoStack に積む
                RedoStack.Push(snapshot.CloneCurrent(snapshot));

                // UndoStack から取り出した snapshot を復元
                snapshot.Restore();
            }
        }

        public void Redo()
        {
            if (RedoStack.TryPop(out var snapshot))
            {
                // 現在の状態を UndoStack に積む
                UndoStack.Push(snapshot.CloneCurrent(snapshot));

                // RedoStack から取り出した snapshot を復元
                snapshot.Restore();
            }
        }

        public void Restore(Stack<CompositeSnapshot> stack)
        {
            if (!stack.TryPop(out var snapshot)) return;

            snapshot.Restore();
        }

    }
}
