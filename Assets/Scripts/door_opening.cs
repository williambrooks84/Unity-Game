using UnityEngine;

public class door_opening : MonoBehaviour
{

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        this.transform.Rotate(0.0f, 90.0f, 0.0f);
    }

    void OnTriggerExit(Collider collider)
    {
        this.transform.Rotate(0.0f, -90.0f, 0.0f);
    }
}
