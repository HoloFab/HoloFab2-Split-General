#define DEBUG
#define DEBUGWARNING
// #undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using HoloFab.CustomData;

namespace HoloFab {
	// TCP agent.
	public partial class TCPAgent : NetworkAgent {
        
		// Network Objects:
		protected override string agentName {
			get {
				return "TCP Agent Interface";
			}
		}
		protected TcpClient _client;
		internal NetworkStream stream;
		protected override object client => _client;
        
		public enum AgentType {Server, Client}
		protected AgentType agentType;
		// Client Specific objects
		protected TcpListener listener;
		protected TaskInterface clientListener;
        
		public TCPAgent(object _owner, string _IP, int _port = 12121, bool _sendingEnabled = true, bool _receivingEnabled = true, string _ownerName="", AgentType _agentType = AgentType.Server) :
			                                                                                                                                                                                     base(_owner, _IP, _port, _sendingEnabled, _receivingEnabled, _ownerName) {
			this.agentType = _agentType;
		}
        
		// Delay in milliseconds to check the connection.
		internal readonly uint delayForConnection = 2000;
		// Size of buffer to try to read at once.
		internal readonly uint bufferSize = 8192;
        
		////////////////////////////////////////////////////////////////////////
		//----------------------<Connection_Management>----------------------//
		////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Connect the TCP Agent to the End-Point and Start the Sending and
		/// Receiving threads.
		/// </summary>
		/// <returns></returns>
		public override bool Connect() {
			// TODO shouldn't return cause some is async
			switch (this.agentType) {
			 case AgentType.Client:
				 return Client_Connect();
			 case AgentType.Server:
			 default:
				 return Server_Connect();
			}
		}
        
