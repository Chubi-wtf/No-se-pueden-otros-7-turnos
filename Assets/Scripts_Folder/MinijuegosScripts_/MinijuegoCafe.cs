using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MinijuegoCafe : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image imagenCafe;
    public TextMeshProUGUI textoSecuencia;
    public TextMeshProUGUI textoTemporizador;

    [Header("Ajustes del Minijuego")]
    public Sprite[] posiblesCafes;
    public int longitudSecuencia = 5;
    public float tiempoMaximo = 4f;

    private KeyCode[] teclasPermitidas = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.F };

    private List<KeyCode> secuenciaActual = new List<KeyCode>();
    private int indiceActual = 0;
    private float tiempoRestante = 0f;
    private bool minijuegoActivo = false;

    void OnEnable()
    {
        if (posiblesCafes != null && posiblesCafes.Length > 0)
            imagenCafe.sprite = posiblesCafes[Random.Range(0, posiblesCafes.Length)];

        secuenciaActual.Clear();
        for (int i = 0; i < longitudSecuencia; i++)
            secuenciaActual.Add(teclasPermitidas[Random.Range(0, teclasPermitidas.Length)]);

        indiceActual = 0;
        tiempoRestante = tiempoMaximo;
        minijuegoActivo = true;

        ActualizarTextoVisual();
    }

    void Update()
    {
        if (!minijuegoActivo) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        textoTemporizador.text = tiempoRestante.ToString("F1") + "s";

        if (tiempoRestante <= 0f)
        {
            PerderMinijuego();
            return;
        }

        ComprobarTeclas();
    }

    void ComprobarTeclas()
    {
        foreach (KeyCode tecla in teclasPermitidas)
        {
            if (!Input.GetKeyDown(tecla)) continue;

            if (tecla == secuenciaActual[indiceActual])
            {
                indiceActual++;
                ActualizarTextoVisual();

                if (indiceActual >= secuenciaActual.Count)
                    GanarMinijuego();
            }
            else
            {
                indiceActual = 0;
                ActualizarTextoVisual();
                Debug.Log("Error de tipeo! Empieza de nuevo.");
            }
        }
    }

    void ActualizarTextoVisual()
    {
        string txt = "";
        for (int i = 0; i < secuenciaActual.Count; i++)
        {
            string nombre = secuenciaActual[i].ToString();
            if (i < indiceActual) txt += "<color=#00FF00>" + nombre + "</color> ";
            else if (i == indiceActual) txt += "<color=#FFFF00>" + nombre + "</color> ";
            else txt += "<color=#FFFFFF>" + nombre + "</color> ";
        }
        textoSecuencia.text = txt;
    }

    void GanarMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("Cafe preparado con exito!");
        MinigameManager.Instance?.CerrarMinijuego(true);
    }

    void PerderMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("Tiempo agotado!");
        MinigameManager.Instance?.CerrarMinijuego(false);
    }
}