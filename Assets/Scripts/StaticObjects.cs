using UnityEngine;

public class StaticObjects : MonoBehaviour
{
    public static CameraController CameraController { get; set; }
    public static GameController GameController { get; set; }
    public static MapCreationController MapCreationController { get; set; }
    public static MapTransitionController MapTransitionController { get; set; }
    public static PlatformingEventController PlatformingEventController { get; set; }
    public static CrowdClient CrowdClient { get; set; }
    public static CrowdController CrowdController { get; set; }
    public static Transform MainCameraTransform { get; set; }
    public static BackgroundMusic BackgroundMusic { get; set; }
    public static ItemPool ItemPool { get; set; }

    public static UIManager UIManager { get; set; }
    
    public static SpeakerLinesManager SpeakerLinesManager { get; set; }
    
    public static int PlayerNumber { get; set; }
    
    public static string RoomName { get; set; }

    public static void ResetStaticObjects()
    {
        Destroy(CameraController.gameObject);
        CameraController = null;
        Destroy(GameController.gameObject);
        GameController = null;
        Destroy(MapCreationController.gameObject);
        MapCreationController = null;
        Destroy(MapTransitionController.gameObject);
        MapTransitionController = null;
        Destroy(PlatformingEventController.gameObject);
        PlatformingEventController = null;
        Destroy(CrowdClient.gameObject);
        CrowdClient = null;
        Destroy(CrowdController.gameObject);
        CrowdController = null;
        Destroy(MainCameraTransform.gameObject);
        MainCameraTransform = null;
        Destroy(UIManager.gameObject);
        UIManager = null;
        Destroy(SpeakerLinesManager.gameObject);
        SpeakerLinesManager = null;
    }
}
