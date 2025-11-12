using BlazorApp.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Core.Model.SnapShots
{
    public interface IReversible
    {
        UndoActionType Type { get; }

        LayoutStatus LayoutStatus { get; }

        /// <summary>
        /// Snapshotの状態に戻す
        /// </summary>
        void Restore(); 

        /// <summary>
        /// 現在の状態のSnapshotを取得する
        /// </summary>
        /// <returns></returns>
        IReversible CloneCurrent(); 
    }

    public enum UndoActionType
    {
        Dragged,
        Deleted,
        StyleEdited,
    }
}
