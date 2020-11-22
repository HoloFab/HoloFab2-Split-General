#define DEBUG
#define DEBUGWARNING
// #undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
#else
using System.Net.Sockets;
using System.Threading;
#endif

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// TCP sender.
	public class TCPSend {
		// An IP and a port for TCP communication to send to.
		public string remoteIP;
		private int remotePort;
		public int timeout = 2000;
        
		public bool flagConnected = false;
		public bool flagSuccess = false;
        
		// Network Objects:
		#if WINDOWS_UWP
		private string sourceName = "TCP Send Interface UWP";
		// Connection Object Reference.
		private StreamSocket client;
		// Task Object Reference.
		private CancellationTokenSource connectionCancellationTokenSource;
		private CancellationTokenSource sendCancellationTokenSource;
		#else
		private string sourceName = "TCP Send Interface";
		// Connection Object Reference.
		private TcpClient client;
		private NetworkStream stream;
		#endif
		// History:
		// - Debug History.
		public List<string> debugMessages = new List<string>();
		// Queuing:
		// Interface to keep checking queue on background and send it.
		ThreadInterface sender;
		// Queue of buffers to send.
		private Queue<byte[]> sendQueue = new Queue<byte[]>();
		// Accessor to check if there is data in queue
		public bool IsNotEmpty {
			get {
				return this.sendQueue.Count > 0;
			}
		}
        
		// Main Constructor
		public TCPSend(string _remoteIP, int _remotePort=11111) {
			this.remoteIP = _remoteIP;
			this.remotePort = _remotePort;
			this.debugMessages = new List<string>();
			this.sender = new ThreadInterface(SendFromQueue);
			// Reset.
			Disconnect();
		}
		// Destructor.
		~TCPSend() {
			Disconnect();
		}
		////////////////////////////////////////////////////////////////////////
		// Queue Functions.
		// Start the thread to send data.
		private void StartSending(){
			// if queue not set create it.
			if (this.sendQueue == null)
				this.sendQueue = new Queue<byte[]>();
			// Start the thread.
			this.sender.Start();
		}
		// Disable Sending.
		public void StopSending() {
			// TODO: Should we reset queue?
			// Reset.
			this.sender.Stop();
		}
		// Enqueue data.
		public void QueueUpData(byte[] newData) {
			lock (this.sendQueue)
				this.sendQueue.Enqueue(newData);
		}
		// Check the queue and try send it.
		public void SendFromQueue() {
			try {
				if (this.IsNotEmpty) {
					byte[] currentData;
					lock (this.sendQueue)
						currentData = this.sendQueue.Dequeue();
					// Peek message to send
					Send(currentData);
					// if no exception caught and data sent successfully - remove from queue.
					// if (!this.flagSuccess)
					// 	lock (this.sendQueue)
					// 		this.sendQueue.Dequeue(currentData);
				}
			} catch (Exception exception) {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Queue Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		#if WINDOWS_UWP
		////////////////////////////////////////////////////////////////////////
		// Establish Connection
		public async void Connect() {
			// Reset.
			Disconnect();
			this.client = new StreamSocket();
			this.connectionCancellationTokenSource = new CancellationTokenSource();
			this.sendCancellationTokenSource = new CancellationTokenSource();
			this.flagConnected = false;
			try {
				this.connectionCancellationTokenSource.CancelAfter(this.timeout);
				await this.client.ConnectAsync(new HostName(this.remoteIP), this.remotePort.ToString())
				.AsTask(this.connectionCancellationTokenSource.Token);
				StartSending();
				this.flagConnected = true;
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Connection Stablished!", ref this.debugMessages);
				#endif
				// return true;
			} catch(TaskCanceledException) {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Failed to connect", ref this.debugMessages);
				#endif
			} catch (Exception exception) {
				SocketErrorStatus webErrorStatus = SocketError.GetStatus(exception.GetBaseException().HResult);
				string errorMessage = (webErrorStatus.ToString() != "Unknown") ? webErrorStatus.ToString() : exception.Message;
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "UnhandledException: " + errorMessage, ref this.debugMessages);
				#endif
				// return false;
			}
		}
		// Start a connection and send given byte array.
		public async void Send(byte[] sendBuffer) {
			this.flagSuccess = false;
			try {
				if ((this.client != null) && this.flagConnected) {
					// Write.
					using (Stream outputStream = this.client.OutputStream.AsStreamForWrite()) {
						await outputStream.WriteAsync(sendBuffer, 0, sendBuffer.Length, this.sendCancellationTokenSource.Token);
					}
					// Acknowledge.
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
					#endif
					this.flagSuccess = true;
				} else {
					#if DEBUGWARNING
					DebugUtilities.UniversalWarning(this.sourceName, "Not connected", ref this.debugMessages);
					#endif
					this.flagConnected = false;
				}
			} catch(TaskCanceledException) {
				// timeout
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Connection timed out!", ref this.debugMessages);
				#endif
				this.flagConnected = false;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		// Stop Connection.
		public async void Disconnect() {
			if (this.connectionCancellationTokenSource != null) {
				this.connectionCancellationTokenSource.Cancel();
				this.connectionCancellationTokenSource.Dispose();
				this.connectionCancellationTokenSource = null;
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Resetting cancellation token source.", ref this.debugMessages);
				#endif
			}
			if (this.sendCancellationTokenSource != null) {
				this.sendCancellationTokenSource.Cancel();
				this.sendCancellationTokenSource.Dispose();
				this.sendCancellationTokenSource = null;
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Resetting cancellation token source.", ref this.debugMessages);
				#endif
			}
			if (this.client != null) {
				this.client.Dispose();
				this.client = null;
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Stopping Client.", ref this.debugMessages);
				#endif
			}
			StopSending();
			this.flagConnected = false;
		}
		#else
		////////////////////////////////////////////////////////////////////////
		// Establish Connection
		public bool Connect() {
			// Reset.
			Disconnect();
			this.client = new TcpClient();
			this.flagConnected = false;
			try {
				// Open.
				if (!this.client.ConnectAsync(this.remoteIP, this.remotePort).Wait(this.timeout)) {
					// connection failure
					#if DEBUGWARNING
					DebugUtilities.UniversalWarning(this.sourceName, "Failed to connect", ref this.debugMessages);
					#endif
					return false;
				}
				this.stream = this.client.GetStream();
				this.flagConnected = true;
				StartSending();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Connection Stablished!", ref this.debugMessages);
				#endif
				return true;
			} catch (ArgumentNullException exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "ArgumentNullException: " + exception.ToString(), ref this.debugMessages);
				#endif
				return false;
			} catch (SocketException exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "SocketException: " + exception.ToString(), ref this.debugMessages);
				#endif
				return false;
			} catch (Exception exception) {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "UnhandledException: " + exception.ToString(), ref this.debugMessages);
				#endif
				return false;
			}
		}
		// Start a connection and send given byte array.
		private void Send(byte[] sendBuffer) {
			this.flagSuccess = false;
			try {
				if (!this.client.Connected) {
					#if DEBUGWARNING
					DebugUtilities.UniversalWarning(this.sourceName, "Client Disconnected!", ref this.debugMessages);
					#endif
					return;
				}
                
				// Write.
				this.stream.Write(sendBuffer, 0, sendBuffer.Length);
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
				#endif
				this.flagSuccess = true;
			} catch (ArgumentNullException exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "ArgumentNullException: " + exception.ToString(), ref this.debugMessages);
				#endif
			} catch (SocketException exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "SocketException: " + exception.ToString(), ref this.debugMessages);
				#endif
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		// Stop Connection.
		public void Disconnect() {
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null;
			}
			if (this.stream != null) {
				this.stream.Close();
				this.stream = null; // Good Practice?
			}
			StopSending();
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Disconnected.", ref this.debugMessages);
			#endif
			this.flagConnected = false;
		}
		#endif
	}
}