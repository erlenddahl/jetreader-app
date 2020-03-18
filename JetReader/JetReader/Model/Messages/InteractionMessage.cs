using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JetReader.Model.Messages
{
    public class InteractionMessage
    {
        public JObject EventData { get; private set; }

        public InteractionMessage(JObject e){
            EventData = e;
        }
    }
}
