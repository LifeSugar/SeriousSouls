using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class CubeInteraction : MonoBehaviour
    {
        public bool riseSignal;
        void Update()
        {
            if (riseSignal)
                Rise();

        }

        void Rise()
        {
            //logic
            riseSignal = false;
            
        }
        
        
    }

}