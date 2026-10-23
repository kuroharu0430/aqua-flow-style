namespace BlazorApp.Core.Model
{
    public static class IntentParser
    {
        public static Intent Parse(string text, ViewModelMetadata mata)
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

                // AIからもらうJSONの理想形
                //{
                //    "intent": "Set",
                //    "slot": {
                //    "targetVM": "PersonEditVM",
                //    "field": "Name",
                //    "type": "string",
                //    "value": "山田"
                //}

            };
        }
    }
}
