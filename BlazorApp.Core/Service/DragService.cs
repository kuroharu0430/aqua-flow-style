using BlazorApp.Core.Model;
using BlazorApp.Core.State;
using System.Diagnostics;

namespace BlazorApp.Core.Service
{
    public class DragService : InteractionService
    {
        protected (int dx, int dy) confirmedDirection = (0, 0);

        public bool? TryDrag(int gridX, int gridY, OverlapMode mode)
        {
            // TODO 不要
            //var (gridX, gridY) = ToGridPosition(relativeX, relativeY, State.DisplayOption.WidthPerCell, State.DisplayOption.HeightPerCell);

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

            var selectedButtons = allLayouts
                .Where(btn => btn.SelectionState == SelectionState.Selected || btn == dragTarget)
                .ToList();


            // 変更前に記録 + 移動 + validate
            foreach (var btn in selectedButtons)
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
                OverlapMode.Push => TryPush(dragTarget, directionX, directionY, allLayouts, selectedButtons),
                OverlapMode.Swap => TrySwap(dragTarget, directionX, directionY, allLayouts),
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

        public bool? TrySwap(IDraggable hitButton, int directionX, int directionY, List<IDraggable> allButtons)
        {
            if (!TryMove(hitButton, directionX, directionY))
            {
                // ダメなら処理終了
                return false;
            }
            hitButton.InteractionPhase = InteractionPhase.Floating;

            confirmedDirection = (
                    confirmedDirection.Item1 + directionX,
                    confirmedDirection.Item2 + directionY
                );

            // 一部はみだしがないかCheck
            var overlaps = allButtons
                .Where(btn => btn != hitButton && btn.GridBounds.Intersects(hitButton.GridBounds));

            // 重なりがなければ処理成功
            if (!overlaps.Any())
            {
                return true;
            }

            // 合成領域を作成（Union）
            var union = overlaps.First().GridBounds;
            foreach (var overlap in overlaps.Skip(1))
            {
                union = union.Union(overlap.GridBounds);
            }

            // 判定：hitButton がunionを完全に覆っているか？
            if (!hitButton.GridBounds.Contains(union))
            {
                // はみ出し者がいるので失敗 
                //CurrentMoveSession.Restore();
                return false;
            }

            // 成立→スワップ処理
            foreach (var target in overlaps)
            {
                // 変更前に記録
                Session.Record(target);

                int pushX = Math.Sign(confirmedDirection.dx) * (hitButton.GridBounds.SizeX);
                int pushY = Math.Sign(confirmedDirection.dy) * (hitButton.GridBounds.SizeY);

                target.GridBounds.X -= pushX;
                target.GridBounds.Y -= pushY;

                target.NeedsRectUpdate = true;
            }
            // 成立→0に戻す
            confirmedDirection= (0, 0);
            // 巻き戻しパターンはなし
            return true;
        }

        // TODO 多分難易度が激高なので実装しない方向で
        //public bool? TryReorder(IDraggable hitButton, int gridX, int gridY, List<IDraggable> allButtons)
        //{
        //    // 現在の順序を構築（Y優先 → X）
        //    var ordered = allButtons
        //        .OrderBy(btn => btn.GridBounds.Y)
        //        .ThenBy(btn => btn.GridBounds.X)
        //        .ToList();

        //    // 対象ボタンを一旦除外
        //    ordered.Remove(hitButton);

        //    // 目標インデックスを計算（サイズは無視して順序のみ）
        //    int targetIndex = gridY * ColumnNumber + gridX;
        //    targetIndex = Math.Clamp(targetIndex, 0, ordered.Count);

        //    // 新しい順序に挿入
        //    ordered.Insert(targetIndex, hitButton);

        //    // 配置カーソルと行の高さ
        //    int cursorX = 0;
        //    int cursorY = 0;
        //    int currentRowHeight = 1;

        //    foreach (var btn in ordered)
        //    {
        //        // 改行処理（横幅オーバー）
        //        if (cursorX + btn.GridBounds.SizeX > ColumnNumber)
        //        {
        //            cursorX = 0;
        //            cursorY += currentRowHeight;
        //            currentRowHeight = 1;
        //        }

        //        // 範囲チェック（縦オーバー）
        //        if (cursorY + btn.GridBounds.SizeY > RowNumber)
        //        {
        //            Session.Restore();
        //            return false;
        //        }

        //        Session.Record(btn);

        //        btn.GridBounds.X = cursorX;
        //        btn.GridBounds.Y = cursorY;
        //        btn.NeedsRectUpdate = true;

        //        cursorX += btn.GridBounds.SizeX;

        //        // 行の高さを更新（最大SizeY）
        //        currentRowHeight = Math.Max(currentRowHeight, btn.GridBounds.SizeY);
        //    }
        //    return true;
        //}

        private bool TryMove(IDraggable hitButton, int directionX, int directionY)
        {
            // 変更前に記録
            Session.Record(hitButton);

            // 通常移動
            hitButton.GridBounds.X += directionX;
            hitButton.GridBounds.Y += directionY;
            hitButton.NeedsRectUpdate = true;

            // 妥当性チェック
            if (!hitButton.GridBounds.IsValid(ColumnNumber, RowNumber))
            {
                Session.Revert();
                return false;
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
