using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class HandleIK : MonoBehaviour
    {
        Animator anim; //Animator组件

        //辅助物体，用于控制角色不同部位的位置
        Transform handHelper; //手
        Transform bodyHelper; //身体
        Transform headHelper; //头
        Transform shoulderHelper; //肩膀

        Transform headTrans; //用于储存头部骨骼的问题

        public float weight; //控制IK权重的插值

        public IKSnapShot[] ikSnapShots;
        public Vector3 defaultHeadPos;

        //查找并返回一个特定类型的IKSnapShot快照（姿态数据）
        IKSnapShot GetSnapShot(IKSnapShotType type)
        {
            for (int i = 0; i < ikSnapShots.Length; i++)
            {
                if (ikSnapShots[i].type == type)
                {
                    return ikSnapShots[i];
                }
            }

            return null;
        }

        public void Init(Animator a)
        {
            // 将传入的 Animator 组件赋值给本类的 anim 变量，用于控制角色的动画。
            anim = a;

            // 创建辅助对象（空的 GameObject）用于处理 IK（反向运动学）的不同部位。
            // 这些辅助对象的用途是作为 IK 操作的目标位置。

            // 创建用于控制头部位置的辅助对象
            headHelper = new GameObject().transform;
            headHelper.name = "head_helper";  // 为调试方便，命名为 "head_helper"

            // 创建用于控制手部位置的辅助对象
            handHelper = new GameObject().transform;
            handHelper.name = "hand_helper";  // 命名为 "hand_helper"

            // 创建用于控制身体位置的辅助对象
            bodyHelper = new GameObject().transform;
            bodyHelper.name = "body_helper";  // 命名为 "body_helper"

            // 创建用于肩膀控制的辅助对象
            shoulderHelper = new GameObject().transform;
            shoulderHelper.name = "shoulder_helper";  // 命名为 "shoulder_helper"

            // 将 shoulderHelper 设为父对象，帮助所有部位的辅助对象实现同步运动
            shoulderHelper.parent = transform.parent;  // 将肩膀辅助对象的父对象设为当前对象的父对象，以便同步
            shoulderHelper.localPosition = Vector3.zero;  // 初始化肩膀的局部位置为 (0,0,0)
            shoulderHelper.localRotation = Quaternion.identity;  // 初始化肩膀的局部旋转为默认方向

            // 将 headHelper、bodyHelper 和 handHelper 设置为 shoulderHelper 的子对象
            // 这样肩膀的变动会自动影响到这些辅助对象的位置和旋转。
            headHelper.parent = shoulderHelper;
            bodyHelper.parent = shoulderHelper;
            handHelper.parent = shoulderHelper;

            // 通过 HumanBodyBones 枚举获取头部的骨骼 Transform，并赋值给 headTrans，方便后续的头部控制
            headTrans = anim.GetBoneTransform(HumanBodyBones.Head);
        }

        public void UpdateIKTargets(IKSnapShotType type, bool isLeft)
        {
            // 根据指定的类型 (type)，从快照列表中获取对应的 IKSnapShot 实例
            IKSnapShot snap = GetSnapShot(type);

            // 使用快照数据更新手部、身体、和头部辅助对象的位置和旋转信息
            // 设置手部辅助对象的局部位置和旋转，控制手的位置和方向
            handHelper.localPosition = snap.handPos;
            handHelper.localEulerAngles = snap.hand_eulers;

            // 设置身体辅助对象的局部位置，用于控制身体的平衡或整体位置
            bodyHelper.localPosition = snap.bodyPos;

            // 判断是否需要覆盖头部位置，如果快照中设置了 overwriteHeadPos，则使用快照中的头部位置
            // 否则，将头部辅助对象的位置恢复为默认位置
            if (snap.overwriteHeadPos)
                headHelper.localPosition = snap.headPos;
            else
                headHelper.localPosition = defaultHeadPos;
        }

        public void IKTick(AvatarIKGoal goal, float w)
        {
            // 使用插值平滑地调整权重，使其逐渐接近目标权重 w
            weight = Mathf.Lerp(weight, w, Time.deltaTime * 5);

            // 设置目标的 IK 权重，控制手的目标位置和旋转的影响程度
            anim.SetIKPositionWeight(goal, weight);
            anim.SetIKRotationWeight(goal, weight);

            // 设置手的 IK 目标位置和旋转，使手部跟随 handHelper 的位置和方向
            anim.SetIKPosition(goal, handHelper.position);
            anim.SetIKRotation(goal, handHelper.rotation);

            // 设置头部的“看向”权重和目标位置，使角色的头部朝向 bodyHelper 的位置
            anim.SetLookAtWeight(weight, 0.8f, 1, 1, 1);  // 设置看向的权重，分别控制位置、角度、距离等
            anim.SetLookAtPosition(bodyHelper.position);   // 设置目标位置为 bodyHelper 的位置
        }


        public void OnAnimatorMoveTick(bool isLeft)
        {
            // 获取左或右肩的骨骼变换，基于参数 isLeft 决定使用哪一侧的肩膀
            Transform shoulder = anim.GetBoneTransform(
                (isLeft) ? HumanBodyBones.LeftShoulder : HumanBodyBones.RightShoulder);

            // 将 shoulderHelper 对齐到选定的肩膀位置
            shoulderHelper.transform.position = shoulder.position;
        }

        //此方法在每帧的 LateUpdate 中调用，用于平滑地将角色的头部（headTrans）朝向 headHelper 的位置，以实现头部朝向控制。
        public void LateTick()
        {
            // 检查 headTrans 和 headHelper 是否为 null，确保它们已被初始化
            if (headTrans == null || headHelper == null)
                return;

            // 计算 headHelper 到 headTrans 的方向向量
            Vector3 direction = headHelper.position - headTrans.position;

            // 如果方向向量为零，则将方向设置为头部的前方向
            if (direction == Vector3.zero)
                direction = headTrans.forward;

            // 计算目标旋转，使得头部朝向 headHelper 的位置
            Quaternion targetRot = Quaternion.LookRotation(direction);

            // 使用 Slerp 插值将当前头部旋转平滑地过渡到目标旋转，插值由 weight 控制
            Quaternion curRot = Quaternion.Slerp(headTrans.rotation, targetRot, weight);

            // 应用新的旋转到 headTrans
            headTrans.rotation = curRot;
        }
    }

    public enum IKSnapShotType
    {
        breath_r, // 右手施法时的姿势
        breath_l, // 左手施法时的姿势
        shield_r, // 右手持盾时的姿势
        shield_l // 左手持盾时的姿势
    }


    [System.Serializable]
    public class IKSnapShot
    {
        public IKSnapShotType type; // 快照类型
        public Vector3 handPos; // 手的位置
        public Vector3 hand_eulers; // 手的欧拉角旋转值
        public Vector3 bodyPos; // 身体的位置偏移

        public bool overwriteHeadPos; // 是否覆盖默认的头部位置
        public Vector3 headPos; // 头部位置
    }
}