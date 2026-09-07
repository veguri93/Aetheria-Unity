using UnityEngine;

public class PlayerMovementNetwork : MonoBehaviour
{
    private PlayerMovement _movement;
    private GameServerConnection _connection;

    private Vector3 _lastSentPosition;
    private float _sendTimer;
    private bool _spawnReceived;

    [SerializeField] private float _sendInterval = 0.1f;
    [SerializeField] private float _positionThreshold = 0.05f;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
    }

    public void OnSpawnReceived()
    {
        _spawnReceived = true;

        _lastSentPosition = transform.position;
        _sendTimer = 0f;
    }

    private void Update()
    {
        if (!_spawnReceived)
            return;

        if (_connection == null)
        {
            _connection =
                FindAnyObjectByType<GameServerConnection>();

            if (_connection == null)
                return;
        }

        _sendTimer += Time.deltaTime;

        if (_sendTimer < _sendInterval)
            return;

        _sendTimer = 0f;

        Vector3 currentPosition = transform.position;

        if (Vector3.Distance(
                currentPosition,
                _lastSentPosition) < _positionThreshold)
        {
            return;
        }

        _lastSentPosition = currentPosition;

        _connection.SendPlayerMove(
            currentPosition,
            transform.eulerAngles.y);
    }
}