using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class WorldInteraction : MonoBehaviour
    {
        public string interactionId;
        public UIActionType actionType;

        public virtual void InteractActual()
        {
            
        }
    }
}