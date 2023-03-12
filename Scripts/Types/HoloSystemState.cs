using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HoloFab {
	namespace CustomData {
		// Structure to holdfab system info.
		[System.Serializable]
		public class HoloSystemState {
			public string serverIP;
			public List<HoloComponent> holoComponents;
            
			public bool ContainsID(int _id) {
				return this.holoComponents.Any(component => component.id == _id);
			}
			public HoloComponent this[int _id] {
				get {
					return this.holoComponents.First(component => component.id == _id);
				}
			}
            
			public HoloSystemState() {
				this.serverIP = NetworkUtilities.LocalIPAddress();
				this.holoComponents = new List<HoloComponent>();
			}
		}
		[System.Serializable]
		public class HoloComponent {
			public SourceType sourceType;
			public SourceCommunicationType communicationType;
			public int port;
            
			public int id {
				get {
					int _id =
						(((int)this.sourceType).ToString()
						 +((int)this.communicationType).ToString()
						 +this.port.ToString())
						.GetHashCode();
					return _id;
				}
			}
            
			public HoloComponent(SourceType _sourceType, SourceCommunicationType _sourceCommunicationType, int portOverride = -1) {
				int port = 12121;
				switch (_sourceType) {
				 case SourceType.UDP:
					 switch (_sourceCommunicationType) {
					  case SourceCommunicationType.Sender:
						  //port = 12121;
						  break;
					  case SourceCommunicationType.Receiver:
						  //port = 12121;
						  break;
					 }
					 break;
				 case SourceType.TCP:
					 //port = 12121;
					 break;
				}
				if (portOverride != -1)
					port = portOverride;
				this.sourceType = _sourceType;
				this.communicationType = _sourceCommunicationType;
				this.port = port;
			}
            
			public NetworkAgent ToNetworkAgent(object owner, string remoteIP) {
				NetworkAgent agent = null;
				switch (this.sourceType) {
				 case SourceType.UDP:
					 switch (this.communicationType) {
					  case SourceCommunicationType.Sender:
						  agent = (NetworkAgent) new UDPSend(owner, remoteIP);
						  break;
					  case SourceCommunicationType.Receiver:
						  agent = (NetworkAgent) new UDPReceive(owner);
						  break;
					 }
					 break;
				 case SourceType.TCP:
					 agent = (NetworkAgent) new TCPAgent(owner, remoteIP);
					 break;
				}
				return agent;
			}
		}
	}
}