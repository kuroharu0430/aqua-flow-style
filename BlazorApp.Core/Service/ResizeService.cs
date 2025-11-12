using BlazorApp.Core.State;
using BlazorApp.Core.Model;

namespace BlazorApp.Core.Service
{
    public class ResizeService : InteractionService
    {
        public bool? TryResize(int relativeX, int relativeY, OverlapMode mode)
        {
            var (gridX, gridY) = ToGridPosition(relativeX, relativeY, State.DisplayOption.WidthPerCell, State.DisplayOption.HeightPerCell);

            if (!StartDrag(dragTarget, gridX, gridY))
                return null;

            // 移動量の算出
            int directionX = 0;
            int directionY = 0;
            if (LastGridPosition is (var lastX, var lastY))
            {
                directionX = gridX - lastX;
                directionY = gridY - lastY;
                LastGridPosition =(gridX, gridY);
            }

            var result = mode switch
            {
                OverlapMode.PlaceFree => TryPlaceFree(dragTarget, directionX, directionY, allLayouts),
                OverlapMode.Push => TryPush(dragTarget, directionX, directionY, allLayouts),
                _ => null
            };

            if (result == true)
            {
                Session.PromoteInteractionPhases(); // ✅ 成功時だけ昇格
            }
            return result;
        }

        public bool? TryPush(IDraggable hitButton, int directionX, int directionY, List<IDraggable> allButtons)
        {
            // 変更前に記録
            Session.Record(hitButton); 

            // size変更
            hitButton.GridBounds.SizeX = Math.Abs(hitButton.GridBounds.SizeX + directionX);
            hitButton.GridBounds.SizeY = Math.Abs(hitButton.GridBounds.SizeY + directionY);
            hitButton.NeedsRectUpdate = true;

            // validate
            if(!hitButton.GridBounds.IsValid(ColumnNumber, RowNumber))
            {
                Session.Revert();
                return false;
            }

            // 衝突判定（GridBoundsの Intersects を使う）
            var targets = allButtons.Where(btn =>
                btn != hitButton && btn.GridBounds.Intersects(hitButton.GridBounds));

            foreach (var target in targets)
            {
                bool success = TryBePushed(target, directionX, directionY, allButtons);
                if (!success)
                {
                    Session.Revert();
                    return false;
                }
            }
            return true;
        }

        public bool? TryPlaceFree(IDraggable hitButton, int directionX, int directionY, List<IDraggable> allButtons)
        {
            // 変更前に記録
            Session.Record(hitButton);

            // size変更
            hitButton.GridBounds.SizeX = Math.Abs(hitButton.GridBounds.SizeX + directionX);
            hitButton.GridBounds.SizeY = Math.Abs(hitButton.GridBounds.SizeY + directionY);
            hitButton.NeedsRectUpdate = true;

            // 妥当性チェック
            if (!hitButton.GridBounds.IsValid(ColumnNumber, RowNumber))
            {
                Session.Revert();
                return false;
            }

            // 衝突判定
            var targets = allButtons.Where(btn =>
                btn != hitButton && btn.GridBounds.Intersects(hitButton.GridBounds));

            if (targets.Any())
            {
                Session.Revert();
                return false;
            }
            return true;
        }
    }
}