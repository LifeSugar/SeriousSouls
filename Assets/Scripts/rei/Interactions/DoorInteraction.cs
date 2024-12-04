using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace rei
{
    public class DoorInteraction : WorldInteraction
    {
        public GameObject obj;
        public float riseHeight = 1.0f; // 上升的高度
        public float duration = 1.0f;    // 上升所用时间

        public override void InteractActual()
        {
            obj.SetActive(true);
            RiseObjectWithDOTween();
            base.InteractActual();
        }

        private void RiseObjectWithDOTween()
        {
            Vector3 endPosition = obj.transform.position + new Vector3(0, riseHeight, 0);
            obj.transform.DOMove(endPosition, duration).SetEase(Ease.Linear);
        }
    }

}