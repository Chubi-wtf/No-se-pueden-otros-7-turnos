using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DraggableIngredient : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public string ingredienteName;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Vector2 posicionOriginal;
    private Transform padreOriginal;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        posicionOriginal = rect.anchoredPosition;
        padreOriginal = transform.parent;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData e)
    {
        rect.anchoredPosition += e.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        transform.SetParent(padreOriginal, true);
        rect.anchoredPosition = posicionOriginal;
    }

  
    public void ColocarEnSlot(Transform slot)
    {
        transform.SetParent(slot, true);
        rect.anchoredPosition = Vector2.zero;
    }
}