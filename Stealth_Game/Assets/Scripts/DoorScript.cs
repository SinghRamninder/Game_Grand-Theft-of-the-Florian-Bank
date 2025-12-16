using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private GameObject instructionKey1;
    [SerializeField] private GameObject instructionKey2;

    [Header("Teleport Locations")]
    [SerializeField] private Transform teleportLocationA;   // main destination
    [SerializeField] private bool allowSecondTeleport = false;
    [SerializeField] private Transform teleportLocationB;   // optional second destination

    [Header("Input Keys")]
    [SerializeField] private KeyCode keyToA = KeyCode.E;
    [SerializeField] private KeyCode keyToB = KeyCode.Q;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float teleportDelay = 0.5f;

    [Header("Key Requirement (ONLY for Location B)")]
    [SerializeField] private bool keyRequired = false;

    [Tooltip("Enable this if this door should be unlocked by Basement 1 Key")]
    [SerializeField] private bool basement1Door = false;

    [Tooltip("Enable this if this door should be unlocked by Basement 2 Key")]
    [SerializeField] private bool basement2Door = false;

    private bool isPlayerInside = false;
    private Transform player;
    private PickPoket pickPoket;          // script on the player
    private bool isTeleporting = false;

    // Once unlocked, Location B stays usable permanently
    private bool locationBUnlocked = false;

    private void Start()
    {
        // Optional fallback if you want (but we also cache via trigger)
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) pickPoket = player.GetComponent<PickPoket>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (instructionKey1 != null)
        {
            instructionKey1.SetActive(true);
        }
        if (instructionKey2 != null)
        {
            instructionKey2.SetActive(true);
        }

        isPlayerInside = true;

        // Cache references using trigger (as you requested)
        player = other.transform;
        pickPoket = other.GetComponent<PickPoket>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (instructionKey1 != null)
        {
            instructionKey1.SetActive(false);
        }
        if (instructionKey2 != null)
        {
            instructionKey2.SetActive(false);
        }

        isPlayerInside = false;
    }

    private void Update()
    {
        if (!isPlayerInside || isTeleporting) return;
        if (!IsPlayerActuallyNear()) return;

        // Teleport to A (no key logic)
        if (Input.GetKeyDown(keyToA))
        {
            TryTeleport(teleportLocationA);
            return;
        }

        // Teleport to B (optional + key logic only here)
        if (allowSecondTeleport && Input.GetKeyDown(keyToB))
        {
            if (CanUseTeleportB())
                TryTeleport(teleportLocationB);
        }
    }

    private bool CanUseTeleportB()
    {
        if (!allowSecondTeleport) return false;
        if (teleportLocationB == null) return false;

        // If no key required, allow directly
        if (!keyRequired) return true;

        // If already unlocked earlier, allow permanently
        if (locationBUnlocked) return true;

        // Need PickPoket script to check keys
        if (pickPoket == null) return false;

        // Door type validation (avoid both off / both on mistakes)
        if (basement1Door == basement2Door)
        {
            Debug.LogWarning($"{name}: Set ONLY one of basement1Door or basement2Door to true.");
            return false;
        }

        bool hasCorrectKey =
            (basement1Door && pickPoket.basement1Key) ||
            (basement2Door && pickPoket.basement2Key);

        if (hasCorrectKey)
        {
            locationBUnlocked = true; // permanently unlocked
            return true;
        }

        // No correct key
        return false;
    }

    private bool IsPlayerActuallyNear()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(player.position, transform.position);
        bool isNear = distance <= maxDistance;

        if (!isNear)
            isPlayerInside = false;

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
