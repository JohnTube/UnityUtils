using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool {

    //public int MaxSize = 10;
    //public int InitialSize;
    public GameObject ObjectToClone;

    public Stack Clones = new Stack();

    internal GameObject GetObject() {
        if (Clones.Count > 0) {
            return Clones.Pop() as GameObject;
        }
        else {
            return Object.Instantiate(ObjectToClone) as GameObject;
        }
    }

    internal void StoreObject(GameObject objectToStore) {
        Clones.Push(objectToStore);
    }
}