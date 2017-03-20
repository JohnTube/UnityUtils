namespace PolyTics.UnityUtils
{

    using UnityEngine.UI;
    using UnityEngine;
    using System;

    [RequireComponent(typeof(Toggle))]
    [DisallowMultipleComponent]
    public class BetterToggle : MonoBehaviour
    {
        protected Toggle toggle;

        public static event Action<BetterToggle> ToggleValueChanged;

        protected virtual void Start()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        protected virtual void OnToggleValueChanged(bool isOn)
        {
            if (ToggleValueChanged != null)
            {
                ToggleValueChanged(this);
            }
        }
    }


}
