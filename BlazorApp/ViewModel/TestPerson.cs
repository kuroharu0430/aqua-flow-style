using System.ComponentModel;

namespace BlazorApp.ViewModel
{
    [DisplayName("ユーザ情報")]
    public class TestPerson
    {
        [DisplayName("名前")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("年齢")]
        public int Age { get; set; }
        [DisplayName("住所")]
        public string Address { get; set; } = string.Empty;
    }
}
