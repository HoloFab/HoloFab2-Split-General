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
	public class UDPSend : UDPAgent {
		protected override string sourceName {
			get {
				return "UDP Send Interface";
			}
		}
		public UDPSend(object _owner, string _IP, int _port = 12121) :
			                                                         base(_owner, _IP: _IP, _port: _port, _sendingEnabled: true) {}
		public override bool Connect(){
			if (!this.IsConnected) {
				try {
					this._client = new UdpClient();
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
			//// Reset.
			//if (client != null) {
			//    client.Close();
			//    client = null; // Good Practice?
			//}
			try {
				Connect();
				this._client.Send(data, data.Length, this.IP, this.port);
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
			}
			// if failed - queue up again
			QueueUpData(data);
		}
		#endregion
		/*
		   // Queue of buffers to send.
		   private Queue<byte[]> sendQueue = new Queue<byte[]>();
		   // Accessor to check if there is data in queue
		   public bool IsNotEmpty { get { return this.sendQueue.Count > 0; } }

		   private ThreadInterface sendingTask;

		   /// <summary>
		   /// Creates a UDP Agent for handling the sending and/or receiving data.
		   /// </summary>
		   /// <param name="owner">The class that owns the client</param>
		   /// <param name="remoteIP">IP of the target device for sending</param>
		   /// <param name="remotePort">Port of the target device for sending</param>
		   public UDPSend(Object owner, string remoteIP, int remotePort)
		    : base(owner, remoteIP, remotePort) {
		    this.sendingTask = new ThreadInterface(SendFromQueue);
		   }

		   ~UDPSend()
		   {
		    Stop();
		   }

		   ////////////////////////////////////////////////////////////////////////
		   // Queue Functions.
		   // Start the thread to send data.
		   public void Start()
		   {
		    // if queue not set create it.
		    if (this.sendQueue == null)
		        this.sendQueue = new Queue<byte[]>();
		    this.sendingTask.Start();
		   }

		   // Disable Sending.
		   public void Stop()
		   {
		    this.sendingTask.Stop();
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
		            // Try to send a message to send
		            Send(currentData);
		        }
		    }
		    catch (Exception exception)
		    {
		        DebugUtilities.UniversalDebug(this.sourceName,
		            "Queue Exception: " + exception.ToString(),
		            ref this.debugMessages);
		    }
		   }
		   ////////////////////////////////////////////////////////////////////////
		   // Start a connection and send given byte array.
		   private void Send(byte[] sendBuffer)
		   {
		    SendUniversal("Data", this.remoteIP, sendBuffer);
		    //client = new UdpClient(this.remoteIP, this.remotePort);
		   }

		   // Broadcast a Message to everyone.
		   public void Broadcast(byte[] sendBuffer)
		   {
		    SendUniversal("Broadcast", IPAddress.Broadcast.ToString(), sendBuffer);
		    //client = new UdpClient(new IPEndPoint(IPAddress.Broadcast, this.remotePort));
		   }
		   // Send a Message to everyone.
		   public void SendUniversal(string sourceType, string ip, byte[] sendBuffer)
		   {
		    this.flagSuccess = false;
		    //// Reset.
		    //if (client != null)
		    //{
		    //    client.Close();
		    //    client = null; // Good Practice?
		    //}
		    try
		    {
		        this.client = new UdpClient(ip, this.remotePort);
		        this.client.Send(sendBuffer, sendBuffer.Length);
		        this.client.Close();
		 #if DEBUG
		        DebugUtilities.UniversalDebug(this.sourceName,
		            sourceType + " Sent!",
		            ref this.debugMessages);
		 #endif
		        this.flagSuccess = true;
		        return;
		    }
		    catch (Exception exception)
		    {
		        string exceptionName = "OtherException";
		 #if DEBUGWARNING
		        DebugUtilities.UniversalWarning(this.sourceName,
		            "Exception: " + sourceType + ": " + exceptionName +  ": " + exception.ToString(),
		            ref this.debugMessages);
		 #endif
		    }
		    QueueUpData(sendBuffer);
		   }
		 */
	}
}