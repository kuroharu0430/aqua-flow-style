using BlazorApp.Core.Model;

namespace BlazorApp.Service
{
    public class ResizeService : InteractionService
    {
        public bool? TryResize(int gridX, int gridY, OverlapMode mode)
        {
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
                //OverlapMode.PlaceFree => TryPlaceFree(dragTarget, directionX, directionY, allLayouts),
                OverlapMode.Push => TryPush(dragTarget, directionX, directionY, allLayouts),
                _ => null
            };

            if (result == true)
            {
                Session.PromoteInteractionPhases(); // ✅ 成功時だけ昇格
            }
            return result;
        }

        public bool? TryPush(IDraggable hitLayout, int directionX, int directionY, List<IDraggable> allButtons)
        {
            // 変更前に記録
            Session.Record(hitLayout);

            // size変更
            hitLayout.GridBounds.SizeX += directionX;
            hitLayout.GridBounds.SizeY += directionY;
            hitLayout.NeedsRectUpdate = true;

            // validate
            if(!hitLayout.GridBounds.IsValid(ColumnNumber, RowNumber))
            {
                Session.Revert();
                return false;
            }

            // 衝突判定（GridBoundsの Intersects を使う）
            var targets = allButtons.Where(btn =>
                btn != hitLayout && btn.GridBounds.Intersects(hitLayout.GridBounds));

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

        public bool? TryPlaceFree(IDraggable hitLayout, int directionX, int directionY, List<IDraggable> allButtons)
        {
            // 変更前に記録
            Session.Record(hitLayout);

            // size変更
            hitLayout.GridBounds.SizeX = Math.Abs(hitLayout.GridBounds.SizeX + directionX);
            hitLayout.GridBounds.SizeY = Math.Abs(hitLayout.GridBounds.SizeY + directionY);
            hitLayout.NeedsRectUpdate = true;

            // 妥当性チェック
            if (!hitLayout.GridBounds.IsValid(ColumnNumber, RowNumber))
            {
                Session.Revert();
                return false;
            }

            // 衝突判定
            var targets = allButtons.Where(btn =>
                btn != hitLayout && btn.GridBounds.Intersects(hitLayout.GridBounds));

            if (targets.Any())
            {
                Session.Revert();
                return false;
            }
            return true;
        }
    }
}