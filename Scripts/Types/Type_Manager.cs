#if UNITY_ANDROID || WINDOWS_UWP || UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;

namespace HoloFab {
	// TODO: Move to Unity submodule
	public class Type_Manager<T> : MonoBehaviour where T : UnityEngine.Object {
		// Static accessor.
		private static T _instance;
		public static T instance {
			get {
				Type managerType = typeof(T);
				if (_instance == null)
					_instance = (T)FindObjectOfType(managerType);
				return _instance;
			}
		}
		protected virtual void Awake(){
			// TODO: Check. Had a bug that instance was not getting assigned early enough.
			if (instance == null);
		}
	}
}
#endif