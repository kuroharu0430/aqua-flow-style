using BlazorApp.Core.Model.SnapShots;
using BlazorApp.Core.Model;

namespace BlazorApp.Session
{
    public record class MoveSession
    {
        public IDraggable DragTarget { get; set; }

        public List<IDraggable>? AllButtons { get; set; } = null;

        public List<LayoutSnapshot> PreMoveSnapshot { get; } = new();
        public List<LayoutSnapshot> OldRecord { get; } = new();

        public (int X, int Y)? LastGridPosition { get; set; }
        public (int X, int Y)? ValidGridPosition { get; set; }

        public void Record(IDraggable layout)
        {
            var snapshot = new LayoutSnapshot(
                layout,
                layout.GridBounds.DeepCopy(),
                layout.LayoutStatus
            );

            PreMoveSnapshot.Add(snapshot);

            if (!OldRecord.Any(x => x.target == layout))
            {
                OldRecord.Add(snapshot); // 最初の状態だけ積む！
            }
        }

        public void Revert()
        {
            foreach (var snap in PreMoveSnapshot)
            {
                snap.Restore();
            }
            LastGridPosition = ValidGridPosition; // 座標の巻き戻しは別責務として残す
        }


        public void PromoteInteractionPhases()
        {
            foreach (var snap in PreMoveSnapshot)
            {
                snap.target.InteractionPhase = InteractionPhase.Confirmed;
            }
        }
    }

}
