using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.ProBuilder.Shapes;

namespace rei
{
    [System.Serializable]
    public enum Doors
    {
        SlidingDoorleft,
        SlidingDoorright,
        PushingDoor,
        PullingDoor,
    }

    [System.Serializable]
    public class Key : Item
    {
        public int key;
    }
    public class DoorInteraction : WorldInteraction
    {
        public Doors doorType;
        public bool oneSide;
        public GameObject doorObject;
        public bool needKey;
        public Key matchKey;
        public float doorOpenTime = 1.5f;
        public float slideDistance = 1.5f;
        public float openAngle = 90;
        void Start()
        {
            PickableItemsManager.instance.interactions.Add(this);
        }

        public override void InteractActual()
        {
            Debug.Log(this.gameObject.name);
            if (oneSide)
            {
                Vector3 openDir = transform.forward; //z轴正向
                Vector3 playerDir = (transform.position - InputHandler.instance.transform.position).normalized;
                float angle = Vector3.Angle(openDir, playerDir);
                if (angle > 90)
                {
                    InputHandler.instance._playerStates.anim.Play("Empty");
                    UIManager.instance.OpenInteractionInfoCanvas("Cannot Open From This Side");
                    return;
                }
                else
                {
                    if (needKey)
                    {
                        if (InputHandler.instance._playerStates.inventoryManager.inventory.keys.Exists(c =>
                            c.key == matchKey.key))
                        {
                            var key = InputHandler.instance._playerStates.inventoryManager.inventory.keys.Find(c =>
                                c.key == matchKey.key);
                            string keyName =key.itemName + " Used";
                            UIManager.instance.OpenInteractionInfoCanvas(keyName);
                            HandleDoorOpen();
                            InputHandler.instance._playerStates.inventoryManager.inventory.keys.Remove(key);
                        }
                        else
                        {
                            InputHandler.instance._playerStates.anim.Play("Empty");
                            UIManager.instance.OpenInteractionInfoCanvas("Locked");
                            return;
                        }
                    }
                    else
                    {
                        HandleDoorOpen();
                    }
                }
            }
            else
            {
                if (needKey)
                {
                    if (InputHandler.instance._playerStates.inventoryManager.inventory.keys.Exists(c =>
                            c.key == matchKey.key))
                    {
                        var key = InputHandler.instance._playerStates.inventoryManager.inventory.keys.Find(c =>
                            c.key == matchKey.key);
                        string keyName =key.itemName + " Used";
                        UIManager.instance.OpenInteractionInfoCanvas(keyName);
                        HandleDoorOpen();
                        InputHandler.instance._playerStates.inventoryManager.inventory.keys.Remove(key);
                    }
                    else
                    {
                        InputHandler.instance._playerStates.anim.Play("Empty");
                        UIManager.instance.OpenInteractionInfoCanvas("Locked");
                        return;
                    }
                }
                else
                {
                    HandleDoorOpen();
                }
            }
            
        }

        void HandleDoorOpen()
        {
            // 禁用碰撞器
            // doorObject.GetComponent<MeshCollider>().enabled = false;

            switch (doorType)
            {
                case Doors.SlidingDoorleft:
                    Vector3 targetPosLeft = doorObject.transform.position + doorObject.transform.right * slideDistance;
                    doorObject.transform.DOMove(targetPosLeft, doorOpenTime);
                    break;
                case Doors.SlidingDoorright:
                    Vector3 targetPosRight = doorObject.transform.position - doorObject.transform.right * slideDistance;
                    doorObject.transform.DOMove(targetPosRight, doorOpenTime);
                    break;
                case Doors.PushingDoor:
                    Vector3 targetRotationPush = doorObject.transform.localEulerAngles + new Vector3(0f, -openAngle, 0f);
                    doorObject.transform.DORotate(targetRotationPush, doorOpenTime, RotateMode.LocalAxisAdd);
                    break;
                case Doors.PullingDoor:
                    Vector3 targetRotationPull = doorObject.transform.localEulerAngles + new Vector3(0f, openAngle, 0f);
                    doorObject.transform.DORotate(targetRotationPull, doorOpenTime, RotateMode.LocalAxisAdd);
                    break;
                default:
                    break;
            }

            PickableItemsManager.instance.interactions.Remove(this);
        }
    }

}