using UnityEngine;

public class door_opening : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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
