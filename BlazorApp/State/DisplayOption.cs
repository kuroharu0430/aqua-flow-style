using BlazorApp.Core.Enum;
using BlazorApp.Core.Model;
using System.ComponentModel.DataAnnotations;

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

        [Range(1, 100)]
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

        [Range(1, 100)]
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

        [Range(2, 200)]
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

        [Range(2, 200)]

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


        public bool ValidateDynamic(int minColumn, int minRow, out string? error)
        {
            if (ColumnNumber < minColumn)
            {
                error = $"列数は {minColumn} 以上である必要があります。";
                return false;
            }

            if (RowNumber < minRow)
            {
                error = $"行数は {minRow} 以上である必要があります。";
                return false;
            }

            error = null;
            return true;
        }

    }
}