		public bool Server_Connect(){
			// Reset Connection just in case
			Disconnect();
			this._client = new TcpClient();
			try {
				// Open.
				// NB! Blocking should parallelize?
				if (!this._client.ConnectAsync(this.IP,
				                               this.port).Wait(TimeSpan.FromMilliseconds(this.delayForConnection))) {
					// connection failure
					this.flagConnected = false;
					return false;
				}
				this.stream = ((TcpClient)this.client).GetStream();
				this.flagConnected = true;
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName,
				                              "Connection Stablished!",
				                              ref this.debugMessages);
				#endif
				return true;
			} catch (Exception exception) {
				string exceptionName;
				if (exception is ArgumentNullException)
					exceptionName = "ArgumentNullException";
				else if (exception is SocketException)
					exceptionName = "SocketException";
				else exceptionName = "Exception";
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName,
				                                exceptionName + exception.ToString(),
				                                ref this.debugMessages);
				#endif
				this.flagConnected = false;
				return false;
			}
		}
        
		public bool Client_Connect(){
			IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, this.port);
			this.listener = new TcpListener(anyIP);
			this.listener.Start();
			this.clientListener = new TaskInterface(Client_WaitForConnection, _taskName: "TCP Agent Client Listener");
			this.clientListener.Start();
			return true;
		}
		public void Client_WaitForConnection(){
			if (!this.IsConnected) {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Listening for a Client!", ref this.debugMessages);
				#endif
				// Task<TcpClient> listeningSession = this.listener.AcceptTcpClientAsync();
				// if (!listeningSession.Wait(TimeSpan.FromMilliseconds(this.delayForConnection))) {
				// 	// connection failure
				// 	this.flagConnected = false;
				// } else {
				// 	this.client = listeningSession.Result;
				// 	this.stream = ((TcpClient)this.client).GetStream();
				// 	this.flagConnected = true;
				// }
				this._client = this.listener.AcceptTcpClient();
				if (this._client != null) {
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Client Found", ref this.debugMessages);
					#endif
					this.stream = ((TcpClient)this.client).GetStream();
					this.flagConnected = true;
				} else {
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Client Not Found", ref this.debugMessages);
					#endif
					// connection failure
					this.flagConnected = false;
				}
			} else {
				// Just in case? TODO: necessary?
				this.flagConnected = false;
			}
		}
        
		/// <summary>
		/// Disconnect the TCP Agent and remove the stream and client.
		/// </summary>
		public override void Disconnect() {
            
			switch (this.agentType) {
			 case AgentType.Client:
				 Client_Disconnect();
				 break;
			 case AgentType.Server:
			 default:
				 Server_Disconnect();
				 break;
			}
		}
		protected void Client_Disconnect(){
			if (this.clientListener != null) {
				this.clientListener.Stop();
				this.clientListener = null;
			}
			if (this.listener != null) {
				this.listener.Stop();
				this.listener = null;
			}
			Server_Disconnect();
		}
		protected void Server_Disconnect(){
			// Reset.
			if (this._client != null) {
				this._client.Close();
				this._client = null;
			}
			if (this.stream != null) {
				this.stream.Close();
				this.stream = null; // Good Practice? Guess not, already fucked me twice . . .
			}
			this.flagConnected = false;
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName,
			                              "Disconnected.",
			                              ref this.debugMessages);
			#endif
		}
        
		// Check if Server Disconnected.
		public override bool IsConnected {
			get {
				// TODO this is a bit ugly simplify?
				try {
					if (this._client != null && this._client.Client != null && this._client.Client.Connected) {
						/* pear to the documentation on Poll:
						 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
						 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
						 * -or- true if data is available for reading;
						 * -or- true if the connection has been closed, reset, or terminated;
						 * otherwise, returns false
						 */
                        
						// Detect if client disconnected
						if (this._client.Client.Poll(1000, SelectMode.SelectRead)) {
							byte[] buff = new byte[1];
							if (this._client.Client.Receive(buff, SocketFlags.Peek) == 0) {
								// Client disconnected
								return false;
							} else {
								return true;
							}
						}
						return true;
					} else {
						return false;
					}
				} catch {
					return false;
				}
			}
		}
	}
	# if TEST
	// TCP sender.
	public class TCPAgent {
		// An IP and a port for TCP communication to send to.
		//public string remoteIP;
		//private int remotePort;
		//public bool flagSuccess = false;
		//public bool flagConnected = false;
        
		// Network Objects:
		//#if WINDOWS_UWP
		//		private readonly string sourceName = "TCP Send Interface UWP";
		//#else
		//        private readonly string sourceName = "TCP Send Interface";
		//        // Connection Object Reference.
		//        private TcpClient client;
		//        private NetworkStream stream;
		//#endif
		//// History:
		//public List<string> debugMessages = new List<string>();
        
		// Thread Object Reference.
		//ThreadInterface sender;
		//private Thread connectionReceiver = null;
		// OR:
		private CancellationTokenSource recvCancellation;
		private CancellationTokenSource sendCancellation;
        
		// Queue of buffers for send and receive.
		private Queue<byte[]> sendQueue = new Queue<byte[]>();
		public Queue<string> dataMessages = new Queue<string>();
        
		// Size of buffer to try to read at once.
		private readonly uint bufferSize = 8192;
        
		// Flag raised when server is found
		public bool flagConnectionFound;
        
        
		/// <summary>
		/// Main constructor of the TCP Agent.
		/// </summary>
		/// <param name="remoteIP"></param>
		/// <param name="remotePort"></param>
		//public TCPAgent(string remoteIP, int remotePort = 11111)
		//{
		//    this.remoteIP = remoteIP;
		//    this.remotePort = remotePort;
		//    this.debugMessages = new List<string>();
		//    //this.sender = new ThreadInterface();
		//    //this.sender.threadAction = SendFromQueue;
		//    //Reset.
		//    //Disconnect();
		//}
        
		~TCPAgent()
		{
			Disconnect();
		}
        
		#if WINDOWS_UWP
		////////////////////////////////////////////////////////////////////////
		// Establish Connection
		public async void Connect() {}
		// Start a connection and send given byte array.
		private async void Send(byte[] sendBuffer) {}
		// Stop Connection.
		public async void Disconnect() {}
		#else
		////////////////////////////////////////////////////////////////////////
		//----------------------<Connection_Management>----------------------//
		////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Connect the TCP Agent to the End-Point and Start the Sending and
		/// Receiving threads.
		/// </summary>
		/// <returns></returns>
		public bool Connect()
		{
			Disconnect();       // Reset Connection just in case
			this.client = new TcpClient();
			try {
				// Open.
				if (!this.client.ConnectAsync(this.remoteIP,
				                              this.remotePort).Wait(2000)) {
					// connection failure
					this.flagConnected = false;
					return false;
				}
				this.stream = this.client.GetStream();
				this.flagConnected = true;
				StartSending();
				StartListenning();
				// Acknowledge.
				DebugUtilities.UniversalDebug(this.sourceName,
				                              "Connection Stablished!",
				                              ref this.debugMessages);
				return true;
			} catch (Exception exception) {
				String excName;
				if (exception is ArgumentNullException)
					excName = "ArgumentNullException";
				else if (exception is SocketException)
					excName = "SocketException";
				else excName = "Exception";
				DebugUtilities.UniversalDebug(this.sourceName,
				                              excName + exception.ToString(),
				                              ref this.debugMessages);
				this.flagConnected = false;
				return false;
			}
		}
        
		/// <summary>
		/// Disconnect the TCP Agent and remove the stream and client.
		/// </summary>
		public void Disconnect()
		{
			StopSending();
			StopListenning();
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null;
			}
			if (this.stream != null) {
				this.stream.Close();
				this.stream = null; // Good Practice? Guess not, already fucked me twice . . .
			}
			DebugUtilities.UniversalDebug(this.sourceName,
			                              "DisflagConnected.",
			                              ref this.debugMessages);
		}
        
		/// <summary>
		/// Ping connection to check if it is still alive.
		/// </summary>
		/// <returns></returns>
		public bool PingConnection()
		{
			if (this.client == null) return false;
			if (!this.client.Connected) return false;
			// This is how you can determine whether a socket is still connected.
			bool blockingState = client.Client.Blocking;
			try {
				byte[] tmp = new byte[1];
				client.Client.Blocking = false;
				client.Client.Send(tmp, 0, 0);
				return true;
			} catch (SocketException e) {
				// 10035 == WSAEWOULDBLOCK
				// https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2
                
				if (e.NativeErrorCode.Equals(10035))
					return true;
				else {
					return false;
				}
			} finally {
				client.Client.Blocking = blockingState;
			}
		}
		#endif
		////////////////////////////////////////////////////////////////////////
		//-------------------------<State_Management>-------------------------//
		////////////////////////////////////////////////////////////////////////
		private void StartSending()
		{
			// if queue not set create it.
			if (this.sendQueue == null)
				this.sendQueue = new Queue<byte[]>();
			sendCancellation = new CancellationTokenSource();
			Task tcpAgent = Task.Run(() =>
			{
				while (true) SendFromQueue();
			}, sendCancellation.Token);
		}
        
		// Disable Sending.
		public void StopSending()
		{
			// TODO: Should we reset queue? Guess we doooo . . .
			if (sendCancellation != null) sendCancellation.Cancel();
		}
        
		public void StartListenning()
		{
			recvCancellation = new CancellationTokenSource();
			Task tcpAgent = Task.Run(() =>
			{
				while (true) ReceiveData();
			}, recvCancellation.Token);
			DebugUtilities.UniversalDebug(this.sourceName,
			                              "Thread Started.",
			                              ref this.debugMessages);
		}
        
		public void StopListenning()
		{
			if (recvCancellation != null) recvCancellation.Cancel();
		}
        
        
		////////////////////////////////////////////////////////////////////////
		//--------------------------<Communications>--------------------------//
		////////////////////////////////////////////////////////////////////////
		// Start a connection and send given byte array.
		private bool Send(byte[] sendBuffer)
		{
			if (!this.client.Connected) {   // You wouldn't know until you try
				DebugUtilities.UniversalDebug(this.sourceName,
				                              "Client DisflagConnected!",
				                              ref this.debugMessages);
				this.flagSuccess = false;
				return false;
			}
			try {
				// Write.
				this.stream.Write(sendBuffer, 0, sendBuffer.Length);
				// Acknowledge.
				DebugUtilities.UniversalDebug(this.sourceName,
				                              "Data Sent!",
				                              ref this.debugMessages);
				this.flagSuccess = true;
				return true;                             // POR QUE . . . ?
			} catch (Exception exception) {
				this.flagSuccess = false;
				String excName;
				if (exception is SocketException) excName = "SocketException";
				else if (exception is ArgumentNullException)
					excName = "ArgumentNullException";
				else excName = "Exception";
				DebugUtilities.UniversalDebug(this.sourceName,
				                              excName + exception.ToString(),
				                              ref this.debugMessages);
				return false;
			}
		}
        
		private void ReceiveData()
		{
			// How necessary is this shit? . . . Why not to block until with get something?
			try {
				if (this.stream.DataAvailable) {
					OnClientFound();
					DebugUtilities.UniversalDebug(this.sourceName,
					                              "Reading Data: " + this.client.Available.ToString(),
					                              ref this.debugMessages);
				} else {
					if (PingConnection()) { // Delay Required . . .
						#if DEBUGWARNING
						DebugUtilities.UniversalWarning(this.sourceName,
						                                "No Data Available!",
						                                ref this.debugMessages);
						#endif
					} else {
						#if DEBUGWARNING
						DebugUtilities.UniversalWarning(this.sourceName,
						                                "Client Disconnected",
						                                ref this.debugMessages);
						#endif
						this.stream.Close();
						this.client.Close();
						this.client = null;
					}
				}
			} catch (Exception exception) {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName,
				                                "Exception: " + exception.ToString(),
				                                ref this.debugMessages);
				#endif
				this.stream.Close();
				this.client.Close();
				this.client = null; // ?
			}
		}
        
		////////////////////////////////////////////////////////////////////////
		//-------------------------<HELPER_FUNCTIONS>-------------------------//
		////////////////////////////////////////////////////////////////////////
		// Enqueue data for Sending.
		public void QueueUpData(byte[] newData)
		{
			lock (this.sendQueue)
			{
				this.sendQueue.Enqueue(newData);
			}
		}
        
		// Accessor to check if there is data in queue
		public bool IsNotEmpty
		{
			get {
				return this.sendQueue.Count > 0;
			}
		}
        
		// Check the queue and try send it.
		public void SendFromQueue()
		{
			try {
				if (this.IsNotEmpty) {
					byte[] currentData;
					lock (this.sendQueue)
					{
						currentData = this.sendQueue.Dequeue();
					}
					// Peek message to send
					if (!Send(currentData)) {
						// What if connection is terminated?
						lock (this.sendQueue)
						{
							//this.sendQueue.Enqueue(currentData);
						}
					}
				}
			} catch (Exception exception) {
				DebugUtilities.UniversalDebug(this.sourceName,
				                              "Queue Exception: " + exception.ToString(),
				                              ref this.debugMessages);
				this.flagSuccess = false;
			}
		}
        
		//--------------------------------------------------------------------//
        
		private void OnClientFound()
		{
			this.flagConnectionFound = true;
			byte[] buffer = new byte[bufferSize];
			string message = string.Empty;
            
			// Receive Bytes.
			int dataLength = this.stream.Read(buffer, 0, buffer.Length);
			while (dataLength != 0) {
				string data = Encoding.UTF8.GetString(buffer, 0, dataLength);
				message += data;
				if (data.Contains(EncodeUtilities.messageSplitter)) break;
				dataLength = this.stream.Read(buffer, 0, buffer.Length);
			}
			int index = message.Trim().IndexOf(EncodeUtilities.messageSplitter);
			if (index > 0)
				OnDataReceived(this,
				               new DataReceivedArgs(message.Substring(0, index)));
		}
        
		////////////////////////////////////////////////////////////////////////
		//------------------------<Data_Receive_Event>------------------------//
		////////////////////////////////////////////////////////////////////////
		public event EventHandler<DataReceivedArgs> DataReceived;
        
		private void OnDataReceived(object sender, DataReceivedArgs e)
		{
			var handler = DataReceived;
			if (handler != null) DataReceived(this, e);  // re-raise event
		}
	}
	#endif
}