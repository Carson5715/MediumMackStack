using UnityEngine;
using TMPro;

public class PlateController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float boundary = 3.5f;

    // UI elements.
    public TextMeshProUGUI stackCountText;
    public TextMeshProUGUI highestPointText;
    public TextMeshProUGUI wobbleText;
    
    // Wobble settings.
    public float wobbleFrequency = 20f;
    public AnimationCurve wobbleCurve = AnimationCurve.Linear(0, 0, 5, 0.2f);
    public Vector3 currentWobble { get; private set; } = Vector3.zero;
    public float loseWobbleThreshold = 0.15f;
    
    // Camera follow settings.
    public Camera mainCamera;
    public float cameraFollowSpeed = 2f;
    public float cameraYOffset = 5f;
    
    // Win condition settings.
    public int winStackCount = 15;             // Number of items required to win.
    public Spawner spawner;                    // Reference to the spawner to stop it.
    public GameObject winStackContainer;       // Destination for teleporting the player.
    public Transform winCameraTarget;          // Target transform for the camera when winning.
    public float winCameraPanSpeed = 1f;       // Speed at which the camera pans upward after win.
    
    // Objects to disable on win.
    public GameObject disableObject1;
    public GameObject disableObject2;
    
    // Internal flags.
    private bool gameLost = false;
    private bool winConditionTriggered = false;
    
    // Singleton instance.
    public static PlateController Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
    }
    
    void Update()
    {
        // Only allow player movement if win condition hasn't been triggered.
        if (!winConditionTriggered)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;
            newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary);
            transform.position = newPosition;
        }
        
        // Calculate total off-center offset.
        float totalOffset = FallingObject.GetTotalOffset();
        // Evaluate the wobble amplitude based on the total offset.
        float amplitude = wobbleCurve.Evaluate(totalOffset);
        // If win condition is triggered, disable the wobble.
        if (winConditionTriggered)
        {
            amplitude = 0f;
        }
        currentWobble = new Vector3(Mathf.Sin(Time.time * wobbleFrequency) * amplitude, 0, 0);
        
        // Trigger lose condition if the wobble amplitude is too high.
        if (!gameLost && amplitude >= loseWobbleThreshold)
        {
            gameLost = true;
            FallingObject.ReleaseStack();
            Invoke("QuitGame", 1f);
        }
        
        // Update positions of stacked objects.
        FallingObject.MoveStack(transform.position);
        
        // Update UI: stack count.
        if (stackCountText != null)
            stackCountText.text = "Items: " + FallingObject.GetStackCount().ToString();
        
        // Update UI: highest point.
        float highestPoint = FallingObject.GetHighestPoint();
        if (highestPointText != null)
            highestPointText.text = "Highest: " + highestPoint.ToString("F2");
        
        // Update UI with wobble
        if (wobbleText != null)
            wobbleText.text = "Wobble: " + amplitude.ToString("F2");
        
        // Check win condition.
        if (!winConditionTriggered && FallingObject.GetStackCount() >= winStackCount)
        {
            winConditionTriggered = true;
            
            // Stop the spawner.
            if (spawner != null)
            {
                spawner.CancelInvoke("SpawnObject");
                spawner.enabled = false;
            }
            
            // Teleport the player to the winStackContainer's position.
            if (winStackContainer != null)
            {
                transform.position = winStackContainer.transform.position;
            }
            
            // Teleport the camera to the win camera target.
            if (mainCamera != null && winCameraTarget != null)
            {
                mainCamera.transform.position = winCameraTarget.position;
                mainCamera.transform.rotation = winCameraTarget.rotation;
            }
            
            // Disable the two designated objects.
            if (disableObject1 != null)
            {
                disableObject1.SetActive(false);
            }
            if (disableObject2 != null)
            {
                disableObject2.SetActive(false);
            }
        }
    }
    
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // If win condition hasn't been triggered, follow the stack normally.
            if (!winConditionTriggered)
            {
                float highestPoint = FallingObject.GetHighestPoint();
                if (highestPoint == float.MinValue)
                    highestPoint = mainCamera.transform.position.y - cameraYOffset;
                
                float desiredY = highestPoint + cameraYOffset;
                float targetY = Mathf.Max(mainCamera.transform.position.y, desiredY);
                
                Vector3 currentCamPos = mainCamera.transform.position;
                Vector3 targetCamPos = new Vector3(currentCamPos.x, targetY, currentCamPos.z);
                mainCamera.transform.position = Vector3.Lerp(currentCamPos, targetCamPos, Time.deltaTime * cameraFollowSpeed);
            }
            // After win condition, slowly pan the camera upward to follow the top of the stack.
            else
            {
                float highestPoint = FallingObject.GetHighestPoint();
                if (highestPoint == float.MinValue)
                    highestPoint = mainCamera.transform.position.y;
                
                Vector3 currentCamPos = mainCamera.transform.position;
                Vector3 targetCamPos = new Vector3(currentCamPos.x, highestPoint + cameraYOffset, currentCamPos.z);
                mainCamera.transform.position = Vector3.Lerp(currentCamPos, targetCamPos, Time.deltaTime * winCameraPanSpeed);
            }
        }
    }
    
    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
