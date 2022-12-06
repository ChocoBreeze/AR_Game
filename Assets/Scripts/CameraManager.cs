using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public float showUpTime = 3.0f;
    private new Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        camera = GetComponent<Camera>();
        camera.enabled = false;
    }

    public void showUp()
    {
        camera.enabled = true;
        Invoke("hide", showUpTime);
    }

    void hide()
    {
        camera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
