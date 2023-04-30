#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using HoloFab.CustomData;

using Grasshopper;

namespace HoloFab {
	public class ClientAcknowledger : UDPReceive {
		protected override string agentName {
			get {
				return "UDP Client Acknowledger Interface";
			}
		}
        
		public ClientAcknowledger(object owner, int _port=8803, string _ownerName="") :
			                  base(_owner: owner, _port: _port, _ownerName: _ownerName){
			this.OnDataReceived += OnDeviceReceived;
		}
		////////////////////////////////////////////////////////////////////////
		#region RECEIVING
		private void OnDeviceReceived(object owner, DataReceivedArgs receivedArgs) {
			string clientAddress, clientData;
            
			clientAddress = receivedArgs.source;//FindServer.receiver.connectionHistory.Dequeue();// connectionHistory[FindServer.receiver.connectionHistory.Count - 1].ToString();
			clientData = receivedArgs.data;//FindServer.receiver.dataMessages.Dequeue();// [FindServer.receiver.dataMessages.Count - 1];
            
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName,
			                              "Received data: " + clientAddress + ":" + clientData,
			                              ref this.debugMessages);
			#endif
			
			string[] messageComponents = clientData.Split(new string[] {EncodeUtilities.headerSplitter}, 2, StringSplitOptions.RemoveEmptyEntries);
			if (messageComponents.Length > 1) {
				string header = messageComponents[0], content = messageComponents[1];
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Header: " + header + ", content: " + content);
				#endif
				if (header == "HOLOACKNOWLEDGE") {
					HoloSystemState clientState = EncodeUtilities.InterpreteHoloState(content);
                    ((HoloConnect)this.owner).connect.CheckState(clientState);
				}
			}
		}
		#endregion
	}
}
#endif