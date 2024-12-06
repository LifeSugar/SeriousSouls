using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class LitFireInteraction : WorldInteraction
    {
        public GameObject light;
        public GameObject campfireInteraction;
        public override void InteractActual()
        {
            StartCoroutine(LitFire());
        }

        IEnumerator LitFire()
        {
            yield return new WaitForSeconds(2.1f);
            light.SetActive(true);
            campfireInteraction.SetActive(true);
            gameObject.GetComponent<CampFire>().active = true;
        }

        private void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }
    }
}