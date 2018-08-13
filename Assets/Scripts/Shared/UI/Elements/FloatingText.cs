using UnityEngine;
using System.Collections;

// Todo: Outline on the text to make it easier to read. This should be good enough for now though
public class FloatingText : MonoBehaviour {
    
    public long duration = 5000; // text on screen for 5s
    private long leftRightinterval = 750; // text moves left to right in this number of ms
    private long distToTop = 10; // distance the text travels in the y direction
    private long startTime;
    private long lastDirectionChange;
    private bool directionLeft = true;
    public TextMesh textMesh;
    UIManager uiManager;
    RTSCamera mainCamera;
    
    
    void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RTSCamera>();
        textMesh = GetComponent<TextMesh>();
        startTime = StepManager.gameTime;
        lastDirectionChange = StepManager.gameTime;
    }

	// Update is called once per frame
	 void Update() {
        if (StepManager.gameTime - startTime > duration)
        {
            uiManager.RemoveText(this);
            Destroy(gameObject);
            return;
        }
        else if (StepManager.gameTime - lastDirectionChange > leftRightinterval)
        {
            directionLeft = !directionLeft;
            lastDirectionChange = StepManager.gameTime;
        }

        float speed = distToTop / (float)duration;
        int dt = StepManager.GetDeltaStep();
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
