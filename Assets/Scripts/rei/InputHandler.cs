using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class InputHandler : MonoBehaviour
    {
        float vertical;
        float horizontal;
        bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;
        
        


        bool rb_input;
        bool lb_input;

        float rt_axis;
        bool rt_input;

        float lt_axis;
        bool lt_input;

        float d_y;
        float d_x;

        bool d_up;
        bool d_down;
        bool d_right;
        bool d_left;

        bool p_d_up;
        bool p_d_down;
        bool p_d_left;
        bool p_d_right;

        bool leftAxis_down;
        bool rightAxis_down;

        float b_timer;
        float rt_timer;
        float lt_timer;
        float close_timer = 0;
        float a_input_count = 1.5f;

        StateManager states;
        CameraManager camManager;
        UIManager uiManager;
        DialogueManager dialogueManager;


        float delta;


        //用于win平台方向键
        private const float threshold = 0.5f;  // 阈值，用于判断DPad和扳机键是否按下
        private bool isLTPressed = false;  // LT按键状态
        private bool isRTPressed = false;  // RT按键状态

        // DPad按键状态（仅适用于Windows平台）
        private bool isDPadUpPressed = false;
        private bool isDPadDownPressed = false;
        private bool isDPadLeftPressed = false;
        private bool isDPadRightPressed = false;

        //-----------------------------------------------------------------------

        void Start()
        {
            Debug.Log("start!");
            states = GetComponent<StateManager>();
            if (states == null)
                Debug.LogWarning("No StateManager component found!");
            else
                Debug.Log("StateManager component found!");
            states.Init();
            
            

            camManager = CameraManager.instance;
            Debug.Log(camManager.name);
            if(camManager == null)
                Debug.Log("No camera found!!!!!!!!!");
            else
                Debug.Log(camManager.name);
            camManager.Init(states);
            
            uiManager = UIManager.instance;

            dialogueManager = DialogueManager.instance;


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // intitialize the movement and camera functionalities
        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateStates();
            states.FixedTick(delta);
            camManager.Tick(delta);
            states.MonitorStats();
        }

        bool preferItem;

        void Update()
        {
            
            delta = Time.deltaTime;
            if (a_input)
                a_input_count++;
            // Debug.Log(delta);
            states.Tick(delta);
            
            // if (!dialogueManager.dialogueActive)
            // {
            //     if (states.pickManager.itemCandidate != null || states.pickManager.interactionCandidate != null)
            //     {
            //         if (states.pickManager.itemCandidate && states.pickManager.interactionCandidate)
            //         {
            //             if (preferItem)
            //             {
            //                 PickupItem();
            //             }
            //             else
            //                 Interact();
            //         }
            //
            //         if (states.pickManager.itemCandidate && !states.pickManager.interactionCandidate)
            //         {
            //             PickupItem();
            //         }
            //
            //         if (!states.pickManager.itemCandidate && states.pickManager.interactionCandidate)
            //         {
            //             Interact();
            //         }
            //     }
            //     else
            //     {
            //         uiManager.CloseInteractCanvas();
            //         if (uiManager.ItemCards[0].gameObject.activeSelf == true
            //             || uiManager.ItemCards[1].gameObject.activeSelf == true
            //             || uiManager.ItemCards[2].gameObject.activeSelf == true
            //             || uiManager.ItemCards[3].gameObject.activeSelf == true
            //             || uiManager.ItemCards[4].gameObject.activeSelf == true)
            //             close_timer += 1;
            //         if (close_timer > 190)
            //         {
            //             close_timer = 0;
            //             uiManager.CloseItemCards();
            //             a_input = false;
            //         }
            //     }
            // }
            // else
            // {
            //     uiManager.CloseInteractCanvas();
            // }
        
            if (a_input_count > 1f)
            {
                a_input = false;
                a_input_count = 0;
            }
        
        
            // dialogueManager.Tick(a_input);
            
            // states.MonitorStats();
            ResetInputNState();
            uiManager.Tick(states.characterStats, delta, states);
        }
        
        void PickupItem()
        {
            uiManager.OpenInteractCanvas(UIActionType.pickup);
            if (a_input)
            {
                Vector3 targetDir = states.pickManager.itemCandidate.transform.position - transform.position;
                states.SnapToRotation(targetDir);
                states.pickManager.PickCandidate(states);
                states.PlayAnimation("pick_up");
                a_input = false;
            }
        }

        void Interact()
        {
            uiManager.OpenInteractCanvas(states.pickManager.interactionCandidate.actionType);
            if (a_input)
            {
                states.audio_source.PlayOneShot(ResourceManager.instance.GetAudio("interact").audio_clip);
                states.InteractLogic();
                a_input = false;
            }
        }


        void GetInput()
        {
            vertical = Input.GetAxis(GlobalStrings.Vertical);
            horizontal = Input.GetAxis(GlobalStrings.Horizontal);

            b_input = Input.GetButton(GlobalStrings.B);
            a_input = Input.GetButton(GlobalStrings.A);
            x_input = Input.GetButton(GlobalStrings.X);
            y_input = Input.GetButtonUp(GlobalStrings.Y);

            rb_input = Input.GetButton(GlobalStrings.RB);
            lb_input = Input.GetButton(GlobalStrings.LB);

            // rt_input = Input.GetButton(GlobalStrings.RT);
            // rt_axis = Input.GetAxis(GlobalStrings.RT);
            //
            // if (rt_axis != 0)
            //     rt_input = true;
            //
            // lt_input = Input.GetButton(GlobalStrings.LT);
            // lt_axis = Input.GetAxis(GlobalStrings.LT);
            // if (lt_axis != 0)
            //     lt_input = true;
            
            // 检查LT和RT是否“按下”
            float ltValue = Input.GetAxis(GlobalStrings.LT);
            float rtValue = Input.GetAxis(GlobalStrings.RT);

            // LT按键处理：按下时显示一次"LT Pressed"
            if (ltValue > threshold && !isLTPressed)
            {
                lt_input = true;
                isLTPressed = true;
                
            }
            else if (ltValue <= threshold && isLTPressed)
            {
                
                isLTPressed = false;  // 重置状态
                lt_input = false;
                
            }
            else
            {
                lt_input = false;
            }

            // RT按键处理：按下时显示一次"RT Pressed"
            if (rtValue > threshold && !isRTPressed)
            {
                rt_input = true;
                
                isRTPressed = true;
            }
            else if (rtValue <= threshold && isRTPressed)
            {
                rt_input = false;
                
                isRTPressed = false;  // 重置状态
                
            }
            else
            {
                rt_input = false;
            }

            rightAxis_down = Input.GetButtonUp(GlobalStrings.L);

            if (b_input)
                b_timer += delta;
#if UNITY_STANDALONE_WIN
            // d_x = Input.GetAxis(GlobalStrings.DadHorizontal);
            // d_y = Input.GetAxis(GlobalStrings.DadVertical);
            //
            // d_up = Input.GetKeyUp(KeyCode.Alpha1) || d_y > 0;
            // d_down = Input.GetKeyUp(KeyCode.Alpha2) || d_y < 0;
            // d_left = Input.GetKeyUp(KeyCode.Alpha3) || d_x < 0;
            // d_right = Input.GetKeyUp(KeyCode.Alpha4) || d_x > 0;
            
            
            
       // 获取DPad的轴值
        d_x = Input.GetAxis(GlobalStrings.DPadHorizontal);
        d_y = Input.GetAxis(GlobalStrings.DPadVertical);

        // 检查DPad上方向
        if (d_y > threshold)
        {
            if (!wasDPadUpPressed)
            {
                wasDPadUpPressed = true;
            }
        }
        else
        {
            if (wasDPadUpPressed)
            {
                p_d_up = true;  // 抬起时返回True
                inputLog += "DPad Up Released\n";
                wasDPadUpPressed = false;
            }
        }

        // 检查DPad下方向
        if (d_y < -threshold)
        {
            if (!wasDPadDownPressed)
            {
                wasDPadDownPressed = true;
            }
        }
        else
        {
            if (wasDPadDownPressed)
            {
                p_d_down = true;  // 抬起时返回True
                inputLog += "DPad Down Released\n";
                wasDPadDownPressed = false;
            }
        }

        // 检查DPad左方向
        if (d_x < -threshold)
        {
            if (!wasDPadLeftPressed)
            {
                wasDPadLeftPressed = true;
            }
        }
        else
        {
            if (wasDPadLeftPressed)
            {
                p_d_left = true;  // 抬起时返回True
                inputLog += "DPad Left Released\n";
                wasDPadLeftPressed = false;
            }
        }

        // 检查DPad右方向
        if (d_x > threshold)
        {
            if (!wasDPadRightPressed)
            {
                wasDPadRightPressed = true;
            }
        }
        else
        {
            if (wasDPadRightPressed)
            {
                p_d_right = true;  // 抬起时返回True
                inputLog += "DPad Right Released\n";
                wasDPadRightPressed = false;
            }
        }

        // 显示并输出到Console
        if (!string.IsNullOrEmpty(inputLog))
        {
            displayText.text = inputLog;  // 显示到UI
            Debug.Log(inputLog);  // 输出到Console
        }

        // 在下一帧重置抬起状态
        ResetReleaseStates();
    }

    // 在下一帧重置抬起状态
    private void ResetReleaseStates()
    {
        p_d_up = false;
        p_d_down = false;
        p_d_left = false;
        p_d_right = false;
    }


#elif UNITY_STANDALONE_OSX
            // d_up = Input.GetButton(GlobalStrings.DPadUp);
            // d_down = Input.GetButton(GlobalStrings.DPadDown);
            // d_left = Input.GetButton(GlobalStrings.DPadLeft);
            // d_right = Input.GetButton(GlobalStrings.DPadRight);
#endif
        }

        // passing values to StateManager variables and functions.
        void UpdateStates()
        {
            states.vertical = vertical;
            states.horizontal = horizontal;

            states.itemInput = x_input;
            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;


            // moveDir
            Vector3 v = states.vertical * camManager.transform.forward;
            Vector3 h = states.horizontal * camManager.transform.right;
            states.moveDir = (v + h).normalized;

            // moveAmount
            float m = Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical);
            states.moveAmount = Mathf.Clamp01(m);

            // B_input: 
            if (b_input && b_timer > 0.3f)
            {
                // run when holding down.
                states.run = (states.moveAmount > 0.8f) && states.characterStats._stamina > 0;
            }

            /* roll when tap the button b_input, thus b_input must equal false at the following FixedFrame because b_input == false when not the button for it is released.
             and the timer at that following FixedFrame still equals to the last calculated value before being set back to 0 in the next FixedFrame after this one. */
            if (b_input == false && b_timer > 0 && b_timer < 0.5f)
                states.rollInput = true;

            if (y_input)
            {
                if (states.pickManager.itemCandidate && states.pickManager.interactionCandidate)
                {
                    preferItem = !preferItem;
                }
                else
                {
                    states.isTwoHanded = !states.isTwoHanded;
                    states.HandleTwoHanded();
                }
            }

            if (states.lockOnTarget != null)
            {
                // if (states.lockOnTarget.eStates.isDead)
                // {
                //     states.lockOn = false;
                //     states.lockOnTarget = null;
                //     states.lockOnTransform = null;
                //     camManager.lockOn = false;
                //     camManager.lockOnTarget = null;
                // }
            }
            else
            {
                states.lockOn = false;
                states.lockOnTarget = null;
                states.lockOnTransform = null;
                camManager.lockOn = false;
                camManager.lockOnTarget = null;
            }


            if (rightAxis_down)
            {
            }


            if (x_input)
                b_input = false;

            HandleQuickSlotChanges();
        }

        void HandleQuickSlotChanges()
        {
            if (states.isSpellCasting || states.usingItem)
                return;

            if (d_up)
            {
                if (!p_d_up)
                {
                    p_d_up = true;
                    states.inventoryManager.ChangeToNextSpell();
                }
            }

            if (!d_up)
                p_d_up = false;

            if (d_down)
            {
                if (!p_d_down)
                {
                    p_d_down = true;
                    states.inventoryManager.ChangeToNextConsumable();
                }
            }

            if (!d_up)
                p_d_down = false;

            if (states.onEmpty == false)
                return;

            if (states.isTwoHanded)
                return;

            if (d_left)
            {
                if (!p_d_left)
                {
                    states.inventoryManager.ChangeToNextWeapon(true);
                    p_d_left = true;
                }
            }

            if (d_right)
            {
                if (!p_d_right)
                {
                    states.inventoryManager.ChangeToNextWeapon(false);
                    p_d_right = true;
                }
            }


            if (!d_down)
                p_d_down = false;
            if (!d_left)
                p_d_left = false;
            if (!d_right)
                p_d_right = false;
        }

        void ResetInputNState()
        {
            // reset b_timer when b_input is released from keyboard
            if (b_input == false)
                b_timer = 0;
            // turn off rollInput and run state after being pressed.
            if (states.rollInput)
                states.rollInput = false;
            if (states.run)
                states.run = false;
        }
    }
}