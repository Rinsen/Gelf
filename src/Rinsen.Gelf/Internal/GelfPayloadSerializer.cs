using System.Text.Json;

namespace Rinsen.Gelf
{
    internal class GelfPayloadSerializer
    {

        public static string Serialize(GelfPayload gelfPayload)
        {
            return JsonSerializer.Serialize(gelfPayload);
        }


    }
}
