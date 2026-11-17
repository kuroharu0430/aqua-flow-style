using BlazorApp.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp.Core.Service
{
    public class IntentService
    {
        public async Task<List<Intent>> ParseAsync(
            string text,
            string operationMap)
        {
            //    var prompt =
            //        IntentParser.BuildPrompt(
            //            text,
            //            operationMap);

            //    var response =
            //        await _openAI.Chat(prompt);

            //    return JsonSerializer.Deserialize<List<Intent>>
            //        (response) ?? [];


            // TODO スタブ
            return
            [
                new Intent
                {
                    Type = IntentType.Set,
                    TargetId = 1,
                    Value = "山田",
                    Confidence = 1
                }
            ];
        }
    }
}
