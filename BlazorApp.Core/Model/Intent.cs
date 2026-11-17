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

        public int TargetId { get; set; }

        public string? Value { get; set; }

        public float Confidence { get; set; }

        public string RawText { get; set; } = "";
    }

    public enum IntentType
    {
        Set,
        Submit,
        Undo,
        Select
    }
}
