using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RTSCamera : MyMonoBehaviour {
    bool isRotating = false;
    Vector3 mouseStartPostion;
    Vector3 startRotation;
    public World world;
    public Vector2 RotationSensitivity = new Vector2 (0.5f, 0.5f);
    public Vector2 TranslationSensitivity = new Vector2(1f, 1f);
    public float zoomSensitivity = 15f;
    public float cameraElevationRate = 1f;

    //public Vector3 maxTranslation;
    //public Vector3 minTranslation;
    public float maxHeight;

    List<ICameraObserver> observers = new List<ICameraObserver>();
    // Update is called once per frame
    public override void MyUpdate()
    {
        
    }
    
    public static float InvertMouseY(float mouseY)
    {
        return Screen.height - mouseY;
    }

    public void CheckCameraUpdate()
    {
        bool hasChanged = false;

        if (Input.GetKey(KeyCode.Mouse2))
        {
            if (!isRotating)
            {
                isRotating = true;
                mouseStartPostion = Input.mousePosition;
                startRotation = transform.rotation.eulerAngles;
            }
            else
            {
                Vector3 currentMousePosition = Input.mousePosition;
                transform.rotation = Quaternion.Euler(new Vector3(startRotation.x - (currentMousePosition.y - mouseStartPostion.y) * RotationSensitivity.y, 
                                                                  startRotation.y + (currentMousePosition.x - mouseStartPostion.x) * RotationSensitivity.x, 
                                                                  0));
            }
            hasChanged = true;
        }
        else {
            isRotating = false;
        }
        transform.Translate(new Vector3(
                                        Input.GetAxis("Horizontal"),
                                        TranslationSensitivity.x 
                                            * Input.GetAxis("Vertical") 
                                            * Mathf.Sin(transform.rotation.eulerAngles.x * Mathf.PI / 180), 
                                        TranslationSensitivity.y 
                                            * Input.GetAxis("Vertical") 
                                            * Mathf.Cos(transform.rotation.eulerAngles.x * Mathf.PI / 180) 
                                            + Input.GetAxis("Mouse ScrollWheel") 
                                            * zoomSensitivity), 
                            Space.Self);
        //transform.Translate(new Vector3(Input.GetAxis("Horizontal"), TranslationSensitivity.x * Input.GetAxis("Vertical") * Mathf.Sin(transform.rotation.eulerAngles.x * Mathf.PI / 180), TranslationSensitivity.y * Input.GetAxis("Vertical") * Mathf.Cos(transform.rotation.eulerAngles.x * Mathf.PI / 180) + Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity), Space.Self);

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            hasChanged = true;
        }
        /* camera bounds
        if (transform.position.x > maxTranslation.x)
        {
            transform.position = new Vector3(maxTranslation.x, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < minTranslation.x)
        {
            transform.position = new Vector3(minTranslation.x, transform.position.y, transform.position.z);
        }
        if (transform.position.y > maxTranslation.y)
        {
            transform.position = new Vector3(transform.position.x, maxTranslation.y, transform.position.z);
        }
        else if (transform.position.y < minTranslation.y)
        {
            transform.position = new Vector3(transform.position.x, minTranslation.y, transform.position.z);
        }
        if (transform.position.z > maxTranslation.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, maxTranslation.z);
        }
        else if (transform.position.z < minTranslation.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, minTranslation.z);
        }*/

        if (hasChanged)
        {
            UpdateObservers();
        }
    }
    public void Subscribe(ICameraObserver obs)
    {
        observers.Add(obs);
    }

    void UpdateObservers()
    {
        foreach (ICameraObserver i in observers)
        {
            i.OnCameraMove(transform.position, world);
        }
    }
    
    public void RaiseCamera()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + cameraElevationRate, transform.position.z);
    }
    public void LowerCamera()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - cameraElevationRate, transform.position.z);
    }
}
