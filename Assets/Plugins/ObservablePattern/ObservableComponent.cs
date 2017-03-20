namespace PolyTics.UnityUtils
{
    using System.ComponentModel;
    using UnityEngine;

    public class ObservableComponent : BetterBehaviour
    {
        [SerializeField]
        private ObservableInfo info;
        [SerializeField]
        protected bool observeWhenDisabled;
        [SerializeField]
        protected bool refreshOnEnable;
        private bool subscribed;

        public ObservableInfo Info
        {
            get { return info; }
            set
            {
                //Debug.LogFormat("{0} Info.Setter value={1}", name, value.Stringify());
                if (info != null && info.Equals(value))
                {
                    return;
                }
                UnsubscribeToChanges();
                info = value;
                if (value == null)
                {
                    return;
                }
                if (observeWhenDisabled || gameObject.activeInHierarchy)
                {
                    SubscribeToChanges();
                    SetDefaultUI();
                }
            }
        }

        protected override void OnFirstEnable()
        {
            if (Info != null && !subscribed)
            {
                SubscribeToChanges();
                SetDefaultUI();
            }
        }

        protected override void OnEnableSafe()
        {
            //Debug.LogFormat("{0} OnEnable Info={1}", name, Info.Stringify());
            if (!observeWhenDisabled)
            {
                SubscribeToChanges();
            }
            if (Info != null && refreshOnEnable)
            {
                RefreshUI();
            }
        }

        protected void SubscribeToChanges()
        {
            if (Info != null && !subscribed)
            {
                subscribed = true;
                Info.PropertyChanged += UpdateUI;
            }
        }

        protected void UnsubscribeToChanges()
        {
            if (Info != null && subscribed)
            {
                subscribed = false;
                Info.PropertyChanged -= UpdateUI;
            }
        }

        private void OnDisable()
        {
            if (!observeWhenDisabled)
            {
                UnsubscribeToChanges();
            }
        }

        protected void OnDestroy()
        {
            UnsubscribeToChanges();
        }

        protected virtual void UpdateUI(object sender, PropertyChangedEventArgs args)
        {
            //Debug.Log("base" + args.PropertyName);
        }

        protected virtual void SetDefaultUI()
        {
        }

        protected virtual void RefreshUI() { }
    }
}