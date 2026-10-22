using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Core.Model
{
    public static class IntentParser
    {
        public static Intent Parse(string text)
        {
            // ★ スタブ：とりあえず「名前を「山田」にして」だけ拾う
            return new Intent
            {
                Type = IntentType.Set,
                Slot = new Slot
                {
                    TargetVM ="TestPerson",
                    Field = "Name",
                    Type = "string",
                    Value = "山田"
                }
            };
        }
    }
}
