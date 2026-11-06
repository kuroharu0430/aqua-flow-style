using BlazorApp.Core.Model.SnapShots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Core.Model
{
    public interface ITrackable<T>
    {
        T DeepCopy();
        T Old { get; set; }

        CompositeSnapshot CreateCompositeSnap();
    }
}
