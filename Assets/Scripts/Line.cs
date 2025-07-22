using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeFollower : MonoBehaviour
{
    public Transform[] planches; // Mets ici toutes les planches du pont (dans l'ordre)
    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = planches.Length;
    }

    void Update()
    {
        for (int i = 0; i < planches.Length; i++)
        {
            lr.SetPosition(i, planches[i].position);
        }
    }
}
