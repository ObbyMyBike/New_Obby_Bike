using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class CameraControl : MonoBehaviour
{
    private const float MANUAL_ROT_EPS = 0.0001f;
    public static bool IsPause;

    [Header("General")]
    [SerializeField] private float _sensitivity = 10f;
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _height = 2.5f;

    [Header("Over The Shoulder")]
    [SerializeField] private float _offsetPosition = 0f;

    [Header("Invert")]
    [SerializeField] private InversionX _inversionX = InversionX.Disabled;
    [SerializeField] private InversionY _inversionY = InversionY.Disabled;

    [Header("Collision")]
    [SerializeField] private LayerMask _collisionLayers;
    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _sphereRadius = 0.2f;

    [Header("Finish Orbit")]
    [SerializeField] private float _autoOrbitSpeedDeg = 30f;
    [SerializeField] private float _autoOrbitPitchDeg = 10f;

    private Transform _playerTransform;
    private UIInfo _uiInfo;
    private AudioSource _cameraSource;
    private IUIController _uiController;
    private Transform _orbitTarget;

    private Vector3 _baseOffset;

    private int _cameraTouchId = -1;
    private float _yaw;
    private float _pitch;
    private bool _autoOrbit;

    [Inject]
    public void Construct(Player player, [Inject(Optional = true)] UIInfo uiInfo, IUIController uiController)
    {
        _playerTransform = player.PlayerCharacterRoot.transform;
        _uiInfo = uiInfo;
        _uiController = uiController;
        _cameraSource = GetComponent<AudioSource>();

        _baseOffset = new Vector3(_offsetPosition, _height, -_distance);

        Vector3 initialEulerAngles = transform.eulerAngles;
        _yaw = NormalizeAngle180(initialEulerAngles.y);
        _pitch = NormalizeAngle180(initialEulerAngles.x);
    }

    private void Start()
    {
        gameObject.tag = "MainCamera";
        
        _yaw = NormalizeAngle180(0.7132265f);
        _pitch = NormalizeAngle180(3.5332987f);
        _autoOrbit = false;
        _baseOffset = new Vector3(_offsetPosition, _height, -_distance);
    }

    private void OnEnable()
    {
        if (_uiController != null)
        {
            _uiController.PauseRequested += OnPauseRequested;
            _uiController.ResumeRequested += OnResumeRequested;
        }
    }

    private void OnDisable()
    {
        if (_uiController != null)
        {
            _uiController.PauseRequested -= OnPauseRequested;
            _uiController.ResumeRequested -= OnResumeRequested;
        }
    }

    private void LateUpdate()
    {
        if (_playerTransform == null || IsPause)
            return;

        if (_uiInfo != null && _uiInfo.IsDown)
            return;

        if (_autoOrbit)
        {
            _yaw += _autoOrbitSpeedDeg * Time.deltaTime;
            _pitch = _autoOrbitPitchDeg;

            UpdateCamera(true);
        }
        else
        {
            UpdateCamera(false);
        }
    }

    public void SetUIController(IUIController uiController)
    {
        _cameraSource = GetComponent<AudioSource>();
        _uiController = uiController;
    }

    public void StartAutoOrbit(Transform target, float speedDeg = -1f, float pitchDeg = float.NaN)
    {
        _orbitTarget = target != null ? target : _playerTransform;

        if (speedDeg > 0f)
            _autoOrbitSpeedDeg = speedDeg;

        if (!float.IsNaN(pitchDeg))
            _autoOrbitPitchDeg = pitchDeg;

        _autoOrbit = true;
    }

    public void StopAutoOrbit() => _autoOrbit = false;

    private float NormalizeAngle180(float angle)
    {
        angle %= 360f;

        if (angle > 180f)
            angle -= 360f;

        if (angle < -180f)
            angle += 360f;

        return angle;
    }

    private void UpdateCamera(bool ignoreUserInput = false)
    {
        float deltaX = 0f;
        float deltaY = 0f;
        bool isRotating = false;

        if (!ignoreUserInput)
        {
            if (Application.isMobilePlatform)
            {
                if (_cameraTouchId == -1)
                {
                    foreach (Touch touch in Input.touches)
                    {
                        if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width * 0.5f)
                        {
                            if (EventSystem.current != null &&
                                EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                                continue;

                            _cameraTouchId = touch.fingerId;

                            break;
                        }
                    }
                }

                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.touches[i];

                    if (touch.fingerId == _cameraTouchId)
                    {
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        {
                            _cameraTouchId = -1;

                            break;
                        }

                        if (touch.phase == TouchPhase.Moved)
                        {
                            isRotating = true;
                            deltaX = touch.deltaPosition.x;
                            deltaY = touch.deltaPosition.y;
                        }

                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                            _cameraTouchId = -1;

                        break;
                    }
                }
            }
            else
            {
                isRotating = Input.GetMouseButton(0);

                if (isRotating && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    isRotating = false;

                if (isRotating)
                {
                    deltaX = Input.GetAxis("Mouse X");
                    deltaY = Input.GetAxis("Mouse Y");
                }
            }

            if (isRotating && (Mathf.Abs(deltaX) > MANUAL_ROT_EPS || Mathf.Abs(deltaY) > MANUAL_ROT_EPS))
            {
                int inversionX = _inversionX == InversionX.Disabled ? 1 : -1;
                int inversionY = _inversionY == InversionY.Disabled ? -1 : 1;

                _yaw += deltaX * _sensitivity * Time.deltaTime * inversionX;
                _pitch += deltaY * _sensitivity * Time.deltaTime * inversionY;

                _yaw = NormalizeAngle180(_yaw);
                _pitch = NormalizeAngle180(_pitch);
            }
        }

        if (!ignoreUserInput && isRotating && (Mathf.Abs(deltaX) > MANUAL_ROT_EPS || Mathf.Abs(deltaY) > MANUAL_ROT_EPS))
        {
            int inversionX = _inversionX == InversionX.Disabled ? 1 : -1;
            int inversionY = _inversionY == InversionY.Disabled ? -1 : 1;

            _yaw = NormalizeAngle180(_yaw + deltaX * _sensitivity * Time.deltaTime * inversionX);
            _pitch = NormalizeAngle180(_pitch + deltaY * _sensitivity * Time.deltaTime * inversionY);
        }
        
        Transform target = _orbitTarget != null ? _orbitTarget : _playerTransform;
        Quaternion cameraRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        
        _baseOffset.x = _offsetPosition;
        _baseOffset.y = _height;
        _baseOffset.z = -_distance;
        
        Vector3 worldOffset = cameraRotation * _baseOffset;
        Vector3 lookAtTarget = target.position + Vector3.up * _height;
        Vector3 desiredPosition = lookAtTarget + worldOffset;
        Vector3 direction = desiredPosition - lookAtTarget;
        float maxDistance = direction.magnitude;
        
        if (Physics.SphereCast(new Ray(lookAtTarget, direction.normalized), _sphereRadius, out RaycastHit hit, maxDistance, _collisionLayers))
        {
            float hitDistance = Mathf.Max(hit.distance, _minDistance);
            desiredPosition = lookAtTarget + direction.normalized * hitDistance;
        }

        transform.position = desiredPosition;
        transform.rotation = Quaternion.LookRotation(lookAtTarget - transform.position, Vector3.up);
    }

    private void OnPauseRequested()
    {
        IsPause = true;

        if (_cameraSource != null && _cameraSource.isPlaying)
            _cameraSource.Pause();
    }

    private void OnResumeRequested()
    {
        IsPause = false;

        if (_cameraSource != null)
            _cameraSource.UnPause();
    }
}