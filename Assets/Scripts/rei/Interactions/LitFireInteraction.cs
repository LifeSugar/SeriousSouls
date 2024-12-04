using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class LitFireInteraction : WorldInteraction
    {
        public GameObject light;
        public GameObject campfire;
        public override void InteractActual()
        {
            StartCoroutine(LitFire());
        }

        IEnumerator LitFire()
        {
            yield return new WaitForSeconds(0.5f);
            light.SetActive(true);
            campfire.SetActive(true);
            
        }

        private void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }
    }
}