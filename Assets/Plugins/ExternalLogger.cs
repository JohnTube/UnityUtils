namespace PolyTics.UnityUtils
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using UnityEngine;
#if UNITY_5_5_OR_NEWER
    using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif


    public class ExternalLogger : Singleton<ExternalLogger>
    {
        private string filePath = string.Empty;
        private Queue<LogEntry> logsQueue;

        [SerializeField]
        private string logglyURL;

        private const string logEntriesSeparator = "__\r\n";
        private const string logValuesSeparator = "|||";

        private static WWWForm loggingForm;

        private void Awake()
        {
            logsQueue = new Queue<LogEntry>();
            filePath = Path.Combine(Application.persistentDataPath, "temp.log");
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                string[] lines = content.Split(new[] { logEntriesSeparator }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(new[] { logValuesSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    LogEntry lE = new LogEntry();
                    if (parts.Length > 0) lE.Type = parts[0];
                    if (parts.Length > 1) lE.StackTrace = parts[1];
                    if (parts.Length > 2) lE.Message = parts[2];
                    if (parts.Length > 3) lE.BuildVersion = parts[3];
                    if (parts.Length > 4) lE.UserId = parts[4];
                    logsQueue.Enqueue(lE);
                }
            }
            loggingForm = new WWWForm();
            loggingForm.AddField("DeviceModel", SystemInfo.deviceModel);
            loggingForm.AddField("OS", SystemInfo.operatingSystem);
        }

        private void OnEnable()
        {
            // Debug.LogFormat("ExternalLogger -> OnEnable to {0}", filePath);
            Application.logMessageReceived += MessageReceived;
            Application.logMessageReceivedThreaded += MessageReceived;
            StartCoroutine("SendLogs");
        }

        IEnumerator SendLogs()
        {
            while (true)
            {
                if (InternetWatchdog.IsConnectedToInternet && logsQueue != null && logsQueue.Count > 0)
                {
                    yield return StartCoroutine(SendToServer(logsQueue.Peek()));
                }
                yield return null;
            }
        }

        LogEntry previous;

        private void MessageReceived(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                //case LogType.Warning:
                case LogType.Log:
                    break;

                case LogType.Warning:
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                if (string.IsNullOrEmpty(stackTrace)) {
                    stackTrace = GetStackTrace();
                }
#endif
                    if (previous == null || !previous.Message.Equals(condition))
                    {
                        LogEntry lE = new LogEntry();
                        lE.Message = condition.Replace(logValuesSeparator, "").Replace(logEntriesSeparator, "");
                        lE.StackTrace = stackTrace.Replace(logValuesSeparator, "").Replace(logEntriesSeparator, "");
                        lE.BuildVersion = Application.version;
                        //.Replace(logValuesSeparator, "").Replace(logEntriesSeparator, "");
                        previous = lE;
                        logsQueue.Enqueue(lE);
                    }
                    break;
            }
        }

        public void OnApplicationQuit()
        {
            //Debug.Log("ON APPLICATION QUIT SAVING EXTERNAL LOGS");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                while (logsQueue.Count > 0)
                {
                    LogEntry lE = logsQueue.Dequeue();
                    writer.WriteLine("{1}{0}{2}{0}{3}{0}{4}{0}{5}", logValuesSeparator, lE.Type, lE.Message,
                        lE.StackTrace, lE.BuildVersion, lE.UserId);
                }
            }
            //StopAllCoroutines();
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= MessageReceived;
            Application.logMessageReceivedThreaded -= MessageReceived;
            StopCoroutine("SendLogs");
        }

        private IEnumerator SendToServer(LogEntry logEntry)
        {
#if UNITY_EDITOR
            logsQueue.Dequeue();
            yield return null;
#else
            //Add log message to WWWForm
            if (!logEntry.Type.IsNullOrEmpty()) loggingForm.AddField("LEVEL", logEntry.Type);
            if (!logEntry.Message.IsNullOrEmpty()) loggingForm.AddField("Message", logEntry.Message);
            if (!logEntry.StackTrace.IsNullOrEmpty()) loggingForm.AddField("StackTrace", logEntry.StackTrace);
            if (!logEntry.BuildVersion.IsNullOrEmpty()) loggingForm.AddField("BuildVersion", logEntry.BuildVersion);
            if (!logEntry.UserId.IsNullOrEmpty()) loggingForm.AddField("PlayFabId", logEntry.UserId);
            //Add any User, Game, or Device MetaData that would be useful to finding issues later

            using (UnityWebRequest www = UnityWebRequest.Post(logglyURL, loggingForm))
            {
                yield return www.Send();
                if (!www.isError)
                {
                    logsQueue.Dequeue();
                }
            }
#endif
        }

        private string GetStackTrace()
        {
            StackTrace trace = new StackTrace(4, true);
            return trace.ToString();
        }
    }

    class LogEntry
    {
        public string Message;
        public string StackTrace;
        public string Type;
        public string UserId;
        public string BuildVersion;
    }
}