using UnityEngine;
using UnityEngine.AI;

public class FollowTarget : MonoBehaviour
{
    public Transform player;

    public float moveSpeed = 10f;
    public float stoppingDistance = 2f;

    void Update()
    {
        if (player == null)
            return;

        Vector3 direction = player.position - transform.position;

        if (direction.magnitude > stoppingDistance)
        {
            direction.Normalize();

            transform.position +=
                direction * moveSpeed * Time.deltaTime;

            transform.LookAt(player);
        }
    }

   // private void OnCollisionEnter(Collision Other)
   // {
       // if (gameObject.CompareTag("Enemy"))
   // }
}