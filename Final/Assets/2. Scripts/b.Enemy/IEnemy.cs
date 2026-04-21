using System.Collections;

public interface IEnemy
{
    int hp {get;}
    int criticalProb{get;}
    int criticalDmg{get;}
    float skillCoolDown{get;}
    float moveSpeed{get;}
    float jumpPower{get;}
    void Skill1();
    void Skill2();
    void Skill3();
    void Skill4();
    StartCoroutine(SkillCtrl());
    IEnumerator SkillCtrl()
    {
        while(true)
        {
            
        }
    }

}
