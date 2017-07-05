using UnityEngine;

public class DayNightCycle : MyMonoBehaviour {
    public float dayLength;
    public float time = 0;
    public float maxLightIntensity = 1;
    public Light directionalLight;

    // Use this for initialization
    public override void MyStart() {
        if (directionalLight == null)
        {
            directionalLight = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>();
        }
	}
	
	// Update is called once per frame
	public override void MyUpdate() {
        /*
        float dt = Time.deltaTime;
        time += dt;
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(time / dayLength * 180, 330, 0));
        if (time > dayLength)
        {
            float nightAmount = (time - dayLength) / dayLength;

            if (nightAmount > 0 && nightAmount < .1)
            {
                directionalLight.intensity = (1 - (nightAmount * 10)) * maxLightIntensity;
            }
            else if (nightAmount > 0.9 && nightAmount < 1)
            {
                directionalLight.intensity = ((nightAmount - 0.9f) * 10) * maxLightIntensity;
            }
        }

        
        time = time % (dayLength * 2);*/
	}
}