using UnityEngine;

public class VanishingObj : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Material newMat;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeMaterial()
    {
        Renderer rend = GetComponent<Renderer>();

        if (rend != null && newMat != null)
        {
            rend.material = newMat;
        }
        
    }
}
