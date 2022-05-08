#define DEBUG
#define DEBUGWARNING
// #undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using HoloFab.CustomData;

namespace HoloFab {
	// UDP sender.
	public class UDPBroadcast : UDPSend {
		protected override string sourceName {
			get {
				return "UDP Broadcast Interface";
			}
		}
		public UDPBroadcast(object _owner, int _port = 12121) :
			                                                  base(_owner, _IP: null, _port: _port)
		{}
		public override bool Connect(){
			if (!this.IsConnected) {
				try {
					this._client = new UdpClient();
					this._client.EnableBroadcast = true;
					this._client.ExclusiveAddressUse = false;
					return true;
				} catch {
					;
				}
				return false;
			}
			return true;
		}
		public override void Disconnect(){
			this._client.Close();
		}
		////////////////////////////////////////////////////////////////////////
		#region SENDING
		protected override void SendData(byte[] data) {
			this.flagSuccess = false;
			try {
				Connect();
				this._client.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, this.port));
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName,
				                              "Data Sent!",
				                              ref this.debugMessages);
				#endif
				this.flagSuccess = true;
				return;
			} catch (Exception exception) {
				string exceptionName = "OtherException";
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName,
				                                "Sending Exception: " + exceptionName +  ": " + exception.ToString(),
				                                ref this.debugMessages);
				#endif
				Disconnect();
			}
			// if failed - queue up again
			QueueUpData(data);
		}
		#endregion
	}
}