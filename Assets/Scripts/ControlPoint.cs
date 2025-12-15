using UnityEngine;
using UnityEngine.UI;

public class ControlPoint : MonoBehaviour
{
    [Header("Visuals")]
    public Renderer ringRenderer;   
    public Material redMaterial;   
    public Material blueMaterial; 

    [Header("Capture Settings")]
    public Slider captureSlider; 
    public float captureTime = 15f; 

    private float currentTime = 0f; 
    private bool playerInside = false;
    private bool captured = false;

    public bool IsCaptured() { return captured; }

    void Start()
    {
        ringRenderer.material = redMaterial;
        captureSlider.gameObject.SetActive(false); 
        captureSlider.value = 0;
    }

    void Update()
    {
        if (captured) return; 

        if (playerInside)
        {
            currentTime += Time.deltaTime;
            captureSlider.value = currentTime;

            if (currentTime >= captureTime)
            {
                CapturePoint();
            }
        }
        else if (currentTime > 0)
        {
            currentTime -= Time.deltaTime * 2f;
            if (currentTime < 0) currentTime = 0;
            captureSlider.value = currentTime;
        }
    }

    private void CapturePoint()
    {
        captured = true;
        ringRenderer.material = blueMaterial; 
        captureSlider.gameObject.SetActive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !captured)
        {
            playerInside = true;
            captureSlider.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !captured)
        {
            playerInside = false;
        }
    }
}
