using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    private void OnEnable()
    {
        InputManager.OnResetRequested += HandleResetRequested;
    }

    private void OnDisable()
    {
        InputManager.OnResetRequested -= HandleResetRequested;
    }

    private void HandleResetRequested()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
