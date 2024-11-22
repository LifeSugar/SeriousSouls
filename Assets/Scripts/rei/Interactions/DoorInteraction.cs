using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            StartCoroutine(RiseObject());
            base.InteractActual();
        }

        private IEnumerator RiseObject()
        {
            Vector3 startPosition = obj.transform.position;
            Vector3 endPosition = startPosition + new Vector3(0, riseHeight, 0);
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                // 线性插值计算当前位置
                obj.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null; // 等待下一帧
            }

            // 确保最终位置精确
            obj.transform.position = endPosition;
        }
    }

}