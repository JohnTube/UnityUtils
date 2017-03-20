namespace PolyTics.UnityUtils
{

    using System.Collections.Generic;
    using UnityEngine;

    public class PoolManager : Singleton<PoolManager>
    {

        private Dictionary<int, Pool> pools = new Dictionary<int, Pool>();

        [SerializeField]
        private Transform poolContainer;

        protected override void AwakeSingleton()
        {
            if (poolContainer != null)
            {
                poolContainer.gameObject.SetActiveSafe(false);
            }
        }

        public void InitPool(GameObject original, int initialSize)
        {
            CheckPoolExistence(original);
            for (int i = 0; i < initialSize; i++)
            {
                GameObject clone = Instantiate(original);
                clone.transform.SetParent(poolContainer, false);
                //clone.SetActiveSafe(false);
                pools[original.GetInstanceID()].StoreObject(clone);
            }
        }

        public GameObject SpawnObject(GameObject type, Transform parent)
        {
            CheckPoolExistence(type);
            GameObject objectToSpawn = pools[type.GetInstanceID()].GetObject();
            //objectToSpawn.SetActiveSafe(true);
            objectToSpawn.transform.SetParent(parent, false);
            return objectToSpawn;
        }

        public void DespawnObject(GameObject original, GameObject objectToKill)
        {
            if (ApplicationIsQuitting)
            {
                //Destroy(objectToKill);
                return;
            }
            objectToKill.transform.SetParent(poolContainer, false);
            //objectToKill.SetActiveSafe(false);
            CheckPoolExistence(original);
            pools[original.GetInstanceID()].StoreObject(objectToKill);
        }

        public void DespawnChildren(GameObject original, Transform parent)
        {
            if (ApplicationIsQuitting)
            {
                return;
            }
            while (parent.childCount > 0)
            {
                DespawnObject(original, parent.GetChild(0).gameObject);
            }
        }

        private void CheckPoolExistence(GameObject original)
        {
            if (!pools.ContainsKey(original.GetInstanceID()))
            {
                Pool pool = new Pool();
                //pool.MaxSize = maxSize;
                pool.ObjectToClone = original;
                //pool.InitialSize = initialSize;
                pools.Add(original.GetInstanceID(), pool);
            }
        }
    }
}