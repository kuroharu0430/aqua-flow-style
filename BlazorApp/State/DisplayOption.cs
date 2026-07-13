using BlazorApp.Core.Enum;
using BlazorApp.Core.Model;

namespace BlazorApp._state
{
    public class DisplayOption : IDisplayOptionEditable
    {
        public event Action<DisplayOption>? OnChanged;

        /// <summary>
        /// Undo/Redo の Restore 中かどうか
        /// </summary>
        public bool IsRestoring { get; set; }

        public LayoutStatus LayoutStatus => LayoutStatus.Modified;

        private int _columnNumber;
        public int ColumnNumber
        {
            get => _columnNumber;
            set
            {
                if (_columnNumber != value)
                {
                    if (!IsRestoring)
                        OnChanged?.Invoke(this);

                    _columnNumber = value;
                }
            }
        }

        private int _rowNumber;
        public int RowNumber
        {
            get => _rowNumber;
            set
            {
                if (_rowNumber != value)
                {
                    if (!IsRestoring)
                        OnChanged?.Invoke(this);

                    _rowNumber = value;
                }
            }
        }

        private int _widthPerCell;
        public int WidthPerCell
        {
            get => _widthPerCell;
            set
            {
                if (_widthPerCell != value)
                {
                    if (!IsRestoring)
                        OnChanged?.Invoke(this);

                    _widthPerCell = value;
                }
            }
        }

        private int _heightPerCell;
        public int HeightPerCell
        {
            get => _heightPerCell;
            set
            {
                if (_heightPerCell != value)
                {
                    if (!IsRestoring)
                        OnChanged?.Invoke(this);

                    _heightPerCell = value;
                }
            }
        }
    }
}
