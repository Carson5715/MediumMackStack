using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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
    public float winCameraPanSpeed = 1f;         // Speed at which the camera pans upward after win.
    
    // Objects to disable on win.
    public List<GameObject> disableObjects;
    
    // Public variables for win spawn.
    public GameObject winSpawnPrefab;          // The prefab to spawn when win condition is reached.
    public Transform winSpawnLocation;         // The location at which to spawn the prefab.
    
    // Scene names to load.
    public string winSceneName = "WinScene";
    public string loseSceneName = "LoseScene";
    
    // How long to wait after spawning the win object before loading the win scene.
    public float winWaitTime = 5f;
    
    // Internal flags.
    private bool gameLost = false;
    private bool winConditionTriggered = false;
    private bool winSceneLoaded = false;
    private bool loseSceneLoaded = false;
    
    // Gyroscope calibration fields.
    private bool gyroCalibrated = false;
    private float initialTiltAngle = 0f;

    // Singleton instance.
    public static PlateController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
    
    // Enable the gyroscope if available and calibrate it.
    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            StartCoroutine(CalibrateGyro());
        }
    }
    
    // Wait a moment before calibrating the initial tilt.
    IEnumerator CalibrateGyro()
    {
        yield return new WaitForSeconds(0.5f);
        initialTiltAngle = GetTiltAngle();
        gyroCalibrated = true;
    }
    
    // Helper method to get the current tilt angle around the Z-axis.
    private float GetTiltAngle()
    {
        Quaternion deviceRotation = Input.gyro.attitude;
        // Adjust for Unity's coordinate system.
        deviceRotation = Quaternion.Euler(90f, 0f, 0f) * new Quaternion(-deviceRotation.x, -deviceRotation.y, deviceRotation.z, deviceRotation.w);
        float tiltAngle = deviceRotation.eulerAngles.z;
        // Convert from 0–360 to -180–180.
        if (tiltAngle > 180f)
            tiltAngle -= 360f;
        return tiltAngle;
    }
    
    void Update()
    {
        // Only allow player movement if win condition hasn't been triggered.
        if (!winConditionTriggered)
        {
            float horizontalInput = 0f;
            
            // Use gyroscope input if supported and calibrated.
            if (SystemInfo.supportsGyroscope && gyroCalibrated)
            {
                // Calculate relative tilt.
                float tiltAngle = GetTiltAngle() - initialTiltAngle;
                // Define a maximum tilt angle (in degrees) for full left/right movement.
                float maxTilt = 10f;
                // Normalize the tilt so that full left/right (i.e., -maxTilt to maxTilt) maps to -1 to 1.
                horizontalInput = Mathf.Clamp(tiltAngle, -maxTilt, maxTilt) / maxTilt;
            }
            else
            {
                // Fallback to keyboard input if gyroscope not available or not yet calibrated.
                horizontalInput = Input.GetAxis("Horizontal");
            }
            
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
            if (!loseSceneLoaded)
            {
                loseSceneLoaded = true;
                Invoke("LoadLoseScene", 1f);
            }
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
        
        // Update UI: wobble.
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
            
            // Disable the designated objects.
            foreach (GameObject obj in disableObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
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
                
                // Once the camera is nearly at the target, spawn the win object, wait, then load the win scene.
                if (!winSceneLoaded && Mathf.Abs(mainCamera.transform.position.y - (highestPoint + cameraYOffset)) < 0.1f)
                {
                    winSceneLoaded = true;
                    StartCoroutine(SpawnAndWaitAndLoadWinScene());
                }
            }
        }
    }
    
    IEnumerator SpawnAndWaitAndLoadWinScene()
    {
        // Spawn the win object at the specified spawn location.
        if (winSpawnPrefab != null && winSpawnLocation != null)
        {
            Instantiate(winSpawnPrefab, winSpawnLocation.position, winSpawnLocation.rotation);
        }
        // Wait for winWaitTime seconds.
        yield return new WaitForSeconds(winWaitTime);
        // Load the win scene.
        SceneManager.LoadScene(winSceneName);
    }
    
    void LoadLoseScene()
    {
        SceneManager.LoadScene(loseSceneName);
    }
}