using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class HUDFadeGroup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool revealOnHover = true;

    public CanvasGroup Group { get; private set; }
    public bool IsHovering { get; private set; }

    void Awake() => Group = GetComponent<CanvasGroup>();
    void OnEnable()  => HUDFadeManager.Instance?.Register(this);
    void OnDisable() { IsHovering = false; HUDFadeManager.Instance?.Unregister(this); }

    public void OnPointerEnter(PointerEventData e) { if (revealOnHover) IsHovering = true; }
    public void OnPointerExit(PointerEventData e)  => IsHovering = false;
}