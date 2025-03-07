using UnityEngine;
using TMPro;

public class PlateController : MonoBehaviour
{
    public float moveSpeed = 5f;
    // Public TextMeshProUGUI to display the current total offset (for debugging or UI).
    public TextMeshProUGUI stackCountText;
    
    // Wobble frequency (editable in the inspector).
    public float wobbleFrequency = 20f;
    // AnimationCurve to set wobble amplitude based on the total off-center offset.
    public AnimationCurve wobbleCurve = AnimationCurve.Linear(0, 0, 5, 0.2f);
    
    // The current wobble offset (can be referenced later, e.g., for wind mechanics).
    public Vector3 currentWobble { get; private set; } = Vector3.zero;
    
    // Public threshold for triggering the lose condition (if wobble amplitude gets too high).
    public float loseWobbleThreshold = 0.15f;
    
    // Flag to ensure the lose condition is triggered only once.
    private bool gameLost = false;
    
    // Singleton instance for easy access.
    public static PlateController Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }
    
    void Update()
    {
        // Move the plate left and right.
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;
        transform.position = newPosition;
        
        // Compute the total off-center offset from the stack.
        float totalOffset = FallingObject.GetTotalOffset();
        // Evaluate the wobble amplitude based on the total offset using the AnimationCurve.
        float amplitude = wobbleCurve.Evaluate(totalOffset);
        // Compute the wobble offset using a sine wave.
        currentWobble = new Vector3(Mathf.Sin(Time.time * wobbleFrequency) * amplitude, 0, 0);
        
        // Trigger lose condition if the wobble amplitude exceeds the threshold.
        if (!gameLost && amplitude >= loseWobbleThreshold)
        {
            gameLost = true;
            FallingObject.ReleaseStack();
            // Quit the game after a short delay.
            Invoke("QuitGame", 1f);
        }
        
        // Update the horizontal positions of stacked objects.
        FallingObject.MoveStack(transform.position);
        
        // Update the UI with the current total offset.
        if (stackCountText != null)
        {
            stackCountText.text = totalOffset.ToString("F2");
        }
    }
    
    // This method quits the game.
    void QuitGame()
    {
#if UNITY_EDITOR
        // For testing in the Unity Editor.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
