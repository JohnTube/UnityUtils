namespace PolyTics.UnityUtils
{

    using UnityEngine;
    using UnityEngine.UI;

    public class ResetScrollRect : MonoBehaviour
    {

        private ScrollRect packagesScrollRect;

        [SerializeField]
        [Range(0f, 1f)]
        private float defaultPosition = 1f;

        protected void Awake()
        {
            packagesScrollRect = GetComponentInChildren<ScrollRect>();
        }

        protected void OnEnable()
        {
            packagesScrollRect.verticalNormalizedPosition = defaultPosition;
        }
    }


}
