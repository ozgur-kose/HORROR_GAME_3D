/*
using UnityEngine;

public class doorScript : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door;
    public float openY = 270f;
    public float closeY = 0f;
    public float speed = 2f;

    [Header("Detection Settings")]
    public string enemyTag = "MainEnemy";
    public string playerTag = "Player";

    private bool open = false;

    void Awake()
    {
        // Eðer Inspector'da atanmadýysa, parent'tan pivotu bul
        if (door == null)
        {
            Transform p = transform.parent;
            while (p != null)
            {
                if (p.name.ToLower().Contains("pivot"))
                {
                    door = p;
                    Debug.Log($"[DoorScript] Pivot otomatik bulundu: {door.name}");
                    break;
                }
                p = p.parent;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag) || other.CompareTag(playerTag))
            open = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag) || other.CompareTag(playerTag))
            open = false;
    }

    void Update()
    {
        if (door == null) return; // Hata önlemi

        float targetY = open ? openY : closeY;
        Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);
        door.rotation = Quaternion.Lerp(door.rotation, targetRotation, Time.deltaTime * speed);
    }
}
*/
using UnityEngine;

public class doorScript : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door;          // Pivot (manuel ya da otomatik)
    public float openY = 270f;
    public float closeY = 0f;
    public float speed = 2f;

    [Header("Detection Settings")]
    public string enemyTag = "MainEnemy";
    public string playerTag = "Player";

    private bool open = false;

    void Awake()
    {
        // Eðer Inspector'da atanmadýysa, SADECE kendi parent'ýnda pivot ara
        if (door == null)
        {
            if (transform.parent != null && transform.parent.name.ToLower().Contains("pivot"))
            {
                door = transform.parent;
                Debug.Log($"[DoorScript] Pivot bulundu: {door.name} (parent)");
            }
            else
            {
                Debug.LogWarning($"[DoorScript] Bu kapýnýn parent'ýnda pivot bulunamadý: {name}");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag) || other.CompareTag(playerTag))
            open = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag) || other.CompareTag(playerTag))
            open = false;
    }

    void Update()
    {
        if (door == null) return; // atanmadýysa hiçbir þey yapma

        float targetY = open ? openY : closeY;
        Quaternion target = Quaternion.Euler(0, targetY, 0);
        door.rotation = Quaternion.Lerp(door.rotation, target, Time.deltaTime * speed);
    }

    void TriggerEnter(Collider other)
    {
        Debug.Log("Trigger tetiklendi: " + other.name); // test satýrý
        if (other.CompareTag(enemyTag) || other.CompareTag(playerTag))
            open = true;
    }
}
