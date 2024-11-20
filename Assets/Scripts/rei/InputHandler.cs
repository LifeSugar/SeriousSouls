using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class InputHandler : MonoBehaviour
    {
        float vertical;
        float horizontal;
        public bool b_input;
        public bool a_input;
        public bool x_input;
        public bool y_input;
        
        


        bool rb_input;
        bool lb_input;

        float rt_axis;
        bool rt_input;

        float lt_axis;
        bool lt_input;

        float d_y;
        float d_x;

        public bool d_up;
        public bool d_down;
        public bool d_right;
        public bool d_left;

        bool p_d_up;
        bool p_d_down;
        bool p_d_left;
        bool p_d_right;

        bool leftAxis_down;
        bool rightAxis_down;

        public float b_timer;
        float rt_timer;
        float lt_timer;
        float close_timer = 0;
        float a_input_count = 1.5f;

        StateManager states;
        CameraManager camManager;
        UIManager uiManager;
        DialogueManager dialogueManager;


        float delta;


        // //用于方向键
        // private const float threshold = 0.5f;  // 阈值，用于判断DPad和扳机键是否按下
        //
        // // DPad按键状态（仅适用于Windows平台）
        // private bool isDPadUpPressed = false;
        // private bool isDPadDownPressed = false;
        // private bool isDPadLeftPressed = false;
        // private bool isDPadRightPressed = false;

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
            // GetInput();
            // UpdateStates();
            
            if (b_input)
                b_timer += delta;
            FixedUpstaeStates();
            FixedResetInputNState();
            
            states.FixedTick(delta);
            camManager.Tick(delta);
            // states.MonitorStats();
        }

        bool preferItem;

        void Update()
        {
            GetInput();
            UpdateStates();
            
            delta = Time.deltaTime;
            if (a_input)
                a_input_count++;
            // Debug.Log(delta);
            states.Tick(delta);
            
            
            if (!dialogueManager.dialogueActive)
            {
                if (states.pickManager.itemCandidate != null || states.pickManager.interactionCandidate != null)
                {
                    if (states.pickManager.itemCandidate && states.pickManager.interactionCandidate)
                    {
                        if (preferItem)
                        {
                            PickupItem();
                        }
                        else
                            Interact();
                    }
            
                    if (states.pickManager.itemCandidate && !states.pickManager.interactionCandidate)
                    {
                        // Debug.Log("picking item");
                        PickupItem();
                    }
            
                    if (!states.pickManager.itemCandidate && states.pickManager.interactionCandidate)
                    {
                        Interact();
                    }
                }
                else
                {
                    uiManager.CloseInteractCanvas();
                    if (uiManager.ItemCards[0].gameObject.activeSelf == true
                        || uiManager.ItemCards[1].gameObject.activeSelf == true
                        || uiManager.ItemCards[2].gameObject.activeSelf == true
                        || uiManager.ItemCards[3].gameObject.activeSelf == true
                        || uiManager.ItemCards[4].gameObject.activeSelf == true)
                        close_timer += 1;
                    if (close_timer > 190)
                    {
                        close_timer = 0;
                        uiManager.CloseItemCards();
                        a_input = false;
                    }
                }
            }
            else
            {
                uiManager.CloseInteractCanvas();
            }
        
            if (a_input_count > 1f)
            {
                a_input = false;
                a_input_count = 0;
            }
        
        
            // dialogueManager.Tick(a_input);
            
            states.MonitorStats();
            ResetInputNState();
            uiManager.Tick(states.characterStats, delta, states);
            camManager.FixedTick(delta);
        }
        
        void PickupItem()
        {
            uiManager.OpenInteractCanvas(UIActionType.pickup);
            if (a_input)
            {
                Debug.Log("pickup!");
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

            b_input = Input.GetButton(GlobalStrings.B); //连击问题可以使用counter解决
            a_input = Input.GetButton(GlobalStrings.A);
            x_input = Input.GetButton(GlobalStrings.X);
            y_input = Input.GetButton(GlobalStrings.Y);

            rb_input = Input.GetButton(GlobalStrings.RB);
            lb_input = Input.GetButton(GlobalStrings.LB);

            
            rt_input = Input.GetButton(GlobalStrings.RT);
            lt_input = Input.GetButton(GlobalStrings.LT);
            
            rightAxis_down = Input.GetButton(GlobalStrings.R);
            
            // if (b_input)
            //     b_timer += delta;
            
            
            
            d_x = Input.GetAxis(GlobalStrings.DPadHorizontal);
            d_y = Input.GetAxis(GlobalStrings.DPadVertical);

            d_up = Input.GetKeyUp(KeyCode.Alpha1) || d_y > 0;
            d_down = Input.GetKeyUp(KeyCode.Alpha2) || d_y < 0;
            d_left = Input.GetKeyUp(KeyCode.Alpha3) || d_x < 0;
            d_right = Input.GetKeyUp(KeyCode.Alpha4) || d_x > 0;

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

            // // B_input: 
            // if (b_input && b_timer > 0.5f)
            // {
            //     
            //     states.run = (states.moveAmount > 0.8f) && states.characterStats._stamina > 0;
            // }
            //
            // if (b_input == false && b_timer > 0 && b_timer < 0.5f)
            //     states.rollInput = true;

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

        void FixedUpstaeStates()
        {
            // B_input: 
            if (b_input && b_timer > 0.5f)
            {
                
                if ((states.moveAmount > 0.8f) && states.characterStats._stamina > 0)
                {
                    Debug.Log("running");
                    states.run = true;
                }
                // states.run = (states.moveAmount > 0.8f) && states.characterStats._stamina > 0;
            }

            if (b_input == false && b_timer > 0 && b_timer < 0.5f)
            {
                states.rollInput = true;
                Debug.Log("roll input!");
            }
                
            
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

        void FixedResetInputNState()
        {
            // reset b_timer when b_input is released from keyboard
            if (b_input == false)
                b_timer = 0;
        }

        void ResetInputNState()
        {
            // turn off rollInput and run state after being pressed.
            if (states.rollInput)
                states.rollInput = false;
            if (states.run)
                states.run = false;
        }
    }
}