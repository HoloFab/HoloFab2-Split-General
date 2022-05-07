using System;
using System.Collections.Generic;

namespace HoloFab
{
    // Structure to hold Custom data types holding data to be sent.
    namespace CustomData
    {
#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)  // WHAT DOES THE FUCK?
        // A struct holding network info for other components.
        public class HoloConnection
        {
            public string remoteIP;
            public bool status;

            // UDP Connections List for tracking
            private List<NetworkAgent> networkAgents = new List<NetworkAgent>();

            public bool MessagesAvailable{
                get {
                    return false;
                }
            }
            
            public HoloConnection(string remoteIP)
            {
                this.remoteIP = remoteIP;
                this.status = false;


            }

            ~HoloConnection() { 
                //Disconnect();
            }
            //////////////////////////////////////////////////////////////////////////////
            public bool Connect() {
                return true;
            }
            public void Disconnect() { }
            //////////////////////////////////////////////////////////////////////////////
            
            public void QueueUpData(SourceType sourceType, byte[] data) { 
            }
            public string LastMessage() { 
                return string.Empty;
            }
            //////////////////////////////////////////////////////////////////////////////

            /*
            /// <summary>
            /// Connect the TCP Agent if not already connected
            /// </summary>
            /// <returns>boolean defining the successful connection</returns>
            public bool SafeConnect()
            {
                // Check if connection is Alive
                foreach(TCPAgent agent in this.tcpAgents)
                    if (agent != null)
                    {
                       // if (this.tcpAgent.PingConnection())
                        //{
                        //    return true;
                        //}
                    }
                Disconnect();
                this.tcpAgent = new TCPAgent(this, this.remoteIP);
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
                if (this.tcpAgent != null)
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

            public UDPReceive UdpRecv(Object owner, int port)
            {
                //foreach (UDPSend agent in udpRecvTasks)
                //{
                //    if (agent.remotePort == port)
                //    {
                //        return agent;
                //    }
                //}
                UDPReceive udpRecv = new UDPReceive(owner, remoteIP, port);
                udpRecv.StartListening();
                udpTasksList.Add(udpRecv);
                return udpRecv;
            }

            public void StopUdpRecv(Object owner)
            {
                foreach (UDPReceive agent in udpTasksList)
                {
                    if (agent.owner == owner)
                    {
                        agent.StopListening();
                        udpTasksList.Remove(agent);
                    }
                }
            }
            */
        }
#endif
    }
}