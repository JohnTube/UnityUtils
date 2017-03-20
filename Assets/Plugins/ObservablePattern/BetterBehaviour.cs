namespace PolyTics.UnityUtils
{
    using UnityEngine;

    public class BetterBehaviour : MonoBehaviour
    {

        protected bool initialized;

        // Use this for initialization
        protected virtual void Start()
        {
            initialized = true;
        }

        private void OnEnable()
        {
            if (initialized)
            {
                OnEnableSafe();
            }
            else
            {
                OnFirstEnable();
            }
        }

        protected virtual void OnEnableSafe() { }

        protected virtual void OnFirstEnable() { }
    }
}
