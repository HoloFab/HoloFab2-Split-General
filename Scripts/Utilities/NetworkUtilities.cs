//#define DEBUG
//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
//#undef DEBUGWARNING

using System;

#if WINDOWS_UWP
using Windows.Networking.Connectivity;
using System.Linq;
#else
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
#endif

namespace HoloFab {
	// Tools for working with netwrok.
	public static class NetworkUtilities {
		// Get Local IP address.
		public static string LocalIPAddress() {
			string localIP = "0.0.0.0";
			#if WINDOWS_UWP
			ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
			localIP = NetworkInformation.GetHostNames().SingleOrDefault(hn =>
			                                                            hn.IPInformation?.NetworkAdapter != null &&
			                                                            (hn.IPInformation.NetworkAdapter.NetworkAdapterId == connectionProfile.NetworkAdapter.NetworkAdapterId)).ToString();
			#else
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()){
            	if (item.OperationalStatus == OperationalStatus.Up) {
            		foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
            			#if DEBUG2
            			DebugUtilities.UniversalDebug("Network Utility",
                            "NetworkInterfaceType: " + item.NetworkInterfaceType.ToString()
                            + ". NetworkInterfaceName: " + item.Name
                            + ". NetworkInterfaceDescription: " + item.Description
                            + ". IP: " + ip.Address.ToString()
							+ ". Family: " + ip.Address.AddressFamily.ToString());
            			#endif
            			if ((item.NetworkInterfaceType == NetworkInterfaceType.Ethernet
							|| item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
							|| (item.NetworkInterfaceType == 0 && item.Name.ToLower().Contains("wlan")))
                            && ip.Address.AddressFamily == AddressFamily.InterNetwork) {
            				localIP = ip.Address.ToString();
            			}
            		}
            	}
            }
			#endif
			#if DEBUG
            DebugUtilities.UniversalDebug("Network Utility", "Selected Local IP: " + localIP);
            #endif
			return localIP;
		}
        
		public static string BroadcastIP(){
			#if WINDOWS_UWP
			return string.Empty();
			#else
			return IPAddress.Broadcast.ToString();
			#endif
		}
	}
}