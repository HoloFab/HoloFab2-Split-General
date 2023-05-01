using System;
using System.Collections.Generic;
using Grasshopper;

#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP) 
using Grasshopper.Kernel;
#endif

namespace HoloFab {
    // Structure to hold Custom data types holding data to be sent.
    namespace CustomData {
#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)  // WHAT DOES THE FUCK?
        // A struct holding network info for other components.
        public class HoloConnection {
            public string remoteIP;
            public bool status;

            public GH_Component owner;
            private ClientUpdater clientUpdater;
            private ClientAcknowledger clientAcknowledger;

            // UDP Connections List for tracking
            private Dictionary<int, NetworkAgent> networkAgents = new Dictionary<int, NetworkAgent>();
            
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
                Instances.InvalidateCanvas();
            }
            //////////////////////////////////////////////////////////////////////////////
            public void SetupUpdate() {
                // Special network agents not tracked by system.
                // - Create new acknowledger if needed.
                if (this.clientAcknowledger == null) {
                    int port = 8803;
                    this.clientAcknowledger = new ClientAcknowledger(this.owner, _port: port);
                    this.clientAcknowledger.Connect();
                    this.clientAcknowledger.StartReceiving();
                }
                // - Create new updater if needed.
                //   Check if invalid updater is present.
                if ((this.clientUpdater != null) && (this.clientUpdater.IP != this.remoteIP)) {
                    this.clientUpdater.StopSending();
                    this.clientUpdater.Disconnect();
                    this.clientUpdater = null;
                }
                if (this.clientUpdater == null) {
                    int port = 8802;
                    this.clientUpdater = new ClientUpdater(this.owner, remoteIP, _port: port);
                    this.clientUpdater.Connect();
                    this.clientUpdater.StartSending();
                    this.clientUpdater.UpdateDevice();
                }
            }
            public void CheckState(HoloSystemState clientState) {
                //if (this.clientUpdater.CompareState(clientState))
                Connect();
            }
            private bool Connect() {
                // Connect is decoupled form Set up to allow agents to register
                // and allow for the client to acknowledge.
                foreach (NetworkAgent agent in this.networkAgents.Values) {
                    agent.Connect();
                    agent.StartSending();
                    agent.StartReceiving();
                }
                return true;
            }
            public void Disconnect() {
                // Disconnect all agents including specialized ones.
                foreach (NetworkAgent agent in this.networkAgents.Values) {
                    agent.StopSending();
                    agent.StopReceiving();
                    agent.Disconnect();
                }
                this.networkAgents.Clear();
                if (this.clientUpdater != null) { 
                    this.clientUpdater.StopSending();
                    this.clientUpdater.Disconnect();
                    this.clientUpdater = null;
                }
                if (this.clientAcknowledger != null) { 
                    this.clientAcknowledger.StopReceiving();
                    this.clientAcknowledger.Disconnect();
                    this.clientAcknowledger = null;
                }
                this.status = false;
            }
            //////////////////////////////////////////////////////////////////////////////
            public int RegisterAgent(SourceType _sourceType, SourceCommunicationType _sourceCommunicationType) {
                if (_sourceType == SourceType.TCP)
                    _sourceCommunicationType = SourceCommunicationType.SenderReceiver;
                HoloComponent component = new HoloComponent(_sourceType, _sourceCommunicationType);
                if (!this.clientUpdater.ContainsID(component.id)){
                    this.clientUpdater.RegisterAgent(component);
                    NetworkAgent agent = component.ToNetworkAgent(this.owner, this.remoteIP);
                    if (this.networkAgents.ContainsKey(component.id))
                        this.networkAgents[component.id] = agent;
                    else
                        this.networkAgents.Add(component.id, agent);
                }
                return component.id;
            }
            public void RegisterReceiverCallback(int communicatorID, EventHandler<DataReceivedArgs> OnDataReceived) {
                if (this.networkAgents.ContainsKey(communicatorID)) { 
                    this.networkAgents[communicatorID].OnDataReceived -= OnDataReceived;
                    this.networkAgents[communicatorID].OnDataReceived += OnDataReceived;
                }
            }
            //////////////////////////////////////////////////////////////////////////////
            public void QueueUpData(int _componentID, byte[] _data) {
                if (this.networkAgents.ContainsKey(_componentID))
                    this.networkAgents[_componentID]?.QueueUpData(_data);
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