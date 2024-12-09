using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace rei
{
    public class CampFire : MonoBehaviour
    {
        public string name;
        public bool active;
        public bool sitting;
        public PlayerStateInfo playerStateInfo;

        void Start()
        {
            CampFireUIHandler.instance.OnTeleport += TeleportPlayer;
        }

        void TeleportPlayer(string targetCampFireName)
        {
            if (targetCampFireName != name)
                return;
            
            InputHandler.instance.transform.position = playerStateInfo.position;
            InputHandler.instance.HandleCampFireCanvas();
        }
    }
}