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

        //锁定对象 EnemyTarget or AimTarget lockOnTarget 先用GameObject代替
        public GameObject lockOnTarget;


        [Header ("Stats")]
        public float followSpeed = 9; // 相机跟随目标的速度
        public float mouseSpeed = 2; // 鼠标控制相机旋转的速度
        public float turnSmoothing = .1f; // 相机平滑旋转的系数
        public float minAngle = -35; // 垂直旋转的最小角度
        public float maxAngle = 35; // 垂直旋转的最大角度

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

        public void Init(StateManager st)
        {
            states = st;
            if (states == null)
                Debug.Log("No state manager in Camera Manager");
            else
            {
                Debug.Log(states.name + " state manager in Camera Manager");
            }
            followTarget = st.transform; // 将目标设置为StateManager的Transform

            camTrans = Camera.main.transform; // 获取主相机的Transform
            pivot = camTrans.parent; // 设置pivot为相机的父对象
        }


        public void Tick(float d)
        {
            float h = Input.GetAxis(GlobalStrings.RightHorizontal); // 获取水平输入
            float v = Input.GetAxis(GlobalStrings.RightVertical); // 获取垂直输入
            float targetSpeed = mouseSpeed; // 设定相机移动速度

            changeTargetLeft = Input.GetKeyUp(KeyCode.V); // 检测左切换目标的按键
            changeTargetRight = Input.GetKeyUp(KeyCode.B); // 检测右切换目标的按键

            if (lockOnTarget != null)
            {
            }

            FollowTarget(d); // 调用跟随目标方法
            HandleRotations(d, v, h, targetSpeed); // 调用旋转处理方法
        }

        //使用线性插值来平滑跟随目标位置。
        void FollowTarget(float d)
        {
            float speed = d * followSpeed;
            //delay follow
            Vector3 camPosition = Vector3.Lerp(transform.position, followTarget.position, speed);
            transform.position = camPosition;
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
        
        //单例
        public static CameraManager instance;
        void Awake(){
            instance = this;
        }
    }
}