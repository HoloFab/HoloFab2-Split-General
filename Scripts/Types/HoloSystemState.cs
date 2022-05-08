using System;
using System.Collections;
using System.Collections.Generic;

namespace HoloFab {
	namespace CustomData {
		// Structure to holdfab system info.
		[System.Serializable]
		public class HoloSystemState {
			public string serverIP;
			public List<HoloComponents> holoComponents;
            
			public HoloSystemState(){
				this.serverIP = NetworkUtilities.LocalIPAddress();
				this.holoComponents = new List<HoloComponents>();
			}
		}
		[System.Serializable]
		public class HoloComponents {
			public SourceType sourceType;
			public SourceCommunicationType communicationType;
			public int port;
		}
	}
}