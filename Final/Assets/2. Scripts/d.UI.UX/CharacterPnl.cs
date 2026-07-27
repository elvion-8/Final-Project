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
        if (playerStatManage == null)
        {
            playerStatManage = FindObjectOfType<PlayerStatManage>();
        }

        scPlayerStat stat = null;
        if (playerStatManage != null)
        {
            stat = playerStatManage.stat;
        }
        else if (Managers.Instance != null && Managers.Data != null)
        {
            stat = Managers.Data.stat;
        }

        if (stat == null) return;

        // Try to get active player stats in scene
        PlayerCtrl player = PlayerCtrl.localPlayer;
        PlayerStatController statCtrl = player != null ? player.statController : null;

        // ── 유저네임 ──────────────────────────────────────────────
        if (userNameText != null)
            userNameText.text = "플레이어";

        // ── HP ───────────────────────────────────────────────────
        float maxHp = statCtrl != null ? statCtrl.MaxHP : (PlayerStatController.BASE_MAX_HP + stat.hpUpgrade * PlayerStatController.HP_PER_UPGRADE);
        float curHp = player != null ? player.hp : maxHp;

        if (hpSlider != null) { hpSlider.maxValue = maxHp; hpSlider.value = curHp; }
        if (hpText   != null) hpText.text = $"{curHp:0} / {maxHp:0}";

        // ── SANc ─────────────────────────────────────────────────
        float SANc = 0f;

        if (SANcSlider != null) { SANcSlider.maxValue = 100f; SANcSlider.value = SANc; }
        if (SANcText   != null) SANcText.text = $"{SANc:0}%";

        // ── 기본 능력치 (PlayerStatController SSOT 참조) ───────────────────
        float attack, defense, critProb, critDmg, atkSpeed, moveSpeed;

        if (statCtrl != null)
        {
            attack = statCtrl.TotalAttack;
            defense = statCtrl.TotalDefense;
            critProb = statCtrl.TotalCritProb;
            critDmg = statCtrl.TotalCritDmg;
            atkSpeed = statCtrl.TotalAttackSpeed;
            moveSpeed = statCtrl.MoveSpeed;
        }
        else
        {
            attack = PlayerStatController.BASE_ATTACK + stat.weaponDmgUpgradeCnt * PlayerStatController.ATTACK_PER_UPGRADE;
            defense = PlayerStatController.BASE_DEFENSE;
            critProb = PlayerStatController.BASE_CRIT_PROB + stat.weaponCritProbUpgradeCnt * PlayerStatController.CRIT_PROB_PER_UPG;
            critDmg = PlayerStatController.BASE_CRIT_DMG + stat.weaponCritDmgUpgradeCnt * PlayerStatController.CRIT_DMG_PER_UPG;
            atkSpeed = PlayerStatController.BASE_ATTACK_SPD + stat.weaponAttackSpeed * PlayerStatController.ATTACK_SPD_PER_UPG;
            moveSpeed = PlayerStatController.BASE_WALK_SPEED;
        }

        if (attackText      != null) attackText.text      = $"{attack:0}";
        if (defenseText     != null) defenseText.text     = $"{defense:0}";
        if (critProbText    != null) critProbText.text    = $"{critProb:0.0}%";
        if (critDmgText     != null) critDmgText.text     = $"{critDmg:0.0}%";
        if (attackSpeedText != null) attackSpeedText.text = $"{atkSpeed:0.0}";
        if (moveSpeedText   != null) moveSpeedText.text   = $"{moveSpeed:0.0}";
    }
}