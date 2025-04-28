using System;
using UnityEngine;

public class matchScript : MonoBehaviour
{   
    const float MATCH_LIGHT_RADUIS = 0.65f;
    public GameObject closestBunsenBurner;
    public Transform tip;
    public ParticleSystem flame;
    public bool lit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tip = transform.Find("Tip");
        flame = transform.Find("Flame").GetComponent<ParticleSystem>();

        //Destroy(gameObject, 8f);
    }

    // Update is called once per frame
    void Update()
    {   
        if (!gameObject) return;
        
        if (flame)
            lit = flame.IsAlive();

        findClosestBunsenBurner();

        
    }

    void LateUpdate()
    {
        if (closestBunsenBurner){
            if (lit){ // Light the bunsen burner
                closestBunsenBurner.GetComponent<bunsenBurnerScript>().isLit = lit;
                if (!closestBunsenBurner.transform.Find("Flame").GetComponent<ParticleSystem>().isPlaying)
                    closestBunsenBurner.transform.Find("Flame").GetComponent<ParticleSystem>().Play();
            }
        }
    }

    void findClosestBunsenBurner(){
        float minDist = Mathf.Infinity;

        foreach (GameObject currentBurner in GameObject.FindGameObjectsWithTag("BunsenBurner")){

            if (!currentBurner) return;

            float dist = Vector3.Distance(tip.position, currentBurner.transform.position);
            
            if (dist < minDist && dist < MATCH_LIGHT_RADUIS){
                minDist = dist;
                closestBunsenBurner = currentBurner;
            }
        }
    }
}
