using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    PhotonView pv;
    private PlayerInput _playerInput;
    #region 키 조작을 위한 변수
    public Vector2 move;
    public bool jumpKey = false;
    public bool isJumpHeld = false;
    public bool attackKey = false;
    public bool rollingKey = false;
    public Vector2 WeaponChange;
    public bool inventoryKey = false;
    public Vector2 screenMove;
    public bool runKey = false;
    public bool lockOnKey = false;
    public bool selectKey = false;
    public bool escapeKey = false;
    //------------------inventory
    public bool one;
    public bool two;
    public bool three;
    public bool four;
    private csInvenManager inven;
    public Vector2 _moveInputX;
    public bool onInven;
    public AttackMotion atm;

    //-=---------------------------
    #endregion
    #region Input callback
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
        if (!inven.pnlInven.activeSelf) return;
        _moveInputX.x = value.Get<Vector2>().x;
        if (Mathf.Abs(_moveInputX.x) > 0.7f)
        {
            inven.HandleInventorySelect(inven._selectedSlot);
        }
        else if (_moveInputX.x < 0.2f)
        {
            inven.movingInv = false;
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed) jumpKey = true;
        isJumpHeld = value.isPressed;
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed) attackKey = true;
    }
    public void OnRolling(InputValue value)
    {
        if (value.isPressed) rollingKey = true;
    }
    public void OnWeaponChange(InputValue value)
    {
        // if (pv == null)
        // {
        //     pv = GameObject.Find("Player").GetComponent<PhotonView>();
        // }
        if (pv.isMine || !PhotonNetwork.inRoom)
        {
            float weaponIndex = value.Get<float>();

            switch (weaponIndex)
            {
                case 1:
                    one = true;
                    break;

                case 2:
                    two = true;
                    break;

                case 3:
                    three = true;
                    break;

                case 4:
                    four = true;
                    break;
            }
            if (GameObject.FindWithTag("Player").GetComponent<PlayerCtrl>().isAttacking == false)
            {
                weaponIndex = value.Get<float>();
                if (weaponIndex == 0) return;
                Debug.Log(weaponIndex);

                ItemData hotbarItem = csInvenManager.Instance.GetHotbarItem((int)weaponIndex - 1);
                if (hotbarItem == null || hotbarItem.itemPrefab == null) return;

                int prefabIndex = -1;

                for (int i = 0; i < atm.weaponPrefabs.Length; i++)
                {
                    if (atm.weaponPrefabs[i] == hotbarItem.itemPrefab)
                    {
                        prefabIndex = i;
                        break;
                    }
                }

                if (prefabIndex == -1)
                {
                    Debug.LogError("weaponPrefabs 배열에 등록되지 않은 무기 프리팹입니다!");
                    return;
                }

                if (PhotonNetwork.inRoom)
                {
                    pv.RPC("EquipWeapon", PhotonTargets.AllBuffered, prefabIndex);
                }
                else atm.EquipWeapon(prefabIndex);

            }
        }
    }
    public void OnInventory(InputValue value)
    {
        if (value.isPressed)
        {
            if (!inventoryKey) onInven = true;
            else if (inven.pnlInven.activeSelf) { inven.CloseInventory(); }
        }
    }
    public void OnScreenMove(InputValue value)
    {
        screenMove = value.Get<Vector2>();
    }
    public void OnRun(InputValue value) { runKey = value.isPressed; }
    public void OnLockOn(InputValue value) { }
    public void OnSelect(InputValue value) { }
    public void OnEscape(InputValue value) { }
    #endregion

    public void Init()
    {
        _playerInput = gameObject.GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            _playerInput = gameObject.AddComponent<PlayerInput>();
        }
        InputActionAsset action = Resources.Load<InputActionAsset>("Player");
        if (action != null)
        {
            _playerInput.actions = action;
            _playerInput.defaultActionMap = "player";
        }
        else
        {
            Debug.Log("[InputManager]Resources 폴더에 액션에셋 없음.");
        }
    }
    public void ResetKey()
    {
        jumpKey = false;
        attackKey = false;
        rollingKey = false;
        inventoryKey = false;
        //runKey = false;
        //lockOnKey = false;
        //selectKey = false;
        //escapeKey = false;
    }
    void Awake()
    {
        inven = GameObject.Find("Inventory").GetComponent<csInvenManager>();

    }
    // Start is called before the first frame update
    void Start()
    {
        // atm = GameObject.FindGameObjectWithTag("Player").GetComponent<AttackMotion>();  //아직 안됨. 호출 순서가 달라서 그런 듯
        // pv = GameObject.FindGameObjectWithTag("Player").GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}