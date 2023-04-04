using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMovingObstacle : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float moveSpeed = 3f;

    private Vector3 center;
    private float angle;

    private void Start() => center = transform.position;

    private void Update()
    {
        //increment angle based on time and speed
        angle += moveSpeed * Time.deltaTime;

        //calculate new position using angle and radius
        Vector3 newPosition = new Vector3(
            center.x + radius * Mathf.Cos(angle),
            transform.position.y,
            center.z + radius * Mathf.Sin(angle)
        );

        //move object to new position
        transform.position = newPosition;
    }
}
