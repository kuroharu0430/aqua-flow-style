using BlazorApp.Core.Enum;
using BlazorApp.Core.Model;
using BlazorApp.Core.Model.SnapShots;

namespace BlazorApp.Model.SnapShot
{
    public class Snapshot<T> : IReversible
        where T : ITrackable<T>
    {
        public UndoActionType undoActionType { get; } = UndoActionType.Modifyed;
        public T Target { get; }
        public LayoutStatus LayoutStatus { get; }
        public T Prev { get; }

        public Snapshot(UndoActionType type, T target, LayoutStatus status, T prev)
        {
            undoActionType = type;
            Target = target;
            LayoutStatus = status;
            Prev = prev;
        }

        public void Restore()
        {
            Target.CopyFrom(Prev);
        }

        public IReversible CloneCurrent()
        {
            return new Snapshot<T>(
                undoActionType,
                Target,
                Target.LayoutStatus,
                Target.DeepCopy()
            );
        }
    }
}
