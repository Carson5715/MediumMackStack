using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
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

    // ======= Event System Methods for Touch =======

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Unhighlight();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Press();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Highlight();
        ExecuteAction();
    }

    // ======= Legacy Mouse Support (Optional for Editor) =======

    void OnMouseEnter()
    {
        Highlight();
    }

    void OnMouseExit()
    {
        Unhighlight();
    }

    void OnMouseDown()
    {
        Press();
    }

    void OnMouseUp()
    {
        Highlight();
        ExecuteAction();
    }

    // ======= Helper Methods =======

    private void Highlight()
    {
        if (rend != null)
        {
            rend.material.color = highlightColor;
        }
    }

    private void Unhighlight()
    {
        if (rend != null)
        {
            rend.material.color = originalColor;
        }
    }

    private void Press()
    {
        if (rend != null)
        {
            rend.material.color = pressedColor;
        }
    }

    private void ExecuteAction()
    {
        if (buttonType == ButtonType.Play)
        {
            // Replace "Cutscene1" with your actual game scene name.
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
}