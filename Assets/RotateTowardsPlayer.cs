using UnityEngine;

public class RotateTowardsPlayer : MonoBehaviour
{
    public Transform player;
    public float rotationSpeed = 2f;
    
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
    }
    
    void Update()
    {
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}