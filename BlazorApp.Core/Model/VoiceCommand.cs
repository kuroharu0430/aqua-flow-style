using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Core.Model
{
    public class VoiceCommand
    {
        public object Target { get; set; }
        public string Verb { get; set; }
        public object? Value { get; set; }
    }

}
