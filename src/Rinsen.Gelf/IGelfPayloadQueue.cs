using System.Collections.Generic;

namespace Rinsen.Gelf
{
    interface IGelfPayloadQueue
    {
        void AddLog(GelfPayload gelfPayload);
        void GetReportedGelfPayloads(List<GelfPayload> logItems);
    }
}
