using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Camera control parameters")]
    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _fastMovementSpeed = 100f;
    [SerializeField] private float _freeLookSensitivity = 3f;
    [SerializeField] private float _zoomSensitivity = 10f;
    [SerializeField] private float _fastZoomSensitivity = 50f;
    [SerializeField] private float _panSensitivity = 0.3f;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var movementSpeed = fastMode ? _fastMovementSpeed : this._movementSpeed;

        Vector3 moveDirection = new Vector3();
        if (Input.GetKey(KeyCode.W) || (Input.GetKey(KeyCode.Mouse1) && Input.GetKey(KeyCode.Mouse2))) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.W)) moveDirection += -transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection += -transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
        if (Input.GetKey(KeyCode.Space)) moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) moveDirection += Vector3.down;
        moveDirection = moveDirection * movementSpeed * Time.deltaTime;

        transform.position = TryToMove(transform.position, moveDirection);


        if (Input.GetKey(KeyCode.Mouse1))
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * _freeLookSensitivity;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * _freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        float axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var zoomSensitivity = fastMode ? this._fastZoomSensitivity : this._zoomSensitivity;
            transform.position = TryToMove(transform.position, transform.forward * axis * zoomSensitivity);
        }       

    }

    /// <summary>
    /// Checks if the original vector is within parameters after offset translation.
    /// </summary>
    /// <returns>Translated vector if its within parameters, or original vector if not</returns>
    private Vector3 TryToMove(Vector3 position, Vector3 offset)
    {
        if ((position + offset).x < 35f && (position + offset).x > -35f)
        {
            position.x += offset.x;
        }

        if ((position + offset).z < 35f && (position + offset).z > -35)
        {
            position.z += offset.z;
        }

        if ((position + offset).y < 35f && (position + offset).y > 2f)
        {
            position.y += offset.y;
        }

        return position;
    }

}
