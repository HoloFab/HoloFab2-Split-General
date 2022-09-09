using System;
using System.Collections.Generic;

#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP) 
using Grasshopper.Kernel;
#endif

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

            public GH_Component owner;
            private ClientUpdater clientUpdater;

            // UDP Connections List for tracking
            private Dictionary<int,NetworkAgent> networkAgents = new Dictionary<int, NetworkAgent>();

            public bool MessagesAvailable{
                get {
                    return false;
                }
            }
            
            public HoloConnection(GH_Component owner, string remoteIP){
                this.owner = owner;
                this.remoteIP = remoteIP;
                this.status = false;
            }

            ~HoloConnection() { 
                Disconnect();
            }

            public void RefreshOwner() {
                this.owner.ExpireSolution(true);
            }
            //////////////////////////////////////////////////////////////////////////////
            public bool Connect() {
                SetupUpdate();
                foreach (NetworkAgent agent in this.networkAgents.Values) {
                    agent.Connect();
                    agent.StartSending();
                    agent.StartReceiving();
                }
                this.clientUpdater.UpdateDevice();
                return true;
            }
            public void Disconnect() {
                foreach (NetworkAgent agent in this.networkAgents.Values) {
                    agent.StopSending();
                    agent.StopReceiving();
                    agent.Disconnect();
                }
                this.networkAgents.Clear();
                this.clientUpdater = null;
                this.status = false;
            }
            //////////////////////////////////////////////////////////////////////////////
            void SetupUpdate() {
                // check if invalid updater is present.
                if ((this.clientUpdater != null) && (this.clientUpdater.IP != this.remoteIP)) {
                    this.clientUpdater.StopSending();
                    this.clientUpdater.Disconnect();
                    this.clientUpdater = null;
                }
                // Create new updater if needed
                if (this.clientUpdater == null) {
                    HoloComponent component = new HoloComponent(SourceType.UDP, SourceCommunicationType.Sender, 8889);
                    this.clientUpdater = new ClientUpdater(this.owner, remoteIP);
                    if (this.networkAgents.ContainsKey(component.id))
                        this.networkAgents[component.id] = (NetworkAgent)this.clientUpdater;
                    else
                        this.networkAgents.Add(component.id, (NetworkAgent)this.clientUpdater);
                }
                
            }
            
            public int RegisterAgent(SourceType _sourceType, SourceCommunicationType _sourceCommunicationType) {
                HoloComponent component = new HoloComponent(_sourceType, _sourceCommunicationType);
                if (!this.clientUpdater.ContainsID(component.id)){
                    this.clientUpdater.RegisterAgent(component);
                    NetworkAgent agent = component.ToNetworkAgent(this.owner, this.remoteIP);
                    bool success = agent.Connect();
                    if (success) { 
                        agent.StartSending();
                        agent.StartReceiving();

                        if (this.networkAgents.ContainsKey(component.id))
                            this.networkAgents[component.id] = agent;
                        else
                            this.networkAgents.Add(component.id, agent);
                    } else {
                        Disconnect();
                        return -1;
                    }
                }
                return component.id;
            }
            //////////////////////////////////////////////////////////////////////////////
            public void QueueUpData(int componentID, byte[] data) {
                if (this.networkAgents.ContainsKey(componentID))
                    this.networkAgents[componentID]?.QueueUpData(data);
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