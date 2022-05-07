// #define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using HoloFab.CustomData;


namespace HoloFab
{
    // UDP Agent.
    public class UDPAgent : NetworkAgent {
        // Network Objects:
        protected override string sourceName {
            get { 
                return "UDP Agent Interface";
            }
        }
        protected UdpClient _client;
        protected override object client  => _client;

        public UDPAgent(object _owner, string _remoteIP, int _remotePort = 12121, bool _sendingEnabled = false, bool _receivingEnabled = false) :
            base(_owner, _remoteIP, _remotePort, _sendingEnabled, _receivingEnabled)
        { }
    }
}