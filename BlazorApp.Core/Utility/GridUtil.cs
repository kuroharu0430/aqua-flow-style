using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Core.Utility
{
    public static class GridUtil
    {
        public static (int gridX, int gridY) ToGridPosition(int relativeX, int relativeY, int widthPerCell, int heightPerCell)
        {
            return (
                relativeX / widthPerCell,
                relativeY / heightPerCell
            );
        }
    }

}
