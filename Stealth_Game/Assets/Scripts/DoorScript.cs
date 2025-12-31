using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Instruction Objects (shown near door)")]
    [SerializeField] private GameObject instructionKey1; // for Location A
    [SerializeField] private GameObject instructionKey2; // for Location B

    [Header("Teleport Locations")]
    [SerializeField] private Transform teleportLocationA;
    [SerializeField] private bool allowSecondTeleport = false;
    [SerializeField] private Transform teleportLocationB;

    [Header("Input Keys")]
    [SerializeField] private KeyCode keyToA = KeyCode.E;
    [SerializeField] private KeyCode keyToB = KeyCode.Q;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float teleportDelay = 0.5f;

    [Header("Key Requirement")]
    [SerializeField] private bool keyRequiredForA = false;
    [SerializeField] private bool keyRequiredForB = false;

    [Tooltip("Enable this if this door should be unlocked by Basement 1 Key")]
    [SerializeField] private bool basement1Door = false;

    [Tooltip("Enable this if this door should be unlocked by Basement 2 Key")]
    [SerializeField] private bool basement2Door = false;

    [Header("Instruction Colors")]
    [SerializeField] private Color hasKeyColor = Color.green;
    [SerializeField] private Color missingKeyColor = Color.red;
    [SerializeField] private Color notRequiredColor = Color.white;

    private bool isPlayerInside = false;
    private Transform player;
    private PickPoket pickPoket;
    private bool isTeleporting = false;

    // Permanent unlock per destination (optional but useful)
    private bool locationAUnlocked = false;
    private bool locationBUnlocked = false;

    // Cached sprite renderers
    private SpriteRenderer instruction1SR;
    private SpriteRenderer instruction2SR;

    private void Awake()
    {
        instruction1SR = instructionKey1 ? instructionKey1.GetComponent<SpriteRenderer>() : null;
        instruction2SR = instructionKey2 ? instructionKey2.GetComponent<SpriteRenderer>() : null;

        // start hidden
        if (instructionKey1) instructionKey1.SetActive(false);
        if (instructionKey2) instructionKey2.SetActive(false);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) pickPoket = player.GetComponent<PickPoket>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        player = other.transform;
        pickPoket = other.GetComponent<PickPoket>();

        if (instructionKey1) instructionKey1.SetActive(true);
        if (instructionKey2) instructionKey2.SetActive(true);

        UpdateInstructionColors();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (instructionKey1) instructionKey1.SetActive(false);
        if (instructionKey2) instructionKey2.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInside || isTeleporting) return;
        if (!IsPlayerActuallyNear()) return;

        // Keep colors updated (in case key is picked while standing near door)
        UpdateInstructionColors();

        // Teleport to A
        if (Input.GetKeyDown(keyToA))
        {
            if (CanUseTeleportA())
                TryTeleport(teleportLocationA);
            return;
        }

        // Teleport to B
        if (allowSecondTeleport && Input.GetKeyDown(keyToB))
        {
            if (CanUseTeleportB())
                TryTeleport(teleportLocationB);
        }
    }

    // ---------- Key Checks ----------
    private bool CanUseTeleportA()
    {
        if (teleportLocationA == null) return false;

        if (!keyRequiredForA) return true;
        if (locationAUnlocked) return true;

        if (!HasCorrectKeyForThisDoor()) return false;

        locationAUnlocked = true;
        return true;
    }

    private bool CanUseTeleportB()
    {
        if (!allowSecondTeleport) return false;
        if (teleportLocationB == null) return false;

        if (!keyRequiredForB) return true;
        if (locationBUnlocked) return true;

        if (!HasCorrectKeyForThisDoor()) return false;

        locationBUnlocked = true;
        return true;
    }

    // This door requires either basement1Key or basement2Key depending on inspector
    private bool HasCorrectKeyForThisDoor()
    {
        if (pickPoket == null) return false;

        // Must choose exactly one
        if (basement1Door == basement2Door)
        {
            Debug.LogWarning($"{name}: Set ONLY one of basement1Door or basement2Door to true.");
            return false;
        }

        if (basement1Door) return pickPoket.basement1Key;
        if (basement2Door) return pickPoket.basement2Key;

        return false;
    }

    // ---------- Instruction Colors ----------
    private void UpdateInstructionColors()
    {
        bool hasKey = HasCorrectKeyForThisDoor();

        // Instruction 1 = Location A
        if (instruction1SR != null)
        {
            if (!keyRequiredForA) instruction1SR.color = notRequiredColor;
            else instruction1SR.color = hasKey ? hasKeyColor : missingKeyColor;
        }

        // Instruction 2 = Location B
        if (instruction2SR != null)
        {
            if (!keyRequiredForB) instruction2SR.color = notRequiredColor;
            else instruction2SR.color = hasKey ? hasKeyColor : missingKeyColor;
        }

        // If B is not allowed, you may want to hide instruction 2 completely
        if (instructionKey2 != null)
            instructionKey2.SetActive(allowSecondTeleport && isPlayerInside);
    }

    // ---------- Range + Teleport ----------
    private bool IsPlayerActuallyNear()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(player.position, transform.position);
        bool isNear = distance <= maxDistance;

        if (!isNear) isPlayerInside = false;

        return isNear;
    }

    private void TryTeleport(Transform target)
    {
        if (player == null || target == null) return;
        StartCoroutine(TeleportWithDelay(target));
    }

    private IEnumerator TeleportWithDelay(Transform target)
    {
        isTeleporting = true;
        isPlayerInside = false;

        yield return new WaitForSeconds(teleportDelay);

        if (player != null && target != null)
            player.position = target.position;

        yield return new WaitForSeconds(0.05f);
        isTeleporting = false;
    }

    private void OnDisable()
    {
        isPlayerInside = false;
        isTeleporting = false;
    }
}
