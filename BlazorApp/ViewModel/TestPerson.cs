using System.ComponentModel;

namespace BlazorApp.ViewModel
{
    public class TestPerson
    {
        public TestPerson OldValue { get; set; } = null;
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
