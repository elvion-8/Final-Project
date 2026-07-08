using System.Collections.Generic;
using UnityEngine;

public enum CharacterStatType
{
    MaxHP,
    WeaponDamage,
    CritProbability,
    CritDamage,
    AttackSpeed,
    MoveSpeed,
    RunSpeed,
    JumpCount,
    JumpPower,
    ClimbSpeed,
    LifeSteal
}

public class PlayerStatController : MonoBehaviour
{
    [System.Serializable]
    public class ActiveBuff
    {
        public CharacterStatType StatType;
        public float Value;
        public float Duration; // 0 or less means permanent for the run

        public ActiveBuff(CharacterStatType statType, float value, float duration)
        {
            StatType = statType;
            Value = value;
            Duration = duration;
        }
    }

    private static List<ActiveBuff> activeRunBuffs = new List<ActiveBuff>();

    public static System.Action OnStatsChanged;

    private scPlayerStat permanentStat;
    private PhotonView pv;

    // Base values
    public const float BASE_MAX_HP = 100f;
    public const float HP_PER_UPGRADE = 50f;
    public const float BASE_ATTACK = 100f;
    public const float ATTACK_PER_UPGRADE = 20f;
    public const float BASE_CRIT_PROB = 5f;
    public const float CRIT_PROB_PER_UPG = 1.5f;
    public const float BASE_CRIT_DMG = 120f;
    public const float CRIT_DMG_PER_UPG = 8f;
    public const float BASE_ATTACK_SPD = 1.0f;
    public const float ATTACK_SPD_PER_UPG = 0.1f;

