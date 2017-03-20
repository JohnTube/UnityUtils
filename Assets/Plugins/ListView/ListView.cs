namespace PolyTics.UnityUtils
{

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ListView : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler
    {

        [SerializeField] private GameObject itemPrefab;

        [SerializeField] private bool snapToChildren;

        [SerializeField] private bool clampToInitalPosition;

        [SerializeField] private float deceleration = 1.5f;

        [SerializeField] private float elasticity = 1f;

        [SerializeField] private float flingTimeThreshold = 0.35f;

        [SerializeField] private float contentCapacityCoefficient = 1.5f;

        [SerializeField] private float circularThresholdRatio = 4f;

        [SerializeField] private bool horizontal;

        private Vector2 AnchoredPosition
        {
            get { return contentRect.anchoredPosition; }
            set
            {
                if (horizontal)
                {
                    value.y = initialContentPosition.y;
                }
                else
                {
                    value.x = initialContentPosition.x;
                }
                contentRect.anchoredPosition = value;
            }
        }

        private bool routeToParent;

        private PoolManager poolManager;
        private RectTransform viewRect;

        protected List<ObservableInfo> itemsCollection;

        public List<ObservableInfo> ItemsCollection
        {
            get
            {
                if (itemsCollection == null)
                {
                    itemsCollection = new List<ObservableInfo>();
                }
                return itemsCollection;
            }
            set
            {
                itemsCollection = value;
                InstantiateList();
            }
        }

        private LayoutGroup layoutGroup;

        private float itemSize;

        public float ItemSize
        {
            get { return itemSize + spacing; }
        }

        private bool easing, dragging;

        public int MaxItemsOnScreen
        {
            get { return Mathf.Min(ItemsCount, screenCapacity); }
        }

        private int screenCapacity;
        private int firstItemIndex;

        private float itemsOut;

        [SerializeField] private RectTransform contentRect;

        public int LastItemIndex
        {
            get { return firstItemIndex + maxListSize - 1; }
        }

        public int ListCount
        {
            get { return contentRect.childCount; }
        }

        public int ItemsCount
        {
            get { return ItemsCollection.Count; }
        }

        protected int maxListSize;

        private float internalOffset;
        private Vector2 initialContentPosition;
        private float threshold;
        private float spacing;
        private Vector2 dragEndTarget;
        private float startDragTime;
        private Vector2 externalOffset;

        private Vector2 velocity;

        public Vector2 Velocity
        {
            get { return velocity; }
            private set
            {
                if (horizontal)
                {
                    value.y = 0f;
                }
                else
                {
                    value.x = 0f;
                }
                velocity = value;
            }
        }

        private Vector2 previousPosition;

        protected virtual void Awake()
        {
            poolManager = PoolManager.Instance;
            viewRect = GetComponent<RectTransform>();
            Init();
            poolManager.InitPool(itemPrefab, maxListSize);
        }

        protected virtual void OnEnable()
        {
            InstantiateList();
        }

        protected virtual void OnDisable()
        {
            Clear();
        }

        public virtual void Clear()
        {
            //ItemsCollection.Clear();
            AnchoredPosition = initialContentPosition;
            if (horizontal) layoutGroup.padding.right = 0;
            else layoutGroup.padding.top = 0;
            poolManager.DespawnChildren(itemPrefab, contentRect);
            firstItemIndex = 0;
            easing = false;
            dragging = false;
            velocity = Vector2.zero;
        }

        // TODO : Refactor and return something GO or ObservableComponent
        public virtual void AddItem(ObservableInfo newItem)
        {
            ItemsCollection.Add(newItem);
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (ListCount < maxListSize)
            {
                SpawnItem().GetComponent<ObservableComponent>().Info = newItem;
            }
        }

        public virtual void AddItemAt(ObservableInfo newItem, int position)
        {
            if (position > -1 && position < firstItemIndex)
            {
                ItemsCollection.Insert(position, newItem);
                firstItemIndex++;
            }
            else if (position < LastItemIndex)
            {
                ItemsCollection.Insert(position, newItem);
            }
            else if (position <= ItemsCount)
            {
                ItemsCollection.Insert(position, newItem);
            }
        }

        // TODO : REFACTOR TO OPTIMIZE
        public virtual void RemoveItem(ObservableInfo itemInfo)
        {
            int index = ItemsCollection.IndexOf(itemInfo);
            if (index >= 0 && index < ItemsCount)
            {
                RemoveItemAt(index);
            }
        }

        public virtual void RemoveItemAt(int index)
        {
            if (index > ItemsCount - 1)
            {
                return;
            }
            ItemsCollection.RemoveAt(index);
            // TODO : THINK ABOUT CHANGING STRATEGY AND MAKING USE OF LayoutGroups !
            if (index > firstItemIndex && index < LastItemIndex)
            {
                // remove item from screen
                for (int i = index; i < LastItemIndex && i < contentRect.childCount; i++)
                {
                    Transform itemTransform = contentRect.GetChild(i);
                    ObservableComponent item = itemTransform.GetComponent<ObservableComponent>();
                    item.Info = itemsCollection[i];
                }
            }
        }

        protected GameObject SpawnItem()
        {
            return poolManager.SpawnObject(itemPrefab, contentRect);
        }

        private void Init()
        {
            initialContentPosition = AnchoredPosition;
            float visibleAreaSize;
            if (horizontal)
            {
                visibleAreaSize = viewRect.rect.width;
                itemSize = itemPrefab.GetComponent<LayoutElement>().preferredWidth;
                HorizontalLayoutGroup layout = GetComponentInChildren<HorizontalLayoutGroup>();
                spacing = layout.spacing;
                layoutGroup = layout;
            }
            else
            {
                visibleAreaSize = viewRect.rect.height;
                itemSize = itemPrefab.GetComponent<LayoutElement>().preferredHeight;
                VerticalLayoutGroup layout = GetComponentInChildren<VerticalLayoutGroup>();
                spacing = layout.spacing;
                layoutGroup = layout;
            }
            screenCapacity = Mathf.RoundToInt(visibleAreaSize/ItemSize);
            itemsOut = Mathf.RoundToInt(screenCapacity/circularThresholdRatio);
            internalOffset = Mathf.Abs(AnchoredPosition.y - viewRect.anchoredPosition.y);
            threshold = internalOffset;
            maxListSize = Mathf.RoundToInt(contentCapacityCoefficient*screenCapacity);
            //previousPosition = AnchoredPosition;
        }

        private void InstantiateList()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            //contentRect.DestroyChildren();
            //layoutGroup.padding.top = 0; // what if initial padding was not 0 ?
            int count = Mathf.Min(maxListSize, ItemsCount);
            for (int i = 0; i < count; i++)
            {
                SpawnItem().GetComponent<ObservableComponent>().Info = ItemsCollection[i];
            }
        }

        public void OnItemOutUp()
        {
            if (LastItemIndex >= ItemsCount - 1)
            {
                //Debug.Log("CLAMP UP ?");
                return;
            }
            else
            {
                firstItemIndex++;
                //Debug.LogFormat("UP dragEndTarget={0} firstIndex={1}", dragEndTarget, firstItemIndex);
                Transform itemTransform = contentRect.GetChild(0);
                ObservableComponent item = itemTransform.GetComponent<ObservableComponent>();
                item.Info = itemsCollection[LastItemIndex];
                layoutGroup.padding.top += (int) ItemSize;
                itemTransform.SetAsLastSibling();
                threshold = (firstItemIndex + itemsOut)*ItemSize + internalOffset;
            }
        }

        public void OnItemOutDown()
        {
            if (firstItemIndex == 0)
            {
                //Debug.Log("CLAMP DOWN ?");
                return;
            }
            firstItemIndex--;
            //Debug.LogFormat("DOWN dragEndTarget={0} firstIndex={1}", dragEndTarget, firstItemIndex);
            Transform itemTransform = contentRect.GetChild(ListCount - 1);
            ObservableComponent item = itemTransform.GetComponent<ObservableComponent>();
            item.Info = itemsCollection[firstItemIndex];
            layoutGroup.padding.top -= (int) ItemSize;
            itemTransform.SetAsFirstSibling();
            threshold = (firstItemIndex + itemsOut)*ItemSize + internalOffset;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
                routeToParent = true;
            else if (horizontal && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
                routeToParent = true;
            else
                routeToParent = false;

            if (routeToParent)
            {
                DoForParents<IBeginDragHandler>((parent) => { parent.OnBeginDrag(eventData); });
                return;
            }
            easing = false;
            dragging = true;
            velocity = Vector2.zero;
            //startDragPosition = AnchoredPosition;
            startDragTime = Time.time;
            previousPosition = AnchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (routeToParent)
            {
                DoForParents<IDragHandler>((parent) => { parent.OnDrag(eventData); });
                return;
            }
            Vector3 vPosition = AnchoredPosition;
            vPosition.y += eventData.delta.y;
            AnchoredPosition = vPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (routeToParent)
            {
                DoForParents<IEndDragHandler>((parent) => { parent.OnEndDrag(eventData); });
                return;
            }
            dragEndTarget.y = AnchoredPosition.y;
            if (Time.time - startDragTime < flingTimeThreshold)
            {
                dragEndTarget.y += velocity.y/deceleration;
            }
            else
            {
                velocity = Vector2.zero;
            }

            //velocity = Vector2.zero;

            if (snapToChildren)
            {
                Snap();
            }
            Clamp();
            //Debug.LogFormat("velocity={0} dragTime={1}", velocity, dragTime);
            easing = true;
            //StartCoroutine("EasingCoroutine");
            dragging = false;
            //Debug.LogFormat("target={0} velocity={1}", dragEndTarget.y, velocity.y);
        }

        private void LateUpdate()
        {
            if (!dragging && !easing)
            {
                return;
            }

            // 1. Check if we need to move item : drag threshold exceeded + not previous state
            // 2. Get scroll/drag direction : UP or DOWN
            externalOffset = AnchoredPosition - viewRect.anchoredPosition;
            //Debug.LogFormat("distance={0} threshold={1}", externalOffset.y, threshold);
            if (externalOffset.y >
                threshold)
            {
                OnItemOutUp();
            }
            else if (externalOffset.y <
                     threshold - ItemSize)
            {
                OnItemOutDown();
            }
            float deltaTime = Time.unscaledDeltaTime;

            if (dragging)
            {
                Vector3 newVelocity = (AnchoredPosition - previousPosition)/deltaTime;
                velocity = Vector3.Lerp(velocity, newVelocity, deltaTime*10);
                previousPosition = AnchoredPosition;
            }
            else
            {
                // easing
                Vector2 position = AnchoredPosition;
                if (velocity != Vector2.zero)
                {
                    float speed = velocity.y;
                    position.y = Mathf.SmoothDamp(AnchoredPosition.y,
                        dragEndTarget.y, ref speed, elasticity, Mathf.Infinity, deltaTime);
                    velocity.y = speed;
                }
                else
                {
                    deltaTime = Time.deltaTime;
                    position.y = Utils.Spring(position.y, dragEndTarget.y, deltaTime);
                }
                AnchoredPosition = position;
            }
        }

        private void Snap()
        {
            if (!snapToChildren) return;
            float totalDistance = dragEndTarget.y - initialContentPosition.y - internalOffset;
            dragEndTarget.y = ItemSize*Mathf.RoundToInt(totalDistance/ItemSize);
        }

        private void Clamp()
        {
            float clampedPosition;
            if (clampToInitalPosition)
                clampedPosition = Mathf.Clamp(dragEndTarget.y, initialContentPosition.y,
                    initialContentPosition.y + contentRect.sizeDelta.y - ItemSize);
            else
                clampedPosition = Mathf.Clamp(dragEndTarget.y, initialContentPosition.y + internalOffset,
                    initialContentPosition.y + internalOffset + (ItemsCount - MaxItemsOnScreen)*ItemSize - spacing
                    /*float.MaxValue*/);
            if (dragEndTarget.y != clampedPosition)
            {
                velocity.y = 0f;
                dragEndTarget.y = clampedPosition;
            }
        }

        /// <summary>
        /// Do action for all parents
        /// </summary>
        private void DoForParents<TJ>(Action<TJ> action) where TJ : IEventSystemHandler
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                foreach (var component in parent.GetComponents<Component>())
                {
                    if (component is TJ)
                        action((TJ) (IEventSystemHandler) component);
                }
                parent = parent.parent;
            }
        }

        /// <summary>
        /// Always route initialize potential drag event to parents
        /// </summary>
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            DoForParents<IInitializePotentialDragHandler>((parent) => { parent.OnInitializePotentialDrag(eventData); });
        }

        private void OnTransformChildrenChanged()
        {
            Debug.Log("TRANSFORM CHILDREN CHANGED");
        }
    }
}