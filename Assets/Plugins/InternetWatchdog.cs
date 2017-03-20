namespace PolyTics.UnityUtils
{

    using System.Collections;
    using UnityEngine;
#if UNITY_5_5_OR_NEWER
    using UnityEngine.Networking;

#else
using UnityEngine.Experimental.Networking;
#endif

    [DisallowMultipleComponent]
    public class InternetWatchdog : Singleton<InternetWatchdog>
    {
        [SerializeField]
        private float CheckTimer = 1f;
        private float timer;
        private static bool? isConnectedToInternet;
        private static Coroutine coroutine;

        public delegate void OnInternetStatusChanged(bool internet);

        public static event OnInternetStatusChanged InternetStatusChanged;

        public static bool IsConnectedToInternet
        {
            get { return isConnectedToInternet != null && isConnectedToInternet.Value; }
            private set
            {
                if (isConnectedToInternet == value)
                {
                    return;
                }
                if ((!value || isConnectedToInternet != null) && InternetStatusChanged != null)
                {
                    InternetStatusChanged(value);
                }
                isConnectedToInternet = value;
            }
        }

        protected override void AwakeSingleton()
        {
            PingService();
        }

#if UNITY_EDITOR
        private void OnApplicationFocus(bool focusStatus)
        {
            OnApplicationPause(!focusStatus);
        }
#endif

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                timer = 0;
            }
        }

        private void OnApplicationQuit()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        private void PingService()
        {
            if (coroutine == null)
            {
                coroutine = StartCoroutine(Google204());
            }
        }

        //https://google.com/generate_204
        //https://connectivitycheck.gstatic.com/generate_204
        //https://www.gstatic.com/generate_204
        private IEnumerator Google204()
        {
            while (true)
            {
                using (UnityWebRequest www = UnityWebRequest.Get("https://clients3.google.com/generate_204"))
                {
                    yield return www.Send();
                    IsConnectedToInternet = !www.isError && www.responseCode == 204;
                }
                timer = CheckTimer;
                while (timer > 0f)
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
}