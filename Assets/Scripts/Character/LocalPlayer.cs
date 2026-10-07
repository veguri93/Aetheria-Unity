using UnityEngine;

public class LocalPlayer : MonoBehaviour, ITargetable
{
    public static LocalPlayer Instance { get; private set; }
    [SerializeField]
    private PlayerNameplate playerNameplate;
    [SerializeField]
    private GameObject _selectionCircle;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    public int PlayerId { get; private set; }
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;
    private string _playerName = string.Empty;

    public int ObjectId =>
        PlayerId;

    public string Name =>
        _playerName;

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

        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(
                false);
        }
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
        _playerName =
            playerName;

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
    public void Select()
    {
        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(true);
        }
    }

    public void Deselect()
    {
        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(false);
        }
    }
    public void SetPlayerId(
    int playerId)
    {
        PlayerId =
            playerId;
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