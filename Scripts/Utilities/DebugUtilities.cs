using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID || WINDOWS_UWP || UNITY_EDITOR
using UnityEngine;
#endif

using System;
using HoloFab;
using HoloFab.CustomData;
using System.Diagnostics;

namespace HoloFab {
	public static class DebugUtilities {
		public enum MessageType {Normal, Warning, Error};

		public static void UserMessage(string message, MessageType messageType = MessageType.Normal){
			// Unity Debugging.
			#if UNITY_ANDROID
			AndroidUtilities.ToastMessage(message);
			#elif WINDOWS_UWP
			if (messageType == MessageType.Normal)
				Debug.Log(message);
			if (messageType == MessageType.Warning)
				Debug.LogWarning(message);
			else
				Debug.LogError(message);
			// Grasshopper Debugging
			#else
			Console.Write(message);
			#endif
		}
		[Conditional("DEBUG")]
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

		[Conditional("DEBUG")]
		public static void UniversalDebug(string source, string message, MessageType messageType = MessageType.Normal){
			DebugUtilities.UniversalDebug("[" + DateTime.Now.ToString() + "]" + source + ": " + message, messageType);
		}

		[Conditional("DEBUG")]
		public static void UniversalDebug(string source, string message, ref List<string> log, MessageType messageType = MessageType.Normal){
			message = "[" + DateTime.Now.ToString() + "]" + source + ": " + message;
			log.Add(message);
			DebugUtilities.UniversalDebug(message, messageType);
		}

		[Conditional("DEBUG")]
		public static void UniversalWarning(string source, string message){
			UniversalDebug(source, message, MessageType.Warning);
		}

		[Conditional("DEBUG")]
		public static void UniversalWarning(string source, string message, ref List<string> log){
			UniversalDebug(source, message, ref log, MessageType.Warning);
		}
	}
}