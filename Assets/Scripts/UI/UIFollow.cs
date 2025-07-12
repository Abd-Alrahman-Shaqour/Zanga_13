using UnityEngine;

public class UIFollow : MonoBehaviour
{
    void Update()
    {
        //Quaternion rotation = Camera.main.transform.rotation;
        //transform.LookAt(transform.position * rotation * Vector3.forward, rotation * Vector3.up);
        Vector3 direction = Camera.main.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }
}
