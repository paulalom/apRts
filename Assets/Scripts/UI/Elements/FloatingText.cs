using UnityEngine;
using System.Collections;

// Todo: Outline on the text to make it easier to read. This should be good enough for now though
public class FloatingText : MonoBehaviour {
    
    public float duration = 5; // text on screen for 5s
    private float leftRightinterval = 0.75f; // text moves left to right in this number of seconds
    private float distToTop = 10; // distance the text travels in the y direction
    private float startTime;
    private float lastDirectionChange;
    private bool directionLeft = true;
    public TextMesh textMesh;
    UIManager uiManager;
    RTSCamera mainCamera;
    
    
    void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        textMesh = GetComponent<TextMesh>();
        startTime = Time.time; // this will need more logic if we implement pause
        lastDirectionChange = Time.time; // this too, and more!
    }

	// Update is called once per frame
	 void Update() {
        if (Time.time - startTime > duration)
        {
            uiManager.RemoveText(this);
            Destroy(gameObject);
            return;
        }
        else if (Time.time - lastDirectionChange > leftRightinterval)
        {
            directionLeft = !directionLeft;
            lastDirectionChange = Time.time;
        }

        float speed = distToTop / duration;
        float dt = StepManager.GetDeltaStep();
        float dy = speed * dt;
        // I want text to wave left and right but i would need to do trig and whatnot or use some other method of positioning (eg. GUI.Label), so thats TODO
        //+ (directionLeft ? transform.position.x - leftRightDisplacement : transform.position.x + leftRightDisplacement)
        transform.position = new Vector3(transform.position.x , transform.position.y + dy, transform.position.z);
    }
    public void SetColor(Color color)
    {
        textMesh.color = color;
    }

    void OnGUI()
    {
        transform.transform.rotation = mainCamera.transform.rotation;
        //GUI.Label(new Rect(position.x, position.y, Screen.width, Screen.height), textMesh.text);
    }
}
