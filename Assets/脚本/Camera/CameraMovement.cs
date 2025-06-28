using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Range(0f, 100f)]
    public float CameraSpeed = 20;
    float ScrollWheel => Input.GetAxis("Mouse ScrollWheel");
    static InputActions input;
    public new Camera camera;
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
        if (ScrollWheel != 0)
            camera.orthographicSize *= (1 - ScrollWheel);
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
        transform.position += CameraSpeed * Time.deltaTime * Mathf.Pow(camera.orthographicSize / 25, 0.5f) * new Vector3(CameraMove.x, CameraMove.y);
    }
}
