using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class CookingMinigame : MonoBehaviour
{
    public static CookingMinigame Instance;

    [Header("Secuencia Correcta")]
    // Puedes cambiar el orden desde el Inspector
    public List<string> secuenciaCorrecta = new List<string>
        { "Pan", "Carne", "Lechuga", "Tomate", "Pan" };

    [Header("Referencias — Slots (mismo orden que secuenciaCorrecta)")]
    public List<IngredientSlot> slots;

    [Header("Referencias — Ingredientes arrastrables")]
    public List<DraggableIngredient> ingredientes;

    [Header("Sprites de ingredientes (mismo orden que secuenciaCorrecta)")]
    public List<Sprite> spritesIngredientes;

    [Header("UI")]
    public TextMeshProUGUI textoResultado;

    private int slotsBienColocados = 0;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Inicializar();
    }

    void Inicializar()
    {
        slotsBienColocados = 0;

        if (textoResultado != null)
            textoResultado.gameObject.SetActive(false);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ingredienteEsperado = secuenciaCorrecta[i];
            slots[i].estaOcupado = false;
        }

     
        List<string> pendientes = new List<string>(secuenciaCorrecta);

        foreach (DraggableIngredient ing in ingredientes)
        {
           
            if (pendientes.Count > 0)
            {
                ing.ingredienteName = pendientes[0];
                pendientes.RemoveAt(0);
            }

            int idx = ingredientes.IndexOf(ing);
            if (spritesIngredientes != null && idx < spritesIngredientes.Count)
            {
                Image img = ing.GetComponent<Image>();
                if (img != null) img.sprite = spritesIngredientes[idx];
            }

            ing.gameObject.SetActive(true);
            ing.transform.SetParent(ing.transform.parent, false);
        }

        MezclarIngredientes();
    }

    void MezclarIngredientes()
    {
        if (ingredientes.Count == 0) return;
        Transform panel = ingredientes[0].transform.parent;

        List<Transform> hijos = new List<Transform>();
        foreach (Transform h in panel) hijos.Add(h);

        for (int i = hijos.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            hijos[i].SetSiblingIndex(j);
        }
    }

    public void VerificarProgreso()
    {
        slotsBienColocados++;

        if (slotsBienColocados >= slots.Count)
        {

            MostrarResultado("¡Hamburguesa lista! 🍔", true);
        }
    }

    void MostrarResultado(string mensaje, bool exito)
    {
        if (textoResultado != null)
        {
            textoResultado.text = mensaje;
            textoResultado.color = exito ? Color.green : Color.red;
            textoResultado.gameObject.SetActive(true);
        }

        StartCoroutine(CerrarConRetraso(exito));
    }

    System.Collections.IEnumerator CerrarConRetraso(bool exito)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        MinigameManager.Instance?.CerrarMinijuego(exito);
    }
}