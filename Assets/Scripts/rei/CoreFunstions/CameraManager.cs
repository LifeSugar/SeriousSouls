using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class CameraManager : MonoBehaviour
    {
        [Header("States")] public bool lockOn;

        [Header("Targets and Lock")] public Transform followTarget; //跟随目标
        public Transform lockOnTransform; //锁定的目标
        public EnemyTarget lockOnTarget;

        [Header("Stats")] public float followSpeed = 9; // 相机跟随目标的速度
        public float mouseSpeed = 2; // 鼠标控制相机旋转的速度
        public float turnSmoothing = .1f; // 相机平滑旋转的系数
        public float minAngle = -35; // 垂直旋转的最小角度
        public float maxAngle = 35; // 垂直旋转的最大角度
        public float defaultDistance;
        public Vector3 offset = new Vector3(0, 1.3f, 0);
        public float lockOffset = 0.5f;


        [Header("MoveStat")] public Vector3 targetDir; // 目标方向向量
        public float lookAngle; // 水平旋转角度
        public float tiltAngle; // 垂直旋转角度

        [HideInInspector] public Transform pivot; // 相机的旋转轴
        [HideInInspector] public Transform camTrans; // 相机的Transform
        StateManager states; // 管理相机状态的对象


        float smoothX;
        float smoothY;
        float smoothXvelocity;
        float smoothYvelocity;


        bool usedRightAxis;

        bool changeTargetLeft;
        bool changeTargetRight;

        private float savedLookAngle; // 保存的水平角度
        private float savedTiltAngle; // 保存的垂直角度
        private bool wasLockedOn = false; // 跟踪上一帧是否处于锁定状态

        public void Init(StateManager st)
        {
            states = st;
            followTarget = st.transform; // 将目标设置为StateManager的Transform
            camTrans = Camera.main.transform; // 获取主相机的Transform
            pivot = camTrans.parent; // 设置pivot为相机的父对象
            defaultDistance = new Vector3(0, offset.y, 0).magnitude;
            pivot.localPosition = offset;
        }


        public void Tick(float d)
        {
            if (lockOnTarget == null)
            {
                // 检测是否从锁定状态切换到普通状态
                if (wasLockedOn)
                {
                    // 初始化 lookAngle 和 tiltAngle，防止切换后突变
                    lookAngle = transform.eulerAngles.y;
                    tiltAngle = pivot.localRotation.eulerAngles.x;

                    // 修正 tiltAngle 范围 (Euler 角度修正)
                    if (tiltAngle > 180)
                        tiltAngle -= 360;

                    wasLockedOn = false; // 更新为非锁定状态
                }

                float h = Input.GetAxis(GlobalStrings.RightHorizontal) + Input.GetAxis("mouseX"); // 获取水平输入
                float v = Input.GetAxis(GlobalStrings.RightVertical) + Input.GetAxis("mouseY"); // 获取垂直输入
                float targetSpeed = mouseSpeed; // 设定相机移动速度
                HandleRotations(d, v, h, targetSpeed); // 调用旋转处理方法
            }
            else
            {
                changeTargetLeft = Input.GetKeyUp(KeyCode.V); // 检测左切换目标的按键
                changeTargetRight = Input.GetKeyUp(KeyCode.B); // 检测右切换目标的按键

                if (lockOnTransform == null)
                {
                    lockOnTransform = lockOnTarget.GetTarget();
                    states.lockOnTransform = lockOnTransform;
                }

                if (changeTargetLeft || changeTargetRight)
                {
                    lockOnTransform = lockOnTarget.GetTarget(changeTargetLeft);
                    states.lockOnTransform = lockOnTransform;
                }

                // 计算从 followTarget 到 lockOnTransform 的方向向量，并加上y轴的偏移
                Vector3 follow = followTarget.position + new Vector3(0, lockOffset, 0);
                Vector3 direction = lockOnTransform.position - follow;
                direction.y = 0; // 忽略 Y 轴，确保水平旋转只影响 transform.rotation.y
                direction.Normalize();

                // 计算水平旋转角度
                float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; // 水平方向的目标角度
                Quaternion targetYawRotation = Quaternion.Euler(0, targetYaw, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetYawRotation, d * followSpeed);

                // 计算垂直旋转角度
                Vector3 localDirection = lockOnTransform.position - camTrans.position; // 相机与目标的相对方向
                float targetPitch =
                    Mathf.Atan2(localDirection.y, new Vector3(localDirection.x, 0, localDirection.z).magnitude) *
                    Mathf.Rad2Deg; // 计算垂直角度
                targetPitch = Mathf.Clamp(targetPitch, minAngle, maxAngle); // 限制垂直角度范围
                pivot.localRotation = Quaternion.Slerp(pivot.localRotation, Quaternion.Euler(-targetPitch, 0, 0),
                    d * followSpeed);

                // 保存当前角度
                savedLookAngle = lookAngle;
                savedTiltAngle = tiltAngle;

                wasLockedOn = true; // 更新为锁定状态
            }

            FollowTarget(d); // 调用跟随目标方法
        }

        public void FixedTick(float d)
        {
            HandleCameraCollision(d);
        }

        //使用线性插值来平滑跟随目标位置。
        void FollowTarget(float d)
        {
            float speed = d * followSpeed;
            //delay follow
            Vector3 camPosition = Vector3.Lerp(transform.position, followTarget.position, speed);
            transform.position = followTarget.position;
        }

        void HandleRotations(float d, float v, float h, float targetSpeed)
        {
            if (turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, h, ref smoothXvelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, v, ref smoothYvelocity, turnSmoothing);
            }
            else
            {
                smoothX = h;
                smoothY = v;
            }

            // 垂直旋转
            tiltAngle -= smoothY * targetSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

            // 锁定目标方向
            if (lockOn)
            {
                //
            }

            // 水平旋转
            lookAngle += smoothX * targetSpeed;
            transform.rotation = Quaternion.Euler(0, lookAngle, 0);
        }

        void HandleCameraCollision(float d)
        {
            Vector3 follow = followTarget.position + new Vector3(0, offset.y, 0);
            // 定义摄像机与目标之间的最大距离
            Vector3 rayDir = (camTrans.position - follow).normalized;

            Debug.DrawRay(follow, rayDir * defaultDistance, Color.red);

            // 定义摄像机与目标之间的最小距离
            float minDistance = 0.5f;

            // 定义用于忽略的层（Layer 28）
            int layerMask = 1 << 28;


            RaycastHit hit;


            // 从pivot位置向camTrans方向进行射线检测
            if (Physics.Raycast(follow, rayDir, out hit, defaultDistance, layerMask))
            {
                float distance = hit.distance;
                distance = Mathf.Clamp(distance, minDistance, defaultDistance); //有问题这里
                camTrans.position = Vector3.Lerp(camTrans.position, follow + rayDir * distance, d * followSpeed);
            }
            else
            {
                // 如果没有检测到碰撞，恢复到最大距离
                pivot.localPosition = offset;
                camTrans.localPosition = Vector3.zero;
            }
        }


        //单例
        public static CameraManager instance;

        void Awake()
        {
            instance = this;
        }
    }
}