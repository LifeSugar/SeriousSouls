using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class PickableItem : MonoBehaviour
    {
        public PickItemContainer[] items;

        void Start()
        {
            PickableItemsManager.instance.pick_items.Add(this);
        }

        private void OnDestroy()
        {
            PickableItemsManager.instance.pick_items.Remove(this);
        }
    }

    [System.Serializable]
    public class PickItemContainer {
        public string itemId;
        public ItemType itemType;
    }
}