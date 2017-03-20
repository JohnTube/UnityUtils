using UnityEngine;

namespace PolyTics.UnityUtils
{

    using System;
    using System.Reflection;


    // TODO: add XML comments
    public class Utils
    {

        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public static string Format(string format, params object[] objects)
        {
            return objects.Length > 0 ? string.Format(format, objects) : format;
        }

        public static string GetDeviceId()
        {
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

        // http://stackoverflow.com/a/10261848/1449056
        public static string GetConstantNameFromValue(Type type, object val)
        {
            FieldInfo[] fieldInfos = type.GetFields(
            // Gets all public and static fields

            BindingFlags.Public | BindingFlags.Static |
            // This tells it to get the fields from all base types as well

            BindingFlags.FlattenHierarchy);
            // Go through the list and only pick out the constants
            foreach (FieldInfo fi in fieldInfos)
            {
                // remove deprecated / obsolete fields/properties
                if (fi.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length != 0)
                {
                    continue;
                }
                // IsLiteral determines if its value is written at
                //   compile time and not changeable
                // IsInitOnly determine if the field can be set
                //   in the body of the constructor
                // for C# a field which is readonly keyword would have both true
                //   but a const field would have only IsLiteral equal to true
                if (fi.IsLiteral && !fi.IsInitOnly)
                {
                    object value = fi.GetRawConstantValue();
                    //Console.WriteLine("{0}={1}", fi.Name, value);
                    if (value.Equals(val))
                    {
                        return fi.Name;
                    }
                }
            }
            return val.ToString();
        }

        public static float Spring(float start, float end, float t)
        {
            t = Mathf.Clamp01(t);
            t = (Mathf.Sin(t * Mathf.PI * (.2f + 2.5f * t * t * t)) * Mathf.Pow(1f - t, 2.2f) + t) * (1f + (1.2f * (1f - t)));
            return start + (end - start) * t;
        }
    }
}