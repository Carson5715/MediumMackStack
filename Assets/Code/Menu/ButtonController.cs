using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public enum ButtonType { Play, Quit }
    public ButtonType buttonType;

    // Colors for highlighting and pressing.
    public Color highlightColor = Color.yellow;
    public Color pressedColor = Color.red;

    // Store the original color.
    private Color originalColor;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Save the starting color.
            originalColor = rend.material.color;
        }
    }

    // When the mouse enters, change to the highlight color.
    void OnMouseEnter()
    {
        if (rend != null)
        {
            rend.material.color = highlightColor;
        }
    }

    // When the mouse exits, revert to the original color.
    void OnMouseExit()
    {
        if (rend != null)
        {
            rend.material.color = originalColor;
        }
    }

    // When the button is clicked, change to a pressed color and perform the action.
    void OnMouseDown()
    {
        if (rend != null)
        {
            rend.material.color = pressedColor;
        }

        if (buttonType == ButtonType.Play)
        {
            // Replace "GameScene" with your actual game scene name.
            SceneManager.LoadScene("Cutscene1");
        }
        else if (buttonType == ButtonType.Quit)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    // When the mouse button is released, revert back to the highlight color.
    void OnMouseUp()
    {
        if (rend != null)
        {
            rend.material.color = highlightColor;
        }
    }
}