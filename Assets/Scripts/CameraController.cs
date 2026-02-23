using System;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    private InputManager input;

    public Transform targetTransform;
    public Transform camTransform;
    public Transform pivotTransform;
    
    
    // camera sensitivity
    public static float orbitSpeed = .03f;
    public static float followSpeed = .1f;
    public static float tiltSpeed = .03f;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    
    // limit tilt
    public float minPivot = -35;
    public float maxPivot = 60;
    private float orbitAngle;
    private float pivotAngle;
    
    // sasquatch 
    
    // detecting obstruction
    private Vector3 calcCamPos;
    public LayerMask layersToHit;

    public float cameraSphereRadius = .2f;
    public float cameraCollisionOffset = .2f;
    public float minCollisionOffset = .2f;
    
    public float zoomInSpeed = 10f;   
    public float zoomOutSpeed = 5f;   

    public float targetCamZ;
    private float defaultCamZ;
    
    private void Awake()
    {
        defaultCamZ = camTransform.localPosition.z;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = InputManager.instance;
        
    }

    // Update is called once per frame
    void Update()
    {
        FollowTarget(Time.deltaTime);
        HandleCameraRotation(Time.deltaTime);
        HandleCameraCollision(Time.deltaTime);
    }

    private void FollowTarget(float delta)
    {
        Vector3 targetPos = Vector3.SmoothDamp(transform.position, targetTransform.position, ref cameraFollowVelocity, delta/followSpeed);
        
        transform.position = targetPos;
    }

    private void HandleCameraRotation(float delta)
    {
        float mouseX = input.Look.x;
        float mouseY = input.Look.y;
        
        orbitAngle += (mouseX * orbitSpeed) / delta;
        
        pivotAngle -= (mouseY * tiltSpeed) / delta;
        
        pivotAngle = Mathf.Clamp(pivotAngle, minPivot, maxPivot);
        
        // orbit only
        Vector3 rotation = Vector3.zero;
        
        rotation.y = orbitAngle;
        
        Quaternion targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;
        
        // pivot only
        rotation = Vector3.zero;
        
        rotation.x = pivotAngle;
        
        targetRotation = Quaternion.Euler(rotation);
        pivotTransform.localRotation = targetRotation;
        
    }

    private void HandleCameraCollision(float delta)
    {
        bool isColliding = false;
        // targetcamz is location we want to go
        // defaut is normal
        targetCamZ = defaultCamZ;
        
        Vector3 direction = camTransform.position - pivotTransform.position;
        direction.Normalize();
       
        RaycastHit hit;
        
        if (Physics.SphereCast(pivotTransform.position, cameraSphereRadius, direction, out hit, Math.Abs(targetCamZ), layersToHit))
        {
            isColliding = true;
            float dis =  Vector3.Distance(pivotTransform.position, hit.point);
            
            targetCamZ = -(dis - cameraCollisionOffset);
            // if our new target position is less than our minimum offset, just go staight to minimum offset.
            // this will keep the camera from going inside the player
            if (Mathf.Abs(targetCamZ) < minCollisionOffset)
            {
                targetCamZ = -minCollisionOffset;
            }
            
        }
        
        
        float speed = isColliding ? zoomInSpeed : zoomOutSpeed;
        
        // calculate and move camera to the new position
        calcCamPos.z = Mathf.Lerp(camTransform.localPosition.z, targetCamZ, delta*speed);
        camTransform.localPosition = calcCamPos;
        
    }
    
}
