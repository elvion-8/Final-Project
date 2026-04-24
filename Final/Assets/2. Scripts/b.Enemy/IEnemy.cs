#define CBT_MODE
//#define RELEASE_MODE

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Rand = UnityEngine.Random;

// 필요한 컴포넌트를 위해 어트리뷰트 선언
//[RequireComponent(typeof(AudioSource))]
//[AddComponentMenu("IEnemy")]
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
}

[System.Serializable]
public class Anim
{
    public AnimationClip idle1;
    public AnimationClip idel2;
    public AnimationClip idel3;
    public AnimationClip attack1;
    public AnimationClip attack2;
    public AnimationClip attack3;
    public AnimationClip attack4;
    public AnimationClip hit1;
    public AnimationClip transform1;
    public AnimationClip transform2;
    public AnimationClip die;
}

public class Enemy : MonoBehaviour
{
    // 애니메이션 어트리뷰트
    [Space(10)]
    [Header("ANIMATION")]   
    public Anim anims;
    private Animation _anim;
    AnimationState animState;
    private float randAnimTime;
    private int randAnim;

    // NavMeshAgent 레퍼런
    private NavMeshAgent myTraceAgent;

    //자신과 타겟 Transform 참조 변수  
    private Transform myTr;
    private Transform traceTarget;

    //추적을 위한 변수
    private bool traceObject;
    private bool traceAttack;

    //추적 대상 거리체크 변수 
    float dist1;
    float dist2;

    //플레이어를 찾기 위한 배열 
    private GameObject[] players;
    private Transform playerTarget;

    [HideInInspector]
    //죽었는지 상태변수 
    public bool isDie;

    // Enemy의 현재 상태정보를 위한 Enum 자료형 선언  
    public enum MODE_STATE { IDLE = 1, MOVE, SURPRISE, TRACE, ATTACK, HIT, EAT, SLEEP, DIE };

    // Enemy의 종류 정보를 위한 Enum 자료형 선언  
    public enum MODE_KIND { ENEMY_1 = 1, ENEMY_2, ENEMY_BOSS };

    //인스펙터의 헤더의 표현을 위한 어트리뷰트 선언
    [Header("몬스터 상태")]
    //인스펙터의 헤더의 표현을 위한 어트리뷰트 선언
    [Header("STATE")]
    //Enemy의 상태 셋팅
    public MODE_STATE enemyMode = MODE_STATE.IDLE;

    // 인스펙터의 헤더의 표현을 위한 어트리뷰트 선언
    [Header("SETTING")]
    // Enemy종류 셋팅
    public MODE_KIND enemyKind = MODE_KIND.ENEMY_BOSS;

    // 인스펙터의 헤더의 표현을 위한 어트리뷰트 선언
    [Header("몬스터 인공지능")]
    //변수들의 간격을 위한 어트리뷰트 선언(보기 좋다)
    [Space(10)]
    //변수에 팁을 달아줄 수  있다.(인스펙터에서 확인)
    [Tooltip("몬스터의 HP")]
    [Range(0, 1000)] public int hp = 1000;

    //거리에 따른 상태 체크 변수 
    [Tooltip("몬스터 발견거리!!!")]
    [Range(10f, 30f)][SerializeField] float findDist = 20.0f;
    [Tooltip("몬스터 공격거리!!!")]
    [Range(1f, 30f)][SerializeField] float attackDist = 20.0f;

    [Header("TEST")]
    [SerializeField] private bool isHit;
    private float isHitTime;

    // 리지드바디
    Rigidbody rbody;

    void Awake()
    {
        //레퍼런스 할당 
        myTraceAgent = GetComponent<NavMeshAgent>();
        //자신의 자식에 있는 Animation 컴포넌트를 찾아와 레퍼런스에 할당 
        _anim = GetComponentInChildren<Animation>();
        //자기 자신의 Transform 연결
        myTr = GetComponent<Transform>();

        // 리지드바디 연결
        rbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //myTraceAgent.SetDestination(traceTarget.position);

        //랜덤 애니메이션 선택 
        if (Time.time > randAnimTime)
        {
            randAnim = Rand.Range(0, 4);
            randAnimTime = Time.time + 5.0f;
        }

        //공격 받았을 경우 
        if (isHit)
        {
           if (Time.time > isHitTime)
            {
               isHit = false;
            }
         }
    }

    IEnumerator ModeSet()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);

            //자신과 Player의 거리 셋팅 
            float dist = Vector3.Distance(myTr.position, traceTarget.position);

            if (isHit)  //공격 받았을시
            {
                enemyMode = MODE_STATE.HIT;
            }
            else if (dist <= attackDist) // Attack 사거리에 들어왔는지 ??
            {
                enemyMode = MODE_STATE.ATTACK; //몬스터의 상태를 공격으로 설정 
            }
            else
            {
                enemyMode = MODE_STATE.IDLE; //몬스터의 상태를 idle 모드로 설정 
            }
        }
    }

    IEnumerator TargtSetting()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);

            // 자신과 가장 가까운 플레이어 찾음
            players = GameObject.FindGameObjectsWithTag("Player");

            //플레이어가 있을경우 
            if (players.Length != 0)
            {
                playerTarget = players[0].transform;
                dist1 = (playerTarget.position - myTr.position).sqrMagnitude;
                foreach (GameObject _players in players)
                {
                    if ((_players.transform.position - myTr.position).sqrMagnitude < dist1)
                    {
                        playerTarget = _players.transform;
                        dist1 = (playerTarget.position - myTr.position).sqrMagnitude;
                    }
                }
            }
        }
    }

    // 몬스터 사망 처리
    public void EnemyDie()
    {
        StartCoroutine(this.Die());
    }

    // Enemy의 사망 처리
    IEnumerator Die()
    {
        // Enemy를 죽이자
        isDie = true;
        // 죽는 애니메이션 시작
        _anim.CrossFade(anims.die.name, 0.3f);
        // Enemy의 모드를 die로 설정
        enemyMode = MODE_STATE.DIE;
        // Enemy의 태그를 Untagged로 변경하여 더이상 플레이어랑 포탑이 찾지 못함
        this.gameObject.tag = "Untagged";
        this.gameObject.transform.Find("EnemyBody").tag = "Untagged";
        //네비게이션 멈추고 (추적 중지)
        myTraceAgent.isStopped = true;

        // Enemy에 추가된 모든 Collider를 비활성화(모든 충돌체는 Collider를 상속했음 따라서 다음과 같이 추출 가능)
        foreach (Collider coll in gameObject.GetComponentsInChildren<Collider>())
        {
            coll.enabled = false;
        }

        // 4.5초 후 오브젝트 삭제
        yield return new WaitForSeconds(4.5f);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
       // Debug.Log("Destroy");
        // 모든 코루틴을 정지시키자
        StopAllCoroutines();
    }

    //인스펙터에 스크립트 우 클릭시 컨텍스트 메뉴에서 함수호출 가능
    [ContextMenu("FuncStart")]
    void FuncStart()
    {
        Debug.Log("Func start"); 
    }  

}
