using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableIngredient : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public string ingredienteName;

    private RectTransform rect;
    private CanvasGroup group;
    private Canvas rootCanvas;
    private Transform panelOriginal;
    private Vector2 posOriginal;
    private int siblingOriginal;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
        GetComponent<Image>().raycastTarget = true;
    }

    public void GuardarPosicionOriginal()
    {
        panelOriginal = transform.parent;
        posOriginal = rect.anchoredPosition;
        siblingOriginal = transform.GetSiblingIndex();
    }

    Canvas GetRootCanvas()
    {
        Canvas[] lista = GetComponentsInParent<Canvas>(true);
        return lista.Length > 0 ? lista[lista.Length - 1] : null;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        rootCanvas = GetRootCanvas();
        if (rootCanvas == null) return;

        group.blocksRaycasts = false;
        group.alpha = 0.75f;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData e)
    {
        if (rootCanvas == null) return;
        rect.anchoredPosition += e.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        group.alpha = 1f;
        group.blocksRaycasts = true; 

        if (transform.parent == rootCanvas.transform)
            VolverAlPanel();
    }

    public void VolverAlPanel()
    {
        if (panelOriginal == null) return;
        transform.SetParent(panelOriginal, false);
        transform.SetSiblingIndex(siblingOriginal);
        rect.anchoredPosition = posOriginal;
        group.blocksRaycasts = true;
        group.alpha = 1f;
    }

    public void ColocarEnSlot(Transform slot)
    {
        transform.SetParent(slot, false);
        rect.anchoredPosition = Vector2.zero;
        group.blocksRaycasts = true; 
        group.alpha = 1f;
    }

    public void ResetearEstado()
    {
        group.blocksRaycasts = true;
        group.interactable = true;
        group.alpha = 1f;
        enabled = true;
    }
}