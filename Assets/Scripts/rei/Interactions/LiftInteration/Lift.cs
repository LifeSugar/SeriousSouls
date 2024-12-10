using System;
using UnityEngine;
using DG.Tweening;

namespace rei
{
    public class Lift : MonoBehaviour
    {
        public bool isRunning;
        public bool onTop;
        public WorldInteraction liftedInteractionDown;
        public WorldInteraction liftedInteractionUp;

        public float liftDistance; // 电梯移动的距离
        public float liftTime = 5f; // 电梯移动的时间

        public void LiftOperation()
        {
            PickableItemsManager.instance.interactions.Remove(liftedInteractionDown);
            PickableItemsManager.instance.interactions.Remove(liftedInteractionUp);
            var distance = onTop ? -liftDistance : liftDistance;
            RunTheLift(distance);
        }

        private void RunTheLift(float distance)
        {
            if (isRunning) return; // 防止重复触发
            isRunning = true; // 设置运行状态

            // 使用 DOTween 使电梯平滑移动
            transform.DOMoveY(transform.position.y + distance, liftTime)
                .SetEase(Ease.InOutSine) // 使用平滑的缓动效果
                .OnComplete(() =>
                {
                    // 移动完成后执行
                    isRunning = false;
                    onTop = !onTop; // 切换电梯位置状态
                    if (onTop)
                        PickableItemsManager.instance.interactions.Add(liftedInteractionDown);
                    else
                        PickableItemsManager.instance.interactions.Add(liftedInteractionUp);
                });

            Debug.Log("Running lift " + distance);
        }

        private void Start()
        {
            if (onTop)
                PickableItemsManager.instance.interactions.Add(liftedInteractionDown);
            else
                PickableItemsManager.instance.interactions.Add(liftedInteractionUp);
        }
    }
}