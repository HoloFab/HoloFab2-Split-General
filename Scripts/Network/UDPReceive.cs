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
    // UDP sender.
    public class UDPRecv : UDPAgent    // : IDisposable
    {
        private CancellationTokenSource recvCancellation;

        // - addresses of incomiing connections (corresponding to data)
        public Queue<string> connectionHistory = new Queue<string>();

        public event EventHandler<DataReceivedArgs> DataReceived;

        /// <summary>
        /// Creates a UDP Agent for handling the sending and/or receiving data.
        /// </summary>
        /// <param name="owner">The class that owns the client</param>
        /// <param name="remoteIP">IP of the target device for sending</param>
        /// <param name="remotePort">Port of the target device for sending</param>
        public UDPRecv(Object owner, string remoteIP, int remotePort)
            : base(owner, remoteIP, remotePort)
        {
            connectionHistory = new Queue<string>();
        }

        ~UDPRecv()
        {
            StopListening();
        }

        ////////////////////////////////////////////////////////////////////////
        public void StartListening()
        {
            recvCancellation = new CancellationTokenSource();
            Task udpReceiver = Task.Run(() =>
            {
                client = new UdpClient(remotePort);
                while (true) ReceiveData();
            }, recvCancellation.Token);
        }

        public void StopListening()
        {
            if (recvCancellation != null) recvCancellation.Cancel();
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
#else
        ////////////////////////////////////////////////////////////////////////
        // Constantly check for new messages on given port.
        private void ReceiveData()
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data;
            string message;

            try
            {
                // Receive Bytes.
                data = client.Receive(ref anyIP);
                if (data.Length > 0)
                {
                    // If buffer not empty - decode it.
                    message = EncodeUtilities.DecodeData(data);
                    // If string not empty and not read yet - react to it.
                    if (!string.IsNullOrEmpty(message))
                    {
#if DEBUG2
						DebugUtilities.UniversalDebug(this.sourceName,
                            "Total Data found: " + receiveString,
                            ref this.debugMessages);
#endif
                        // Raise Event
                        int index = message.Trim()
                            .IndexOf(EncodeUtilities.messageSplitter);
                        if (index > 0)
                            OnDataReceived(this,
                                new DataReceivedArgs(
                                    message.Substring(0, index)));
                        this.connectionHistory.Enqueue(
                            anyIP.Address.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                String excName;
                if (exception is SocketException) excName = "SocketException";
                else excName = "Exception";
#if DEBUGWARNING
                DebugUtilities.UniversalWarning(this.sourceName,
                    "Exception: " + exception.ToString(),
                    ref this.debugMessages);
#endif
            }
        }

        ////////////////////////////////////////////////////////////////////////
        //------------------------<Data_Receive_Event>------------------------//
        ////////////////////////////////////////////////////////////////////////
        private void OnDataReceived(object sender, DataReceivedArgs e)
        {
            var handler = DataReceived;
            if (handler != null) DataReceived(this, e);  // re-raise event
        }
#endif
    }
}