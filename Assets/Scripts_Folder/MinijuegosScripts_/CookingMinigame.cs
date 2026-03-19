using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CookingMinigame : MonoBehaviour
{
    public static CookingMinigame Instance;

    [Header("Secuencias posibles (se elige una aleatoria)")]
    public List<Secuencia> secuenciasPosibles = new List<Secuencia>
    {
        new Secuencia { nombre = "Clasica", ingredientes = new List<string> { "Pan Base", "Carne",   "Lechuga", "Tomate",  "Pan Techo" } },
        new Secuencia { nombre = "BBQ",     ingredientes = new List<string> { "Pan Base", "Carne",   "Tomate",  "Carne",   "Pan Techo" } },
        new Secuencia { nombre = "Verde",   ingredientes = new List<string> { "Pan Base", "Lechuga", "Tomate",  "Lechuga", "Pan Techo" } },
    };

    [Header("Todos los ingredientes disponibles para rellenar el pool")]
    public List<string> todosLosIngredientes = new List<string>
        { "Pan Techo", "Pan Base", "Carne", "Lechuga", "Tomate" };

    [Header("Slots - donde suelta el jugador")]
    public List<IngredientSlot> slots;

    [Header("Ingredientes arrastrables")]
    public List<DraggableIngredient> ingredientes;

    [Header("Sprites por nombre de ingrediente")]
    public List<SpriteEntry> spritePorNombre;

    [Header("UI")]
    public TextMeshProUGUI textoOrden;      
    public TextMeshProUGUI textoResultado;

    
    private Transform panelIngredientes;
    private List<string> secuenciaActual = new List<string>();
    private int slotsBien = 0;
    private bool terminado = false;

    Sprite GetSprite(string nombre)
    {
        foreach (var e in spritePorNombre)
            if (e.nombre == nombre) return e.sprite;
        return null;
    }

    void Mezclar<T>(List<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = lista[i]; lista[i] = lista[j]; lista[j] = tmp;
        }
    }

    void Awake()
    {
        Instance = this;
        if (ingredientes != null && ingredientes.Count > 0)
            panelIngredientes = ingredientes[0].transform.parent;
    }

    void OnEnable()
    {
        StartCoroutine(InicializarConDelay());
    }

    IEnumerator InicializarConDelay()
    {
        yield return null;
        Inicializar();
        yield return null;
        foreach (var ing in ingredientes) ing.GuardarPosicionOriginal();
        MezclarIngredientes();
    }

    void Inicializar()
    {
        slotsBien = 0;
        terminado = false;

        if (textoResultado != null) textoResultado.gameObject.SetActive(false);

        int idx = Random.Range(0, secuenciasPosibles.Count);
        secuenciaActual = new List<string>(secuenciasPosibles[idx].ingredientes);

        if (textoOrden != null)
            textoOrden.text = string.Join("  >  ", secuenciaActual);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ingredienteEsperado = (i < secuenciaActual.Count) ? secuenciaActual[i] : "";
            slots[i].Resetear();
        }

      
        List<string> pool = new List<string>(secuenciaActual);

        Mezclar(pool);

        for (int i = 0; i < ingredientes.Count; i++)
        {
            DraggableIngredient ing = ingredientes[i];

            if (panelIngredientes != null)
                ing.transform.SetParent(panelIngredientes, false);

            ing.ingredienteName = pool[i];

            Image img = ing.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = GetSprite(pool[i]);
                img.color = Color.white;
                img.enabled = true;
            }

            ing.gameObject.SetActive(true);
        }
    }

    void MezclarIngredientes()
    {
        if (panelIngredientes == null) return;
        List<Transform> hijos = new List<Transform>();
        foreach (Transform h in panelIngredientes) hijos.Add(h);
        Mezclar(hijos);
        for (int i = 0; i < hijos.Count; i++) hijos[i].SetSiblingIndex(i);
    }

    public void VerificarProgreso()
    {
        if (terminado) return;
        slotsBien++;

        if (slotsBien >= slots.Count)
        {
            terminado = true;
            MostrarResultado("Hamburguesa lista!", true);
        }
    }

    void MostrarResultado(string msg, bool exito)
    {
        if (textoResultado != null)
        {
            textoResultado.text = msg;
            textoResultado.color = exito ? Color.green : Color.red;
            textoResultado.gameObject.SetActive(true);
        }
        StartCoroutine(CerrarConRetraso(exito));
    }

    IEnumerator CerrarConRetraso(bool exito)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        MinigameManager.Instance?.CerrarMinijuego(exito);
    }
}

[System.Serializable]
public class Secuencia
{
    public string nombre;
    public List<string> ingredientes;
}

[System.Serializable]
public class SpriteEntry
{
    public string nombre;
    public Sprite sprite;
}