using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Core.Model
{
    public class Intent
    {
        public IntentType Type { get; set; }
        public Slot Slot { get; set; } = new();
        public float Confidence { get; set; }
        public string RawText { get; set; }
    }

    public enum IntentType
    {
        Set,
        Move,
        Resize,
        Delete,
        Select,
        Add,
        Toggle,
        Commit,
        Cancel
    }

}
