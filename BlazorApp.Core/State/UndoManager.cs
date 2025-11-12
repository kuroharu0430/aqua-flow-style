using BlazorApp.Core.Model.SnapShots;
using BlazorApp.Core.Enum;

namespace BlazorApp.Service
{
    public class UndoManager
    {
        public Stack<CompositeSnapshot> UndoStack = new();

        public Stack<CompositeSnapshot> RedoStack = new();

        public void Push(CompositeSnapshot snapList) => UndoStack.Push(snapList);

        public void Undo()
        {
            if (UndoStack.TryPeek(out var snapshot))
            {
                // 現在の状態をRedoStackに積む（CloneCurrentで）
                RedoStack.Push(snapshot.CloneCurrent(snapshot));
            }
            Restore(UndoStack);
        }

        public void Redo()
        {
            if (RedoStack.TryPeek(out var snapshot))
            {
                UndoStack.Push(snapshot.CloneCurrent(snapshot)); // 今の状態を保存
            }
            Restore(RedoStack);
        }

        public void Restore(Stack<CompositeSnapshot> stack)
        {
            if (!stack.TryPop(out var snapshot)) return;

            snapshot.Restore();
        }

    }
}
