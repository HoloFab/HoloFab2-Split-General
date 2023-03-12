#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using HoloFab.CustomData;

using Grasshopper;

namespace HoloFab {
	public class ClientFinder : UDPReceive {
		protected override string agentName {
			get {
				return "UDP Client Finder Interface";
			}
		}
		// Client List
		public Dictionary<string, HoloDevice> devices = new Dictionary<string, HoloDevice>();
		private readonly int expireDeivceDelay = 4000;
		public bool flagUpdate = false;

		private TaskInterface deviceUpdater;
        
		public ClientFinder(object owner, int _port=8801, string _ownerName="") :
			                                                    base(_owner: owner, _port: _port, _ownerName: _ownerName){
			this.OnDataReceived += OnDeviceReceived;
            
			this.deviceUpdater = new TaskInterface(UpdateDevices, _delayInTask: 1000);
		}
		////////////////////////////////////////////////////////////////////////
		#region RECEIVING
		public override void StartReceiving() {
			base.StartReceiving();
			if (this.deviceUpdater != null)
				this.deviceUpdater.Start();
		}
		public override void StopReceiving() {
			base.StopReceiving();
			if (this.deviceUpdater != null)
				this.deviceUpdater.Stop();
		}
        
		private void OnDeviceReceived(object owner, DataReceivedArgs receivedArgs) {
			string clientAddress, clientRequest;
            
			clientAddress = receivedArgs.source;//FindServer.receiver.connectionHistory.Dequeue();// connectionHistory[FindServer.receiver.connectionHistory.Count - 1].ToString();
			clientRequest = receivedArgs.data;//FindServer.receiver.dataMessages.Dequeue();// [FindServer.receiver.dataMessages.Count - 1];
            
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName,
			                              "Found Client: " + clientAddress + ":" + clientRequest,
			                              ref this.debugMessages);
			#endif
			lock (this.devices)
				if (!this.devices.ContainsKey(clientAddress))
					this.devices.Add(clientAddress, new HoloDevice(clientAddress, clientRequest));
				else
					this.devices[clientAddress].lastCall = DateTime.Now;
		}
        
		private void UpdateDevices() {
			//this.flagUpdate = false;
			// Check if any of devices have to be excluded.
			List<string> removeList = new List<string>();
			lock (this.devices) {
				try {
					foreach (KeyValuePair<string, HoloDevice> item in this.devices)
						if (DateTime.Now - item.Value.lastCall > TimeSpan.FromMilliseconds(this.expireDeivceDelay)) {
							removeList.Add(item.Key);
                            this.flagUpdate = true;
						}
					// Check if solution need to update.
					if (this.flagUpdate) {
						for (int i = removeList.Count - 1; i>=0; i--)
							this.devices.Remove(removeList[i]);
						//Instances.InvalidateCanvas();
						//((HoloConnect)this.owner).ExpireSolution(true);
					}
				} catch { }
			}
		}
		#endregion
	}
}
#endif