    // Movement bases
    public const float BASE_WALK_SPEED = 1.3f;
    public const float BASE_RUN_SPEED = 6f;
    public const float BASE_JUMP_POWER = 9.0f;
    public const float BASE_JUMP_FORWARD = 6.5f;
    public const int BASE_JUMP_COUNT = 1;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        LoadPermanentStats();
    }

    public void LoadPermanentStats()
    {
        if (Managers.Instance != null && Managers.Data != null)
        {
            permanentStat = Managers.Data.stat;
        }
        else
        {
            permanentStat = new scPlayerStat();
        }
    }

    void Update()
    {
        if (pv == null || pv.isMine || !PhotonNetwork.inRoom)
        {
            UpdateBuffs(Time.deltaTime);
        }
    }

    private void UpdateBuffs(float deltaTime)
    {
        bool changed = false;
        for (int i = activeRunBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeRunBuffs[i];
            if (buff.Duration > 0f)
            {
                buff.Duration -= deltaTime;
                if (buff.Duration <= 0f)
                {
                    activeRunBuffs.RemoveAt(i);
                    changed = true;
                }
            }
        }

        if (changed)
        {
            OnStatsChanged?.Invoke();
        }
    }

    // Apply a new buff to the static run buffs
    public void ApplyBuff(CharacterStatType statType, float value, float duration)
    {
        ActiveBuff newBuff = new ActiveBuff(statType, value, duration);
        activeRunBuffs.Add(newBuff);
        OnStatsChanged?.Invoke();
        Debug.Log($"Applied temporary buff: {statType} +{value} (Duration: {duration}s)");
    }

    // Remove a matching buff modifier
    public void RemoveBuff(CharacterStatType statType, float value)
    {
        for (int i = 0; i < activeRunBuffs.Count; i++)
        {
            if (activeRunBuffs[i].StatType == statType && Mathf.Approximately(activeRunBuffs[i].Value, value))
            {
                activeRunBuffs.RemoveAt(i);
                OnStatsChanged?.Invoke();
                Debug.Log($"Removed buff: {statType} with value {value}");
                break;
            }
        }
    }

    public static void ClearRunBuffs()
    {
        activeRunBuffs.Clear();
        OnStatsChanged?.Invoke();
        Debug.Log("Cleared all temporary run buffs.");
    }

    public float GetBuffValue(CharacterStatType statType)
    {
        float total = 0f;
        foreach (var buff in activeRunBuffs)
        {
            if (buff.StatType == statType)
            {
                total += buff.Value;
            }
        }
        return total;
    }

    // --- Final Stat Calculations ---

    public float MaxHP
    {
        get
        {
            float permUpgrade = permanentStat != null ? permanentStat.hpUpgrade : 0;
            return BASE_MAX_HP + (permUpgrade * HP_PER_UPGRADE) + GetBuffValue(CharacterStatType.MaxHP);
        }
    }

    // 무기 데미지 관련
    public int GetWeaponDamage(int baseWeaponDmg)
    {
        int permUpgrade = permanentStat != null ? permanentStat.weaponDmgUpgradeCnt : 0;
        float tempBuff = GetBuffValue(CharacterStatType.WeaponDamage);
        return Mathf.RoundToInt(baseWeaponDmg + (permUpgrade * 5) + tempBuff);
    }

    // 치확
    public int GetCritProbability(int baseCritProb)
    {
        int permUpgrade = permanentStat != null ? permanentStat.weaponCritProbUpgradeCnt : 0;
        float tempBuff = GetBuffValue(CharacterStatType.CritProbability);
        return Mathf.RoundToInt(baseCritProb + permUpgrade + tempBuff);
    }

    // 치피
    public int GetCritDamage(int baseCritDmg)
    {
        int permUpgrade = permanentStat != null ? permanentStat.weaponCritDmgUpgradeCnt : 0;
        float tempBuff = GetBuffValue(CharacterStatType.CritDamage);
        return Mathf.RoundToInt(baseCritDmg + permUpgrade + tempBuff);
    }

    // 공속
    public float GetWeaponAttackSpeed(float baseAttackSpeed)
    {
        int permUpgrade = permanentStat != null ? permanentStat.weaponAttackSpeed : 0;
        float tempBuff = GetBuffValue(CharacterStatType.AttackSpeed);
        return baseAttackSpeed + permUpgrade + tempBuff;
    }

    // 애니메이션 재생속도 /// 아직 미사용
    public float AttackSpeedMultiplier
    {
        get
        {
            float permUpgrade = permanentStat != null ? permanentStat.weaponAttackSpeed : 0;
            float tempBuff = GetBuffValue(CharacterStatType.AttackSpeed);
            return 1.0f + (permUpgrade * ATTACK_SPD_PER_UPG) + tempBuff;
        }
    }

    public float ClimbSpeedMultiplier
    {
        get
        {
            return 1.0f + GetBuffValue(CharacterStatType.ClimbSpeed);
        }
    }

    // Movement parameters
    public float MoveSpeed
    {
        get
        {
            return BASE_WALK_SPEED + GetBuffValue(CharacterStatType.MoveSpeed);
        }
    }

    public float RunSpeed
    {
        get
        {
            return BASE_RUN_SPEED + GetBuffValue(CharacterStatType.MoveSpeed) + GetBuffValue(CharacterStatType.RunSpeed);
        }
    }

    public float JumpPower
    {
        get
        {
            return BASE_JUMP_POWER + GetBuffValue(CharacterStatType.JumpPower);
        }
    }

    public int JumpCount
    {
        get
        {
            return BASE_JUMP_COUNT + Mathf.RoundToInt(GetBuffValue(CharacterStatType.JumpCount));
        }
    }

    // UI 스텟 표기 용
    public int HpUpgradeCnt => permanentStat != null ? permanentStat.hpUpgrade : 0;
    public int WeaponDmgUpgradeCnt => permanentStat != null ? permanentStat.weaponDmgUpgradeCnt : 0;
    public int WeaponCritProbUpgradeCnt => permanentStat != null ? permanentStat.weaponCritProbUpgradeCnt : 0;
    public int WeaponCritDmgUpgradeCnt => permanentStat != null ? permanentStat.weaponCritDmgUpgradeCnt : 0;
    public int WeaponAttackSpeedUpgradeCnt => permanentStat != null ? permanentStat.weaponAttackSpeed : 0;
    public int CurrentCost => permanentStat != null ? permanentStat.cost : 0;
}
