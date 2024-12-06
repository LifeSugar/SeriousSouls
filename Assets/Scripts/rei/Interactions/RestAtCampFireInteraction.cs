using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class RestAtCampFireIntreraction : WorldInteraction
    {
        public GameObject camera;
        void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }
        public override void InteractActual()
        {
            camera.SetActive(true);
            InputHandler.instance.a_input = false;
            InputHandler.instance.HandleCampFireCanvas();
            gameObject.GetComponentInParent<CampFire>().sitting = true;
            CampFireUIHandler.instance.UpdateCampFireUI();
            CampFireManager.instance.lastCampFire = this.GetComponentInParent<CampFire>();
        }
    }
}