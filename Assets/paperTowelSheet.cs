using Unity.VisualScripting;
using UnityEngine;

public class paperTowelSheet : MonoBehaviour
{
    public int sheets = 1;
    public GameObject doubleSheet;
    public float snapDist = 0.5f;
    [Tooltip("Minimum speed change to detect impact")]
    public float impactThreshold = 1.0f;
    float prevYVel = -1f;
    Rigidbody rb;
    float spawnTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (sheets == 1)
            CheckImpactWithTable();
        
    }

    void CheckImpactWithTable()
    {
        float currentYVel = rb.linearVelocity.y;

        bool wasFalling = prevYVel < 0;
        bool slowedDownFast = currentYVel - prevYVel > impactThreshold;

        if (wasFalling && slowedDownFast && Time.time - spawnTime > 0.3f)
            findClosestPaperTowel();

        
        prevYVel = rb.linearVelocity.y;

    }

    void findClosestPaperTowel(){
        GameObject closest = null;
        float closestDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject gO in GameObject.FindObjectsOfType<GameObject>())
        {
            if (gO.name == "Paper Towel Sheet(Clone)" && gO != gameObject)
            {
                float dist = Vector3.Distance(currentPos, gO.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = gO;
                }
            }
        }

        if (closest != null)
            if (closestDist <= snapDist)
                snapToExistingPaperTowel(closest);
    }

    void snapToExistingPaperTowel(GameObject existing){
        Vector3 avgPos = transform.position / 2f + existing.transform.position / 2f + Vector3.up * 0.1f;
        float yRotation = existing.transform.localEulerAngles.y - 90f;

        GameObject spawnedDoubleSheet = Instantiate(doubleSheet, avgPos, Quaternion.Euler(new Vector3(0f, yRotation, 0f)));
        spawnedDoubleSheet.GetComponent<paperTowelSheet>().sheets = 2;
        Destroy(existing);
        Destroy(gameObject);
    }
}
