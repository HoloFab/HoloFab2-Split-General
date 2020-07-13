using System;
using System.Collections.Generic;

using UnityEngine;

namespace HoloFab {
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
	}
}