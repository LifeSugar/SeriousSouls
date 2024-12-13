using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class NPCInteraction : WorldInteraction
    {
        public string npcId;

        private void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }

        public override void InteractActual()
        {
            if (Input.GetButtonDown(GlobalStrings.A))
            {
                DialogueManager.instance.InitDialogue(this.transform, npcId);
            }
            
        }
    }
}