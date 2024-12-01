using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class LiftInteraction : MonoBehaviour
    {
        public bool isRunning;
        public bool isUpper;
        void OnTriggerEnter(Collider other)
        {
            var player = other.transform.GetComponent<PlayerState>();
            if (player != null && !isRunning)
            {
                var distance = isUpper ? -10f : 10f;
                RunTheLift(distance);
            }
            
        }

        private void RunTheLift(float distance)
        {
            
        }
        
    }
}