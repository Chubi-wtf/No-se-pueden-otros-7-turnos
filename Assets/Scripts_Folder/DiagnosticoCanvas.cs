using UnityEngine;

public class DiagnosticoCanvas : MonoBehaviour
{
  void OnDisable()
    {
        Debug.LogError("CanvasPrincipal fue desactivado!\n" + System.Environment.StackTrace);
    }
}