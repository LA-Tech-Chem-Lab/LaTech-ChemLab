using System.Collections;
using UnityEngine;

public class doCertainThingWith : MonoBehaviour
{
    pickUpObjectsNETWORKING pos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos = GetComponent<pickUpObjectsNETWORKING>();
    }

    // Update is called once per frame
    void Update()
    {
        checkForInput();
    }

    void checkForInput(){
        if (Input.GetMouseButtonDown(1))
            findObjectAndPerformAction();

        if (Input.GetMouseButton(1))
            findObjectAndPerformHeldAction();
    }

    void findObjectAndPerformAction(){
        if (pos.other != null){
            GameObject obj = pos.other;

            if (obj.name == "Fire extinguisher")
                ShootFoam();

            
        }

    }

    void findObjectAndPerformHeldAction()
    {
        if (pos.other != null)
        {
            GameObject obj = pos.other;

            if (obj.name == "Beaker")
                BringObjectCloser();


        }

    }








    void BringObjectCloser()
    {
        pos.distOffset = -pos.holdingDistance * 0.75f;
    }


    void ShootFoam(){
        if (!pos.other.transform.Find("Foam").GetComponent<ParticleSystem>().isPlaying)
        {
            pos.other.transform.Find("Foam").GetComponent<ParticleSystem>().Play();
            StartCoroutine(OnOrOffForDelay(pos.other.transform.Find("Spraying").gameObject, 5f));
            StartCoroutine(OnOrOffForDelay(pos.other.transform.Find("Not Spraying").gameObject, 5f, false));
        }
    }

    IEnumerator OnOrOffForDelay(GameObject obj, float delayTime, bool boolean = true)
    {
        obj.SetActive(boolean);
        yield return new WaitForSeconds(delayTime);
        obj.SetActive(!boolean);
    }
}
