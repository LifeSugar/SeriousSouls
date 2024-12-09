using System;
using UnityEngine;
using DG.Tweening;

namespace rei
{
    public class LiftTrigger : MonoBehaviour
    {
        public Lift lift;
        public float moveDistance = 0.4f;

        private bool moved;
        public bool holding;

        private Vector3 originalPosition;
        private Vector3 targetPosition;

        private void Start()
        {
            originalPosition = transform.localPosition;
            targetPosition = originalPosition - new Vector3(0, moveDistance, 0);
        }

        void Update()
        {
            if (!lift.isRunning && !holding && moved)
            {
                moved = false;
                // 玩家离开触发器后向上移动
                transform.DOLocalMove(originalPosition, 0.5f)
                    .OnComplete(() =>
                    {
                        moved = false; // 标记移动结束
                    });
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var player = other.transform.GetComponent<PlayerState>();
            if (player != null)
            {
                holding = true; // 玩家进入触发器

                if (!moved && !lift.isRunning)
                {
                    // 玩家进入触发器后向下移动
                    transform.DOLocalMove(targetPosition, 0.5f)
                        .OnComplete(() =>
                        {
                            // 移动完成后调用 LiftOperation
                            if (!lift.isRunning)
                            {
                                lift.LiftOperation();
                            }
                        });

                    moved = true; // 标记为已移动
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.transform.GetComponent<PlayerState>())
            {
                holding = false; // 玩家离开触发器
            }
        }
    }
}