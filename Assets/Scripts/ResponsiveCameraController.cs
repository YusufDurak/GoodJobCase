using UnityEngine;

/// <summary>
/// Responsive Camera Controller
/// Automatically adjusts orthographic camera size to fit the game board
/// Ensures the board fits on any device resolution and aspect ratio
/// </summary>
[RequireComponent(typeof(Camera))]
public class ResponsiveCameraController : MonoBehaviour
{
    [Header("Target Board")]
    [SerializeField] private BoardManager boardManager;
    
    [Header("Padding")]
    [SerializeField] private float paddingPercentage = 0.1f; // 10% padding around the board
    [SerializeField] private float minPadding = 0.5f; // Minimum padding in world units
    
    [Header("Auto Update")]
    [SerializeField] private bool updateEveryFrame = false; // For testing different resolutions
    
    private Camera cam;
    private float lastAspect;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        if (cam.orthographic == false)
        {
            Debug.LogWarning("ResponsiveCameraController: Camera must be Orthographic!");
            cam.orthographic = true;
        }
    }

    private void Start()
    {
        // Wait one frame for BoardManager to initialize
        Invoke(nameof(AdjustCameraToBoard), 0.1f);
    }

    private void Update()
    {
        // Auto-adjust if aspect ratio changes (e.g., rotating device)
        if (updateEveryFrame || cam.aspect != lastAspect)
        {
            AdjustCameraToBoard();
            lastAspect = cam.aspect;
        }
    }

    /// <summary>
    /// Adjust camera size to fit the entire board with padding
    /// </summary>
    public void AdjustCameraToBoard()
    {
        if (boardManager == null)
        {
            Debug.LogError("ResponsiveCameraController: BoardManager not assigned!");
            return;
        }

        // Get board dimensions from BoardManager
        Vector2 boardSize = boardManager.GetBoardDimensions();
        
        float boardWidth = boardSize.x;
        float boardHeight = boardSize.y;

        // Calculate required camera size to fit the board
        float targetHeight = boardHeight / 2f;
        float targetWidth = boardWidth / 2f;

        // Account for aspect ratio
        float aspectRatio = cam.aspect;
        float cameraHeightForWidth = targetWidth / aspectRatio;

        // Use the larger of the two sizes to ensure everything fits
        float requiredSize = Mathf.Max(targetHeight, cameraHeightForWidth);

        // Add padding
        float padding = Mathf.Max(requiredSize * paddingPercentage, minPadding);
        cam.orthographicSize = requiredSize + padding;

        // Center camera on the actual center of the board
        Vector3 boardCenter = boardManager.transform.position;
        float cameraZ = transform.position.z;
        
        transform.position = new Vector3(boardCenter.x, boardCenter.y, cameraZ);
    }

    /// <summary>
    /// Manually trigger camera adjustment (useful after board regeneration)
    /// </summary>
    public void RefreshCamera()
    {
        AdjustCameraToBoard();
    }

    // For debugging in editor
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && boardManager != null)
        {
            Vector2 boardSize = boardManager.GetBoardDimensions();
            
            Gizmos.color = Color.yellow;
            Vector3 center = boardManager.transform.position;
            Gizmos.DrawWireCube(center, new Vector3(boardSize.x, boardSize.y, 0));
        }
    }
}
