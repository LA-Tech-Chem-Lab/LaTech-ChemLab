using UnityEngine;

public class orbitCameraAround : MonoBehaviour
{   
    public Transform cam;
    public Transform room;
    public float heightAbove;
    public float dist = 50f;
    public float angle = 0f;
    public float rotationAm = 0.4f;
    
    void Update()
    {
        cam.position = new Vector3(Mathf.Sin(angle) * dist, heightAbove, Mathf.Cos(angle) * dist);
        cam.LookAt(room);

        heightAbove += Time.deltaTime;
        angle += rotationAm * Time.deltaTime;
    }
}
