using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDFadeManager : MonoBehaviour
{
    private static HUDFadeManager instance;
    public static HUDFadeManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<HUDFadeManager>();
            return instance;
        }
    }

    [SerializeField] private InputField chatInput;      // legacy
    [SerializeField] private float fadedAlpha = 0f;
    [SerializeField] private float fadeSpeed  = 4f;
    [SerializeField] private float revealHold = 3f;

    [SerializeField] private KeyCode revealKey = KeyCode.Escape;
    private bool escReveal; 

    public bool ForceReveal { get; set; }               // 팝업 열림 등

    private readonly List<HUDFadeGroup> groups = new List<HUDFadeGroup>();
    private readonly HashSet<Object> combatSources = new HashSet<Object>();
    private float revealTimer;

    void Awake() => instance = this;

    void Start()
    {
        foreach (var g in FindObjectsOfType<HUDFadeGroup>(true))
            Register(g);
        Debug.Log($"[Fade] Start 등록: {groups.Count}개");
    }

    public void Register(HUDFadeGroup g)   { if (g != null && !groups.Contains(g)) groups.Add(g); }
    public void Unregister(HUDFadeGroup g) => groups.Remove(g);

    public void EnterCombat(Object src) => combatSources.Add(src);
    public void ExitCombat(Object src)  => combatSources.Remove(src);
    public void PokeReveal()            => revealTimer = revealHold;

    private bool InCombat
    {
        get { combatSources.RemoveWhere(s => s == null); return combatSources.Count > 0; }
    }
    private bool AnyHover
    {
        get { foreach (var g in groups) if (g != null && g.IsHovering) return true; return false; }
    }
    private bool ChatFocused => chatInput != null && chatInput.isFocused;

    void Update()
    {
        bool inCombatNow = InCombat;
        if (!inCombatNow) escReveal = false; 
        if (Input.GetKeyDown(revealKey)) escReveal = !escReveal;

        if (Input.GetKeyDown(KeyCode.F9))
            Debug.Log($"[Fade] groups={groups.Count} combat={combatSources.Count} " +
                      $"hover={AnyHover} timer={revealTimer:F1} focus={ChatFocused}");

        if (ChatFocused) revealTimer = revealHold;
        else if (revealTimer > 0f) revealTimer -= Time.unscaledDeltaTime;

        bool visible = escReveal || ForceReveal || !inCombatNow || AnyHover || revealTimer > 0f;
        float target = visible ? 1f : fadedAlpha;

        for (int i = groups.Count - 1; i >= 0; i--)
        {
            if (groups[i] == null) { groups.RemoveAt(i); continue; }
            var cg = groups[i].Group;
            cg.alpha = Mathf.MoveTowards(cg.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
        }
    }
}