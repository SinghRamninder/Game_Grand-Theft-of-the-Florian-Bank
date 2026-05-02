using Unity.Cinemachine;
using UnityEngine;

public class StartCutscene : MonoBehaviour
{
    public bool showUIwhilePlaying;
    [HideInInspector] public GameObject keyInventoryUI;

    [HideInInspector] public PlayerMovement playerScript;

    private Camera mainCamera;
    private GameObject cameraObject;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraObject = mainCamera.gameObject;

            var brain = cameraObject != null ? cameraObject.GetComponent<CinemachineBrain>() : null;

            if (brain != null) brain.enabled = false;
        }

        if (playerScript != null) playerScript.enabled = false;

        if (!showUIwhilePlaying)
        {
            if (keyInventoryUI != null) keyInventoryUI.SetActive(false);
        }
        else
        {
            if (keyInventoryUI != null) keyInventoryUI.SetActive(true);
        }
    }

    
    
    
    
    public void SetPlayerScript(PlayerMovement Player)
    {
        playerScript = Player;
    }
}
