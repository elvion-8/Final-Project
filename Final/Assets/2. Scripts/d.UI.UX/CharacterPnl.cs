using UnityEngine;
using UnityEngine.UI;

public class CharacterPnl : MonoBehaviour
{
    [Header("참조")]
    private PlayerStatManage playerStatManage;

    [Header("상단 정보")]
    public Text userNameText;

    [Header("HP")]
    public Slider hpSlider;
    public Text hpText;

    [Header("SANc")]
    public Slider SANcSlider;
    public Text SANcText;

    [Header("기본 능력치")]
    public Text attackText;
    public Text defenseText;
    public Text critProbText;
    public Text critDmgText;
    public Text attackSpeedText;
    public Text moveSpeedText;

    // ── 수치 공식 상수 (필요시 조정) ──────────────────────────────
    private const float BASE_MAX_HP        = 1000f;
    private const float HP_PER_UPGRADE     = 50f;

    private const float BASE_ATTACK        = 100f;
    private const float ATTACK_PER_UPGRADE = 20f;

    private const float BASE_DEFENSE       = 100f;

    private const float BASE_CRIT_PROB     = 5f;
    private const float CRIT_PROB_PER_UPG  = 1.5f;

    private const float BASE_CRIT_DMG      = 120f;
    private const float CRIT_DMG_PER_UPG   = 8f;

    private const float BASE_ATTACK_SPD    = 1.0f;
    private const float ATTACK_SPD_PER_UPG = 0.1f;

    private const float BASE_MOVE_SPEED    = 5f;
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // 씬에서 PlayerStatManage 자동 탐색
        playerStatManage = FindObjectOfType<PlayerStatManage>();

        if (playerStatManage == null)
            Debug.LogWarning("PlayerStatManage를 찾을 수 없습니다.");
    }

    void OnEnable()
    {
        RefreshUI();
        PlayerStatController.OnStatsChanged += RefreshUI;
    }

    void OnDisable()
    {
        PlayerStatController.OnStatsChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        if (playerStatManage == null) return;
        var stat = playerStatManage.stat;

        // Try to get active player stats in scene
        PlayerCtrl player = FindObjectOfType<PlayerCtrl>();
        PlayerStatController statCtrl = player != null ? player.statController : null;

        // ── 유저네임 ──────────────────────────────────────────────
        //public string userName;   // stat에 userName 필드 추가 후 연결
        if (userNameText != null)
            userNameText.text = "플레이어";

        // ── HP ───────────────────────────────────────────────────
        //public float currentHp;    // stat에 currentHp 필드 추가 후 연결
        float maxHp = statCtrl != null ? statCtrl.MaxHP : (BASE_MAX_HP + stat.hpUpgrade * HP_PER_UPGRADE);
        float curHp = player != null ? player.hp : maxHp;

        if (hpSlider != null) { hpSlider.maxValue = maxHp; hpSlider.value = curHp; }
        if (hpText   != null) hpText.text = $"{curHp:0} / {maxHp:0}";

        // ── SANc ─────────────────────────────────────────────────
        // stat에 SANc 필드 추가 후 연결: public float SANc;
        float SANc = 0f; // SANc 필드 추가 후 stat.SANc 로 교체

        if (SANcSlider != null) { SANcSlider.maxValue = 100f; SANcSlider.value = SANc; }
        if (SANcText   != null) SANcText.text = $"{SANc:0}%";

        // ── 기본 능력치 ───────────────────────────────────────────
        float attack, defense, critProb, critDmg, atkSpeed, moveSpeed;

        if (statCtrl != null)
        {
            attack = BASE_ATTACK + statCtrl.WeaponDmgUpgradeCnt * ATTACK_PER_UPGRADE + statCtrl.GetBuffValue(CharacterStatType.WeaponDamage);
            defense = BASE_DEFENSE;
            critProb = BASE_CRIT_PROB + statCtrl.WeaponCritProbUpgradeCnt * CRIT_PROB_PER_UPG + statCtrl.GetBuffValue(CharacterStatType.CritProbability);
            critDmg = BASE_CRIT_DMG + statCtrl.WeaponCritDmgUpgradeCnt * CRIT_DMG_PER_UPG + statCtrl.GetBuffValue(CharacterStatType.CritDamage);
            atkSpeed = BASE_ATTACK_SPD + statCtrl.WeaponAttackSpeedUpgradeCnt * ATTACK_SPD_PER_UPG + statCtrl.GetBuffValue(CharacterStatType.AttackSpeed);
            moveSpeed = BASE_MOVE_SPEED + statCtrl.GetBuffValue(CharacterStatType.MoveSpeed);
        }
        else
        {
            attack = BASE_ATTACK + stat.weaponDmgUpgradeCnt * ATTACK_PER_UPGRADE;
            defense = BASE_DEFENSE;
            critProb = BASE_CRIT_PROB + stat.weaponCritProbUpgradeCnt * CRIT_PROB_PER_UPG;
            critDmg = BASE_CRIT_DMG + stat.weaponCritDmgUpgradeCnt * CRIT_DMG_PER_UPG;
            atkSpeed = BASE_ATTACK_SPD + stat.weaponAttackSpeed * ATTACK_SPD_PER_UPG;
            moveSpeed = BASE_MOVE_SPEED;
        }

        if (attackText      != null) attackText.text      = $"{attack:0}";
        if (defenseText     != null) defenseText.text     = $"{defense:0}";
        if (critProbText    != null) critProbText.text    = $"{critProb:0.0}%";
        if (critDmgText     != null) critDmgText.text     = $"{critDmg:0.0}%";
        if (attackSpeedText != null) attackSpeedText.text = $"{atkSpeed:0.0}";
        if (moveSpeedText   != null) moveSpeedText.text   = $"{moveSpeed:0.0}";
    }
}