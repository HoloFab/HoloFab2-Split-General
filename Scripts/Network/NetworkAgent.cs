﻿#define DEBUG
// #define DEBUG2
#define DEBUGWARNING
// #undef DEBUG
#undef DEBUG2
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using HoloFab.CustomData;

namespace HoloFab {
	public class NetworkAgent {
		// Network Objects:
		protected virtual string sourceName {
			get {
				return "Network Agent Interface";
			}
		}
		protected virtual object client {
			get {
				return null;
			}
			set { }
		}
		public object owner;
        
		// An IP and a port for UDP communication to send to.
		public string IP;
		public int port;
        
		public bool flagSuccess = false;
		public bool flagConnected = false;
        
		public bool sendingEnabled { get; private set; }
		public bool receivingEnabled { get; private set; }
        
		public List<string> debugMessages = new List<string>();
        
		/// <summary>
		/// Creates a Network Agent for handling the sending and/or receiving data.
		/// </summary>
		/// <param name="owner">The class that owns the client</param>
		/// <param name="IP">IP of the target device</param>
		/// <param name="port">Port of the target device</param>
		public NetworkAgent(object _owner, string _IP = null, int _port = 0, bool _sendingEnabled = false, bool _receivingEnabled=false) {
			this.owner = _owner;
			this.IP = _IP;
			this.port = _port;
            
			this.debugMessages = new List<string>();
            
			this.flagConnected = false;
			this.flagSuccess = false;
            
			this.sendingEnabled = _sendingEnabled;
			this.receivingEnabled = _receivingEnabled;
            
			InitializeSending();
			InitializeReceiving();
		}
        
		~NetworkAgent() {
			this.flagConnected = false;
			this.flagSuccess = false;
			StopSending();
			StopReceiving();
			Disconnect();
		}
        
		////////////////////////////////////////////////////////////////////////
		public virtual bool Connect() {
			return false;
		}
        
		public virtual void Disconnect() { }
        
		public virtual bool IsConnected {
			get {
				return this.client != null;
			}
		}
		////////////////////////////////////////////////////////////////////////
		#region SENDING
        
		protected TaskInterface sendingTask;
		// Queue of buffers to send.
		protected Queue<byte[]> sendQueue = new Queue<byte[]>();
		// Accessor to check if there is data in queue
		public bool IsNotEmpty {
			get	{
				bool status = false;
				if (this.sendQueue != null)
					lock (this.sendQueue) { 
						status = this.sendQueue.Count > 0; 
					}
				return status;
			}
		}
        
		protected void InitializeSending() {
			if (this.sendingEnabled)
				this.sendingTask = new TaskInterface(SendFromQueue, _taskName: this.sourceName+":Sender");
		}
		public virtual void StartSending() {
			if (this.sendingEnabled) {
				// if queue not set create it.
				if (this.sendQueue == null)
					this.sendQueue = new Queue<byte[]>();
				this.sendingTask.Start();
			}
		}
		public virtual void StopSending() {
			if (this.sendingEnabled) {
				this.sendingTask.Stop();
				this.sendQueue = null;
			}
		}
		// Enqueue data.
		public void QueueUpData(byte[] newData) {
			if (this.sendingEnabled)
				if (this.sendQueue != null)
					lock (this.sendQueue) {
						this.sendQueue.Enqueue(newData);
					}
		}
		// Check the queue and try send it.
		private void SendFromQueue() {
			try {
				if (this.IsNotEmpty) {
					byte[] currentData;
					lock (this.sendQueue) {
						currentData = this.sendQueue.Dequeue();
					}
					// Try to send a message
					SendData(currentData);
				}
			} catch (Exception exception) {
				DebugUtilities.UniversalDebug(this.sourceName,
				                              "Send from Queue Exception: " + exception.ToString(),
				                              ref this.debugMessages);
			}
		}
		protected virtual void SendData(byte[] data) { }
		#endregion
		////////////////////////////////////////////////////////////////////////
		#region RECEIVING
        
		protected TaskInterface receivingTask;
        
		public event EventHandler<DataReceivedArgs> OnDataReceived;
        
		protected void InitializeReceiving() {
			if (this.receivingEnabled)
				this.receivingTask = new TaskInterface(ReceiveData, _taskName: this.sourceName+":Receiver");
		}
		public virtual void StartReceiving() {
			if (this.receivingEnabled)
				this.receivingTask.Start();
		}
		public virtual void StopReceiving() {
			if (this.receivingEnabled)
				this.receivingTask.Stop();
		}
		protected virtual void ReceiveData(){ }
        
		protected void RaiseDataReceived(string source, string data) {
			var temp = OnDataReceived;
			if (temp != null)
				temp(this, new DataReceivedArgs(source, data));
		}
		#endregion
	}
}