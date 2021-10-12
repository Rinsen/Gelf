using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rinsen.Gelf
{
    internal class GelfPayloadSerializer
    {

        public string Serialize(GelfPayload gelfPayload)
        {
            return JsonSerializer.Serialize(gelfPayload);
        }


    }
}
