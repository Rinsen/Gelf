using System;

namespace Rinsen.Gelf
{
    internal class GelfPayloadSerializer
    {

        public string Serialize(GelfPayload gelfPayload)
        {
            return System.Text.Json.JsonSerializer.Serialize(gelfPayload);
        }


    }
}
