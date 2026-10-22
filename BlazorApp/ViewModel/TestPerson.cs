using System.ComponentModel;

namespace BlazorApp.ViewModel
{
    public class TestPerson
    {
        [DisplayName("名前")]
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public enum RankType { Bronze, Silver, Gold }
        public RankType Rank { get; set; }

        public float Height { get; set; }
        public float Width { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}
