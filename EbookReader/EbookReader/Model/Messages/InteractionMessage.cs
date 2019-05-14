using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EbookReader.Model.Messages
{
    public class InteractionMessage
    {
        public JObject EventData { get; private set; }

        public InteractionMessage(JObject e){
            EventData = e;
        }
    }
}
