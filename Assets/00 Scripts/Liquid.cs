using UnityEngine;

public class Liquid : MonoBehaviour
{
    public Mesh mesh;
    public float fillamount;
    public MeshRenderer rend;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldPos = transform.TransformPoint(new Vector3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z));
        Vector3 pos = worldPos - transform.position - new Vector3(0, fillamount, 0);
        rend.sharedMaterial.SetVector("_FillAmount", pos);
    }
}
