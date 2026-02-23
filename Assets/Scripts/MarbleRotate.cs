using UnityEngine;

public class MarbleRotate : MonoBehaviour
{
    [SerializeField] Transform marbleVisual;
    [SerializeField] float radius = 0.5f;

    void FixedUpdate()
    {
        Vector3 velocity = PlayerController.rb.linearVelocity;
        velocity.y = 0;

        if (velocity.sqrMagnitude < 0.001f)
            return;

        Vector3 axis = Vector3.Cross(Vector3.up, velocity.normalized);
        float angularSpeed = velocity.magnitude / radius;

        marbleVisual.Rotate(
            axis,
            angularSpeed * Mathf.Rad2Deg * Time.deltaTime,
            Space.World
        );
    }
}
