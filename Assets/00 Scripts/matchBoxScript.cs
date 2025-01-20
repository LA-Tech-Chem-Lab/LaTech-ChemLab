using System.Collections;
using UnityEngine;

public class matchBoxScript : MonoBehaviour
{   
    public float speedMult = 1f;
    public bool animationPlaying;
    public int sleeveDir;
    public int matchDir;

    GameObject sleeve;
    GameObject matchMesh;
    ParticleSystem flame;
    Transform spawnPoint;
    
    public GameObject thrownMatch;

    Vector3 initialPos;
    public Vector3 initialMatchPos;
    public Vector3 initialMatchRotation;
    public Quaternion targetRotation;
    public Vector3 targetPosition;
    public float xOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sleeve = transform.Find("Sleeve").gameObject;
        initialPos = sleeve.transform.localPosition;

        matchMesh = transform.Find("Match").gameObject;
        flame = matchMesh.transform.Find("Flame").GetComponent<ParticleSystem>();

        spawnPoint = transform.Find("Spawn Point");

        initialMatchPos = matchMesh.transform.localPosition;
        initialMatchRotation = matchMesh.transform.localEulerAngles;
        targetPosition = new Vector3(0.120360129f, -0.00789999962f, 0.14634639f); // End point for matchstick
        targetRotation = Quaternion.Euler(initialMatchRotation);;
    }

    void Update(){
        if (sleeveDir == 1)
            xOffset = Mathf.MoveTowards(xOffset, 0.13f, Time.deltaTime / (2f/speedMult) );
            
        if (sleeveDir == -1)
            xOffset = Mathf.MoveTowards(xOffset, 0f, Time.deltaTime / (2f/speedMult) );

        if (matchDir == 1)
            targetRotation = new Quaternion(-0.858283758f,0.286016285f,-0.425601512f,0.0201726519f);
        
        if (matchDir == 2)
            matchMesh.transform.localPosition = Vector3.MoveTowards(matchMesh.transform.localPosition, targetPosition, Time.deltaTime * 1.5f * speedMult);

        if (matchDir == 3)
            targetRotation = new Quaternion(-0.479035556f, 0.495721042f, -0.577746212f, 0.43702966f);
        
        sleeve.transform.localPosition = new Vector3(xOffset, 0f, 0f);
        matchMesh.transform.localRotation = Quaternion.Slerp(matchMesh.transform.localRotation, targetRotation, Time.deltaTime * 10f * speedMult);
    }

    public void LightMatch(){
        StartCoroutine(PerformLitAnimation());
    }

    IEnumerator PerformLitAnimation()
    {   
        animationPlaying = true;
        sleeveDir = 1;  // Open sleeve
        

        yield return new WaitForSeconds(0.6f/speedMult);
        sleeveDir = 0;
        matchMesh.SetActive(true); // Show match and stop opening sleeve

        yield return new WaitForSeconds(0.35f / speedMult);
        matchDir = 1; // Quickly rotate match to strike paper

        yield return new WaitForSeconds(0.7f / speedMult);
        matchDir = 2; // Strike match along paper

        yield return new WaitForSeconds(0.055f / speedMult);
        flame.Play(); // Start particle system

        yield return new WaitForSeconds(0.055f / speedMult);
        matchDir = 3; // Strike match along paper

        // Wait to throw match
        yield return new WaitForSeconds(0.7f / speedMult);   // Make original match invisible and throw match
        matchMesh.SetActive(false);
        GameObject currThrownMatch = Instantiate(thrownMatch, spawnPoint.position, Quaternion.Euler(spawnPoint.localEulerAngles));
        Rigidbody matchRB = currThrownMatch.GetComponent<Rigidbody>();
        matchRB.AddForce(transform.right * 5f, ForceMode.Impulse);
        matchRB.AddTorque(matchRB.transform.up * 10f, ForceMode.Force);
        Destroy(currThrownMatch, 5f); // Destroy the match after 5 seconds of being thrown
        

        sleeveDir = -1; // Return sleeve to closed
        yield return new WaitForSeconds(0.5f / speedMult);


        // Reset all values
        sleeveDir = 0;
        matchDir = 0;
        animationPlaying = false;        
        targetRotation = Quaternion.Euler(initialMatchRotation);
        matchMesh.transform.localPosition = initialMatchPos;
        matchMesh.transform.localRotation = targetRotation;
    }







}
