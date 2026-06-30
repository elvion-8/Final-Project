using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class AttackAnimationPreviewer : MonoBehaviour
{
    public Animator animator;
    
    [HideInInspector] // 인스펙터에서 숨기고 커스텀 에디터에서 드롭다운으로 표시
    public int selectedClipIndex = 0;
    
    [Range(0f, 1f), Tooltip("애니메이션 진행도 미리보기 슬라이더")]
    public float previewNormalizedTime = 0f;

    // 에디터에서 사용할 클립 리스트 캐싱용
    [HideInInspector] public List<AnimationClip> availableClips = new List<AnimationClip>();
    [HideInInspector] public string[] clipNames = new string[0];

    private void OnValidate()
    {
        if (animator == null) animator = GetComponent<Animator>();
        UpdateClipList();
        UpdatePreview();
    }

    // 애니메이터 컨트롤러에서 클립들을 자동으로 추출하는 함수
    public void UpdateClipList()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            availableClips.Clear();
            clipNames = new string[0];
            return;
        }

        // 중복 제거를 위해 HashSet 사용
        HashSet<AnimationClip> clipsSet = new HashSet<AnimationClip>();
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null) clipsSet.Add(clip);
        }

        availableClips = new List<AnimationClip>(clipsSet);
        
        // 드롭다운에 표시할 이름 배열 생성
        clipNames = new string[availableClips.Count];
        for (int i = 0; i < availableClips.Count; i++)
        {
            clipNames[i] = availableClips[i].name;
        }
    }

    public void UpdatePreview()
    {
        if (animator == null || availableClips.Count == 0 || selectedClipIndex >= availableClips.Count) return;

        AnimationClip currentClip = availableClips[selectedClipIndex];
        if (currentClip == null) return;

        // 에디터 모드에서 선택된 애니메이션 강제 샘플링
        currentClip.SampleAnimation(animator.gameObject, previewNormalizedTime * currentClip.length);
    }
}

// ---- 기획자를 위한 커스텀 인스펙터 에디터 ----
#if UNITY_EDITOR
[CustomEditor(typeof(AttackAnimationPreviewer))]
public class AttackAnimationPreviewerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AttackAnimationPreviewer previewer = (AttackAnimationPreviewer)target;

        // 기본 인스펙터 요소 그리기 (animator 변수 등)
        EditorGUI.BeginChangeCheck();
        previewer.animator = (Animator)EditorGUILayout.ObjectField("Animator", previewer.animator, typeof(Animator), true);
        if (EditorGUI.EndChangeCheck())
        {
            previewer.UpdateClipList();
        }

        GUILayout.Space(5);

        // 애니메이터 체크
        if (previewer.animator == null)
        {
            EditorGUILayout.HelpBox("Character에 Animator 컴포넌트를 할당해주세요.", MessageType.Warning);
            return;
        }
        if (previewer.animator.runtimeAnimatorController == null)
        {
            EditorGUILayout.HelpBox("Animator에 Animator Controller가 비어있습니다.", MessageType.Warning);
            return;
        }

        // 클립 리스트가 비어있다면 갱신 시도
        if (previewer.clipNames == null || previewer.clipNames.Length == 0)
        {
            previewer.UpdateClipList();
        }

        // 1. 애니메이션 클립 선택 드롭다운 팝업
        if (previewer.clipNames.Length > 0)
        {
            GUILayout.Label("미리볼 애니메이션 선택", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            previewer.selectedClipIndex = EditorGUILayout.Popup("Animation Clip", previewer.selectedClipIndex, previewer.clipNames);
            if (EditorGUI.EndChangeCheck())
            {
                previewer.UpdatePreview();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Animator Controller 내에 등록된 애니메이션 클립이 없습니다.", MessageType.Info);
            return;
        }

        GUILayout.Space(10);

        // 2. 타임라인 슬라이더
        GUILayout.Label("애니메이션 타임라인 제어", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        previewer.previewNormalizedTime = EditorGUILayout.Slider("Timeline (0 ~ 1)", previewer.previewNormalizedTime, 0f, 1f);
        if (EditorGUI.EndChangeCheck())
        {
            previewer.UpdatePreview();
        }

        // 3. 편리한 정보 출력 및 유틸리티 버튼
        AnimationClip activeClip = previewer.availableClips[previewer.selectedClipIndex];
        if (activeClip != null)
        {
            float currentFrame = previewer.previewNormalizedTime * activeClip.frameRate * activeClip.length;
            int totalFrames = Mathf.RoundToInt(activeClip.frameRate * activeClip.length);
            
            EditorGUILayout.HelpBox($"현재 위치: 약 {Mathf.RoundToInt(currentFrame)} 프레임 / 총 {totalFrames} 프레임", MessageType.Info);
        }

        GUILayout.Space(5);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("미리보기 초기화 (0프레임)"))
        {
            previewer.previewNormalizedTime = 0f;
            previewer.UpdatePreview();
        }
    }
}
#endif