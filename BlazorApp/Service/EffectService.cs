using BlazorApp.Core.Model;
using static BlazorApp.Components.InteractionSurface;

namespace BlazorApp.Service
{
    public class EffectService
    {
        #region Ripple
        public MousePosition? RipplePosition { get; private set; }

        public void FireRipple(MousePosition pos)
        {
            RipplePosition = pos;

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                RipplePosition = null;
            });
        }
        #endregion

        #region TrailEffect
        public readonly record struct TrailCell(int gridX, int gridY);
        public List<TrailCell> TrailCells { get; } = new();
        public void AddTrail(int gridX, int gridY)
        {
            var cell = new TrailCell(gridX, gridY);

            if (TrailCells.LastOrDefault() != cell)
            {
                TrailCells.Add(cell);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    TrailCells.Remove(cell);
                });
            }
        }
        #endregion
    }
}
