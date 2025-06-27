using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Range(0f, 100f)]
    public float CameraSpeed = 20;
    static InputActions input;
    static Vector2 CameraMove => input.Camera.Move.ReadValue<Vector2>();
    // Start is called before the first frame update

    private void Awake()
    {
        input = new();
    }
    void Start()
    {
        input.Camera.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraMove.magnitude != 0)
            Move();
        if (input.Camera.Pause.triggered)
            Pause();
    }

    void Pause()
    {
        //if (Time.timeScale == 0)
        //    Time.timeScale = 1;
        //else
        //    Time.timeScale = 0;
        if (WorldManager.TimeScale == 0)
            WorldManager.TimeScale = 1;
        else
            WorldManager.TimeScale = 0;
    }

    void Move()
    {
        transform.position += new Vector3(CameraSpeed * CameraMove.x * Time.deltaTime, CameraSpeed * CameraMove.y * Time.deltaTime);
    }
}
