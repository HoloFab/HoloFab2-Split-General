//#define DEBUG
#define DEBUGWARNING
#undef DEBUG
//#undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID || WINDOWS_UWP || UNITY_EDITOR
using UnityEngine;
#endif

using System;
using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	public static class DebugUtilities {
		public enum MessageType {Normal, Warning, Error};
        
		public static void UserMessage(string message, MessageType messageType = MessageType.Normal){
			// Unity Debugging.
			#if WINDOWS_UWP || UNITY_EDITOR || UNITY_ANDROID
			if (messageType == MessageType.Normal)
				Debug.Log(message);
			else if (messageType == MessageType.Warning)
				Debug.LogWarning(message);
			else
				Debug.LogError(message);
			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidUtilities.ToastMessage(message);
			#endif
			#else // Grasshopper Debugging
			Console.Write(message);
			#endif
		}
        
		private static void UniversalDebug(string message, MessageType messageType = MessageType.Normal){
			// Unity Debugging.
			#if UNITY_ANDROID || WINDOWS_UWP
			// AndroidUtilities.ToastMessage(message);
			// #elif WINDOWS_UWP
			if (messageType == MessageType.Normal)
				Debug.Log(message);
			else if (messageType == MessageType.Warning)
				Debug.LogWarning(message);
			else
				Debug.LogError(message);
			// Grasshopper Debugging
			#else
			Console.Write(message);
			#endif
		}
        
		public static void UniversalDebug(string source, string message, MessageType messageType = MessageType.Normal){
			DebugUtilities.UniversalDebug("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + source + ": " + message, messageType);
		}
        
		public static void UniversalDebug(string source, string message, ref List<string> log, MessageType messageType = MessageType.Normal){
			message = "[" + DateTime.Now.ToString("HH:mm:ss") + "]" + source + ": " + message;
			log.Add(message);
			DebugUtilities.UniversalDebug(message, messageType);
		}
        
		public static void UniversalWarning(string source, string message){
			UniversalDebug(source, message, MessageType.Warning);
		}
        
		public static void UniversalWarning(string source, string message, ref List<string> log){
			UniversalDebug(source, message, ref log, MessageType.Warning);
		}
	}
}