using BlazorApp.Core.Model;

namespace BlazorApp.Service
{
    public class DragService : InteractionService
    {
        protected (int dx, int dy) confirmedDirection = (0, 0);

        public bool? TryDrag(int gridX, int gridY, OverlapMode mode)
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

            var selectedLayouts = allLayouts
                .Where(btn => btn.SelectionState == SelectionState.Selected)
                .ToList();


            // 変更前に記録 + 移動 + validate
            foreach (var btn in selectedLayouts)
            {
                Session.Record(btn);

                btn.GridBounds.X += directionX;
                btn.GridBounds.Y += directionY;
                btn.NeedsRectUpdate = true;

                if (!btn.GridBounds.IsValid(ColumnNumber, RowNumber))
                {
                    Session.Revert();
                    return false;
                }
            }

            var result = mode switch
            {
                OverlapMode.PlaceFree => TryPlaceFree(dragTarget, directionX, directionY, allLayouts),
                OverlapMode.Push => TryPush(dragTarget, directionX, directionY, allLayouts, selectedLayouts),
                OverlapMode.Swap => TrySwap(dragTarget, directionX, directionY, allLayouts, selectedLayouts),
                //OverlapMode.Reorder => TryReorder(hitButton, directionX, directionY, allButtons),
                _ => null
            };

            if (result == true)
            {
                Session.PromoteInteractionPhases(); // ✅ 成功時だけ昇格
            }

            return result;
        }

        public bool? TryPlaceFree(IDraggable hitButton, int directionX, int directionY, List<IDraggable> allButtons)
        {
            // 変更前に記録
            Session.Record(hitButton);

            // 移動
            hitButton.GridBounds.X += directionX;
            hitButton.GridBounds.Y += directionY;
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

        /// <summary>
        /// 移動処理の結果を返す:
        /// true = 成功, false = 失敗, null = 移動なし
        /// </summary>
        public bool? TryPush(IDraggable hitButton, int directionX, int directionY, List<IDraggable> allButtons, List<IDraggable> selectedButtons)
        {
            // 衝突判定（GridBoundsの Intersects を使う）
            foreach (var btn in selectedButtons)
            {
                var targets = allButtons.Where(other =>
                    !selectedButtons.Contains(other) &&
                    other.GridBounds.Intersects(btn.GridBounds)).ToList();

                foreach (var target in targets)
                {
                    bool success = TryBePushed(target, directionX, directionY, allButtons);
                    if (!success)
                    {
                        Session.Revert();
                        return false;
                    }
                }
            }
            return true;
        }

        public bool? TrySwap(IDraggable hitButton, int directionX, int directionY, List<IDraggable> allButtons, List<IDraggable> selectedButtons)
        {
            // 衝突判定（GridBoundsの Intersects を使う）
            foreach (var layout in selectedButtons)
            {
                var targets = allButtons.Where(other =>
                    !selectedButtons.Contains(other) &&
                    other.GridBounds.Intersects(layout.GridBounds)).ToList();

                foreach (var target in targets)
                {
                    Session.Record(layout);
                    Session.Record(target);

                    // 自分のサイズ分だけ相手を反対側にぶん投げ
                    if (directionX != 0)
                    {
                        target.GridBounds.X = target.GridBounds.X +
                            (directionX > 0 ? -layout.GridBounds.SizeX : layout.GridBounds.SizeX);
                    }

                    if (directionY != 0)
                    {
                        target.GridBounds.Y = target.GridBounds.Y +
                            (directionY > 0 ? -layout.GridBounds.SizeY : layout.GridBounds.SizeY);
                    }

                    // 妥当性チェック
                    // はみ出してない？ + 当たってない？
                    var isHitting = allButtons.Any(other =>
                        other != layout && other.GridBounds.Intersects(target.GridBounds));

                    if (!target.GridBounds.IsValid(ColumnNumber, RowNumber) && isHitting)
                    {
                        Session.Revert();

                        return false;
                    }

                    layout.NeedsRectUpdate = true;
                    target.NeedsRectUpdate = true;
                }

            }
            return true;
        }
    }

    public enum OverlapMode
    {
        PlaceFree,
        Push,
        Swap,
        Reorder
    }
}
