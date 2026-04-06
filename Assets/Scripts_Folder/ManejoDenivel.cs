using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ConfiguracionNivel
{
    public string nombre = "Nivel";
    public GameObject escenario;
    public UnityEvent alMostrarNivel;
}

public class ManejoDenivel : MonoBehaviour
{
    public static ManejoDenivel Instance { get; private set; }

    [Header("Niveles en la misma escena")]
    public List<ConfiguracionNivel> niveles = new List<ConfiguracionNivel>();
    [Min(0)] public int nivelInicial = 0;
    public bool aplicarNivelInicialAlEmpezar = true;

    [Header("Eventos")]
    public UnityEvent alCambiarNivel;

    public int NivelActual => nivelActual;

    private int nivelActual = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Ya existe otro ManejoDenivel activo en la escena.", this);
            enabled = false;
            return;
        }

        Instance = this;

        if (aplicarNivelInicialAlEmpezar)
            MostrarNivel(nivelInicial, false);
    }

    public void MostrarNivel(int indiceNivel)
    {
        MostrarNivel(indiceNivel, true);
    }

    public void MostrarSiguienteNivel()
    {
        MostrarNivel(nivelActual + 1, true);
    }

    public bool TieneNivel(int indiceNivel)
    {
        return indiceNivel >= 0 && indiceNivel < niveles.Count;
    }

    private void MostrarNivel(int indiceNivel, bool dispararEventos)
    {
        if (niveles == null || niveles.Count == 0)
        {
            Debug.LogWarning("ManejoDenivel no tiene niveles configurados.", this);
            return;
        }

        int indiceSeguro = Mathf.Clamp(indiceNivel, 0, niveles.Count - 1);

        for (int i = 0; i < niveles.Count; i++)
        {
            GameObject escenario = niveles[i].escenario;
            if (escenario != null)
                escenario.SetActive(i == indiceSeguro);
        }

        bool huboCambioReal = nivelActual != indiceSeguro;
        nivelActual = indiceSeguro;

        if (!dispararEventos || !huboCambioReal)
            return;

        alCambiarNivel?.Invoke();
        niveles[indiceSeguro].alMostrarNivel?.Invoke();
    }
}
