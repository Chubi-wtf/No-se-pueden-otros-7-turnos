using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
public class IngredientSlot : MonoBehaviour, IDropHandler
{
    [HideInInspector] public string ingredienteEsperado;
    [HideInInspector] public bool estaOcupado = false;

    private Image img;

    static readonly Color colorLibre = new Color(0.85f, 0.85f, 0.85f, 0.9f);
    static readonly Color colorCorrecto = new Color(0.4f, 0.9f, 0.4f, 0.9f);
    static readonly Color colorError = new Color(0.95f, 0.3f, 0.3f, 0.9f);

    void Awake()
    {
        img = GetComponent<Image>();
        img.raycastTarget = true;
        img.color = colorLibre;
    }

    public void Resetear()
    {
        estaOcupado = false;
        img.color = colorLibre;
    }

    public void OnDrop(PointerEventData e)
    {
        if (estaOcupado) return;

        DraggableIngredient ing = e.pointerDrag?.GetComponent<DraggableIngredient>();
        if (ing == null) return;

        if (ing.ingredienteName == ingredienteEsperado)
        {
            estaOcupado = true;
            img.color = colorCorrecto;
            ing.ColocarEnSlot(transform);
            SonidoManager.Instance?.Acierto();
            CookingMinigame.Instance?.VerificarProgreso();
        }
        else
        {
            StartCoroutine(FlashError(ing));
        }
    }

    IEnumerator FlashError(DraggableIngredient ing)
    {
        SonidoManager.Instance?.Fallo();
        img.color = colorError;
        yield return new WaitForSecondsRealtime(0.35f);
        img.color = colorLibre;
        ing.VolverAlPanel();
    }
}