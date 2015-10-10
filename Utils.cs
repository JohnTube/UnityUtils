using UnityEngine;

namespace JohnTube.UnityUtils {

    // TODO: add XML comments
    public class Utils {

        public static void Swap<T>(ref T a, ref T b) {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public static string Format(string format, params object[] objects) {
            return objects.Length > 0 ? string.Format(format, objects) : format;
        }

        public static string GetDeviceId() {
#if !UNITY_EDITOR && UNITY_ANDROID
                //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
                using (AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")){
                    using (AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity")){
                        using (AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver")){
                            using (AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure")){
                                return clsSecure.CallStatic<string>("getString", objResolver, "android_id");
                            }
                        }
                    }
                }
#elif UNITY_IPHONE
                    return UnityEngine.iOS.Device.vendorIdentifier;
#else
            return string.Empty;
#endif
        }
    }
}