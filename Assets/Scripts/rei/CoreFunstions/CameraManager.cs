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

        [Header("Stats")] public float followSpeed = 20; // 相机跟随目标的速度
        public float mouseSpeed = 2; // 鼠标控制相机旋转的速度
        public float turnSmoothing = .1f; // 相机平滑旋转的系数
        public float minAngle = -20; // 垂直旋转的最小角度
        public float maxAngle = 35; // 垂直旋转的最大角度
        public float defaultDistance;
        public Vector3 offset = new Vector3(0, 1.3f, 0);
        public float lockOffset = 0.5f;


        [Header("MoveStat")] public Vector3 targetDir; // 目标方向向量
        public float lookAngle; // 水平旋转角度
        public float tiltAngle; // 垂直旋转角度

        [HideInInspector] public Transform pivot; // 相机的旋转轴
        [HideInInspector] public Transform camTrans; // 相机的Transform
        PlayerState _playerStates; // 管理相机状态的对象


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


        // 用于调试的可视化数据
        private Vector3 debugCandidatePos;
        private float debugCameraCollisionRadius;
        private Vector3 debugRayStart;
        private Vector3 debugRayDir;
        private float debugRayDistance;

        public void Init(PlayerState st)
        {
            _playerStates = st;
            followTarget = st.transform; // 将目标设置为StateManager的Transform
            camTrans = Camera.main.transform; // 获取主相机的Transform
            pivot = camTrans.parent; // 设置pivot为相机的父对象
            defaultDistance = new Vector3(0, offset.z, 0).magnitude;
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
                    _playerStates.lockOnTransform = lockOnTransform;
                }

                if (changeTargetLeft || changeTargetRight)
                {
                    lockOnTransform = lockOnTarget.GetTarget(changeTargetLeft);
                    _playerStates.lockOnTransform = lockOnTransform;
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
                Vector3 localDirection = lockOnTransform.position - follow; // 相机与目标的相对方向
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
            // follow为跟随点，即玩家角色头部(或指定offset位置)
            Vector3 follow = followTarget.position + new Vector3(0, offset.y, 0);

            // 计算期望的相机位置。默认距离defaultDistance表示相机应从follow点后退多少距离。
            // desiredCamPos代表理想情况下摄像机应放置的位置（没有碰撞干扰的情况下）
            Vector3 desiredCamPos = follow - transform.forward * defaultDistance;

            // 根据期望位置与follow点的关系，计算出一条从follow点到期望相机位置的方向向量rayDir
            Vector3 rayDir = (desiredCamPos - follow).normalized;

            // 定义相机的球体半径，用于球体碰撞检测
            float cameraCollisionRadius = 0.3f;

            // 定义相机最小距离，避免相机过于接近follow点，产生抖动或穿模感
            float minDistance = 0.15f;

            // layerMask用于指定检测哪些层级的碰撞体
            // 这里是 int layerMask = 1 << 28; 
            // 意思是只检测28层所在的物体(如摄像机可碰撞层)
            int layerMask = 1 << 28;

            // 初始化最终距离finalDistance为默认距离
            float finalDistance = defaultDistance;

            RaycastHit hit;
            // 首先使用Raycast沿rayDir方向从follow点发射一条射线，最大距离为defaultDistance
            // 如果射线检测到了碰撞物体，则说明期望的位置与之重叠或在其后方
            // 我们将finalDistance缩短到hit.distance，这样相机不会穿过障碍物
            if (Physics.Raycast(follow, rayDir, out hit, defaultDistance, layerMask))
            {
                finalDistance = hit.distance;
            }

            // 基于最终计算出的距离finalDistance，确定相机候选位置candidatePos
            Vector3 candidatePos = follow + rayDir * finalDistance;

            // 保存调试数据（用于 OnDrawGizmos 可视化）
            debugCandidatePos = candidatePos;
            debugCameraCollisionRadius = cameraCollisionRadius;
            debugRayStart = follow;
            debugRayDir = rayDir;
            debugRayDistance = finalDistance;

            // 使用 CheckSphere 来检测candidatePos位置处，半径为cameraCollisionRadius的球体
            // 是否与场景发生碰撞。如果有碰撞，说明即使距离缩短了，但考虑相机体积后仍在穿模
            bool isColliding = Physics.CheckSphere(candidatePos, cameraCollisionRadius, layerMask);

            // 如果球体检测仍有碰撞，则需要继续往回缩短距离，直到无碰撞或缩短到最小距离minDistance
            while (isColliding && finalDistance > minDistance)
            {
                // 每次向内收缩0.05f的距离
                finalDistance -= 0.05f;
                finalDistance = Mathf.Max(finalDistance, minDistance);

                // 根据新的距离重新计算candidatePos
                candidatePos = follow + rayDir * finalDistance;

                // 再次检测球体碰撞
                isColliding = Physics.CheckSphere(candidatePos, cameraCollisionRadius, layerMask);

                // 更新调试数据，以便在场景中查看最终结果
                debugCandidatePos = candidatePos;
                debugRayDistance = finalDistance;
            }

            // 将相机的位置平滑插值到最终确定的candidatePos
            // d为deltaTime，followSpeed为插值速度
            camTrans.position = Vector3.Lerp(camTrans.position, candidatePos, d * followSpeed);
        }

        // void HandleCameraCollision(float d)
        // {
        //     Vector3 follow = followTarget.position + new Vector3(0, offset.y, 0);
        //     // 定义摄像机与目标之间的最大距离
        //     Vector3 rayDir = (camTrans.position - follow).normalized;
        //
        //     Debug.DrawRay(follow, rayDir * defaultDistance, Color.red);
        //
        //     // 定义摄像机与目标之间的最小距离
        //     float minDistance = 0.5f;
        //
        //     // 定义用于忽略的层（Layer 28）
        //     int layerMask = 1 << 28;
        //
        //
        //     RaycastHit hit;
        //
        //
        //     // 从pivot位置向camTrans方向进行射线检测
        //     if (Physics.Raycast(follow, rayDir, out hit, defaultDistance, layerMask))
        //     {
        //         float distance = hit.distance;
        //         distance = Mathf.Clamp(distance, minDistance, defaultDistance); //有问题这里
        //         camTrans.position = Vector3.Lerp(camTrans.position, follow + rayDir * distance, d * followSpeed);
        //     }
        //     else
        //     {
        //         // 如果没有检测到碰撞，恢复到最大距离
        //         pivot.localPosition = offset;
        //         camTrans.localPosition = Vector3.zero;
        //     }
        // }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            // 绘制摄像机球体检测位置和半径
            Gizmos.DrawWireSphere(debugCandidatePos, debugCameraCollisionRadius);

            // 在编辑器中也可视化射线: 
            Gizmos.color = Color.green;
            Gizmos.DrawLine(debugRayStart, debugRayStart + debugRayDir * debugRayDistance);
        }


        //单例
        public static CameraManager instance;

        void Awake()
        {
            instance = this;
        }

        public void ReSetCullingMask()
        {
            Camera mainCamera = Camera.main;

            // 检查主相机是否存在
            if (mainCamera == null)
            {
                Debug.LogError("主相机未找到！");
                return;
            }

            // 设置需要渲染的层
            mainCamera.cullingMask = 
                (1 << LayerMask.NameToLayer("Default")) |
                (1 << LayerMask.NameToLayer("UI")) |
                (1 << LayerMask.NameToLayer("enemy")) |
                (1 << LayerMask.NameToLayer("player")) |
                (1 << LayerMask.NameToLayer("Inventory")) |
                (1 << LayerMask.NameToLayer("Terrian"));
        }

        public void SetCullingMask()
        {
            Camera mainCamera = Camera.main;

            // 检查主相机是否存在
            if (mainCamera == null)
            {
                Debug.LogError("主相机未找到！");
                return;
            }

            // 设置需要渲染的层
            mainCamera.cullingMask =
                (1 << LayerMask.NameToLayer("player"));

        }
    }
}