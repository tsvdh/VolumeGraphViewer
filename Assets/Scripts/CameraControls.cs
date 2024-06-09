using System;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    private Vector2 _curMousePos;
    
    private void Start()
    {
        _curMousePos = Input.mousePosition;
    }

    public void Init(Vector3 graphCenter, Vector3 graphMax)
    {
        // tan(half fov) = half height / dist to center
        // dist to center = half height / tan(30)
        float halfFoV = GetComponent<Camera>().fieldOfView / 2;
        Vector3 camPos = transform.position;
        camPos.z = -(graphMax.y - graphCenter.y) / (float)Math.Tan(Math.PI / 180 * halfFoV);
        transform.position = camPos;
    }
    
    private void Update()
    {
        const float moveSpeed = 10;
        const float arrowsTurnSpeed = 90;
        const float mouseTurnSpeed = 10;
        const float scrollSpeed = 10;

        Vector2 newMousePos = Input.mousePosition;
        Vector2 mouseDelta = newMousePos - _curMousePos;
        _curMousePos = newMousePos;
        
        Transform curTransform = transform;

        Vector3 forward = new Vector3(curTransform.forward.x, 0, curTransform.forward.z).normalized;
        
        if (Input.GetKey(KeyCode.W))
            curTransform.position += forward * (Time.deltaTime * moveSpeed);
        if (Input.GetKey(KeyCode.S))
            curTransform.position -= forward * (Time.deltaTime * moveSpeed);
        if (Input.GetKey(KeyCode.A))
            curTransform.position -= curTransform.right * (Time.deltaTime * moveSpeed);
        if (Input.GetKey(KeyCode.D))
            curTransform.position += curTransform.right * (Time.deltaTime * moveSpeed);
        if (Input.GetKey(KeyCode.UpArrow))
            curTransform.position += Vector3.up * (Time.deltaTime * moveSpeed);
        if (Input.GetKey(KeyCode.DownArrow))
            curTransform.position -= Vector3.up * (Time.deltaTime * moveSpeed);
        if (Input.mouseScrollDelta.y != 0)
            curTransform.position += Vector3.up * (Time.deltaTime * Input.mouseScrollDelta.y * scrollSpeed);
        
        Vector3 angles = curTransform.eulerAngles;
        if (Input.GetKey(KeyCode.RightArrow))
            angles.y += Time.deltaTime * arrowsTurnSpeed;
        if (Input.GetKey(KeyCode.LeftArrow))
            angles.y -= Time.deltaTime * arrowsTurnSpeed;
        if (Input.GetMouseButton(1))
            angles.y += Time.deltaTime * mouseDelta.x * mouseTurnSpeed;

        if (angles.x > 180)
            angles.x -= 360;
        
        // if (Input.GetKey(KeyCode.UpArrow))
        //     angles.x -= Time.deltaTime * arrowsTurnSpeed;
        // if (Input.GetKey(KeyCode.DownArrow))
        //     angles.x += Time.deltaTime * arrowsTurnSpeed;
        if (Input.GetMouseButton(1))
            angles.x -= Time.deltaTime * mouseDelta.y * mouseTurnSpeed;
        
        angles.x = Math.Clamp(angles.x, -80, 80);
        if (angles.x < 0)
            angles.x += 360;
        
        curTransform.eulerAngles = angles;
    }
}
