using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {

    public bool isActive = true;
    public float moveSpeed = 0.3f;
    public Vector3 velocity;

    public Vector2 Velocity2D
    {
        get
        {
            return new Vector2(velocity.x, velocity.z);
        }
    }
    public void SetVelocity2D(Vector2 vel2d)
    {
        velocity.x = vel2d.x;
        velocity.z = vel2d.y;
    }
}
