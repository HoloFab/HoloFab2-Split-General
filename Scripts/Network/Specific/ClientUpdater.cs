#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using System.Linq;
using HoloFab.CustomData;

using Grasshopper;

namespace HoloFab {
	public class ClientUpdater : UDPSend {
		private static HoloSystemState holoState;
		protected override string agentName {
			get {
				return "UDP Client Updater Interface";
			}
		}
		public bool ContainsID(int _id) {
			return ClientUpdater.holoState.ContainsID(_id);
		}
		public HoloComponent this[int _id] {
			get {
				return ClientUpdater.holoState?[_id];
			}
		}
		public ClientUpdater(object _owner, string _IP, string _ownerName="") :
			                                                                  base(_owner, _IP: _IP, _port: 8889, _ownerName: _ownerName) {}
		////////////////////////////////////////////////////////////////////////
		public void UpdateDevice() {
			byte[] data = EncodeUtilities.EncodeData("HOLOSTATE", ClientUpdater.holoState, out _);
			QueueUpData(data);
		}
		public void RegisterAgent(HoloComponent component) {
			ClientUpdater.holoState.holoComponents.Add(component);
			UpdateDevice();
		}
		////////////////////////////////////////////////////////////////////////
		public override bool Connect() {
			if (ClientUpdater.holoState == null)
				ClientUpdater.holoState = new HoloSystemState();
			return base.Connect();
		}
		public override void Disconnect() {
			// Notify client.
			byte[] data = EncodeUtilities.EncodeData("HOLOTERMINATED", ClientUpdater.holoState, out _);
			SendData(data);
			// Clear holostate.
			ClientUpdater.holoState = null;
			// Disconnect itself.
			base.Disconnect();
		}
	}
}
#endif