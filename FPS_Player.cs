using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Player : MonoBehaviour
{
    [Header("Zoom Control")]
    public bool EnableZoom;
    float velocityzoom = 0;
    float ViewDistance;
    private float oldFrequency;
    public KeyCode ZoomKey;
    public float ViewMultiplierDistance;
    public float SmoothView;
    float normalView;
    [Header ("Mouse Control")]
    public float mouseSensitivity = 100f;
    [Header ("Player Control")]
    public Transform PlayerBody;
    public Camera cam;
    float xRotation = 0f;


    public CharacterController controller;
    float _speed;
    [Header ("Walk Control")]
    public float Speed = 4f;
    Vector3 velocity;
    public float gravity = -9.81f;
    [Header("Jump Control")]
    public bool enableJump;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;
    public float jumpHeight = 3f;
    bool isGrounded;
    [Header("Sprint Control")]
    public bool EnableSprint;
    public KeyCode SprintKey;
    public float SprintMultiplier;
    float sprintSpeed;
    private float frequencySprint;
    public float Stamina;
    public float SpeedAcceleration = 1;
    float _Sprintspeed;
    float initvelocity = 0;
    [Header("Crouch Control")]
    public bool EnableCrouch;
    public KeyCode CrouchingKey;
    public float crouchHeight = 1.25f;
    public float crouchingMultiplier = 0.25f;
    float crouchingspeed;
    float standingHeight;

    [Header ("Bob head")]
    [SerializeField] private bool EnableBobHead = true;
    [SerializeField, Range(0, 0.1f)] private float _amplitude = 0.015f;
    [SerializeField, Range(0, 30f)] private float _frequency = 10f;
    [SerializeField] private Transform _camera = null;
    [SerializeField] private Transform _cameraHolder = null;

    private float _toggleSpeed = 3.0f;
    private Vector3 _startPos;
    // Start is called before the first frame update
    private void Awake()
    {
        cam = FindObjectOfType<Camera>();
     

    }
    void Start()
    {
        _startPos = _camera.localPosition;
        normalView = cam.fieldOfView;
        sprintSpeed = SprintMultiplier * Speed;
       frequencySprint = (_frequency * SprintMultiplier)/ 1.5f;
        standingHeight = controller.height;
        _speed = Speed;
        _Sprintspeed = sprintSpeed;
        crouchingspeed = Speed * crouchingMultiplier;
        ViewDistance = ViewMultiplierDistance * cam.fieldOfView;
         oldFrequency = _frequency;
        Cursor.lockState = CursorLockMode.Locked;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {

        if (EnableBobHead)
            BobHead();

        if (EnableZoom)
            Zoom();

        CameraView();
        Movement();
    }
    public void BobHead()
    {
        CheckMotion();
        ResetPosition();
        _camera.LookAt(FocusTarget());
    }
    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(PlayerBody.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
        pos += _cameraHolder.forward * 15f;
        return pos;
    }
    private Vector3 FootSteepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * _frequency) * _amplitude;
        pos.x += Mathf.Cos(Time.time * _frequency / 2) * _amplitude * 2;
        return pos;
    }
    private void CheckMotion()
    {

        float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        if (speed < _toggleSpeed) return;

        if (!isGrounded) return;
        PlayMotion(FootSteepMotion());
    }
    private void PlayMotion(Vector3 motion)
    {
        _camera.localPosition += motion;
    }
    private void ResetPosition()
    {
        if (_camera.localPosition == _startPos) return;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPos, 2* Time.deltaTime);
    }
    void CameraView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        _camera.localRotation = Quaternion.Euler(xRotation, 0F, 0F);
        PlayerBody.Rotate(Vector3.up * mouseX);
    }

    public void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(controller.bounds.min, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }
    void Movement()
    {
       
        GroundCheck();
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (enableJump) 
        Jump();
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        controller.Move(move * _speed * Time.deltaTime);

        if(EnableSprint)
         Sprint();

        if (EnableCrouch)
            Crouching();

    }
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {

            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

   
    }
    void Sprint()
    {
            if (Input.GetKey(SprintKey) && Input.GetAxis("Vertical") > 0)
            {
                if (_Sprintspeed > 0)
                {
                    _speed = Mathf.SmoothDamp(_speed, sprintSpeed, ref initvelocity, SpeedAcceleration);
                   _frequency = frequencySprint;
                    _Sprintspeed -= Time.deltaTime / Stamina;
                }
                else
                {
                    _speed = Speed;
                }
            }
            else
            {
                _frequency = oldFrequency;
                if (_Sprintspeed >= sprintSpeed)
                {
                    _Sprintspeed = sprintSpeed;
                    
                }
                else
                {
                    _speed = Speed;
                    _Sprintspeed += Time.deltaTime / Stamina;
                }

            }
        
    
    }

    void Crouching()
    {
            if (Input.GetKey(CrouchingKey) && !Input.GetKey(SprintKey))
            {
            controller.height = crouchHeight;
                _speed = crouchingspeed;
            }
            else
            {
                controller.height = standingHeight;
                
            }
            if (Input.GetKeyUp(CrouchingKey))
            {
                _speed = Speed;
            }
    }
    void Zoom()
    {
        if(Input.GetMouseButton(0) || Input.GetKey(ZoomKey) )
        {
          
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, ViewDistance, ref velocityzoom, SmoothView);
        }
        else
        {
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, normalView, ref velocityzoom, SmoothView) ;
        }
    }
}
