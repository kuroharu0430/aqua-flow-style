using BlazorApp.Core.Enum;
using BlazorApp.Core.Model;
using BlazorApp.Core.Model.SnapShots;
using BlazorApp.Model.SnapShot;
using System.ComponentModel;

namespace BlazorApp.ViewModel
{
    public class TestPerson : ITrackable<TestPerson>
    {
        public LayoutStatus LayoutStatus { get; set; } = LayoutStatus.Modified;

        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Address { get; set; } = string.Empty;

        #region ITrackable実装
        public TestPerson Prev { get; set; } = null!;

        public TestPerson DeepCopy()
        {
            return new TestPerson
            {
                Name = this.Name,
                Age = this.Age,
                Address = this.Address,
                LayoutStatus = this.LayoutStatus
            };
        }

        public void CopyFrom(TestPerson other)
        {
            Name = other.Name;
            Age = other.Age;
            Address = other.Address;
            LayoutStatus = other.LayoutStatus;
        }

        public IReversible CreateSnapShot()
        {
            // 次のためにPrevを更新
            Prev = DeepCopy();
            return new Snapshot<TestPerson>(
                UndoActionType.Modifyed,
                this,
                this.LayoutStatus,
                Prev
            );
        }
        #endregion
    }
}
