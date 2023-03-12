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
		private HoloSystemState holoState;
		protected override string agentName {
			get {
				return "UDP Client Updater Interface";
			}
		}
		public bool ContainsID(int _id) {
			try { 
				return this.holoState.ContainsID(_id);
			} catch {
                this.holoState = new HoloSystemState();
                return false;
			}
		}
		public HoloComponent this[int _id] {
			get {
				try {
                    return this.holoState[_id];
                } catch {
					this.holoState = new HoloSystemState();
					return null;
				}
			}
		}
		public ClientUpdater(object _owner, string _IP, int _port=8802, string _ownerName="") :
														base(_owner, _IP: _IP, _port: _port, _ownerName: _ownerName) {
            this.holoState = new HoloSystemState();
		}
		////////////////////////////////////////////////////////////////////////
		public void UpdateDevice() {
			this.holoState.Update();
			byte[] data = EncodeUtilities.EncodeData("HOLOSTATE", this.holoState, out _);
			QueueUpData(data);
		}
		public void RegisterAgent(HoloComponent component) {
            this.holoState.holoComponents.Add(component);
			UpdateDevice();
		}
		////////////////////////////////////////////////////////////////////////
		public override bool Connect() {
			return base.Connect();
		}
		public override void Disconnect() {
			// Notify client.
			byte[] data = EncodeUtilities.EncodeData("HOLOTERMINATED", this.holoState, out _);
			SendData(data);
			// Delay to let the message through. ? Check if necessary
			Thread.Sleep(10);
            // Clear holostate.
            this.holoState.Clear();
            // Disconnect itself.
			base.Disconnect();
        }
	}
}
#endif