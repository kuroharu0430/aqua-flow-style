using BlazorApp.Core.Enum;

namespace BlazorApp.Core.Model
{
    public interface IDraggableState
    {
        bool NeedsRectUpdate { get; set; }   // UI更新が必要か
        SelectionState SelectionState { get; set; }
        InteractionPhase InteractionPhase { get; set; }
        MobilityState MobilityState { get; set; }
        LayoutStatus LayoutStatus { get; set; }
    }

    public interface IDraggableInGrid : IDraggableState
    {
        GridBounds GridBounds { get; set; }      // グリッド単位の位置・サイズ・境界
    }

    public interface IDraggableOnMouse : IDraggableState
    {
        RectBounds RectBounds { get; set; }        // ピクセル座標系の描画位置
    }

    public interface IDraggable : IDraggableInGrid, IDraggableOnMouse
    {
        // 必要ならここに共通メソッドやイベントを追加できる
    }

    public enum SelectionState
    {
        None,
        TempSelected,
        Selected
    }

    public enum InteractionPhase
    {
        Idle,
        Hovering,
        Floating,
        Confirmed,     // 登録済み
        SoftDeleted    // 削除済み（復元可能）
    }

    public enum MobilityState
    {
        Free,       // 完全に自由
        Anchored,   // 自発的には動けるが、他からは動かされない
        Locked      // 完全に固定（操作不可）
    }
}
