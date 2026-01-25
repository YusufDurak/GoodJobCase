using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ResponsiveCameraController : MonoBehaviour
{
    [Header("Target Board")]
    [SerializeField] private BoardManager boardManager;
    
    [Header("Padding")]
    [SerializeField] private float paddingPercentage = 0.1f;
    [SerializeField] private float minPadding = 0.5f;
    
    [Header("Auto Update")]
    [SerializeField] private bool updateEveryFrame = false;
    
    private Camera cam;
    private float lastAspect;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        if (cam.orthographic == false)
        {
            cam.orthographic = true;
        }
    }

    private void Start()
    {
        Invoke(nameof(AdjustCameraToBoard), 0.1f);
    }

    private void Update()
    {
        if (updateEveryFrame || cam.aspect != lastAspect)
        {
            AdjustCameraToBoard();
            lastAspect = cam.aspect;
        }
    }

    public void AdjustCameraToBoard()
    {
        if (boardManager == null) return;

        Vector2 boardSize = boardManager.GetBoardDimensions();
        
        float boardWidth = boardSize.x;
        float boardHeight = boardSize.y;

        float targetHeight = boardHeight / 2f;
        float targetWidth = boardWidth / 2f;

        float aspectRatio = cam.aspect;
        float cameraHeightForWidth = targetWidth / aspectRatio;

        float requiredSize = Mathf.Max(targetHeight, cameraHeightForWidth);

        float padding = Mathf.Max(requiredSize * paddingPercentage, minPadding);
        cam.orthographicSize = requiredSize + padding;

        Vector3 boardCenter = boardManager.transform.position;
        float cameraZ = transform.position.z;
        
        transform.position = new Vector3(boardCenter.x, boardCenter.y, cameraZ);
    }

    public void RefreshCamera()
    {
        AdjustCameraToBoard();
    }

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
