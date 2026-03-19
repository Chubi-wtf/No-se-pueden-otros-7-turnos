using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IngredientSlot : MonoBehaviour, IDropHandler
{
    [HideInInspector] public string ingredienteEsperado;
    [HideInInspector] public bool estaOcupado = false;

    private Image imagenFondo;

    private static readonly Color colorLibre = new Color(1f, 1f, 1f, 0.3f);
    private static readonly Color colorCorrecto = new Color(0.3f, 1f, 0.3f, 0.5f);
    private static readonly Color colorError = new Color(1f, 0.3f, 0.3f, 0.5f);

    void Awake()
    {
        imagenFondo = GetComponent<Image>();
        if (imagenFondo != null) imagenFondo.color = colorLibre;
    }

    public void OnDrop(PointerEventData e)
    {
        if (estaOcupado) return;

        DraggableIngredient ingrediente = e.pointerDrag?.GetComponent<DraggableIngredient>();
        if (ingrediente == null) return;

        if (ingrediente.ingredienteName == ingredienteEsperado)
        {
            // Drop correcto
            estaOcupado = true;
            ingrediente.ColocarEnSlot(transform);
            if (imagenFondo != null) imagenFondo.color = colorCorrecto;

            CookingMinigame.Instance?.VerificarProgreso();
        }
        else
        {
            StartCoroutine(FlashError());
        }
    }

    System.Collections.IEnumerator FlashError()
    {
        if (imagenFondo != null) imagenFondo.color = colorError;
        yield return new WaitForSecondsRealtime(0.4f);
        if (imagenFondo != null) imagenFondo.color = colorLibre;
    }
}