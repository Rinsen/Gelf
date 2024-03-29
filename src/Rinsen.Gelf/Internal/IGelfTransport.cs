﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.Gelf
{
    internal interface IGelfTransport
    {
        public GelfTransport TransportType { get; }

        Task Send(GelfPayload gelfPayload, CancellationToken stoppingToken);

    }
}
