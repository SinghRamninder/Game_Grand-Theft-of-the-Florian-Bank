using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private Transform teleportLocation; // Assign for EACH door
    private bool isPlayerInside = false;
    private Transform player;

    void Start()
    {
        // Cache the player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log($"Player entered door trigger: {gameObject.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log($"Player exited door trigger: {gameObject.name}");
        }
    }

    void Update()
    {
        // Double-check player is actually close to this door before allowing teleport
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && IsPlayerActuallyNear())
        {
            Debug.Log($"Teleporting through door: {gameObject.name}");
            StartCoroutine(TeleportWithDelay());
        }
    }

    // Additional safety check to ensure player is actually near this door
    private bool IsPlayerActuallyNear()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(player.position, transform.position);
        float maxDistance = 3f; // Adjust this based on your trigger size

        bool isNear = distance <= maxDistance;
        if (!isNear)
        {
            // Reset the flag if player is not actually near
            isPlayerInside = false;
            Debug.LogWarning($"Player not actually near door {gameObject.name}, resetting trigger");
        }

        return isNear;
    }

    IEnumerator TeleportWithDelay()
    {
        // Prevent multiple teleports
        isPlayerInside = false;

        yield return new WaitForSeconds(0.5f);

        if (player != null && teleportLocation != null)
        {
            player.position = teleportLocation.position;
        }
    }

    // Force reset the trigger state (useful for debugging)
    void OnDisable()
    {
        isPlayerInside = false;
    }
}using System.Collections;
using UnityEngine;

public class TeleportTrigger2D : MonoBehaviour
{
    [SerializeField] private Transform teleportLocation; // Assign for EACH door
    private bool isPlayerInside = false;
    private Transform player;

    void Start()
    {
        // Cache the player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log($"Player entered door trigger: {gameObject.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log($"Player exited door trigger: {gameObject.name}");
        }
    }

    void Update()
    {
        // Double-check player is actually close to this door before allowing teleport
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && IsPlayerActuallyNear())
        {
            Debug.Log($"Teleporting through door: {gameObject.name}");
            StartCoroutine(TeleportWithDelay());
        }
    }

    // Additional safety check to ensure player is actually near this door
    private bool IsPlayerActuallyNear()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(player.position, transform.position);
        float maxDistance = 3f; // Adjust this based on your trigger size

        bool isNear = distance <= maxDistance;
        if (!isNear)
        {
            // Reset the flag if player is not actually near
            isPlayerInside = false;
            Debug.LogWarning($"Player not actually near door {gameObject.name}, resetting trigger");
        }

        return isNear;
    }

    IEnumerator TeleportWithDelay()
    {
        // Prevent multiple teleports
        isPlayerInside = false;

        yield return new WaitForSeconds(0.5f);

        if (player != null && teleportLocation != null)
        {
            player.position = teleportLocation.position;
        }
    }

    // Force reset the trigger state (useful for debugging)
    void OnDisable()
    {
        isPlayerInside = false;
    }
}