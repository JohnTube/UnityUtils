namespace PolyTics.UnityUtils
{

    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;

    // modified version of http://answers.unity3d.com/questions/1040319/whats-the-proper-way-to-queue-and-space-function-c.html#answer-1070901
    public class CoroutineQueue
    {
        private readonly MonoBehaviour owner;
        private Coroutine internalCoroutine;
        private Queue<IEnumerator> actions = new Queue<IEnumerator>();
        private IEnumerator current;
        public int Count { get { return actions.Count; } }
        public bool Empty { get { return Count == 0; } }
        public bool Paused { get; set; }
        public bool Started { get; set; }
        public CoroutineQueue(MonoBehaviour aCoroutineOwner)
        {
            owner = aCoroutineOwner;
        }
        public void StartLoop()
        {
            Started = true;
            Paused = false;
            if (internalCoroutine == null)
            {
                internalCoroutine = owner.StartCoroutine(Process());
            }
            else
            {
                Debug.LogWarning("Coroutine Queue's internal coroutine is not null!");
            }
        }

        // https://issuetracker.unity3d.com/issues/coroutine-continue-failure-error-when-using-stopcoroutine
        // http://forum.unity3d.com/threads/new-added-stopcoroutine-coroutine-coroutine-override-function-causes-errors.287481/
        public void StopLoop()
        {
            Started = false;
            Paused = true;
            StopCoroutine(internalCoroutine);
            internalCoroutine = null;
        }

        public void EnqueueAction(IEnumerator aAction)
        {
            actions.Enqueue(aAction);
        }

        private IEnumerator Process()
        {
            while (true)
            {
                if (Started && actions.Count > 0)
                {
                    current = actions.Dequeue();
                    yield return ProcessCoroutine(current);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator ProcessCoroutine(IEnumerator coroutine)
        {
            if (coroutine.IsNull())
            {
                // should never happen but bette be safe than sorry
                Debug.LogError("Coroutine is null");
                yield break;
            }
            while (coroutine.MoveNext())
            {
                while (Paused)
                {
                    yield return null;
                }
                if (coroutine.Current is Coroutine)
                {
                    // we check if we still use StartCoroutine inside a queued animation Courtine somewhere?
                    Debug.LogWarningFormat("Coroutine leak? {0}", coroutine.Current);
                    yield return coroutine.Current;
                }
                else if (coroutine.Current is IEnumerator)
                {
                    yield return ProcessCoroutine(coroutine.Current as IEnumerator);
                }
                else
                {
                    yield return coroutine.Current;
                }
            }
        }

        public void Clear()
        {
            actions.Clear();
        }

        public void StopCurrentCoroutine()
        {
            StopCoroutine(current);
            current = null;
        }

        private void StopCoroutine(IEnumerator coroutine)
        {
            if (coroutine == null)
            {
                return;
            }
            try
            {
                owner.StopCoroutine(coroutine);
            }
            catch { }
        }

        private void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine == null)
            {
                return;
            }
            try
            {
                owner.StopCoroutine(coroutine);
            }
            catch { }
        }
    }

}
