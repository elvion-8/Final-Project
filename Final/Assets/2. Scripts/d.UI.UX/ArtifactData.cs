using UnityEngine;
 
/// <summary>
/// 아티팩트 데이터를 담은 ScriptableObject
/// Project 창에서 우클릭 → Create → Inventory/ArtifactData 로 생성
/// </summary>
/// 
[CreateAssetMenu(fileName = "NewArtifact", menuName = "Inventory/ArtifactData")]
public class ArtifactData : ScriptableObject
{
    [Header("기본 정보")]
    public string artifactName = "아이템 이름";
 
    [TextArea(2, 4)]
    public string description = "아이템 설명";
 
    public string weaponType = "크로노스 웨폰";   // 상단 카테고리 텍스트
 
    [Header("아이콘 스프라이트")]
    public Sprite iconNormal;    // 기본 상태 아이콘
    public Sprite iconLit;       // 선택 상태 아이콘
 
    [Header("슬롯 테두리 스프라이프")]
    public Sprite borderNormal;
    public Sprite borderLit;
}
 