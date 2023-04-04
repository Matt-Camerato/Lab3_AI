using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMovingObstacle : MonoBehaviour
{
    public float minXPosition = -5f;    // The leftmost x position to move to
    public float maxXPosition = 5f;     // The rightmost x position to move to
    public float moveSpeed = 2f;        // The speed at which to move back and forth
    
    private bool movingRight = true;    // Whether the object is currently moving to the right

    void Update()
    {
        // If the object is moving to the right, move towards the rightmost position
        if (movingRight)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;

            // If we've reached the rightmost position, start moving left
            if (transform.position.x >= maxXPosition)
            {
                movingRight = false;
            }
        }
        // If the object is moving to the left, move towards the leftmost position
        else
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            // If we've reached the leftmost position, start moving right
            if (transform.position.x <= minXPosition)
            {
                movingRight = true;
            }
        }
    }
}
