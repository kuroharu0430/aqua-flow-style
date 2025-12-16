using BlazorApp.Core.Model;
using BlazorApp.Core.State;

namespace BlazorApp.Core.Service
{
    public abstract class InteractionService
    {
        protected InteractionState State { get; private set; } = null!;

        public void SetState(InteractionState state)
        {
            if (State != null)
                throw new InvalidOperationException("State has already been set.");
            State = state;
        }

        protected int ColumnNumber => State.DisplayOption.ColumnNumber;
        protected int RowNumber => State.DisplayOption.RowNumber;

        protected MoveSession Session => State.Session;

        protected IDraggable dragTarget
        {  
            get => Session.DragTarget; 
            set => Session.DragTarget = value; 
        }

        protected (int X, int Y)? LastGridPosition
        {
            get => Session != null ? Session.LastGridPosition : null;
            set => Session.LastGridPosition = Session != null ? value : Session.LastGridPosition;
        }

        public (int X, int Y)? ValidGridPosition
        {
            get => Session != null ? Session.ValidGridPosition : null;
            set => Session.ValidGridPosition = Session != null ? value : Session.ValidGridPosition;
        }

        protected List<IDraggable>? allLayouts => Session?.AllButtons;

        protected bool StartDrag(IDraggable hitButton, int gridX, int gridY)
        {
            // 前回MouseMoveの履歴をClear
            Session.PreMoveSnapshot.Clear();

            if (LastGridPosition == null)
            {
                LastGridPosition = (gridX, gridY);
                return false;
            }

            ValidGridPosition = LastGridPosition;

            if (LastGridPosition == (gridX, gridY))
            {
                return false; // 同じ位置 → スキップ
            }

            // 意味のある移動
            return true;
        }

        protected bool TryBePushed(IDraggable overlapping, int MoveDirectionX, int MoveDirectionY, List<IDraggable> allButtons)
        {
            // 変更前に記録
            Session.Record(overlapping);

            // 新しい座標を計算
            overlapping.GridBounds.X += MoveDirectionX;
            overlapping.GridBounds.Y += MoveDirectionY;
            overlapping.NeedsRectUpdate = true;

            // validate
            if (!overlapping.GridBounds.IsValid(ColumnNumber, RowNumber))
            {
                Session.Revert();
                return false;
            }

            // 押し出し先に誰かいる？
            var overlaps = allButtons
                .Where(btn => btn != overlapping && btn.GridBounds.Intersects(overlapping.GridBounds));

            if (overlaps.Any())
            {
                // 再帰的にさらに押し出す
                foreach (var next in overlaps)
                {
                    if (!TryBePushed(next, MoveDirectionX, MoveDirectionY, allButtons))
                        return false;
                }
            }
            return true;
        }
    }
}
