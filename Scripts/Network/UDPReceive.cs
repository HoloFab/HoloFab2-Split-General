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
	// UDP receiver.
	public class UDPReceive : UDPAgent {
		protected override string sourceName {
			get {
				return "UDP Receive Interface";
			}
		}
		public UDPReceive(object _owner, int _port = 12121) :
			                                                base(_owner, _IP: null, _port, _sendingEnabled: false, _receivingEnabled: true)
		{ }
		public override bool Connect(){
			if (!this.IsConnected) {
				try {
					this._client = new UdpClient(this.port);
					return true;
				} catch {
					;
				}
				return false;
			}
			return true;
		}
		public override void Disconnect(){
			if (this._client != null)
				this._client.Close();
		}
		////////////////////////////////////////////////////////////////////////
		#region RECEIVING
        
		protected override void ReceiveData() {
			IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
			byte[] data;
			string fullMessage = string.Empty, message;
			int startIndex = 0, endIndex = 0;
            
			try {
				// Receive Bytes.
				Connect();
				data = this._client.Receive(ref anyIP);
				//Disconnect();
				if (data.Length > 0) {
					// If buffer not empty - decode it.
					fullMessage = EncodeUtilities.DecodeData(data).Trim();
					// If string not empty and not read yet - react to it.
					if (!string.IsNullOrEmpty(fullMessage)) {
						// Raise Events
						do {
							endIndex = fullMessage
							           .IndexOf(EncodeUtilities.messageSplitter, startIndex);
							if (endIndex == -1) endIndex = fullMessage.Length;
							message = fullMessage.Substring(startIndex, endIndex-startIndex);
							RaiseDataReceived(anyIP.Address.ToString(), message);
							#if DEBUG
							DebugUtilities.UniversalDebug(this.sourceName,
							                              "Reading Data: " + message,
							                              ref this.debugMessages);
							#endif
							startIndex = endIndex+1;
						} while (startIndex < fullMessage.Length);
					}
				}
			} catch (Exception exception) {
				string exceptionName;
				if (exception is SocketException) exceptionName = "SocketException";
				else exceptionName = "Exception";
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName,
				                                "Exception: " + exceptionName + ": " + exception.ToString(),
				                                ref this.debugMessages);
				#endif
				//Disconnect();
			}
		}
		#endregion
		/*
		   private ThreadInterface receivingTask;

		   public event EventHandler<DataReceivedArgs> OnDataReceived;

		   /// <summary>
		   /// Creates a UDP Agent for handling reception of data.
		   /// </summary>
		   /// <param name="owner">The class that owns the client</param>
		   /// <param name="remoteIP">IP of the target device for sending</param>
		   /// <param name="remotePort">Port of the target device for sending</param>
		   public UDPReceive(Object owner, string remoteIP, int remotePort)
		    : base(owner, remoteIP, remotePort)
		   {
		    this.receivingTask = new ThreadInterface(ReceiveData);
		   }

		   ~UDPReceive()
		   {
		    Stop();
		   }

		   ////////////////////////////////////////////////////////////////////////
		   public void Start()
		   {
		    this.receivingTask.Start();
		   }

		   public void Stop()
		   {
		    this.receivingTask.Stop();
		   }

		   ////////////////////////////////////////////////////////////////////////
		   // Constantly check for new messages on given port.
		   private void ReceiveData()
		   {
		    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
		    byte[] data;
		    string fullMessage = string.Empty, message;
		    int startIndex = 0, endIndex = 0;

		    try
		    {
		        // Receive Bytes.
		        data = this.client.Receive(ref anyIP);
		        if (data.Length > 0)
		        {
		            // If buffer not empty - decode it.
		            fullMessage = EncodeUtilities.DecodeData(data).Trim();
		            // If string not empty and not read yet - react to it.
		            if (!string.IsNullOrEmpty(fullMessage))
		            {
		                // Raise Events
		                do {
		                    endIndex = fullMessage
		                        .IndexOf(EncodeUtilities.messageSplitter, startIndex);
		                    if (endIndex == -1) endIndex = fullMessage.Length-1;
		                    message = fullMessage.Substring(startIndex, endIndex-startIndex);
		                    if (this.OnDataReceived != null)
		                        this.OnDataReceived(this, new DataReceivedArgs(anyIP.Address.ToString(), message));
		 #if DEBUG
		                    DebugUtilities.UniversalDebug(this.sourceName,
		                        "Reading Data: " + message,
		                        ref this.debugMessages);
		 #endif
		                    startIndex = endIndex+1;
		                } while (startIndex < fullMessage.Length);
		            }
		        }
		    }
		    catch (Exception exception)
		    {
		        string exceptionName;
		        if (exception is SocketException) exceptionName = "SocketException";
		        else exceptionName = "Exception";
		 #if DEBUGWARNING
		        DebugUtilities.UniversalWarning(this.sourceName,
		            "Exception: " + exceptionName + ": " + exception.ToString(),
		            ref this.debugMessages);
		 #endif
		    }
		   }
		 */
	}
}