using BlazorApp.Core.Model;
using BlazorApp.Session;

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
                //OverlapMode.PlaceFree => TryPlaceFree(directionX, directionY, allLayouts),
                OverlapMode.Push => TryPush(directionX, directionY, allLayouts, selectedLayouts),
                OverlapMode.Swap => TrySwap(directionX, directionY, allLayouts, selectedLayouts),
                //OverlapMode.Reorder => TryReorder(hitButton, directionX, directionY, allButtons),
                _ => null
            };

            if (result == true)
            {
                Session.PromoteInteractionPhases(); // ✅ 成功時だけ昇格
            }

            return result;
        }

        public bool? TryPlaceFree(int directionX, int directionY, List<IDraggable> allButtons)
        {
            // 変更前に記録
            //Session.Record(hitButton);

            //// 移動
            //hitButton.GridBounds.X += directionX;
            //hitButton.GridBounds.Y += directionY;
            //hitButton.NeedsRectUpdate = true;

            //// 妥当性チェック
            //if (!hitButton.GridBounds.IsValid(ColumnNumber, RowNumber))
            //{
            //    Session.Revert();
            //    return false;
            //}

            //// 衝突判定
            //var targets = allButtons.Where(btn =>
            //    btn != hitButton && btn.GridBounds.Intersects(hitButton.GridBounds));

            //if (targets.Any())
            //{
            //    Session.Revert();
            //    return false;
            //}
            return true;
        }

        /// <summary>
        /// 移動処理の結果を返す:
        /// true = 成功, false = 失敗, null = 移動なし
        /// </summary>
        public bool? TryPush(int directionX, int directionY, List<IDraggable> allButtons, List<IDraggable> selectedButtons)
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

        public bool? TrySwap(int directionX, int directionY, List<IDraggable> allButtons, List<IDraggable> selectedButtons)
        {
            // 衝突判定（GridBoundsの Intersects を使う）
            foreach (var layout in selectedButtons)
            {
                var targets = allButtons.Where(other =>
                    !selectedButtons.Contains(other) &&
                    other.GridBounds.Intersects(layout.GridBounds)).ToList();

                foreach (var target in targets)
                {
                    Session.Record(target);

                    if (directionX > 0)
                    {
                        // 右からぶつかった → 相手は自分の左側へ
                        target.GridBounds.X = layout.GridBounds.X - target.GridBounds.SizeX;
                    }
                    else if (directionX < 0)
                    {
                        // 左からぶつかった → 相手は自分の右側へ
                        target.GridBounds.X = layout.GridBounds.X + layout.GridBounds.SizeX;
                    }
                    else
                    {
                        // 0移動→何もしない
                    }


                    if (directionY > 0)
                    {
                        // 下からぶつかった → 相手は自分の上側へ
                        target.GridBounds.Y = layout.GridBounds.Y - target.GridBounds.SizeY;
                    }
                    else if (directionY < 0)
                    {
                        // 上からぶつかった → 相手は自分の下側へ
                        target.GridBounds.Y = layout.GridBounds.Y + layout.GridBounds.SizeY;
                    }
                    else
                    {
                        // Y方向に動いてない
                    }

                    // 妥当性チェック
                    var isHitting = allButtons.Any(other =>
                        other != target && other.GridBounds.Intersects(target.GridBounds));
                    // 当たってない？はみ出してない？
                    if (!target.GridBounds.IsValid(ColumnNumber, RowNumber) || isHitting)
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
        //PlaceFree,
        Push,
        Swap,
        //Reorder
    }
}
