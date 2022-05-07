#define DEBUG
// #define DEBUG2
#define DEBUGWARNING
// #undef DEBUG
#undef DEBUG2
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using HoloFab.CustomData;

namespace HoloFab {
    public partial class TCPAgent : NetworkAgent {
        ////////////////////////////////////////////////////////////////////////
        #region SENDING
        protected override void SendData(byte[] data) {
            this.flagSuccess = false;
            try
            {
                // Write.
                this.stream.Write(data, 0, data.Length);
                // Acknowledge.
                #if DEBUG
                DebugUtilities.UniversalDebug(this.sourceName,
                    "Data Sent!",
                    ref this.debugMessages);
                #endif
                this.flagSuccess = true;
                return;
            }
            catch (Exception exception)  {
                string exceptionName = "OtherException";
                if (exception is SocketException) exceptionName = "SocketException";
                else if (exception is ArgumentNullException)
                    exceptionName = "ArgumentNullException";
                #if DEBUGWARNING
                DebugUtilities.UniversalWarning(this.sourceName,
                    "Sending Exception: " + exceptionName + ": " + exception.ToString(),
                    ref this.debugMessages);
                #endif
            }
            // if failed - queue up again
            QueueUpData(data);
        }
        #endregion
    }
}