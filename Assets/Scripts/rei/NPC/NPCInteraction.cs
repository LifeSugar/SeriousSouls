using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class NPCInteraction : WorldInteraction
    {
        public string npcId;

        public override void InteractActual()
        {
            if (Input.GetButtonDown(GlobalStrings.A))
            {
                DialogueManager.instance.InitDialogue(this.transform, npcId);
            }
            
        }
    }
}