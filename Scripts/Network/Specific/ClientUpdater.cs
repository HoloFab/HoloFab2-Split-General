#if !(UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || WINDOWS_UWP)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using HoloFab.CustomData;

using Grasshopper;

namespace HoloFab {
	public class ClientUpdater : UDPSend {
		private static HoloSystemState holoState;
		protected override string sourceName {
			get {
				return "UDP Client Updater Interface";
			}
		}
		public ClientUpdater(object _owner, string _IP) :
			                                            base(_owner, _IP: _IP, _port: 8889) {
			if (ClientUpdater.holoState == null)
				ClientUpdater.holoState = new HoloSystemState();
		}
		////////////////////////////////////////////////////////////////////////
		public void UpdateDevice() {
			byte[] data = EncodeUtilities.EncodeData("HOLOSTATE", ClientUpdater.holoState, out _);
			QueueUpData(data);
		}
	}
}
#endif