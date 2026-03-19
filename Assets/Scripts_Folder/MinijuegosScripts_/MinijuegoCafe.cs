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
    private float tiempoRestante;
    private bool minijuegoActivo = false;

    void OnEnable()
    {
        
        if (posiblesCafes.Length > 0)
        {
            int randomIndex = Random.Range(0, posiblesCafes.Length);
            imagenCafe.sprite = posiblesCafes[randomIndex];
        }

        secuenciaActual.Clear();
        for (int i = 0; i < longitudSecuencia; i++)
        {
            KeyCode teclaAleatoria = teclasPermitidas[Random.Range(0, teclasPermitidas.Length)];
            secuenciaActual.Add(teclaAleatoria);
        }

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

        if (tiempoRestante <= 0)
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
            
            if (Input.GetKeyDown(tecla))
            {
                
                if (tecla == secuenciaActual[indiceActual])
                {
                    indiceActual++;
                    ActualizarTextoVisual();

                    if (indiceActual >= secuenciaActual.Count)
                    {
                        GanarMinijuego();
                    }
                }
                else
                {
                    
                    indiceActual = 0;
                    ActualizarTextoVisual();
                    Debug.Log("¡Error de tipeo! Empieza de nuevo.");
                }
            }
        }
    }

    void ActualizarTextoVisual()
    {
        
        string textoMostrado = "";

        for (int i = 0; i < secuenciaActual.Count; i++)
        {
            string nombreTecla = secuenciaActual[i].ToString();

            if (i < indiceActual)
            {
                
                textoMostrado += "<color=#00FF00>" + nombreTecla + "</color> ";
            }
            else if (i == indiceActual)
            {
                textoMostrado += "<color=#FFFF00>" + nombreTecla + "</color> ";
            }
            else
            {
                textoMostrado += "<color=#FFFFFF>" + nombreTecla + "</color> ";
            }
        }

        textoSecuencia.text = textoMostrado;
    }

    void GanarMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("¡Café preparado con éxito!");
        MinigameManager.Instance.CerrarMinijuego(this.gameObject);
    }

    void PerderMinijuego()
    {
        minijuegoActivo = false;
        Debug.Log("¡Tiempo agotado!");

        SanidadManager.Instance.RecibirDañoMental();

        MinigameManager.Instance.CerrarMinijuego(this.gameObject);
    }

}