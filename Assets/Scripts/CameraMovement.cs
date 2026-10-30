
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private SpriteRenderer background;

    private Vector3 _offset;
    private float _minX, _maxX, _minY, _maxY;
    private Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _offset = transform.position - player.transform.position;

        // Granice t³a
        Bounds bounds = background.bounds;

        float cameraHeight = _camera.orthographicSize;
        float cameraWidth = cameraHeight * _camera.aspect;

        _minX = bounds.min.x + cameraWidth;
        _maxX = bounds.max.x - cameraWidth;
        _minY = bounds.min.y + cameraHeight;
        _maxY = bounds.max.y - cameraHeight;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = player.transform.position + _offset;

        targetPosition.x = Mathf.Clamp(targetPosition.x, _minX, _maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, _minY, _maxY);

        transform.position = targetPosition;
    }
}
