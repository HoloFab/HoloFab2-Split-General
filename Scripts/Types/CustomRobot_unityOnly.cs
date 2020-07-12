using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab {
	// Structure to hold Custom data types holding data to be sent.
	namespace CustomData {
		[Serializable]
		public class UnityRobot {
			public string name;
			public string tag;
			public GameObject goExample;
		}
	}
}