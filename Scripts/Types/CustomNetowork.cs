using System;
using System.Collections.Generic;

namespace HoloFab
{
    // Structure to hold Custom data types holding data to be sent.
    namespace CustomData
    {
#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)  // WHAT DOES THE FUCK?
        // A struct holding network info for other components.
        public class Connection
        {
            public string remoteIP;
            public bool status;

            public TCPAgent tcpAgent;

            // UDP Connections List for tracking
            private List<UDPAgent> udpTasksList = new List<UDPAgent>();

            
            public Connection(string remoteIP)
            {
                this.remoteIP = remoteIP;
                status = false;
            }

            ~Connection()
            {
                Disconnect();
            }

            /// <summary>
            /// Connect the TCP Agent if not already connected
            /// </summary>
            /// <returns>boolean defining the successful connection</returns>
            public bool SafeConnect()
            {
                // Check if connection is Alive
                if (tcpAgent != null)
                {
                    if (tcpAgent.PingConnection())
                    {
                        return true;
                    }
                }
                Disconnect();
                tcpAgent = new TCPAgent(this.remoteIP);
                return Connect();
            }

            private bool Connect()
            {
                // Connect TCP
                if (!this.tcpAgent.Connect()) return false;
                return true;
            }

            /// <summary>
            /// Disconnect the TCP Agent and set it as null
            /// </summary>
            public void Disconnect()
            {
                if (tcpAgent != null)
                {
                    this.tcpAgent.Disconnect();
                    this.tcpAgent = null;
                }
            }

            // GOOD PRACTICE . . . ?
            public bool PendingMessages
            {
                get
                {
                    return (this.tcpAgent.IsNotEmpty);  // || this.udpSender.IsNotEmpty);
                }
            }

            // MOVE TO NETWORKUTILITIES . . . ?
            public void TransmitIP()
            {
                // Send local IPAddress for device to communicate back.
                byte[] bytes = EncodeUtilities.EncodeData("IPADDRESS", NetworkUtilities.LocalIPAddress(), out _);
                //this.udpSender.QueueUpData(bytes);
                //bool success = connect.udpSender.flagSuccess;
            }

            // Make a new UDPSend Task and return the QueueUpData function;
            public UDPSend UdpSend(Object owner, int remotePort)
            {
                // Test if the agent is not active the other way
                //foreach (UDPSend agent in udpSendTasks)
                //{
                //    if (agent.remotePort == port)
                //    {
                //        return agent;
                //    }
                //}
                UDPSend udpSend = new UDPSend(owner, remoteIP, remotePort);
                udpTasksList.Add(udpSend);
                return udpSend;
            }

            public void StopUdpSend(Object owner)
            {
                foreach (UDPSend agent in udpTasksList)
                {
                    if (agent.owner == owner)
                    {
                        agent.StopSending();
                        udpTasksList.Remove(agent);
                    }
                }
            }

            ////////////////////////////////////////////////////////////////////

            public UDPRecv UdpRecv(Object owner, int port)
            {
                //foreach (UDPSend agent in udpRecvTasks)
                //{
                //    if (agent.remotePort == port)
                //    {
                //        return agent;
                //    }
                //}
                UDPRecv udpRecv = new UDPRecv(owner, remoteIP, port);
                udpRecv.StartListening();
                udpTasksList.Add(udpRecv);
                return udpRecv;
            }

            public void StopUdpRecv(Object owner)
            {
                foreach (UDPRecv agent in udpTasksList)
                {
                    if (agent.owner == owner)
                    {
                        agent.StopListening();
                        udpTasksList.Remove(agent);
                    }
                }
            }
        }
#endif
    }
}