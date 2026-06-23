#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(VFXEventModifier))]
public class VFXEventModifierEditor : Editor
{
    private VFXEventModifier targetScript;
    private Animator animator;
    private WeaponEffectManager effectManager;

    private AnimationClip[] clips;
    private string[] clipNames;
    private int selectedClipIndex = 0;
    private int selectedActionIndex = 0;

    private float currentFrame = 0f;
    private int startFrame = 0;
    private int endFrame = 0;
    private bool isPreviewing = false;

    // 프리뷰용 동시 생성 인스턴스들
    private GameObject previewWeaponVFX;
    private GameObject previewParticleVFX;
    private bool isFirstLoad = true;

    private void OnEnable()
    {
        targetScript = (VFXEventModifier)target;
        animator = targetScript.GetComponent<Animator>();
        effectManager = FindObjectOfType<WeaponEffectManager>();
        LoadAnimationClips();
        isFirstLoad = true;
    }

    private void OnDisable() { CleanUpPreview(); }

    private void LoadAnimationClips()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        clips = animator.runtimeAnimatorController.animationClips;
        clipNames = new string[clips.Length];
        for (int i = 0; i < clips.Length; i++) clipNames[i] = clips[i].name;
    }

    private void ParseExistingEvents(AnimationClip clip)
    {
        if (clip == null) return;
        int totalFrames = Mathf.RoundToInt(clip.length * clip.frameRate);
        startFrame = 0; endFrame = totalFrames;

        AnimationEvent[] existingEvents = AnimationUtility.GetAnimationEvents(clip);
        foreach (var ev in existingEvents)
        {
            if (ev.functionName == "TurnOnVFX") startFrame = Mathf.RoundToInt(ev.time * clip.frameRate);
            else if (ev.functionName == "TurnOffVFX") endFrame = Mathf.RoundToInt(ev.time * clip.frameRate);
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        bool offsetChanged = EditorGUI.EndChangeCheck();

        if (animator == null || animator.runtimeAnimatorController == null) { EditorGUILayout.HelpBox("Animator가 필요합니다.", MessageType.Warning); return; }
        if (clips == null || clips.Length == 0) { LoadAnimationClips(); if (clips == null || clips.Length == 0) return; }
        if (selectedClipIndex < 0 || selectedClipIndex >= clips.Length) selectedClipIndex = 0;
        if (isFirstLoad && clips.Length > 0) { ParseExistingEvents(clips[selectedClipIndex]); isFirstLoad = false; }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("★ VFX 애니메이션 이벤트 매니저 (세트 동시 제어 버전)", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        targetScript.useVFXSet = EditorGUILayout.Toggle("1. 매니저 세트 VFX 사용", targetScript.useVFXSet);
        targetScript.useTrailRenderer = EditorGUILayout.Toggle("2. 테일 레더러(잔상) 사용", targetScript.useTrailRenderer);
        EditorGUILayout.Space(10);

        // --- 이펙트 세트 선택 팝업 UI ---
        if (targetScript.useVFXSet && effectManager != null)
        {
            string[] actionIDs = effectManager.GetActionIDList();
            if (actionIDs.Length > 0)
            {
                int currentIndex = System.Array.IndexOf(actionIDs, targetScript.selectedActionID);
                selectedActionIndex = Mathf.Clamp(currentIndex, 0, actionIDs.Length - 1);

                EditorGUI.BeginChangeCheck();
                int nextActionIndex = EditorGUILayout.Popup("적용할 이펙트 세트 ID", selectedActionIndex, actionIDs);

                if (EditorGUI.EndChangeCheck() || string.IsNullOrEmpty(targetScript.selectedActionID))
                {
                    targetScript.selectedActionID = actionIDs[nextActionIndex];
                    targetScript.selectedActionIndex = nextActionIndex;
                    EditorUtility.SetDirty(targetScript);
                    if (isPreviewing) CleanUpPreview();
                }
            }
            else EditorGUILayout.HelpBox("WeaponEffectManager에 등록된 VFX 세트 데이터가 없습니다.", MessageType.Info);
        }

        if (targetScript.useTrailRenderer)
        {
            targetScript.targetTrail = (TrailRenderer)EditorGUILayout.ObjectField("대상 Trail Renderer", targetScript.targetTrail, typeof(TrailRenderer), true);
        }

        EditorGUILayout.Space(10);
        selectedClipIndex = EditorGUILayout.Popup("애니메이션 클립 선택", selectedClipIndex, clipNames);
        AnimationClip selectedClip = clips[selectedClipIndex];

        if (GUI.changed) { ParseExistingEvents(selectedClip); }

        int totalFrames = Mathf.RoundToInt(selectedClip.length * selectedClip.frameRate);
        EditorGUILayout.LabelField($"총 프레임 수: {totalFrames} frames");

        EditorGUILayout.Space(5);
        isPreviewing = EditorGUILayout.Toggle("씬 뷰 프리뷰 활성화", isPreviewing);
        currentFrame = EditorGUILayout.Slider("현재 재생 프레임", currentFrame, 0, totalFrames);

        startFrame = EditorGUILayout.IntSlider("VFX 켜짐 (ON Frame)", startFrame, 0, totalFrames);
        endFrame = EditorGUILayout.IntSlider("VFX 꺼짐 (OFF Frame)", endFrame, 0, totalFrames);

        if (GUI.changed || offsetChanged)
        {
            if (isPreviewing) UpdateScenePreview(selectedClip, totalFrames);
            else CleanUpPreview();
        }

        EditorGUILayout.Space(15);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Apply (애니메이션 이벤트 적용)", GUILayout.Height(30))) { ApplyAnimationEvents(selectedClip); }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
    }

    private void UpdateScenePreview(AnimationClip clip, int totalFrames)
    {
        if (targetScript == null) return;
        if (!AnimationMode.InAnimationMode()) AnimationMode.StartAnimationMode();

        float targetTime = currentFrame / clip.frameRate;
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(targetScript.gameObject, clip, targetTime);
        AnimationMode.EndSampling();

        bool shouldVisible = currentFrame >= startFrame && currentFrame <= endFrame;

        if (targetScript.useTrailRenderer && targetScript.targetTrail != null) targetScript.targetTrail.emitting = shouldVisible;

        if (targetScript.useVFXSet && effectManager != null)
        {
            WeaponEffectManager.VFXSetData activeSet = effectManager.GetVFXSet(targetScript.selectedActionID);
            Transform parentTransform = targetScript.targetTransform != null ? targetScript.targetTransform : targetScript.transform;

            // 무기 이펙트 프리뷰 처리
            if (activeSet.weaponVFXPrefab != null)
            {
                if (previewWeaponVFX == null) { previewWeaponVFX = Instantiate(activeSet.weaponVFXPrefab, parentTransform); previewWeaponVFX.hideFlags = HideFlags.HideAndDontSave; }
                ApplyPreviewOffset(previewWeaponVFX.transform);
                previewWeaponVFX.SetActive(shouldVisible);
            }
            else if (previewWeaponVFX != null) DestroyImmediate(previewWeaponVFX);

            // 파티클 이펙트 프리뷰 처리
            if (activeSet.particleVFXPrefab != null)
            {
                if (previewParticleVFX == null) { previewParticleVFX = Instantiate(activeSet.particleVFXPrefab, parentTransform); previewParticleVFX.hideFlags = HideFlags.HideAndDontSave; }
                ApplyPreviewOffset(previewParticleVFX.transform);
                previewParticleVFX.SetActive(shouldVisible);
            }
            else if (previewParticleVFX != null) DestroyImmediate(previewParticleVFX);
        }
        else
        {
            if (previewWeaponVFX != null) DestroyImmediate(previewWeaponVFX);
            if (previewParticleVFX != null) DestroyImmediate(previewParticleVFX);
        }

        SceneView.RepaintAll();
    }

    private void ApplyPreviewOffset(Transform t)
    {
        Transform currentParent = targetScript.targetTransform != null ? targetScript.targetTransform : targetScript.transform;
        if (t.parent != currentParent) t.SetParent(currentParent);
        t.localPosition = targetScript.positionOffset;
        t.localRotation = Quaternion.Euler(targetScript.rotationOffset);
    }

    private void CleanUpPreview()
    {
        if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
        if (previewWeaponVFX != null) DestroyImmediate(previewWeaponVFX);
        if (previewParticleVFX != null) DestroyImmediate(previewParticleVFX);
        if (targetScript != null && targetScript.targetTrail != null) targetScript.targetTrail.emitting = false;
    }

    private void ApplyAnimationEvents(AnimationClip clip)
    {
        if (clip == null) return;
        List<AnimationEvent> currentEvents = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(clip));
        currentEvents.RemoveAll(e => e.functionName == "TurnOnVFX" || e.functionName == "TurnOffVFX");

        currentEvents.Add(new AnimationEvent { time = (float)startFrame / clip.frameRate, functionName = "TurnOnVFX" });
        currentEvents.Add(new AnimationEvent { time = (float)endFrame / clip.frameRate, functionName = "TurnOffVFX" });

        AnimationUtility.SetAnimationEvents(clip, currentEvents.ToArray());
        EditorUtility.SetDirty(clip); AssetDatabase.SaveAssets();
        Debug.Log($"<color=green><b>[적용 완료]</b></color> {clip.name} 세트 적용 완료!");
    }
}
#endif