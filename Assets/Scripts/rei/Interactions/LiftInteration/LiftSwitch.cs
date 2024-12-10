using UnityEngine;

namespace rei
{
    public class LiftSwitch : WorldInteraction
    {
        public Lift lift;
        [Header("下面的那个开关把这个勾上")]
        public bool isDown; //下面开关的那个 勾上
        
        public override void InteractActual()
        {
            if (isDown == lift.onTop)
                lift.LiftOperation();
        }
    }
}