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
    public class UDPSend : UDPAgent    // : IDisposable
    {
        private CancellationTokenSource sendCancellation;
        
        // Queue of buffers to send.
        private Queue<byte[]> sendQueue = new Queue<byte[]>();
        // Accessor to check if there is data in queue
        public bool IsNotEmpty { get { return this.sendQueue.Count > 0; } }

        /// <summary>
        /// Creates a UDP Agent for handling the sending and/or receiving data.
        /// </summary>
        /// <param name="owner">The class that owns the client</param>
        /// <param name="remoteIP">IP of the target device for sending</param>
        /// <param name="remotePort">Port of the target device for sending</param>
        public UDPSend(Object owner, string remoteIP, int remotePort)
            : base(owner, remoteIP, remotePort)
        {
            sendQueue = new Queue<byte[]>();
        }

        ~UDPSend()
        {
            StopSending();
        }

        ////////////////////////////////////////////////////////////////////////
        // Queue Functions.
        // Start the thread to send data.
        public void StartSending()
        {
            // if queue not set create it.
            if (this.sendQueue == null)
                this.sendQueue = new Queue<byte[]>();
            sendCancellation = new CancellationTokenSource();
            Task udpSender = Task.Run(() =>
            {
                while (true) SendFromQueue();
            }, sendCancellation.Token);
        }

        // Disable Sending.
        public void StopSending()
        {
            // TODO: Should we reset queue?
            // Reset.
            if (sendCancellation != null) sendCancellation.Cancel();
            this.sendQueue = null;
        }

        // Enqueue data.
        public void QueueUpData(byte[] newData)
        {
            lock (this.sendQueue)
            {
                this.sendQueue.Enqueue(newData);
            }
        }

        /// ////////////////////////////////////////////////////////////////////
        // Check the queue and try send it.
        private void SendFromQueue()
        {
            try
            {
                if (this.IsNotEmpty)
                {
                    byte[] currentData;
                    lock (this.sendQueue)
                    {
                        currentData = this.sendQueue.Dequeue();
                    }
                    // Peek message to send
                    Send(currentData);
                    //// if no exception caught and data sent successfully - remove from queue.
                    //if (!this.flagSuccess)
                    //	lock (this.sendQueue) {
                    //		this.sendQueue.Enqueue(currentData);
                    //	}
                }
            }
            catch (Exception exception)
            {
                DebugUtilities.UniversalDebug(this.sourceName,
                    "Queue Exception: " + exception.ToString(),
                    ref this.debugMessages);
                this.flagSuccess = false;
            }
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
        // Start a connection and send given byte array.
        private void Send(byte[] sendBuffer)
        {
            this.flagSuccess = false;
            //UdpClient client;
            try
            {
                client = new UdpClient(this.remoteIP, this.remotePort);
                client.Send(sendBuffer, sendBuffer.Length);
                client.Close();
				DebugUtilities.UniversalDebug(this.sourceName,
                    "Data Sent!",
                    ref this.debugMessages);
                this.flagSuccess = true;
                return;
            }
            catch (Exception exception)
            {
#if DEBUGWARNING
                DebugUtilities.UniversalWarning(this.sourceName,
                    "Exception: " + exception.ToString(),
                    ref this.debugMessages);
#endif
            }
        }

        // Broadcast Message to everyone.
        public void Broadcast(byte[] sendBuffer)
        {
            // Reset.
            if (client != null)
            {
                client.Close();
                client = null; // Good Practice?
            }
            try
            {
                client = new UdpClient(new IPEndPoint(IPAddress.Broadcast, this.remotePort));
                client.Send(sendBuffer, sendBuffer.Length);
                client.Close();
                DebugUtilities.UniversalDebug(this.sourceName,
                    "Broadcast Sent!",
                    ref this.debugMessages);
                flagSuccess = true;
                return;
            }
            catch (Exception exception)
            {
#if DEBUGWARNING
                DebugUtilities.UniversalWarning(this.sourceName,
                    "Exception: " + exception.ToString(),
                    ref this.debugMessages);
#endif
            }
        }
#endif
    }
}