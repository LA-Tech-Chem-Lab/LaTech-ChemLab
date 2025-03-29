using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
// using Microsoft.Unity.VisualStudio.Editor;
using Obi;
using Tripolygon.UModeler.UI.Input;
using Unity.Mathematics;
using Unity.Multiplayer.Center.Common;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class liquidScript : MonoBehaviour
{
    public float totalVolume_mL;
    public float currentVolume_mL;
    public float scaleDown = 1f;
    public float readHere;
    public float testScale = 1f;
    public Color surfaceColor;
    public Color topColor;
    public float densityOfLiquid = 1f;
    public float percentH2SO4 = 0f;
    public float percentKOH = 0f;
    public float percentH2O = 0f;
    public float percentK2SO4 = 0f;
    public float percentAl = 0f;
    public float percentKAlOH4 = 0f;
    public float percentAl2SO43 = 0f;
    public float percentAlum = 0f;
    public float percentAlOH3 = 0f; 
    public float percentKAlSO42 = 0f; 
    public float percentKAlO2 = 0f; 
    public float limreactnum;
    public List<float> solutionMakeup = new List<float>();
    public List<string> compoundNames = new List<string> {"H2SO4", "KOH", "H2O", "K2SO4", "Al", "KAl(OH)4", "Al2(SO4)3", "Alum", "Al(OH)3", "KAl(SO4)2", "KAlO2"};
    public List<float> densities = new List<float> {1.83f, 2.12f, 1f, 2.66f, 2.7f, 1.5f, 2.672f, 1.76f, 2.42f, 1.75f, 1.57f};
    public List<float> molarMasses = new List<float> {98.079f, 56.1056f, 18.01528f, 174.259f, 26.982f, 134.12f, 342.14f, 474.39f, 78f, 258.42f, 98.075f};
    public List<float> specificHeatCapacities = new List<float> {1380f, 1300f, 4186f, 1060f, 900f, 1500f, 1300f, 1200f, 900f, 1060f, 1250f};
    float[] boilingPoints = new float[] {611f, 1388f, 373.15f, 1685f, 2792f, 1110f, 1073f, 1383f, 773f, 1383f, 1280f};
    public List<float> latentHeat = new List<float> {2257000f, 2000000f, 2257000f, 2500000f, 2920000f, 2200000f, 2500000f, 2400000f, 2300000f, 2300000f, 2400000f};
    List<Color> liquidColors = new List<Color> {Color.red, Color.green, Color.blue, Color.yellow, new Color(0.6f, 0.6f, 0.6f), Color.yellow, Color.red, Color.green, Color.yellow, Color.green, Color.blue};
    public List<char> compoundStates = new List<char> { 'a', 'a', 'l', 'a', 's', 'a', 's', 's', 's', 's', 's' };

    //                                            H2SO4       KOH              H20        K2SO4              Al                  KAl(OH)4
    public bool reactionHappening;

    [Header("Spillage")]
    
    public float dotProduct; 
    public float maxSpillRate;
    Renderer rend;
    Rigidbody objectRigidbody;
    float initialObjectMass;

    public Transform liquidTransform;

    [Header("Liquid Heating")]
    public float maxHeat = 1200f;  // Maximum heat at the center
    public float currentHeat = 1f; // Heat affecting the beaker
    float convectiveHeatTransferCoeff = 100f;
    public float liquidTemperature = 293.15f;
    float specificHeatCapacity = 4184f;
    public float roomTemp = 293.15f;
    public bool isinIceBath;

    [Header("Filtering")]
    public bool isFiltering = false;
    public float liquidPercent;

    public bool isPouring = false;

    private float meltingPointH2SO4 = -10f;
    private float meltingPointKOH = 360f;
    private float meltingPointH2O = 0f;
    private float meltingPointK2SO4 = 1069f;
    private float meltingPointAl = 660f;
    private float meltingPointKAlOH4 = 100f;
    private float meltingPointAl2SO43 = 770f;
    private float meltingPointAlum = 92f;
    private float meltingPointAlOH3 = 300f;
    private float meltingPointKAlSO42 = 1200f;
    private float meltingPointKAlO2 = 1000f;

    [Header("Toxic Gas")]
    public float H2Released = 0f;
    public GameObject boilingEffect;
    public bool isBoiling = false;
    GameObject currentBoilingEffect;
    public GameObject explosion;
    float explosionHeightOffset = 0f;
    public float explosionDuration = 5f;
    public bool exploded = false; 
    public float detectionRadius = 1f;
    public GameObject firePrefab;
    public AudioClip boomSound;
    public bool isViolent = false; 
    //public GameObject player;

    // Use this for initialization
    void Start()
    {
        liquidTransform = transform.Find("Liquid").transform;
        rend = transform.Find("Liquid").GetComponent<Renderer>();
        objectRigidbody = GetComponent<Rigidbody>();
        boilingEffect = Resources.Load<GameObject>("boilingEffect");
        explosion = Resources.Load<GameObject>("Explosion Effect");
        firePrefab = Resources.Load<GameObject>("Flame");
        //doCertainThingWith certainThingWith = player.GetComponent<doCertainThingWith>();
        if (gameObject.name == "Capilary tube")
        {
            initialObjectMass = 1.0f; // Set to a default value
        }
        else
        {
            initialObjectMass = objectRigidbody.mass; // Otherwise, use the Rigidbody's mass
        }
        solutionMakeup.AddRange(new float[] { percentH2SO4, percentKOH , percentH2O, percentK2SO4, percentAl, percentKAlOH4, percentAl2SO43, percentAlum, percentAlOH3, percentKAlSO42, percentKAlO2});
        if (currentVolume_mL > 0){
            calculateDensity();
        }
        maxSpillRate = totalVolume_mL * 0.2f;

        updatePercentages();
    }

    private void Update()
    {
        
        // Calculate tilt using the dot product of the beaker's up direction and world up
        dotProduct = Vector3.Dot(transform.up.normalized, Vector3.up);

        // Spill logic
        if (!isPouring){
            if (dotProduct <= 0.25f)
            {
                float loss = (-0.8f * dotProduct + 0.2f) * maxSpillRate * Time.deltaTime;
                currentVolume_mL -= loss;

                if (gameObject.name.StartsWith("Erlenmeyer") && currentVolume_mL / totalVolume_mL < 0.45f)
                    currentVolume_mL = 0f;
                if (gameObject.name.StartsWith("Beaker") && currentVolume_mL / totalVolume_mL < 0.19f)
                    currentVolume_mL = 0f;
                if (gameObject.name.StartsWith("Paper Cone") && currentVolume_mL / totalVolume_mL < 0.19f)
                    currentVolume_mL = 0f; 
                if (gameObject.name.StartsWith("Graduated") && currentVolume_mL / totalVolume_mL < 0.19f)
                    currentVolume_mL = 0f; 
            }
        }

        // Handle liquid (should only be called once)
        handleLiquid();

        // Handle liquid color (if needed)
        handleLiquidColor();

        // Other functions like heat and reactions
        CalculateHeat();

        handleReactions();

        //Finds the flask
        //is the filter in the funneled flask?
        if (gameObject.name == "Paper Cone" && gameObject.transform.parent?.parent && !isFiltering && currentVolume_mL > 1f){
            Transform Flask = gameObject.transform.parent?.parent;
            StartCoroutine(handleFiltering(Flask));
        }

        if (H2Released > 0.1f && !exploded && IsLitMatchNearby()){
            exploded = true;
            explode(); 
            
            for (int i = 0; i < 5; i++){
                // Generate a random position within the spread radius
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 0.2f;
                randomDirection.y = 0; // Keep the fire on the same horizontal plane
                Vector3 spawnPosition = transform.position + randomDirection;

                // Spawn a new fire prefab at the randomly generated position
                Instantiate(firePrefab, spawnPosition, Quaternion.identity);
            }
        }

        if (H2Released > 0){
            H2Released -= 0.00001f;
            if (ventIsNear()){
                H2Released = H2Released / 2f;
            }
        }

        //if (isViolent && Vector3.Distance(player.transform.position, transform.position) < 2f){
        //    
        //}
    }

    private bool IsLitMatchNearby()
{
    Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

    foreach (Collider collider in colliders)
    {
        if (collider.CompareTag("Match")) // Check if the object is tagged as "Match"
        {
            matchScript matchScript = collider.GetComponent<matchScript>();

            if (matchScript != null && matchScript.lit) // Check if the match is lit
            {
                return true; // A lit match is nearby
            }
        }
    }
    return false; // No lit match found in range
}

    bool ventIsNear()  // Changed the return type to bool since it's returning true or false
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius * 3f);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.name == "Joint 6") // Check if the object's name is "Match"
            {
                Debug.Log(FindGreatGreatGreatGrandparent(collider.gameObject.transform).name);
                bool isOn = FindGreatGreatGreatGrandparent(collider.gameObject.transform).GetComponent<VentController>().vacuumOn;
                if (isOn)
                    return true;
            }
        }
        return false; // No lit match found in range
    }

    Transform FindGreatGreatGreatGrandparent(Transform child)
    {
        Transform current = child;

        for (int i = 0; i < 5; i++)  // 4 steps up the hierarchy
        {
            if (current.parent != null)
            {
                current = current.parent;
            }
            else
            {
                return null;  // No great-great-great grandparent exists (out of hierarchy range)
            }
        }

        return current;  // Return the great-great-great grandparent Transform
    }


    bool inRange(float value, float min, float max)
    {
        return value >= min && value < max;
    }

    float reScale(float p, float lo, float hi){
        return (p - lo) / (hi - lo);
    }

    void handleLiquid(){

        // Liquid Colors
        rend.material.SetColor("_SideColor", surfaceColor);
        rend.material.SetColor("_TopColor", topColor);
        
        // Liquid Volume
        currentVolume_mL = Mathf.Clamp(currentVolume_mL, 0f, totalVolume_mL);

        float percentFull = currentVolume_mL / totalVolume_mL;
        float scaledPercentRender = percentFull / scaleDown;
        
        // Just Beakers Level Oh Boy
        if (transform.name.StartsWith("Beaker")) {

            
            transform.Find("Liquid").GetComponent<MeshRenderer>().enabled = percentFull > 0f;
            rend.material.SetFloat("_FillAmount", percentFull);
            readHere = percentFull;

            if (totalVolume_mL == 800f){ // 800 mL Beaker
                float renderedScale800 = 1f;
                if (inRange(percentFull, 0, 0.00125f)) 
                    renderedScale800 = 26.48f
                    ;
                if (inRange(percentFull, 0.00125f, 0.0025f)) 
                    renderedScale800 = Mathf.Lerp(26.48f, 13.5f, reScale(percentFull, 0.00125f, 0.0025f));
                if (inRange(percentFull, 0.0025f, 0.00625f)) 
                    renderedScale800 = Mathf.Lerp(13.5f, 5.55f, reScale(percentFull, 0.0025f, 0.00625f));
                if (inRange(percentFull, 0.00625f, 0.00875f)) 
                    renderedScale800 = Mathf.Lerp(5.55f, 4.19f, reScale(percentFull, 0.00625f, 0.00875f));
                if (inRange(percentFull, 0.00875f, 0.0125f)) 
                    renderedScale800 = Mathf.Lerp(4.19f, 3.17f, reScale(percentFull, 0.00875f, 0.0125f));
                if (inRange(percentFull, 0.0125f, 0.01875f)) 
                    renderedScale800 = Mathf.Lerp(3.17f, 2.34f, reScale(percentFull, 0.0125f, 0.01875f));
                if (inRange(percentFull, 0.01875f, 0.025f)) 
                    renderedScale800 = Mathf.Lerp(2.34f, 2.16f, reScale(percentFull, 0.01875f, 0.025f));
                if (inRange(percentFull, 0.025f, 0.03125f)) 
                    renderedScale800 = Mathf.Lerp(2.16f, 1.68f, reScale(percentFull, 0.025f, 0.03125f));
                if (inRange(percentFull, 0.03125f, 0.04375f)) 
                    renderedScale800 = Mathf.Lerp(1.68f, 1.4f, reScale(percentFull, 0.03125f, 0.04375f));
                if (inRange(percentFull, 0.04375f, 0.0625f)) 
                    renderedScale800 = Mathf.Lerp(1.4f, 1.16f, reScale(percentFull, 0.04375f, 0.0625f));
                if (inRange(percentFull, 0.0625f, 0.08125f)) 
                    renderedScale800 = Mathf.Lerp(1.16f, 1.04f, reScale(percentFull, 0.0625f, 0.08125f));
                if (inRange(percentFull, 0.08125f, 0.09375f)) 
                    renderedScale800 = Mathf.Lerp(1.04f, 1f, reScale(percentFull, 0.08125f, 0.09375f));
                if (inRange(percentFull, 0.09375f, 0.1125f)) 
                    renderedScale800 = Mathf.Lerp(1f, 0.94f, reScale(percentFull, 0.09375f, 0.1125f));
                if (inRange(percentFull, 0.1125f, 0.125f)) 
                    renderedScale800 = Mathf.Lerp(0.94f, 0.92f, reScale(percentFull, 0.1125f, 0.125f));
                if (inRange(percentFull, 0.125f, 0.1875f)) 
                    renderedScale800 = Mathf.Lerp(0.92f, 0.93f, reScale(percentFull, 0.125f, 0.1875f));
                if (inRange(percentFull, 0.1875f, 0.25f)) 
                    renderedScale800 = Mathf.Lerp(0.93f, 0.94f, reScale(percentFull, 0.1875f, 0.25f));
                if (inRange(percentFull, 0.25f, 0.3125f)) 
                    renderedScale800 = Mathf.Lerp(0.94f, 0.95f, reScale(percentFull, 0.25f, 0.3125f));
                if (inRange(percentFull, 0.3125f, 0.375f)) 
                    renderedScale800 = Mathf.Lerp(0.95f, 0.96f, reScale(percentFull, 0.3125f, 0.375f));
                if (inRange(percentFull, 0.375f, 0.4375f)) 
                    renderedScale800 = Mathf.Lerp(0.96f, 0.96f, reScale(percentFull, 0.375f, 0.4375f));
                if (inRange(percentFull, 0.4375f, 0.5f)) 
                    renderedScale800 = Mathf.Lerp(0.96f, 0.965f, reScale(percentFull, 0.4375f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.5625f)) 
                    renderedScale800 = Mathf.Lerp(0.965f, 0.96f, reScale(percentFull, 0.5f, 0.5625f));
                if (inRange(percentFull, 0.5625f, 0.625f)) 
                    renderedScale800 = Mathf.Lerp(0.96f, 0.97f, reScale(percentFull, 0.5625f, 0.625f));
                if (inRange(percentFull, 0.625f, 0.6875f)) 
                    renderedScale800 = Mathf.Lerp(0.97f, 0.97f, reScale(percentFull, 0.625f, 0.6875f));
                if (inRange(percentFull, 0.6875f, 0.75f)) 
                    renderedScale800 = Mathf.Lerp(0.97f, 0.97f, reScale(percentFull, 0.6875f, 0.75f));
                if (inRange(percentFull, 0.75f, 0.8125f)) 
                    renderedScale800 = Mathf.Lerp(0.97f, 0.97f, reScale(percentFull, 0.75f, 0.8125f));
                if (inRange(percentFull, 0.8125f, 0.875f)) 
                    renderedScale800 = Mathf.Lerp(0.97f, 0.97f, reScale(percentFull, 0.8125f, 0.875f));
                if (inRange(percentFull, 0.875f, 0.9375f)) 
                    renderedScale800 = Mathf.Lerp(0.97f, 0.975f, reScale(percentFull, 0.875f, 0.9375f));
                if (inRange(percentFull, 0.9375f, 1f)) 
                    renderedScale800 = Mathf.Lerp(0.975f, 0.974f, reScale(percentFull, 0.9375f, 1f));

                if (percentFull == 1)
                    renderedScale800 = 0.974f;

                readHere = percentFull * renderedScale800;
                rend.material.SetFloat("_FillAmount", percentFull * renderedScale800);

            }

            if (totalVolume_mL == 400f){ // 400 mL Beaker
                float renderedScale400 = 1f;
                if (inRange(percentFull, 0, 0.00125f)) 
                    renderedScale400 = 26.2f;

                if (inRange(percentFull, 0.00125f, 0.001875f)) 
                    renderedScale400 = Mathf.Lerp(26.2f, 17.7f, reScale(percentFull, 0.00125f, 0.001875f));
                if (inRange(percentFull, 0.001875f, 0.0025f)) 
                    renderedScale400 = Mathf.Lerp(17.7f, 13.4f, reScale(percentFull, 0.001875f, 0.0025f));
                if (inRange(percentFull, 0.0025f, 0.003125f)) 
                    renderedScale400 = Mathf.Lerp(13.4f, 10.8f, reScale(percentFull, 0.0025f, 0.003125f));
                if (inRange(percentFull, 0.003125f, 0.00375f)) 
                    renderedScale400 = Mathf.Lerp(10.8f, 9f, reScale(percentFull, 0.003125f, 0.00375f));
                if (inRange(percentFull, 0.00375f, 0.005f)) 
                    renderedScale400 = Mathf.Lerp(9f, 6.86f, reScale(percentFull, 0.00375f, 0.005f));
                if (inRange(percentFull, 0.005f, 0.00875f)) 
                    renderedScale400 = Mathf.Lerp(6.86f, 4.15f, reScale(percentFull, 0.005f, 0.00875f));
                if (inRange(percentFull, 0.00875f, 0.0125f)) 
                    renderedScale400 = Mathf.Lerp(4.15f, 3.08f, reScale(percentFull, 0.00875f, 0.0125f));
                if (inRange(percentFull, 0.0125f, 0.01875f)) 
                    renderedScale400 = Mathf.Lerp(3.08f, 2.22f, reScale(percentFull, 0.0125f, 0.01875f));
                if (inRange(percentFull, 0.01875f, 0.025f)) 
                    renderedScale400 = Mathf.Lerp(2.22f, 1.8f, reScale(percentFull, 0.01875f, 0.025f));
                if (inRange(percentFull, 0.025f, 0.0375f)) 
                    renderedScale400 = Mathf.Lerp(1.8f, 1.375f, reScale(percentFull, 0.025f, 0.0375f));
                if (inRange(percentFull, 0.0375f, 0.05f)) 
                    renderedScale400 = Mathf.Lerp(1.375f, 1.165f, reScale(percentFull, 0.0375f, 0.05f));
                if (inRange(percentFull, 0.05f, 0.0625f)) 
                    renderedScale400 = Mathf.Lerp(1.165f, 1.03f, reScale(percentFull, 0.05f, 0.0625f));
                if (inRange(percentFull, 0.0625f, 0.075f)) 
                    renderedScale400 = Mathf.Lerp(1.03f, 0.955f, reScale(percentFull, 0.0625f, 0.075f));
                if (inRange(percentFull, 0.075f, 0.1f)) 
                    renderedScale400 = Mathf.Lerp(0.955f, 0.847f, reScale(percentFull, 0.075f, 0.1f));
                if (inRange(percentFull, 0.1f, 0.125f)) 
                    renderedScale400 = Mathf.Lerp(0.847f, 0.77f, reScale(percentFull, 0.1f, 0.125f));
                if (inRange(percentFull, 0.125f, 0.1875f)) 
                    renderedScale400 = Mathf.Lerp(0.77f, 0.77f, reScale(percentFull, 0.125f, 0.1875f));
                if (inRange(percentFull, 0.1875f, 0.25f)) 
                    renderedScale400 = Mathf.Lerp(0.77f, 0.77f, reScale(percentFull, 0.1875f, 0.25f));
                if (inRange(percentFull, 0.25f, 0.3125f)) 
                    renderedScale400 = Mathf.Lerp(0.77f, 0.765f, reScale(percentFull, 0.25f, 0.3125f));
                if (inRange(percentFull, 0.3125f, 0.375f)) 
                    renderedScale400 = Mathf.Lerp(0.765f, 0.767f, reScale(percentFull, 0.3125f, 0.375f));
                if (inRange(percentFull, 0.375f, 0.4375f)) 
                    renderedScale400 = Mathf.Lerp(0.767f, 0.765f, reScale(percentFull, 0.375f, 0.4375f));
                if (inRange(percentFull, 0.4375f, 0.5f)) 
                    renderedScale400 = Mathf.Lerp(0.765f, 0.767f, reScale(percentFull, 0.4375f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.5625f)) 
                    renderedScale400 = Mathf.Lerp(0.767f, 0.761f, reScale(percentFull, 0.5f, 0.5625f));
                if (inRange(percentFull, 0.5625f, 0.625f)) 
                    renderedScale400 = Mathf.Lerp(0.761f, 0.767f, reScale(percentFull, 0.5625f, 0.625f));
                if (inRange(percentFull, 0.625f, 0.6875f)) 
                    renderedScale400 = Mathf.Lerp(0.767f, 0.766f, reScale(percentFull, 0.625f, 0.6875f));
                if (inRange(percentFull, 0.6875f, 0.75f)) 
                    renderedScale400 = Mathf.Lerp(0.766f, 0.766f, reScale(percentFull, 0.6875f, 0.75f));
                if (inRange(percentFull, 0.75f, 0.8125f)) 
                    renderedScale400 = Mathf.Lerp(0.766f, 0.766f, reScale(percentFull, 0.75f, 0.8125f));
                if (inRange(percentFull, 0.8125f, 0.875f)) 
                    renderedScale400 = Mathf.Lerp(0.766f, 0.766f, reScale(percentFull, 0.8125f, 0.875f));
                if (inRange(percentFull, 0.875f, 0.9375f)) 
                    renderedScale400 = Mathf.Lerp(0.766f, 0.767f, reScale(percentFull, 0.875f, 0.9375f));
                if (inRange(percentFull, 0.9375f, 1.0f)) 
                    renderedScale400 = Mathf.Lerp(0.767f, 0.7658f, reScale(percentFull, 0.9375f, 1.0f));
                    
                if (percentFull == 1)
                    renderedScale400 = 0.7658f;

                readHere =  percentFull * renderedScale400;
                rend.material.SetFloat("_FillAmount", percentFull * renderedScale400);

            }

            if (totalVolume_mL == 250f){ // 250 mL Beaker
                float renderedScale250 = 1f;
                if (inRange(percentFull, 0, 0.004f)) 
                    renderedScale250 = 8.23f;

                if (inRange(percentFull, 0.004f, 0.008f)) 
                    renderedScale250 = Mathf.Lerp(8.23f, 4.22f, reScale(percentFull, 0.004f, 0.008f));
                if (inRange(percentFull, 0.008f, 0.014f)) 
                    renderedScale250 = Mathf.Lerp(4.22f, 2.52f, reScale(percentFull, 0.008f, 0.014f));
                if (inRange(percentFull, 0.014f, 0.02f)) 
                    renderedScale250 = Mathf.Lerp(2.52f, 1.93f, reScale(percentFull, 0.014f, 0.02f));
                if (inRange(percentFull, 0.02f, 0.03f)) 
                    renderedScale250 = Mathf.Lerp(1.93f, 1.48f, reScale(percentFull, 0.02f, 0.03f));
                if (inRange(percentFull, 0.03f, 0.04f)) 
                    renderedScale250 = Mathf.Lerp(1.48f, 1.24f, reScale(percentFull, 0.03f, 0.04f));
                if (inRange(percentFull, 0.04f, 0.06f)) 
                    renderedScale250 = Mathf.Lerp(1.24f, 1.01f, reScale(percentFull, 0.04f, 0.06f));
                if (inRange(percentFull, 0.06f, 0.1f)) 
                    renderedScale250 = Mathf.Lerp(1.01f, 0.82f, reScale(percentFull, 0.06f, 0.1f));
                if (inRange(percentFull, 0.1f, 0.14f)) 
                    renderedScale250 = Mathf.Lerp(0.82f, 0.74f, reScale(percentFull, 0.1f, 0.14f));
                if (inRange(percentFull, 0.14f, 0.2f)) 
                    renderedScale250 = Mathf.Lerp(0.74f, 0.69f, reScale(percentFull, 0.14f, 0.2f));
                if (inRange(percentFull, 0.2f, 0.3f)) 
                    renderedScale250 = Mathf.Lerp(0.69f, 0.666f, reScale(percentFull, 0.2f, 0.3f));
                if (inRange(percentFull, 0.3f, 0.4f)) 
                    renderedScale250 = Mathf.Lerp(0.666f, 0.655f, reScale(percentFull, 0.3f, 0.4f));
                if (inRange(percentFull, 0.4f, 0.5f)) 
                    renderedScale250 = Mathf.Lerp(0.655f, 0.647f, reScale(percentFull, 0.4f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.6f)) 
                    renderedScale250 = Mathf.Lerp(0.647f, 0.642f, reScale(percentFull, 0.5f, 0.6f));
                if (inRange(percentFull, 0.6f, 0.7f)) 
                    renderedScale250 = Mathf.Lerp(0.642f, 0.638f, reScale(percentFull, 0.6f, 0.7f));
                if (inRange(percentFull, 0.7f, 0.8f)) 
                    renderedScale250 = Mathf.Lerp(0.638f, 0.638f, reScale(percentFull, 0.7f, 0.8f));
                if (inRange(percentFull, 0.8f, 0.9f)) 
                    renderedScale250 = Mathf.Lerp(0.638f, 0.634f, reScale(percentFull, 0.8f, 0.9f));
                if (inRange(percentFull, 0.9f, 1.0f)) 
                    renderedScale250 = Mathf.Lerp(0.634f, 0.632f, reScale(percentFull, 0.9f, 1.0f));

                if (percentFull == 1)
                    renderedScale250 = 0.632f;

                readHere =  percentFull * renderedScale250;
                rend.material.SetFloat("_FillAmount", percentFull * renderedScale250);
            }

            if (totalVolume_mL == 150f){ // 150 mL Beaker
                float renderedScale150 = 1f;
                if (inRange(percentFull, 0, 0.003333333f)) 
                    renderedScale150 = 10.2f;
                
                if (inRange(percentFull, 0.003333333f, 0.006666667f))
                    renderedScale150 = Mathf.Lerp(10.2f, 5.25f, reScale(percentFull, 0.003333333f, 0.006666667f));
                if (inRange(percentFull, 0.006666667f, 0.01f))
                    renderedScale150 = Mathf.Lerp(5.25f, 3.67f, reScale(percentFull, 0.006666667f, 0.01f));
                if (inRange(percentFull, 0.01f, 0.01666667f))
                    renderedScale150 = Mathf.Lerp(3.67f, 2.38f, reScale(percentFull, 0.01f, 0.01666667f));
                if (inRange(percentFull, 0.01666667f, 0.03333334f))
                    renderedScale150 = Mathf.Lerp(2.38f, 1.425f, reScale(percentFull, 0.01666667f, 0.03333334f));
                if (inRange(percentFull, 0.03333334f, 0.05f))
                    renderedScale150 = Mathf.Lerp(1.425f, 1.1f, reScale(percentFull, 0.03333334f, 0.05f));
                if (inRange(percentFull, 0.05f, 0.08333334f))
                    renderedScale150 = Mathf.Lerp(1.1f, 0.85f, reScale(percentFull, 0.05f, 0.08333334f));
                if (inRange(percentFull, 0.08333334f, 0.1333333f))
                    renderedScale150 = Mathf.Lerp(0.85f, 0.71f, reScale(percentFull, 0.08333334f, 0.1333333f));
                if (inRange(percentFull, 0.1333333f, 0.1666667f))
                    renderedScale150 = Mathf.Lerp(0.71f, 0.661f, reScale(percentFull, 0.1333333f, 0.1666667f));
                if (inRange(percentFull, 0.1666667f, 0.25f))
                    renderedScale150 = Mathf.Lerp(0.661f, 0.626f, reScale(percentFull, 0.1666667f, 0.25f));
                if (inRange(percentFull, 0.25f, 0.3333333f))
                    renderedScale150 = Mathf.Lerp(0.626f, 0.608f, reScale(percentFull, 0.25f, 0.3333333f));
                if (inRange(percentFull, 0.3333333f, 0.4166667f))
                    renderedScale150 = Mathf.Lerp(0.608f, 0.597f, reScale(percentFull, 0.3333333f, 0.4166667f));
                if (inRange(percentFull, 0.4166667f, 0.5f))
                    renderedScale150 = Mathf.Lerp(0.597f, 0.5895f, reScale(percentFull, 0.4166667f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.5833333f))
                    renderedScale150 = Mathf.Lerp(0.5895f, 0.583f, reScale(percentFull, 0.5f, 0.5833333f));
                if (inRange(percentFull, 0.5833333f, 0.6666667f))
                    renderedScale150 = Mathf.Lerp(0.583f, 0.58f, reScale(percentFull, 0.5833333f, 0.6666667f));
                if (inRange(percentFull, 0.6666667f, 0.75f))
                    renderedScale150 = Mathf.Lerp(0.58f, 0.576f, reScale(percentFull, 0.6666667f, 0.75f));
                if (inRange(percentFull, 0.75f, 0.8333333f))
                    renderedScale150 = Mathf.Lerp(0.576f, 0.574f, reScale(percentFull, 0.75f, 0.8333333f));
                if (inRange(percentFull, 0.8333333f, 0.9166667f))
                    renderedScale150 = Mathf.Lerp(0.574f, 0.572f, reScale(percentFull, 0.8333333f, 0.9166667f));
                if (inRange(percentFull, 0.9166667f, 1.0f))
                    renderedScale150 = Mathf.Lerp(0.572f, 0.5705f, reScale(percentFull, 0.9166667f, 1.0f));

                if (percentFull == 1)
                    renderedScale150 = 0.5705f;

                readHere =  percentFull * renderedScale150;
                rend.material.SetFloat("_FillAmount", percentFull * renderedScale150);
            }

            if (totalVolume_mL == 100f){ // 100 mL Beaker
                float renderedScale100 = 1f;
                if (inRange(percentFull, 0, 0.0025f)) 
                    renderedScale100 = 13.19f;
                
                if (inRange(percentFull, 0.0025f, 0.005f))
                    renderedScale100 = Mathf.Lerp(13.19f, 6.85f, reScale(percentFull, 0.0025f, 0.005f));
                if (inRange(percentFull, 0.005f, 0.01f))
                    renderedScale100 = Mathf.Lerp(6.85f, 3.64f, reScale(percentFull, 0.005f, 0.01f));
                if (inRange(percentFull, 0.01f, 0.02f))
                    renderedScale100 = Mathf.Lerp(3.64f, 2.02f, reScale(percentFull, 0.01f, 0.02f));
                if (inRange(percentFull, 0.02f, 0.04f))
                    renderedScale100 = Mathf.Lerp(2.02f, 1.23f, reScale(percentFull, 0.02f, 0.04f));
                if (inRange(percentFull, 0.04f, 0.06f))
                    renderedScale100 = Mathf.Lerp(1.23f, 0.961f, reScale(percentFull, 0.04f, 0.06f));
                if (inRange(percentFull, 0.06f, 0.08f))
                    renderedScale100 = Mathf.Lerp(0.961f, 0.831f, reScale(percentFull, 0.06f, 0.08f));
                if (inRange(percentFull, 0.08f, 0.1f))
                    renderedScale100 = Mathf.Lerp(0.831f, 0.752f, reScale(percentFull, 0.08f, 0.1f));
                if (inRange(percentFull, 0.1f, 0.13f))
                    renderedScale100 = Mathf.Lerp(0.752f, 0.68f, reScale(percentFull, 0.1f, 0.13f));
                if (inRange(percentFull, 0.13f, 0.15f))
                    renderedScale100 = Mathf.Lerp(0.68f, 0.647f, reScale(percentFull, 0.13f, 0.15f));
                if (inRange(percentFull, 0.15f, 0.2f))
                    renderedScale100 = Mathf.Lerp(0.647f, 0.592f, reScale(percentFull, 0.15f, 0.2f));
                if (inRange(percentFull, 0.2f, 0.3f))
                    renderedScale100 = Mathf.Lerp(0.592f, 0.558f, reScale(percentFull, 0.2f, 0.3f));
                if (inRange(percentFull, 0.3f, 0.4f))
                    renderedScale100 = Mathf.Lerp(0.558f, 0.541f, reScale(percentFull, 0.3f, 0.4f));
                if (inRange(percentFull, 0.4f, 0.5f))
                    renderedScale100 = Mathf.Lerp(0.541f, 0.5315f, reScale(percentFull, 0.4f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.6f))
                    renderedScale100 = Mathf.Lerp(0.5315f, 0.5253f, reScale(percentFull, 0.5f, 0.6f));
                if (inRange(percentFull, 0.6f, 0.7f))
                    renderedScale100 = Mathf.Lerp(0.5253f, 0.52f, reScale(percentFull, 0.6f, 0.7f));
                if (inRange(percentFull, 0.7f, 0.8f))
                    renderedScale100 = Mathf.Lerp(0.52f, 0.5165f, reScale(percentFull, 0.7f, 0.8f));
                if (inRange(percentFull, 0.8f, 0.9f))
                    renderedScale100 = Mathf.Lerp(0.5165f, 0.514f, reScale(percentFull, 0.8f, 0.9f));
                if (inRange(percentFull, 0.9f, 1.0f))
                    renderedScale100 = Mathf.Lerp(0.514f, 0.512f, reScale(percentFull, 0.9f, 1.0f));

                if (percentFull == 1)
                    renderedScale100 = 0.512f;

                readHere =  percentFull * renderedScale100;
                rend.material.SetFloat("_FillAmount", percentFull * renderedScale100);
            }
            
            if (totalVolume_mL ==  50f){ // 50 mL Beaker
                float renderedScale50 = 1f;
                if (inRange(percentFull, 0, 0.005f)) 
                    renderedScale50 = 6.78f;
                
                if (inRange(percentFull, 0.005f, 0.01f))
                    renderedScale50 = Mathf.Lerp(6.78f, 3.53f, reScale(percentFull, 0.005f, 0.01f));
                if (inRange(percentFull, 0.01f, 0.015f))
                    renderedScale50 = Mathf.Lerp(3.53f, 2.44f, reScale(percentFull, 0.01f, 0.015f));
                if (inRange(percentFull, 0.015f, 0.02f))
                    renderedScale50 = Mathf.Lerp(2.44f, 1.92f, reScale(percentFull, 0.015f, 0.02f));
                if (inRange(percentFull, 0.02f, 0.025f))
                    renderedScale50 = Mathf.Lerp(1.92f, 1.61f, reScale(percentFull, 0.02f, 0.025f));
                if (inRange(percentFull, 0.025f, 0.03f))
                    renderedScale50 = Mathf.Lerp(1.61f, 1.39f, reScale(percentFull, 0.025f, 0.03f));
                if (inRange(percentFull, 0.03f, 0.035f))
                    renderedScale50 = Mathf.Lerp(1.39f, 1.23f, reScale(percentFull, 0.03f, 0.035f));
                if (inRange(percentFull, 0.035f, 0.04f))
                    renderedScale50 = Mathf.Lerp(1.23f, 1.125f, reScale(percentFull, 0.035f, 0.04f));
                if (inRange(percentFull, 0.04f, 0.045f))
                    renderedScale50 = Mathf.Lerp(1.125f, 1.045f, reScale(percentFull, 0.04f, 0.045f));
                if (inRange(percentFull, 0.045f, 0.05f))
                    renderedScale50 = Mathf.Lerp(1.045f, 0.97f, reScale(percentFull, 0.045f, 0.05f));
                if (inRange(percentFull, 0.05f, 0.06f))
                    renderedScale50 = Mathf.Lerp(0.97f, 0.863f, reScale(percentFull, 0.05f, 0.06f));
                if (inRange(percentFull, 0.06f, 0.08f))
                    renderedScale50 = Mathf.Lerp(0.863f, 0.73f, reScale(percentFull, 0.06f, 0.08f));
                if (inRange(percentFull, 0.08f, 0.1f))
                    renderedScale50 = Mathf.Lerp(0.73f, 0.65f, reScale(percentFull, 0.08f, 0.1f));
                if (inRange(percentFull, 0.1f, 0.12f))
                    renderedScale50 = Mathf.Lerp(0.65f, 0.597f, reScale(percentFull, 0.1f, 0.12f));
                if (inRange(percentFull, 0.12f, 0.14f))
                    renderedScale50 = Mathf.Lerp(0.597f, 0.56f, reScale(percentFull, 0.12f, 0.14f));
                if (inRange(percentFull, 0.14f, 0.16f))
                    renderedScale50 = Mathf.Lerp(0.56f, 0.531f, reScale(percentFull, 0.14f, 0.16f));
                if (inRange(percentFull, 0.16f, 0.18f))
                    renderedScale50 = Mathf.Lerp(0.531f, 0.51f, reScale(percentFull, 0.16f, 0.18f));
                if (inRange(percentFull, 0.18f, 0.2f))
                    renderedScale50 = Mathf.Lerp(0.51f, 0.49f, reScale(percentFull, 0.18f, 0.2f));
                if (inRange(percentFull, 0.2f, 0.3f))
                    renderedScale50 = Mathf.Lerp(0.49f, 0.453f, reScale(percentFull, 0.2f, 0.3f));
                if (inRange(percentFull, 0.3f, 0.4f))
                    renderedScale50 = Mathf.Lerp(0.453f, 0.433f, reScale(percentFull, 0.3f, 0.4f));
                if (inRange(percentFull, 0.4f, 0.5f))
                    renderedScale50 = Mathf.Lerp(0.433f, 0.422f, reScale(percentFull, 0.4f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.6f))
                    renderedScale50 = Mathf.Lerp(0.422f, 0.4144f, reScale(percentFull, 0.5f, 0.6f));
                if (inRange(percentFull, 0.6f, 0.7f))
                    renderedScale50 = Mathf.Lerp(0.4144f, 0.408f, reScale(percentFull, 0.6f, 0.7f));
                if (inRange(percentFull, 0.7f, 0.8f))
                    renderedScale50 = Mathf.Lerp(0.408f, 0.4045f, reScale(percentFull, 0.7f, 0.8f));
                if (inRange(percentFull, 0.8f, 0.9f))
                    renderedScale50 = Mathf.Lerp(0.4045f, 0.4015f, reScale(percentFull, 0.8f, 0.9f));
                if (inRange(percentFull, 0.9f, 1.0f))
                    renderedScale50 = Mathf.Lerp(0.4015f, 0.399f, reScale(percentFull, 0.9f, 1.0f));

                if (percentFull == 1)
                    renderedScale50 = 0.399f;

                // renderedScale50 = testScale;
                // readHere = percentFull;

                readHere =  percentFull * renderedScale50;
                rend.material.SetFloat("_FillAmount", percentFull * renderedScale50);
            }
        }


        if (transform.name == "Paper Cone" || transform.name == "graduated Cylinder" || transform.name == "Ice Bucket"){  // 1 to 1 in this case
            
            if (rend.material != null)
            {   
                // Make sure to use the instance material, not the shared one
                if (scaledPercentRender > 0f)
                {
                    transform.Find("Liquid").GetComponent<MeshRenderer>().enabled = true;
                    rend.material.SetFloat("_FillAmount", scaledPercentRender);
                }
                if (scaledPercentRender <= 0f)
                {
                    transform.Find("Liquid").GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        if (transform.name == "Erlenmeyer Flask" || transform.name == "Erlenmeyer Flask L"){
            
            float x = percentFull;
            float heightFromVolume = Mathf.Pow(x, 1.5f);
            float scaledHeight = heightFromVolume/scaleDown;

            rend.material.SetFloat("_FillAmount", scaledHeight);
        }

        if (transform.name == "Pipette"){  // 1 to 1 in this case
            
            float x = percentFull;
            float heightFromVolume = Mathf.Pow(x, 0.8f);
            float scaledHeight = heightFromVolume/scaleDown;

            rend.material.SetFloat("_FillAmount", scaledHeight);
        }
        // Simulate new object mass now containing liquid
        if (gameObject.name == "Capilary tube")
        {
            return;
        }
        else
        {
            objectRigidbody.mass = initialObjectMass + currentVolume_mL * densityOfLiquid / 1000f;
        }
        

    }

    void handleLiquidColor(){
        // Set surface color and top color based on percentages

        Color newSurfaceColor = Color.black;
        float totalSolution = 0f;

        // Calculate the total amount of solution
        foreach (float amount in solutionMakeup)
        {
            totalSolution += amount;
        }

        // Prevent division by zero
        if (totalSolution > 0)
        {
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                newSurfaceColor += (solutionMakeup[i] / totalSolution) * liquidColors[i]; // Normalize contribution
            }
        }

        surfaceColor = newSurfaceColor;

    }

void CalculateHeat()
{
    GameObject burner = findClosestBunsenBurner();
    float radius = transform.localScale.x / 2; // Assuming uniform scaling for X and Z
    float beakerSurfaceArea = Mathf.PI * Mathf.Pow(radius, 2);
            if (burner != null)
            {
                Vector3 burnerPos = burner.transform.position;
                Vector3 beakerPos = transform.position;
                float heatRadius = 0.2f;

                float horizontalDistance = Vector2.Distance(new Vector2(beakerPos.x, beakerPos.z), new Vector2(burnerPos.x, burnerPos.z));
                float heightDifference = beakerPos.y - burnerPos.y;

                bunsenBurnerScript burnerScript = burner.GetComponent<bunsenBurnerScript>();

                if (horizontalDistance <= heatRadius && heightDifference > 0 && burnerScript.isLit)
                {
                    if (H2Released > 0.1f && !exploded)
                    {
                        exploded = true;
                        explode();
                        Debug.Log("Boom");
                        for (int i = 0; i < 5; i++)
                        {
                            // Generate a random position within the spread radius
                            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 0.2f;
                            randomDirection.y = 0; // Keep the fire on the same horizontal plane
                            Vector3 spawnPosition = transform.position + randomDirection;

                            // Spawn a new fire prefab at the randomly generated position
                            Instantiate(firePrefab, spawnPosition, Quaternion.identity);
                        }
                    }
                    float heatFactor = 1 - (horizontalDistance / heatRadius);
                    float burnerIntensity = burnerScript.airflow;
                    currentHeat = (maxHeat * heatFactor * burnerIntensity) + roomTemp;
                }
                else
                {
                    currentHeat = roomTemp;
                }
            }
        else if (isinIceBath == true)
        {
            if (currentHeat != 0f)
            {
                var coolingRate = 5f;
                currentHeat -= coolingRate * Time.deltaTime;
            }
        }
        else
        {
            currentHeat = roomTemp;
        }

        // Apply heat transfer equation
        specificHeatCapacity = 0f;
    for (int i = 0; i < solutionMakeup.Count; i++){
        specificHeatCapacity += solutionMakeup[i] * specificHeatCapacities[i];
    }

    if (currentHeat < liquidTemperature)
    {
        float ambientCoolingRate = 0.05f; // Adjust for faster cooling
        float coolingLoss = ambientCoolingRate * beakerSurfaceArea * (liquidTemperature - currentHeat) * 1000;
        liquidTemperature -= coolingLoss / (GetComponent<Rigidbody>().mass * specificHeatCapacity);
       
    }

    float heatTransferRate = convectiveHeatTransferCoeff * beakerSurfaceArea * (currentHeat - liquidTemperature);
    if (gameObject.name == "Capilary tube")
        {
            float temperatureChange = (heatTransferRate / (1 * specificHeatCapacity)) * Time.deltaTime;
        }
    else
        {
            float temperatureChange = (heatTransferRate / (GetComponent<Rigidbody>().mass * specificHeatCapacity)) * Time.deltaTime;
        }
    liquidTemperature = Mathf.Lerp(liquidTemperature, currentHeat, Time.deltaTime / 15f);

        // Calculate total mass of the solution (assume mass of liquid is given or available)
        float totalSolutionMass = densityOfLiquid * currentVolume_mL;
    // Check for evaporation
    for (int i = 0; i < compoundNames.Count; i++)
    {
        float boilingPoint = boilingPoints[i]; // Get the boiling point for the compound
        float latentHeatOfVaporization = latentHeat[i]; // Latent heat of vaporization for each compound

        // If the temperature exceeds the boiling point, calculate evaporation rate
        if (liquidTemperature >= boilingPoint && solutionMakeup[i] > 0.1f)
        {
            if (!isBoiling){
                currentBoilingEffect = Instantiate(boilingEffect, transform);
                currentBoilingEffect.transform.localPosition = new Vector3(0f, -0.7f, 0f);
                currentBoilingEffect.GetComponent<Renderer>().material.color = surfaceColor;
            }
            isBoiling = true;

            float temperatureDifference = liquidTemperature - boilingPoint;
            
            // Evaporation rate calculation (rate at which mass evaporates per second)
            float evaporationRate = (convectiveHeatTransferCoeff * beakerSurfaceArea * temperatureDifference) / latentHeatOfVaporization;

            // Calculate the mass of the compound based on its percent makeup in the solution
            float compoundMass = solutionMakeup[i] * totalSolutionMass;

            // Calculate the amount of mass to evaporate
            float massToEvaporate = evaporationRate * Time.deltaTime;

            // Reduce the compound's mass in the solution, based on the evaporation rate
            compoundMass -= massToEvaporate;

            // If the compound mass is reduced to 0, set it to 0 (avoid negative mass)
            if (compoundMass < 0)
                compoundMass = 0;

            // Update the solution makeup percentage based on the new mass of the compound
            solutionMakeup[i] = compoundMass / totalSolutionMass;
        }
        else if (!isBoiling){
            Destroy(currentBoilingEffect);
        }
    }
        if (liquidTemperature < 273.15f)
        {
            Transform crystallizationTransform = transform.Find("Crystalization");
            if (crystallizationTransform != null)
            {
                // Activate the Crystalization object when the liquid is below freezing
                if (!crystallizationTransform.gameObject.activeSelf)
                {
                    crystallizationTransform.gameObject.SetActive(true);
                }
                Renderer crystallizationRenderer = crystallizationTransform.GetComponent<Renderer>();
                if (crystallizationRenderer != null)
                {
                    Material crystallizationMaterial = crystallizationRenderer.material;
                    float freezingPoint = 273f; 
                    float freezeSpeed = Mathf.Max(0.1f, Mathf.Clamp01((freezingPoint - liquidTemperature) / 100f));
                    float newGrowth = Mathf.Clamp01(1 - freezeSpeed * (freezingPoint - liquidTemperature) / freezingPoint);
                    crystallizationMaterial.SetFloat("_Growth", newGrowth);
                }
            }
        }
    }

    IEnumerator handleFiltering(Transform Flask)
    {
        isFiltering = true;
        float liquidVolume = 10f;
        List<float> liquidSolution = Enumerable.Repeat(0f, 11).ToList();

        float volumeLeft = 0f;
        if (Flask.name.StartsWith("Erlenmeyer")){
            volumeLeft = currentVolume_mL * 0.05f;
        }
        if (Flask.name.StartsWith("Buchner")){
            volumeLeft = currentVolume_mL * 0.01f;
        }
        // Filtering continues while there is enough solution
        while (liquidVolume > 0.1f && currentVolume_mL > volumeLeft)
        {
            float liquidPercent = 0f; // Reset inside loop to prevent accumulation

            // Identify which compounds are being filtered
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                if (compoundStates[i] == 'l' || compoundStates[i] == 'a')
                {
                    liquidSolution[i] = solutionMakeup[i];
                    liquidPercent += solutionMakeup[i];
                }
                else
                {
                    liquidSolution[i] = 0f;  // Ensure only liquids are transferred
                }
            }

            // Avoid division by zero when calculating liquidVolume
            if (liquidPercent == 0f)
            {
                isFiltering = false;
                yield break;
            }

            liquidVolume = liquidPercent * currentVolume_mL;  // Calculate volume of liquid part

            // Normalize `liquidSolution` to sum to 100%
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                liquidSolution[i] = (liquidSolution[i] * currentVolume_mL) / liquidVolume;
            }

            // Ensure filtering is connected properly
            if (gameObject.transform.parent?.parent)
            {
                if (gameObject.transform.parent.parent.name.StartsWith("Erlenmeyer Flask"))
                {
                    // Perform the filtering step
                    //float filteredLiquid = Mathf.Min(liquidVolume * Time.deltaTime, currentVolume_mL); // Prevent over-filtering
                    float filteredLiquid = liquidVolume * Time.deltaTime;
                    filterSolution(liquidSolution, filteredLiquid, Flask);
                }
                else
                {
                    isFiltering = false;
                    yield break;
                }
            }
            else
            {
                isFiltering = false;
                yield break;
            }

            yield return new WaitForSeconds(0.1f);  // Allow other processes to run
        }
        isFiltering = false;
    }

    GameObject findClosestBunsenBurner()
    {
        float heatRadius = 2f;
        float minDist = Mathf.Infinity;
        GameObject closestBurner = null; // Store the closest burner

        foreach (GameObject currentBurner in GameObject.FindGameObjectsWithTag("BunsenBurner"))
        {
            if (!currentBurner) continue; // Skip null objects

            float dist = Vector3.Distance(transform.position, currentBurner.transform.position);

            if (dist < minDist && dist <= heatRadius)
            {
                minDist = dist;
                closestBurner = currentBurner;
            }
        }
        return closestBurner; // Return the closest one found (or null if none are within range)
    }

    public void updateLiquidPercent(){
        //calculate the percent liquid in the solution
        liquidPercent = 0f;
        for (int i = 0; i < solutionMakeup.Count; i++)
        {
            if (compoundStates[i] == 'l' || compoundStates[i] == 'a')
            {
                liquidPercent += solutionMakeup[i];
            }
        }
    }

    //adds a vollume of a given solution to the current solution
    public void addSolution(List<float> solutionToAdd, float volume)
    {
        float newVolume = currentVolume_mL + volume;
        float sum = 0f;
        for (int i = 0; i < solutionMakeup.Count; i++)
        {
            solutionMakeup[i] = ((solutionMakeup[i] * currentVolume_mL) + (solutionToAdd[i] * volume)) / newVolume;
            sum += solutionMakeup[i]; // Track total sum
        }
        // Adjust to ensure the sum is exactly 1
        float error = 1f - sum;
        for (int i = 0; i < solutionMakeup.Count; i++)
            {
                solutionMakeup[i] += error * (solutionMakeup[i] / sum);
            }
        
        currentVolume_mL += volume;
        
        updatePercentages();
    }

    public void filterSolution(List<float> solutionToFilter, float volume, Transform Flask)
    {
        liquidScript flaskScript = Flask.GetComponent<liquidScript>();

        // Prevent filtering if there is too little solution left
        if (currentVolume_mL < 0.1f)  // Adjust threshold as needed
        {
            
            return;
        }

        // Ensure we don't try to filter more than available
        if (volume > currentVolume_mL)
        {
            Debug.LogWarning("Filtering stopped: Solution too low to continue.");
            volume = currentVolume_mL;  // Adjust to only filter what's available
        }

        float newVolumeTop = currentVolume_mL - volume;
        float newVolumeBottom = flaskScript.currentVolume_mL + volume;

        if (newVolumeTop <= 0)
        {
            //Debug.LogWarning("Filtering stopped: Not enough solution to filter.");
            return;
        }

        float sumTop = 0f;
        float sumBottom = 0f;

        for (int i = 0; i < solutionMakeup.Count; i++)
        {
            float originalSolutionMakeup = solutionMakeup[i] * currentVolume_mL;
            float transferAmount = solutionToFilter[i] * volume;
            float newConcentrationTop = (originalSolutionMakeup - transferAmount) / newVolumeTop;

            if (newConcentrationTop > 0)
            {
                solutionMakeup[i] = newConcentrationTop;
                flaskScript.solutionMakeup[i] = (flaskScript.solutionMakeup[i] * flaskScript.currentVolume_mL + transferAmount) / newVolumeBottom;
            }
            else
            {
                solutionMakeup[i] = 0;
            }

            sumTop += solutionMakeup[i];
            sumBottom += flaskScript.solutionMakeup[i];
        }

        // Normalize to ensure the total sum remains 1
        if (sumTop > 0)
        {
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                solutionMakeup[i] /= sumTop;
            }
        }

        if (sumBottom > 0)
        {
            for (int i = 0; i < flaskScript.solutionMakeup.Count; i++)
            {
                flaskScript.solutionMakeup[i] /= sumBottom;
            }
        }

        // Update volumes
        currentVolume_mL -= volume;
        flaskScript.currentVolume_mL += volume;

        // Prevent floating-point underflow
        if (currentVolume_mL < 0.01f) currentVolume_mL = 0f;
        if (flaskScript.currentVolume_mL < 0.01f) flaskScript.currentVolume_mL = 0f;

        // Update percentages
        updatePercentages();
        flaskScript.updatePercentages();
    }

    //This is to keep the percentages updated so that everything is consistent as well as making sure a couple of other things we are keeping track of are up to date
    public void updatePercentages(){
        percentH2SO4 = solutionMakeup[0];
        percentKOH = solutionMakeup[1];
        percentH2O = solutionMakeup[2];
        percentK2SO4 = solutionMakeup[3];
        percentAl = solutionMakeup[4];
        percentKAlOH4 = solutionMakeup[5];
        percentAl2SO43 = solutionMakeup[6];
        percentAlum = solutionMakeup[7];
        percentAlOH3 = solutionMakeup[8]; 
        percentKAlSO42 = solutionMakeup[9]; 
        percentKAlO2 = solutionMakeup[10]; 

        calculateDensity();

        //updates the percent liquid in a solution to determine if the pipette or scooper should be used
        updateLiquidPercent();
    }

    void calculateDensity(){
        float totalMass = 0f;
        for (int i = 0; i < densities.Count; i++){
            totalMass += densities[i] * solutionMakeup[i] * currentVolume_mL;
        }
        if (currentVolume_mL != 0){
            densityOfLiquid = totalMass / currentVolume_mL;
        }
        else{
            densityOfLiquid = 0f;
        }
    }

    public void handleReactions(){
        //if they are crystal forming then it will ruin the crytsalization and if they produce H2 gas it needs a vent or it could catch fire those with precipitants wont precipitate with added heat (boiling)
        if (!reactionHappening){
            //tested and working
            if (percentAl > 0.02f){ 
                //reaction releases 3 mols of H2 which is flamable and makes bubbles
                //releases heat
                //dark gritty material on the top of the solution
                //accelerated by heat
                //CORRECT PATH
                if (percentKOH > 0.02f && percentH2O > 0.06f){
                    List<string> reactants = new List<string> {"Al", "KOH", "H2O"};
                    List<string> products = new List<string> {"KAl(OH)4"};
                    List<float> Rratio = new List<float> {2, 2, 6};
                    List<float> Pratio = new List<float> {2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, .1f, "H2", 3, false));
                }
                //Tested and Working
                if (percentH2SO4 > 0.03f){
                    //reaction releases 3 mols of H2 which is flamable and makes bubbles
                    //film on aluminum balls if concentrated H2SO4
                    //accelerated by heat
                    List<string> reactants = new List<string> {"Al", "H2SO4"};
                    List<string> products = new List<string> {"Al2(SO4)3"};
                    List<float> Rratio = new List<float> {2, 3};
                    List<float> Pratio = new List<float> {1};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, .1f, "H2", 3, false));
                }
            }
            if (percentH2SO4 > 0.02f){
                // produces heat
                // fast
                //tested and working
                //VIOLENT WITH ADDED HEAT
                if (percentKOH > 0.04f){
                    // Reaction: Potassium hydroxide (KOH) + Sulfuric acid (H2SO4)
                    // Produces potassium sulfate (K2SO4) and water (H2O)
                    //salt forms at the bottom if concentrates if dumped in all at once
                    // disolves completely if heated
                    List<string> reactants = new List<string> {"H2SO4", "KOH"};
                    List<string> products = new List<string> {"K2SO4", "H2O"};
                    List<float> Rratio = new List<float> {1, 2};
                    List<float> Pratio = new List<float> {1, 2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 5f, "none", 0, true)); // moderate heat generation
                }
                //GOAL PRODUCT
                //tested and working
                //split up into steps
                //if h2so4 goes in to fast solid will clump and reaction will slow significanlty
                //hat solves this problem or mixing
                //if (percentKAlOH4 > 0.03f){
                //    // forms alum as white crystals on the top of the solution
                //    List<string> reactants = new List<string> {"H2SO4", "KAl(OH)4"};
                //    List<string> products = new List<string> {"Alum"};
                //    List<float> Rratio = new List<float> {1, 2};
                //    List<float> Pratio = new List<float> {1};
                //    StartCoroutine(react(reactants, Rratio, products, Pratio, 6f, "none", 0, false)); // moderate heat, solid white precipitate
                //}

                if (percentKAlOH4 > 0.03f){
                    //intermediate reaction for alum
                    // CORRECT PATH
                    List<string> reactants = new List<string> {"H2SO4", "KAl(OH)4"};
                    List<string> products = new List<string> {"Al(OH)3", "K2SO4", "H2O"};
                    List<float> Rratio = new List<float> {1, 2};
                    List<float> Pratio = new List<float> {2, 1, 2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 6f, "none", 0, false)); 
                }

                // Potassium aluminate + sulfuric acid
                // Produces potassium alum (KAl(SO4)2) and water (H2O)
                // Fast, produces crystals as the solution cools
                if (percentKAlO2 > 0.02f){
                    // Reaction: Potassium aluminate (KAlO2) + Sulfuric acid (H2SO4)
                    // Produces potassium alum (KAl(SO4)2) and water (H2O)
                    List<string> reactants = new List<string> {"H2SO4", "KAlO2"};
                    List<string> products = new List<string> {"Al(OH)3", "K2SO4"};
                    List<float> Rratio = new List<float> {2, 3};
                    List<float> Pratio = new List<float> {1, 2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 10f, "none", 0, false)); // exothermic, forms crystals over time
                }
                // Reaction: Aluminum hydroxide (Al(OH)3) + Sulfuric acid (H2SO4)
                // Produces aluminum sulfate (Al2(SO4)3) and water (H2O)

                // **Physical Manifestations:**
                // - **Exothermic:** Generates moderate heat, causing a noticeable temperature rise in the solution.
                // - **Dissolution:** The gelatinous, white Al(OH)3 precipitate dissolves upon contact with the acid.
                // - **Clarity Change:** Initially cloudy solution becomes clear as Al(OH)3 dissolves.
                // - **Reaction Speed:** Moderate (3/30 scale), occurs within seconds to minutes
                // CORRECT PATH
                if (percentAlOH3 > 0.02f){
                    List<string> reactants = new List<string> {"Al(OH)3", "H2SO4"};
                    List<string> products = new List<string> {"Al2(SO4)3", "H2O"};
                    List<float> Rratio = new List<float> {2, 3};
                    List<float> Pratio = new List<float> {1, 6};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 3f, "none", 0, false)); 
                }
            }
            if (percentKOH > 0.02f){
                //aloh3 dissolves
                //endothermic
                if (percentAlOH3 > 0.02f){
                    List<string> reactants = new List<string> {"KOH", "Al(OH)3"};
                    List<string> products = new List<string> {"KAl(OH)4"};
                    List<float> Rratio = new List<float> {1, 1};
                    List<float> Pratio = new List<float> {1};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 15f, "none", 0, false)); 
                }
                ////aloh3 is a gel like white precipitate which forms almost immediately
                ////more dilute -> slower
                //if (percentKAlSO42 > 0.02f){
                //    List<string> reactants = new List<string> {"KOH", "KAl(SO4)2"};
                //    List<string> products = new List<string> {"K2SO4", "Al(OH)3"};
                //    List<float> Rratio = new List<float> {3, 1};
                //    List<float> Pratio = new List<float> {2, 1};
                //    StartCoroutine(react(reactants, Rratio, products, Pratio, 6f, "none", 0, false)); 
                //}
            }
            if (percentKAlOH4 > 0.02f) {
               if (percentAl2SO43 > 0.01f) {
                   // Al(OH)3 forms at the bottom as a solid white precipitate
                   // Exothermic, significant heat released
                   // Reaction causes the solution to become a milky, gelatinous fluid
                   // Slow precipitation process, white solid settles at the bottom
                   List<string> reactants = new List<string> {"KAl(OH)4", "Al2(SO4)3"};
                   List<string> products = new List<string> {"Al(OH)3", "K2SO4"};
                   List<float> Rratio = new List<float> {2, 1};
                   List<float> Pratio = new List<float> {2, 1};
                   StartCoroutine(react(reactants, Rratio, products, Pratio, 8f, "none", 0, false));
               }
            }
            if (percentAl2SO43 > 0.02f){
                if (percentKOH > 0.02f){
                    List<string> reactants = new List<string> {"Al2(SO4)3", "KOH"};
                    List<string> products = new List<string> {"Al(OH)3", "K2SO4"};
                    List<float> Rratio = new List<float> {1, 6};
                    List<float> Pratio = new List<float> {2, 3};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 8f, "none", 0, false));
                }
            }
            if (percentK2SO4 > 0.02f){
                if (percentAl2SO43 > 0.02f && percentH2O > 0.24f){
                    //CORRECT PATH
                    // GOAL PRODUCT
                    List<string> reactants = new List<string> {"Al2(SO4)3", "K2SO4", "H2O"};
                    List<string> products = new List<string> {"KAl(SO4)2"};
                    List<float> Rratio = new List<float> {1, 1, 24};
                    List<float> Pratio = new List<float> {4};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 8f, "none", 0, false));
                }
            }
        }
    }

    IEnumerator react(List<string> reactants, List<float> Rratio, List<string> products, List<float> Pratio, float reactSpeed, string gasType = "none", int gasMols = 0, bool violence = false)
    {
        reactionHappening = true;
        limreactnum = 1f;

        // Gradually process the reaction
        while (limreactnum > 0.001f)
        {
            if (liquidTemperature > 25){
                isViolent = violence;
            }
            List<float> solutionMols = Enumerable.Repeat(0f, 11).ToList();

            // Convert percentages to moles for reactants
            for (int i = 0; i < solutionMols.Count; i++)
            {
                float reactantMol = solutionMakeup[i] * currentVolume_mL * densityOfLiquid / molarMasses[i];
                solutionMols[i] = reactantMol;
            }

            // Find the limiting reactant
            List<float> limreactfinder = new List<float>();
            for (int i = 0; i < reactants.Count; i++)
            {
                limreactfinder.Add(solutionMols[compoundNames.IndexOf(reactants[i])] / Rratio[i]);
            }
            limreactnum = limreactfinder.Min();

            // Calculate the amount of reactant used and product formed
            for (int i = 0; i < reactants.Count; i++)
            {
                // Ensure we do not subtract more than we have
                float usedMols = Rratio[i] * limreactnum / 10f;
                solutionMols[compoundNames.IndexOf(reactants[i])] = Mathf.Max(solutionMols[compoundNames.IndexOf(reactants[i])] - usedMols, 0f); // Avoid negative mols
            }

            // Calculate the product formation based on limiting reactant
            for (int i = 0; i < products.Count; i++)
            {
                // Update products in proportion to the limiting reactant
                solutionMols[compoundNames.IndexOf(products[i])] += Pratio[i] * limreactnum / 10f;
                if (gasType == "H2"){
                    H2Released += gasMols * limreactnum / 10f; 
                }
            }

            // Update liquid color (or other visual effects)
            handleLiquidColor();

            // Calculate total mass after reaction progress and update percentages
            List<float> solutionMasses = new List<float>();
            for (int i = 0; i < solutionMols.Count; i++)
            {
                solutionMasses.Add(solutionMols[i] * molarMasses[i]);
            }

            // Calculate the total mass after reaction progress
            float totalMass = solutionMasses.Sum();

            // Check for invalid total mass
            if (totalMass <= 0f)
            {
                break; // Exit the loop to prevent NaN
            }

            // Convert masses to new percentages based on the current mass
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                solutionMakeup[i] = solutionMasses[i] / totalMass;

                // Ensure no NaN values
                if (float.IsNaN(solutionMakeup[i]))
                {
                    solutionMakeup[i] = 0f;
                }
            }

            // Update percentages dynamically
            updatePercentages();

            // Validate duration to prevent NaN/negative
            float duration = (1f / reactSpeed) / liquidTemperature * roomTemp / 2;
            if (float.IsNaN(duration) || duration <= 0f || float.IsInfinity(duration))
            {
                duration = 0.1f; // Default to a safe value
            }

            yield return new WaitForSeconds(duration);  // Allow other game logic to continue
        }

        isViolent = false;
        reactionHappening = false;
    }
    public float GetMeltingPoint()
    {
        // Calculate the total percentage to ensure it sums to 100% or 1
        float totalPercent = percentH2SO4 + percentKOH + percentH2O + percentK2SO4 +
                             percentAl + percentKAlOH4 + percentAl2SO43 + percentAlum +
                             percentAlOH3 + percentKAlSO42 + percentKAlO2;

        // Prevent division by zero if totalPercent is zero
        if (totalPercent == 0f)
        {
            //Debug.LogWarning("Total percentage is 0, returning a default value of 0�C");
            return 0f; // Return 0 if the percentages don't sum up to a meaningful value
        }

        // Calculate the weighted average melting point
        float meltingPoint = (
            (percentH2SO4 * meltingPointH2SO4) +
            (percentKOH * meltingPointKOH) +
            (percentH2O * meltingPointH2O) +
            (percentK2SO4 * meltingPointK2SO4) +
            (percentAl * meltingPointAl) +
            (percentKAlOH4 * meltingPointKAlOH4) +
            (percentAl2SO43 * meltingPointAl2SO43) +
            (percentAlum * meltingPointAlum) +
            (percentAlOH3 * meltingPointAlOH3) +
            (percentKAlSO42 * meltingPointKAlSO42) +
            (percentKAlO2 * meltingPointKAlO2)
        ) / totalPercent;

        return meltingPoint;
    }


    void explode()
    {
        if (explosion != null)
        {
            // Calculate position slightly above the object
            Vector3 explosionPosition = transform.position + new Vector3(0, explosionHeightOffset, 0);

            // Instantiate the explosion effect
            GameObject explosionInstance = Instantiate(explosion, explosionPosition, Quaternion.identity);
            AudioSource.PlayClipAtPoint(boomSound, transform.position);

            // Destroy the explosion effect after the specified duration
            Destroy(explosionInstance, explosionDuration);
        }
        else
        {
            Debug.LogWarning("Explosion effect is not assigned.");
        }
    }
}