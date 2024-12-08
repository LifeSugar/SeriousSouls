using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Menu")]
        public bool inMenu = false;
        public GameObject menu;
        
        [Header("OnCampFire")]
        public bool onCampFire = false;
        public GameObject CampFireCanvas;
        
        [Header("BlackScreen")]
        public ScreenFadeController screenFadeController;
        private bool isFading = false;
        
        [Header("Inputs")] 
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

        public PlayerState _playerStates;
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
        
        public static InputHandler instance;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            Debug.Log("start!");
            _playerStates = GetComponent<PlayerState>();
            if (_playerStates == null)
                Debug.LogWarning("No StateManager component found!");
            else
                Debug.Log("StateManager component found!");
            _playerStates.Init();
            
            

            camManager = CameraManager.instance;
            Debug.Log(camManager.name);
            if(camManager == null)
                Debug.Log("No camera found!!!!!!!!!");
            else
                Debug.Log(camManager.name);
            camManager.Init(_playerStates);
            
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
            
            _playerStates.FixedTick(delta);
            camManager.Tick(delta);
            _playerStates.MonitorStats();
        }

        bool preferItem;

        void Update()
        {
            if (!inMenu && !onCampFire)
            {
                GetInput();
                HandlePickAndInteract();
            }

            if (onCampFire)
            {
                if (Input.GetButtonDown(GlobalStrings.Menu) || Input.GetKeyDown(KeyCode.Escape))
                    HandleCampFireCanvas();
            }
                
            
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(GlobalStrings.Menu)) && !onCampFire)
                HandleMenu();
            UpdateStates();
            
            delta = Time.deltaTime;
            
            _playerStates.Tick(delta);
            
            


            if (dialogueManager.dialogueActive)
            {
                dialogueManager.Tick(ref a_input);
            }
            
            
            
            ResetInputNState();
            uiManager.Tick(_playerStates.characterStats, delta, _playerStates);
            camManager.FixedTick(delta);
        }

        void HandleMenu()
        {
            if (inMenu == false)
            {
                inMenu = true;
                menu.SetActive(true);
                InventoryUI.instance.UpdateInventoryUI(_playerStates.inventoryManager.inventory);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                ResetInputs();
            }
            else
            {
                inMenu = false;
                menu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void HandleCampFireCanvas()
        {
            if (!isFading)
            {
                StartCoroutine(HandleCampFireCanvasCoroutine());
            }
        }
        
        private IEnumerator HandleCampFireCanvasCoroutine()
        {
            isFading = true;

            // 淡入黑屏
            yield return StartCoroutine(screenFadeController.FadeIn());

            // 切换 Canvas 的激活状态
            if (onCampFire == false)
            {
                onCampFire = true;
                CampFireCanvas.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                ResetInputs();
                EnemyManager.instance.ResetAllEnemies();
            }
            else
            {
                CampFire currentCampFire = CampFireManager.instance.GetSittingCampFire();
                currentCampFire.sitting = false;
                currentCampFire.GetComponentInChildren<Camera>().gameObject.SetActive(false);
                onCampFire = false;
                CampFireCanvas.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _playerStates.Recover();
                
            }

            // 淡出黑屏
            yield return StartCoroutine(screenFadeController.FadeOut());

            isFading = false;
        }

        void HandlePickAndInteract()
        {
            if (!dialogueManager.dialogueActive)
            {
                if (!uiManager.InteractionInfoActive)
                {
                    if (_playerStates.pickManager.itemCandidate != null || _playerStates.pickManager.interactionCandidate != null)
                    {
                        if (_playerStates.pickManager.itemCandidate && _playerStates.pickManager.interactionCandidate)
                        {
                            if (preferItem)
                            {
                                PickupItem();
                                return;
                            }
                            else
                            {
                                Interact();
                                return;
                            }
                        }
            
                        if (_playerStates.pickManager.itemCandidate && !_playerStates.pickManager.interactionCandidate)
                        {
                            PickupItem();
                            return;
                        }
            
                        if (!_playerStates.pickManager.itemCandidate && _playerStates.pickManager.interactionCandidate)
                        {
                            Interact();
                            return;
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
                        if (close_timer > 390)
                        {
                            close_timer = 0;
                            uiManager.CloseItemCards();
                        }
                    }
                }
                else
                {
                    uiManager.CloseInteractCanvas();
                }
            }
            else
            {
                uiManager.CloseInteractCanvas();
            }
        
            if (uiManager.InteractionInfoActive)
            {
                if (Input.GetButtonDown(GlobalStrings.A))
                {
                    uiManager.CloseInteractionInfoCanvas();
                }
            }
        }
        
        void PickupItem()
        {
            uiManager.OpenInteractCanvas(UIActionType.pickup);
            if (Input.GetButton(GlobalStrings.A))
            {
                Debug.Log("pickup!");
                Vector3 targetDir = _playerStates.pickManager.itemCandidate.transform.position - transform.position;
                _playerStates.SnapToRotation(targetDir);
                _playerStates.pickManager.PickCandidate(_playerStates);
                _playerStates.PlayAnimation("pick_up");
                a_input = false;
            }
        }

        void Interact()
        {
            uiManager.OpenInteractCanvas(_playerStates.pickManager.interactionCandidate.actionType);
            if (Input.GetButtonDown(GlobalStrings.A) && !dialogueManager.dialogueActive)
            {
                // states.audio_source.PlayOneShot(ResourceManager.instance.GetAudio("interact").audio_clip);
                _playerStates.InteractLogic();
                a_input = false;
            }
        }


        void GetInput()
        {
            
            vertical = Input.GetAxis(GlobalStrings.Vertical);
            horizontal = Input.GetAxis(GlobalStrings.Horizontal);
            

            if (Input.GetKey(KeyCode.W))
                vertical = 1;
            if (Input.GetKey(KeyCode.S))
                vertical = -1;
            if (Input.GetKey(KeyCode.D))
                horizontal = 1;
            if (Input.GetKey(KeyCode.A))
                horizontal = -1;

            b_input = Input.GetButton(GlobalStrings.B); //连击问题可以使用counter解决
            a_input = Input.GetButtonDown(GlobalStrings.A);
            x_input = Input.GetButton(GlobalStrings.X);
            y_input = Input.GetButton(GlobalStrings.Y);

            // rb_input = Input.GetButton(GlobalStrings.RB);
            if (Input.GetButtonDown(GlobalStrings.RB))
                _playerStates.rb = true;
            if (Input.GetButtonDown(GlobalStrings.LB))
                _playerStates.lb = true;
            if (Input.GetButtonDown(GlobalStrings.RT))
                _playerStates.rt = true;
            if (Input.GetButtonDown(GlobalStrings.LT))
                _playerStates.lt = true;

            
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
            _playerStates.vertical = vertical;
            _playerStates.horizontal = horizontal;

            _playerStates.itemInput = x_input;
            // states.rt = rt_input;
            // states.lt = lt_input;
            // states.rb = rb_input;
            // states.lb = lb_input;


            // moveDir
            Vector3 v = _playerStates.vertical * camManager.transform.forward;
            Vector3 h = _playerStates.horizontal * camManager.transform.right;
            _playerStates.moveDir = (v + h).normalized;

            // moveAmount
            float m = Mathf.Abs(_playerStates.horizontal) + Mathf.Abs(_playerStates.vertical);
            _playerStates.moveAmount = Mathf.Clamp01(m);

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
                if (_playerStates.pickManager.itemCandidate && _playerStates.pickManager.interactionCandidate)
                {
                    preferItem = !preferItem;
                }
                else
                {
                    _playerStates.isTwoHanded = !_playerStates.isTwoHanded;
                    _playerStates.HandleTwoHanded();
                }
            }

            if (_playerStates.lockOnTarget != null)
            {
                if (_playerStates.lockOnTarget.eStates.isDead)
                {
                    _playerStates.lockOn = false;
                    _playerStates.lockOnTarget = null;
                    _playerStates.lockOnTransform = null;
                    camManager.lockOn = false;
                    camManager.lockOnTarget = null;
                }
            }
            else
            {
                _playerStates.lockOn = false;
                _playerStates.lockOnTarget = null;
                _playerStates.lockOnTransform = null;
                camManager.lockOn = false;
                camManager.lockOnTarget = null;
            }


            if (Input.GetButtonDown(GlobalStrings.R))
            {
                _playerStates.lockOn = !_playerStates.lockOn;
                _playerStates.lockOnTarget = EnemyManager.instance.GetEnemy(transform.position);
                if (_playerStates.lockOnTarget == null)
                    _playerStates.lockOn = false;

                camManager.lockOnTarget = _playerStates.lockOnTarget;
                // 单个目标有多个锁定点的处理
                _playerStates.lockOnTransform = _playerStates.lockOnTarget.GetTarget();
                camManager.lockOnTransform = _playerStates.lockOnTransform ;
                // 保证相机/角色的锁定状态一致
                camManager.lockOn = _playerStates.lockOn;
            }


            if (x_input)
                b_input = false;

            HandleQuickSlotChanges();
        }
        
        private bool runMaker = true;

        void FixedUpstaeStates()
        {
            // B_input: 
            if (b_input && b_timer > 0.5f)
            {
                if ((_playerStates.moveAmount > 0.8f) && _playerStates.characterStats._stamina > 1 && runMaker)
                {
                    
                    _playerStates.run = true;
                }
                // states.run = (states.moveAmount > 0.8f) && states.characterStats._stamina > 0;
            }

            if (b_input == false && b_timer > 0 && b_timer < 0.5f)
            {
                _playerStates.rollInput = true;
                
            }

            if (_playerStates.characterStats._stamina <= 1)
            {
                runMaker = false;
            }
                
            
        }

        void HandleQuickSlotChanges()
        {
            // if (_playerStates.isSpellCasting || _playerStates.usingItem)
            //     return;
            //
            // if (d_up)
            // {
            //     if (!p_d_up)
            //     {
            //         p_d_up = true;
            //         _playerStates.inventoryManager.ChangeToNextSpell();
            //     }
            // }
            //
            // if (!d_up)
            //     p_d_up = false;
            //
            // if (d_down)
            // {
            //     if (!p_d_down)
            //     {
            //         p_d_down = true;
            //         _playerStates.inventoryManager.ChangeToNextConsumable();
            //     }
            // }
            //
            // if (!d_up)
            //     p_d_down = false;
            //
            // if (_playerStates.onEmpty == false)
            //     return;
            //
            // if (_playerStates.isTwoHanded)
            //     return;
            //
            // if (d_left)
            // {
            //     if (!p_d_left)
            //     {
            //         _playerStates.inventoryManager.ChangeToNextWeapon(true);
            //         p_d_left = true;
            //     }
            // }
            //
            // if (d_right)
            // {
            //     if (!p_d_right)
            //     {
            //         _playerStates.inventoryManager.ChangeToNextWeapon(false);
            //         p_d_right = true;
            //     }
            // }
            //
            //
            // if (!d_down)
            //     p_d_down = false;
            // if (!d_left)
            //     p_d_left = false;
            // if (!d_right)
            //     p_d_right = false;
            
            if (_playerStates.isSpellCasting || _playerStates.usingItem)
                return;

            // 检测当前帧是否为刚按下，而不是一直按住
            bool newlyUp = d_up && !p_d_up;
            bool newlyDown = d_down && !p_d_down;
            bool newlyLeft = d_left && !p_d_left;
            bool newlyRight = d_right && !p_d_right;

            // 更新上一次的状态
            p_d_up = d_up;
            p_d_down = d_down;
            p_d_left = d_left;
            p_d_right = d_right;

            // 如果刚刚按下了上键/1键，切换法术
            if (newlyUp)
                _playerStates.inventoryManager.ChangeToNextSpell();

            // 如果刚刚按下了下键/2键，切换消耗品
            if (newlyDown)
                _playerStates.inventoryManager.ChangeToNextConsumable();

            // 如果不为空手或者是双持状态，就不进行武器切换
            if (!_playerStates.onEmpty || _playerStates.isTwoHanded)
                return;

            // 刚刚按下了左键/3键，切换左手武器
            if (newlyLeft)
                _playerStates.inventoryManager.ChangeToNextWeapon(true);

            // 刚刚按下了右键/4键，切换右手武器
            if (newlyRight)
                _playerStates.inventoryManager.ChangeToNextWeapon(false);
        }

        void FixedResetInputNState()
        {
            // reset b_timer when b_input is released from keyboard
            if (b_input == false)
            {
                b_timer = 0;
                runMaker = true;
            }
                
        }

        void ResetInputNState()
        {
            // turn off rollInput and run state after being pressed.
            if (_playerStates.rollInput)
                _playerStates.rollInput = false;
            if (Input.GetButtonUp(GlobalStrings.B) || _playerStates.characterStats._stamina <= 1)
                _playerStates.run = false;
        }
        
        public void ResetInputs()
        {
            // Reset float parameters to 0
            vertical = 0f;
            horizontal = 0f;
            rt_axis = 0f;
            lt_axis = 0f;
            d_y = 0f;
            d_x = 0f;

            // Reset boolean parameters to false
            b_input = false;
            a_input = false;
            x_input = false;
            y_input = false;
            rb_input = false;
            lb_input = false;
            rt_input = false;
            lt_input = false;
            d_up = false;
            d_down = false;
            d_right = false;
            d_left = false;
            p_d_up = false;
            p_d_down = false;
            p_d_left = false;
            p_d_right = false;
            leftAxis_down = false;
            rightAxis_down = false;
        }
    }
}