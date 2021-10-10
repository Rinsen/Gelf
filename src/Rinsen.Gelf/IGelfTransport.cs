using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    public interface IGelfTransport
    {
        Task Send(string gelfPayload, CancellationToken stoppingToken);

    }
}
