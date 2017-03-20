namespace PolyTics.UnityUtils
{
    public class ObservableBehaviour<T> : ObservableComponent where T : ObservableInfo
    {

        public T ItemInfo
        {
            get { return Info as T; }
            set { Info = value; }
        }
    }
}