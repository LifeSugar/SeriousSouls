using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class RestIntreraction : WorldInteraction
    {
        void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }
        public override void InteractActual()
        {
            InputHandler.instance.a_input = false;
            InputHandler.instance.HandleCampFireCanvas();
        }
    }
}