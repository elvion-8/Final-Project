using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(VFXEventModifier))]
public class VFXEventModifierEditor : Editor
{
    private VFXEventModifier targetScript;
    private Animator animator;
    private WeaponEffectManager effectManager;
    
    private AnimationClip[] clips;
    private string[] clipNames;
    private int selectedClipIndex = 0;
    private int selectedWeaponIndex = 0;

    private float currentFrame = 0f;
    private int startFrame = 0;
    private int endFrame = 0;
    private bool isPreviewing = false;
    private GameObject previewVFXInstance;
    private bool isFirstLoad = true; // 최초 실행 시 이벤트를 한 번 파싱하기 위한 플래그

    private void OnEnable()
    {
        targetScript = (VFXEventModifier)target;
        animator = targetScript.GetComponent<Animator>();
        effectManager = FindObjectOfType<WeaponEffectManager>();
        
        LoadAnimationClips();
        isFirstLoad = true;
    }

    private void OnDisable()
    {
        CleanUpPreview();
    }

    private void LoadAnimationClips()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        clips = animator.runtimeAnimatorController.animationClips;
        clipNames = new string[clips.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            clipNames[i] = clips[i].name;
        }
    }

    // [개선] 선택된 애니메이션 클립에서 기존 VFX 이벤트를 찾아 슬라이더 값을 복원하는 함수
    private void ParseExistingEvents(AnimationClip clip)
    {
        if (clip == null) return;

        int totalFrames = Mathf.RoundToInt(clip.length * clip.frameRate);
        
        // 기본값 세팅 (이벤트가 없을 경우를 대비)
        startFrame = 0;
        endFrame = totalFrames;

        // 클립에 들어있는 모든 이벤트 가져오기
        AnimationEvent[] existingEvents = AnimationUtility.GetAnimationEvents(clip);

        foreach (var ev in existingEvents)
        {
            // 이펙트 ON 이벤트 발견 시 프레임 역산 (Time * FPS = Frame)
            if (ev.functionName == "TurnOnVFX")
            {
                startFrame = Mathf.RoundToInt(ev.time * clip.frameRate);
            }
            // 이펙트 OFF 이벤트 발견 시 프레임 역산
            else if (ev.functionName == "TurnOffVFX")
            {
                endFrame = Mathf.RoundToInt(ev.time * clip.frameRate);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        bool offsetChanged = EditorGUI.EndChangeCheck();

        if (effectManager == null)
        {
            effectManager = FindObjectOfType<WeaponEffectManager>();
            if (effectManager == null)
            {
                EditorGUILayout.HelpBox("하이어라키에 [WeaponEffectManager] 컴포넌트(또는 @Managers)가 있어야 에디터 목록을 가져올 수 있습니다.", MessageType.Error);
                return;
            }
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            EditorGUILayout.HelpBox("오브젝트에 Animator 컴포넌트와 Controller가 지정되어 있어야 합니다.", MessageType.Warning);
            return;
        }

        string[] weaponIDs = effectManager.GetWeaponIDList();
        if (weaponIDs.Length == 0)
        {
            EditorGUILayout.HelpBox("WeaponEffectManager에 등록된 무기 데이터가 없습니다.", MessageType.Warning);
            return;
        }

        selectedWeaponIndex = Mathf.Max(0, System.Array.IndexOf(weaponIDs, targetScript.selectedWeaponID));

        if (clips == null || clips.Length == 0)
        {
            LoadAnimationClips();
            if (clips == null || clips.Length == 0) return;
        }

        // 최초 1회 로드 시 현재 지정된 클립의 이벤트를 파싱
        if (isFirstLoad && clips.Length > 0)
        {
            ParseExistingEvents(clips[selectedClipIndex]);
            isFirstLoad = false;
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("★ VFX 애니메이션 이벤트 매니저 (데이터 복원 지원)", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        // 무기 목록 선택
        EditorGUI.BeginChangeCheck();
        selectedWeaponIndex = EditorGUILayout.Popup("적용할 무기 VFX ID", selectedWeaponIndex, weaponIDs);
        if (EditorGUI.EndChangeCheck() || string.IsNullOrEmpty(targetScript.selectedWeaponID))
        {
            targetScript.selectedWeaponID = weaponIDs[selectedWeaponIndex];
            EditorUtility.SetDirty(targetScript);
            if (isPreviewing) RecreatePreviewInstance();
        }

        // [개선] 애니메이션 클립 선택 변경 감지
        EditorGUI.BeginChangeCheck();
        selectedClipIndex = EditorGUILayout.Popup("애니메이션 클립 선택", selectedClipIndex, clipNames);
        AnimationClip selectedClip = clips[selectedClipIndex];
        
        if (EditorGUI.EndChangeCheck())
        {
            // 클립이 바뀌는 순간 기존에 저장된 이벤트를 읽어서 슬라이더 값을 바꿈
            ParseExistingEvents(selectedClip);
            currentFrame = 0f; // 재생 타임라인도 0프레임으로 초기화
        }

        int totalFrames = Mathf.RoundToInt(selectedClip.length * selectedClip.frameRate);
        EditorGUILayout.LabelField($"총 프레임 수: {totalFrames} frames (FPS: {selectedClip.frameRate})");

        EditorGUILayout.Space(10);

        // 프리뷰 활성화 토글
        EditorGUI.BeginChangeCheck();
        isPreviewing = EditorGUILayout.Toggle("씬 뷰 프리뷰 활성화", isPreviewing);

        currentFrame = EditorGUILayout.Slider("현재 재생 프레임", currentFrame, 0, totalFrames);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("VFX 이벤트 범위 설정", EditorStyles.boldLabel);
        startFrame = EditorGUILayout.IntSlider("VFX 켜짐 (ON Frame)", startFrame, 0, totalFrames);
        endFrame = EditorGUILayout.IntSlider("VFX 꺼짐 (OFF Frame)", endFrame, 0, totalFrames);

        if (EditorGUI.EndChangeCheck() || offsetChanged)
        {
            if (isPreviewing)
            {
                UpdateScenePreview(selectedClip, totalFrames);
            }
            else
            {
                CleanUpPreview();
            }
        }

        EditorGUILayout.Space(15);

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Apply (애니메이션 이벤트 적용)", GUILayout.Height(30)))
        {
            ApplyAnimationEvents(selectedClip);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
    }

    private void RecreatePreviewInstance()
    {
        if (previewVFXInstance != null)
            DestroyImmediate(previewVFXInstance);
    }

    private void UpdateScenePreview(AnimationClip clip, int totalFrames)
    {
        if (targetScript == null || effectManager == null) return;

        if (!AnimationMode.InAnimationMode())
        {
            AnimationMode.StartAnimationMode();
        }

        float targetTime = currentFrame / clip.frameRate;

        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(targetScript.gameObject, clip, targetTime);
        AnimationMode.EndSampling();

        GameObject activePrefab = effectManager.GetVFXPrefab(targetScript.selectedWeaponID);

        if (activePrefab != null)
        {
            if (previewVFXInstance == null)
            {
                Transform parentTransform = targetScript.targetTransform != null ? targetScript.targetTransform : targetScript.transform;
                previewVFXInstance = Instantiate(activePrefab, parentTransform);
                previewVFXInstance.hideFlags = HideFlags.HideAndDontSave;
            }

            Transform currentParent = targetScript.targetTransform != null ? targetScript.targetTransform : targetScript.transform;
            if (previewVFXInstance.transform.parent != currentParent)
            {
                previewVFXInstance.transform.SetParent(currentParent);
            }

            previewVFXInstance.transform.localPosition = targetScript.positionOffset;
            previewVFXInstance.transform.localRotation = Quaternion.Euler(targetScript.rotationOffset);

            bool shouldVisible = currentFrame >= startFrame && currentFrame <= endFrame;
            if (previewVFXInstance.activeSelf != shouldVisible)
            {
                previewVFXInstance.SetActive(shouldVisible);
            }
        }
        else
        {
            CleanUpPreview();
        }

        SceneView.RepaintAll();
    }

    private void CleanUpPreview()
    {
        if (AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }

        if (previewVFXInstance != null)
            DestroyImmediate(previewVFXInstance);
    }

    private void ApplyAnimationEvents(AnimationClip clip)
    {
        if (clip == null) return;

        List<AnimationEvent> currentEvents = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(clip));
        currentEvents.RemoveAll(e => e.functionName == "TurnOnVFX" || e.functionName == "TurnOffVFX");

        AnimationEvent onEvent = new AnimationEvent
        {
            time = (float)startFrame / clip.frameRate,
            functionName = "TurnOnVFX"
        };
        currentEvents.Add(onEvent);

        AnimationEvent offEvent = new AnimationEvent
        {
            time = (float)endFrame / clip.frameRate,
            functionName = "TurnOffVFX"
        };
        currentEvents.Add(offEvent);

        AnimationUtility.SetAnimationEvents(clip, currentEvents.ToArray());
        
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green><b>[적용 완료]</b></color> {clip.name} 클립에 이벤트를 저장했습니다 (ON: {startFrame}F, OFF: {endFrame}F)");
    }
}
#endif