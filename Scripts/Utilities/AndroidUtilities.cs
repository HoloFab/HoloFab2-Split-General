#if UNITY_ANDROID
using UnityEngine;

namespace HoloFab {
    public static class AndroidUtilities {
		public static void ToastMessage(string message) {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); // Shouldn't this be Holofab?
			AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
			if (unityActivity != null) {
				AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
				unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
					AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText",
					                                                                         unityActivity,
					                                                                         message, 0);
					toastObject.Call("show");
				}));
			}
		}
	}
}
#endif