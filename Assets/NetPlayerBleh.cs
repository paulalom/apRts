using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetPlayerBleh : NetworkBehaviour {
    
	void Update () {
        //If local player
        if (isLocalPlayer)
        {
            //Check rotation input
            float angle = 0;
            if (Input.GetKey(KeyCode.A)) angle -= 90f;
            if (Input.GetKey(KeyCode.D)) angle += 90f;

            //Rotate
            float finalAngle = transform.localRotation.eulerAngles.y + angle * Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0, finalAngle, 0);

            //Check movement input
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) movement += transform.forward * 4;
            if (Input.GetKey(KeyCode.S)) movement -= transform.forward * 4;

            //Move
            transform.localPosition += movement * Time.deltaTime;
        }
    }
}
