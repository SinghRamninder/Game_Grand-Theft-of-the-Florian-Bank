using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StealMoney : MonoBehaviour
{
    [System.Serializable]
    public class CameraStep
    {
        public Transform target;

        [Header("Move")]
        public float moveSpeed = 4f;
        public float waitTime = 1f;

        [Header("Zoom")]
        public bool changeOrthoSize = false;
        public float orthographicSize = 5f;
        public float zoomSpeed = 6f;

        [Header("Follow (optional)")]
        public bool followTarget = false;
        public float followDuration = 2f;
    }

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject money;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Light2D alretLight1;
    [SerializeField] private Light2D alretLight2;
    [SerializeField] private Light2D alretLight3;
    [SerializeField] private GameObject instructionKey;
    [SerializeField] private float blinkSpeed = 1f;

    [Header("Bull Guard Spawn")]
    [SerializeField] private GameObject bullGuard; // prefab
    [SerializeField] private Vector3 bullSpawnPosition = new Vector3(-28.63387f, 5.05f, -0.1599123f);
    [SerializeField] private Vector3 bullPointALocalPosition = new Vector3(6.9f, -1.466668f, 1.066082f);
    [SerializeField] private string bullCameraTargetChildName = "Bull (Guard)";
    [SerializeField] private string bullPointAChildName = "PointA";

    [Header("Bull Camera Step")]
    [SerializeField] private CameraStep bullCameraStep; // set moveSpeed, waitTime, zoom, followDuration here

    [Header("Camera Path")]
    [SerializeField] private CameraStep[] cameraSteps;

    [Header("Return To Player")]
    [SerializeField] private float returnSpeed = 6f;

    [Header("Return Zoom")]
    [SerializeField] private float returnZoomSpeed = 6f;

    public bool blink = false;
    private Coroutine blinkRoutine;

    private bool isNear;
    private PlayerMovement playerMovement;
    private CinemachineBrain cinemachineBrain;
    private Coroutine cameraRoutine;

    private Camera cam;
    private float originalOrthoSize;

    private GameObject spawnedBull;

    private void Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
        cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();

        cam = mainCamera.GetComponent<Camera>();
        if (cam == null)
            cam = mainCamera.GetComponentInChildren<Camera>();

        if (cam != null)
            originalOrthoSize = cam.orthographicSize;
    }

    private void Update()
    {
        if (blink && blinkRoutine == null)
        {
            blinkRoutine = StartCoroutine(Blink());
        }
        else if (!blink && blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
            alretLight1.intensity = 0f;
            alretLight2.intensity = 0f;
            alretLight3.intensity = 0f;
        }

        if (Input.GetKeyDown(KeyCode.C) && isNear)
        {
            playerMovement.enabled = false;
            cinemachineBrain.enabled = false;

            instructionKey.SetActive(false);
            money.SetActive(false);

            blink = true;

            audioManager.StopMusic();
            audioManager.PlaySFXLoop(audioManager.siren);

            StartCoroutine(startCutscene());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            instructionKey.SetActive(true);
            isNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            instructionKey.SetActive(false);
            isNear = false;
        }
    }

    private IEnumerator startCutscene()
    {
        yield return new WaitForSeconds(2f);

        if (cameraRoutine != null)
            StopCoroutine(cameraRoutine);

        cameraRoutine = StartCoroutine(CameraMove());
    }

    private IEnumerator Blink()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * blinkSpeed;
            float v = Mathf.PingPong(t, 1f);

            alretLight1.intensity = v;
            alretLight2.intensity = v;
            alretLight3.intensity = v;

            yield return null;
        }
    }

    private IEnumerator CameraMove()
    {
        if (cameraSteps == null || cameraSteps.Length == 0)
            yield break;

        Transform camT = mainCamera.transform;

        // 1) Normal camera steps
        foreach (CameraStep step in cameraSteps)
        {
            if (step == null || step.target == null)
                continue;

            yield return StartCoroutine(MoveZoomToTarget(camT, step));

            DoorScript stepDoor = step.target.GetComponent<DoorScript>();
            if (stepDoor != null)
            {
                yield return new WaitForSeconds(0.5f);
                stepDoor.lockAllDoors();
            }

            if (step.waitTime > 0f)
                yield return new WaitForSeconds(step.waitTime);
        }

        // 2) Spawn bull guard and setup its PointA
        SpawnBullGuardAndSetup();

        // 3) Go to Bull (Guard) child and follow it using bullCameraStep settings
        if (spawnedBull != null && bullCameraStep != null)
        {
            Transform bullCamTarget = FindDeepChild(spawnedBull.transform, bullCameraTargetChildName);

            if (bullCamTarget != null)
            {
                bullCameraStep.target = bullCamTarget;

                yield return StartCoroutine(MoveZoomToTarget(camT, bullCameraStep));

                if (bullCameraStep.followTarget && bullCameraStep.followDuration > 0f)
                {
                    float t = 0f;
                    while (t < bullCameraStep.followDuration)
                    {
                        t += Time.deltaTime;

                        Vector3 pos = bullCamTarget.position;
                        pos.z = camT.position.z;
                        camT.position = pos;

                        if (cam != null && bullCameraStep.changeOrthoSize)
                        {
                            cam.orthographicSize = Mathf.MoveTowards(
                                cam.orthographicSize,
                                bullCameraStep.orthographicSize,
                                bullCameraStep.zoomSpeed * Time.deltaTime
                            );
                        }

                        yield return null;
                    }
                }

                if (bullCameraStep.waitTime > 0f)
                    yield return new WaitForSeconds(bullCameraStep.waitTime);
            }
        }

        // 4) Return to player + restore ortho size
        Vector3 playerPos = player.transform.position;
        playerPos.z = camT.position.z;

        while (Vector3.Distance(camT.position, playerPos) > 0.01f)
        {
            camT.position = Vector3.MoveTowards(
                camT.position,
                playerPos,
                returnSpeed * Time.deltaTime
            );

            if (cam != null)
            {
                cam.orthographicSize = Mathf.MoveTowards(
                    cam.orthographicSize,
                    originalOrthoSize,
                    returnZoomSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        if (cam != null)
        {
            while (Mathf.Abs(cam.orthographicSize - originalOrthoSize) > 0.01f)
            {
                cam.orthographicSize = Mathf.MoveTowards(
                    cam.orthographicSize,
                    originalOrthoSize,
                    returnZoomSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // 5) Restore gameplay
        cinemachineBrain.enabled = true;
        playerMovement.enabled = true;

        blink = false;

        audioManager.SetSFXVolume(0.03f);
        audioManager.PlayChaseMusic();
    }

    private void SpawnBullGuardAndSetup()
    {
        if (bullGuard == null) return;
        if (spawnedBull != null) return;

        spawnedBull = Instantiate(bullGuard, bullSpawnPosition, bullGuard.transform.rotation);

        Transform pointA = FindDeepChild(spawnedBull.transform, bullPointAChildName);
        if (pointA != null)
        {
            pointA.localPosition = bullPointALocalPosition;
        }
    }

    private IEnumerator MoveZoomToTarget(Transform camT, CameraStep step)
    {
        Vector3 targetPos = step.target.position;
        targetPos.z = camT.position.z;

        while (Vector3.Distance(camT.position, targetPos) > 0.01f)
        {
            camT.position = Vector3.MoveTowards(
                camT.position,
                targetPos,
                step.moveSpeed * Time.deltaTime
            );

            if (cam != null && step.changeOrthoSize)
            {
                cam.orthographicSize = Mathf.MoveTowards(
                    cam.orthographicSize,
                    step.orthographicSize,
                    step.zoomSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        if (cam != null && step.changeOrthoSize)
        {
            while (Mathf.Abs(cam.orthographicSize - step.orthographicSize) > 0.01f)
            {
                cam.orthographicSize = Mathf.MoveTowards(
                    cam.orthographicSize,
                    step.orthographicSize,
                    step.zoomSpeed * Time.deltaTime
                );
                yield return null;
            }
        }
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null) return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }

        return null;
    }
}
