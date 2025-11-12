using BlazorApp.Core.Enum;
using BlazorApp.Core.Styling;

namespace BlazorApp.Core.Model.SnapShots
{
    /// <summary>
    /// 一操作をまとめたSnapShotのList
    /// </summary>
    /// <param name="Snapshots"></param>
    /// <param name="Type"></param>
    public record CompositeSnapshot(
    IReadOnlyList<IReversible> Snapshots,
    UndoActionType Type
    )
    {
        public void Restore()
        {
            foreach (var snapshot in Snapshots)
            {
                snapshot.Restore();
            }
        }

        public CompositeSnapshot CloneCurrent(CompositeSnapshot original)
        {
            var cloned = original.Snapshots
                .Select(s => s.CloneCurrent())
                .ToList();

            return new CompositeSnapshot(cloned, original.Type);
        }
    }

    public record LayoutSnapshot(
        IDraggable target,
        GridBounds Bounds,
        LayoutStatus LayoutStatus
    ) : IReversible // 継承
    {
        public UndoActionType Type => UndoActionType.Dragged;

        public void Restore()
        {
            target.GridBounds = Bounds.DeepCopy();
            target.LayoutStatus = LayoutStatus;
            target.NeedsRectUpdate = true; // UI再描画トリガー
        }

        public IReversible CloneCurrent()
        {
            return new LayoutSnapshot(target, target.GridBounds.DeepCopy(), target.LayoutStatus);
        }
    }

    public record StyleSnapshot(
        IStylable target, // スタイルを持つ対象（ボタンとか）
        StyleBuilder Style,
        StyleBuilder WrapperStyle,
        LayoutStatus LayoutStatus
    ) : IReversible
    {
        public UndoActionType Type => UndoActionType.StyleEdited;

        public void Restore()
        {
            // スタイルを復元
            target.Style = Style.DeepCopy();
            target.WrapperStyle = WrapperStyle.DeepCopy();
        }

        public IReversible CloneCurrent()
        {
            return new StyleSnapshot(target, target.Style.DeepCopy(), target.WrapperStyle.DeepCopy(), target.LayoutStatus);
        }
    }

    public record FieldValueSnapShot(
        IFieldValuable target,
        string Value,
        LayoutStatus LayoutStatus
    ) : IReversible
    {
        public UndoActionType Type => UndoActionType.StyleEdited;

        public void Restore()
        {
            target.Value = Value;
            target.LayoutStatus = LayoutStatus;
        }

        public IReversible CloneCurrent()
        {
            return new FieldValueSnapShot(target, target.Value, target.LayoutStatus);
        }
    }
}
