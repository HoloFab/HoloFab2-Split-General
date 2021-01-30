// #define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#endif

using HoloFab.CustomData;


namespace HoloFab
{
    public class UDPAgent	// : IDisposable
    {
        // Network Objects:
#if WINDOWS_UWP
		private string sourceName = "UDP Send Interface UWP";
		// Connection Object Reference.
		private DatagramSocket client;
		private static string broadcastAddress = "255.255.255.255";
#else
        internal string sourceName = "UDP Send Interface";
        internal UdpClient client;
#endif

        // An IP and a port for UDP communication to send to.
        public string remoteIP;
        public int remotePort;

        public bool flagSuccess = false;
        public List<string> debugMessages = new List<string>();
        public object owner;


        /// <summary>
        /// Creates a UDP Agent for handling the sending and/or receiving data.
        /// </summary>
        /// <param name="owner">The class that owns the client</param>
        /// <param name="remoteIP">IP of the target device for sending</param>
        /// <param name="remotePort">Port of the target device for sending</param>
        public UDPAgent(Object owner, string remoteIP, int remotePort)   //, int remotePort = 12121)
        {
            this.owner = owner;
            this.remoteIP = remoteIP;
            this.remotePort = remotePort;
            this.debugMessages = new List<string>();
        }

        ~UDPAgent()
        {
            ;
        }

        ////////////////////////////////////////////////////////////////////////
#if WINDOWS_UWP
		// Start a connection and send given byte array.
		private async void Send(byte[] sendBuffer) {
			this.flagSuccess = false;
			// Stop client if set previously.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
			}
			try {
				// Open new one.
				this.client = new DatagramSocket();
				// Write.
				using (var stream = await this.client.GetOutputStreamAsync(new HostName(this.remoteIP),
				                                                           this.remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				this.client.Dispose();
				this.client = null; // Good Practice?
				// Acknowledge.
#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
#endif
				this.flagSuccess = true;
				return;
			} catch (Exception exception) {
				// Exception.
#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
#endif
			}
		}
		// Broadcast Message to everyone.
		public async void Broadcast(byte[] sendBuffer) {
			// Reset.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new DatagramSocket();
				// Write.
				using (var stream = await this.client.GetOutputStreamAsync(new HostName(UDPSend.broadcastAddress),
				                                                           this.remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				this.client.Dispose();
				// Acknowledge.
#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Broadcast Sent!", ref this.debugMessages);
#endif
				this.flagSuccess = true;
				return;
			} catch (Exception exception) {
				// Exception.
#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
#endif
			}
		}
#endif
    }
}