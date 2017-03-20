namespace PolyTics.UnityUtils
{

    [UnityEngine.RequireComponent(typeof(UnityEngine.UI.LayoutElement))]
    public class ListItem : ObservableComponent
    {
        public ListView ParentListView;
        public int Index;
        public int Size; // height or width
    }
}