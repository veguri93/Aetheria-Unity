using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    public static LocalPlayer Instance { get; private set; }
    [SerializeField]
    private PlayerNameplate playerNameplate;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void Update()
    {
        if (transform.position != _lastPosition)
        {
            _lastPosition = transform.position;
        }

        if (transform.rotation != _lastRotation)
        {
            _lastRotation = transform.rotation;
        }
    }
    public void SetPlayerInfo(
    string playerName,
    string title)
    {
        if (playerNameplate == null)
        {
            Debug.LogWarning(
                "LocalPlayer nameplate is not assigned.");

            return;
        }

        playerNameplate.SetPlayerInfo(
            playerName,
            title);
    }
    public void SetSpawnPosition(
        Vector3 position,
        float rotationY)
    {
        transform.position = position;

        transform.rotation =
            Quaternion.Euler(
                0f,
                rotationY,
                0f);

        PlayerMovementNetwork network =
            GetComponent<PlayerMovementNetwork>();

        if (network != null)
        {
            network.OnSpawnReceived();
        }

    }
}