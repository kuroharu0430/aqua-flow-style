using BlazorApp.Core.Enum;
using BlazorApp.Core.Model.SnapShots;

namespace BlazorApp.Core.Model
{
    public interface ITrackable<T>
    {
        T DeepCopy();
        void CopyFrom(T other);

        T Prev { get; set; }
        LayoutStatus LayoutStatus { get; set; }

        IReversible CreateSnapShot();
    }

}
