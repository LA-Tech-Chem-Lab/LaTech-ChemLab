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
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class liquidScript : MonoBehaviour
{
    public float totalVolume_mL;
    public float currentVolume_mL;
    public float readHere;
    public float readHere2;
    public float testScale = 1f;
    [Header("Spillage")]

    private AudioSource BoilingAudioSource;
    public float dotProduct; 
    float maxSpillRate = 0f;
    public bool autoAssignSpillRate;
    Renderer rend;
    Rigidbody objectRigidbody;
    float initialObjectMass;
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
    public float percentCH3CH2OH = 0f; 
    public float limreactnum;
    public List<float> solutionMakeup = new List<float>();
    public bool step3Done = false;
    public bool step4Done = false;
    public List<string> compoundNames = new List<string> {
        "H2SO4", "KOH", "H2O", "K2SO4", "Al", "KAl(OH)4", "Al2(SO4)3", "Alum", "Al(OH)3", "KAl(SO4)2*12H2O", "KAlO2", "CH3CH2OH"
    };

    public List<float> densities = new List<float> {
        1.83f, 2.12f, 1f, 2.66f, 2.7f, 1.5f, 2.672f, 1.76f, 2.42f, 1.75f, 1.57f, 0.789f
    };

    public List<float> molarMasses = new List<float> {
        98.079f, 56.1056f, 18.01528f, 174.259f, 26.982f, 134.12f, 342.14f, 474.39f, 78f, 464.46f, 98.075f, 46.069f 
    };

    public List<float> specificHeatCapacities = new List<float> {
        1380f, 1300f, 4186f, 1060f, 900f, 1500f, 1300f, 1200f, 900f, 1060f, 1250f, 2440f 
    };

    public float[] boilingPoints = new float[] {
        611f, 1388f, 373.15f, 1685f, 2792f, 1110f, 1073f, 1383f, 773f, 1383f, 1280f, 351.44f  
    };

    public List<float> latentHeat = new List<float> {
        2257000f, 2000000f, 2257000f, 2500000f, 2920000f, 2200000f, 2500000f, 2400000f, 2300000f, 2300000f, 2400000f, 846000f 
    };

    List<Color> liquidColors = new List<Color> {Color.red, Color.green, Color.blue, Color.yellow, new Color(0.6f, 0.6f, 0.6f), Color.yellow, Color.red, Color.white, Color.yellow, Color.green, Color.blue, new Color(0.9f, 0.5f, 0.3f)};
    public List<char> compoundStates = new List<char> { 'a', 'a', 'l', 'a', 's', 'a', 's', 's', 's', 'a', 's', 'l' };

    //                                            H2SO4       KOH              H20        K2SO4              Al                  KAl(OH)4
    public bool reactionHappening;
    public int currReactionID = 0;


    public Transform liquidTransform;

    [Header("Liquid Heating")]
    public float maxHeat = 1200f;  // Maximum heat at the center
    public float currentHeat = 1f; // Heat affecting the beaker
    float convectiveHeatTransferCoeff = 100f;
    public float liquidTemperature = 293.15f;
    float specificHeatCapacity = 4184f;
    public float roomTemp = 293.15f;
    public bool isinIceBath;
    public AudioClip boilingSound;

    [Header("Filtering")]
    public bool isFiltering = false;
    public float liquidPercent;

    public bool isPouring = false;

    List<float> meltingPoints_K = new List<float>
    {
        283.45f,  // H2SO4 (10.3 + 273.15)
        633.15f,  // KOH (360 + 273.15)
        273.15f,  // H2O (0 + 273.15)
        1342.15f, // K2SO4 (1069 + 273.15)
        933.45f,  // Al (660.3 + 273.15)
        473.15f,  // KAl(OH)4 (estimated ~200 + 273.15)
        1043.15f, // Al2(SO4)3 (770 + 273.15)
        365.15f,  // Alum (~92 + 273.15)
        573.15f,  // Al(OH)3 (~300 + 273.15)
        365.15f,  // KAl(SO4)2·12H2O (same as Alum)
        1233.15f,  // KAlO2 (~960 + 273.15)
        159f //ethanol
    };

    private float freezeProgress = 1f;
    public bool isCrystalizedAlum;
    public GameObject crystallizationPrefab;
    public GameObject liquidCrystalPrefab;
    private Transform crystallizationTransform;
    public GameObject solidinliquideffect;
    public bool hasSpawnedSolid = false;
    public GameObject newSolid = null;

    [Header("Toxic Gas")]
    public float H2Released = 0f;
    public GameObject boilingEffect;
    public bool isBoiling = false;
    GameObject currentBoilingEffect;
    public GameObject explosion;
    float explosionHeightOffset = -1.55f;
    public float explosionDuration = 5f;
    public bool exploded = false; 
    public float detectionRadius = 1f;
    public GameObject firePrefab;
    public AudioClip boomSound;
    public bool isViolent = false; 
    public GameObject player;
    public GameObject stuffInEyesFilter;
    public GameObject placeholderFaucet;
    public bool startedBoiling = false;
 


    // Use this for initialization
    void Start()
    {
        BoilingAudioSource = GetComponent<AudioSource>();
        isCrystalizedAlum = false;
        liquidTransform = transform.Find("Liquid").transform;
        rend = transform.Find("Liquid").GetComponent<Renderer>();
        objectRigidbody = GetComponent<Rigidbody>();
        boilingEffect = Resources.Load<GameObject>("boilingEffect");
        explosion = Resources.Load<GameObject>("Explosion Effect");
        firePrefab = Resources.Load<GameObject>("Flame");
        crystallizationPrefab = Resources.Load<GameObject>("Crystalization");
        liquidCrystalPrefab = Resources.Load<GameObject>("Crystalized Liquid");
        solidinliquideffect = Resources.Load<GameObject>("Solid In Liquid Effect");
        boomSound = Resources.Load<AudioClip>("boomSound");

        //doCertainThingWith certainThingWith = player.GetComponent<doCertainThingWith>();
        if (gameObject.name == "Capilary tube")
        {
            initialObjectMass = 1.0f; // Set to a default value
        }
        else
        {
            initialObjectMass = objectRigidbody.mass; // Otherwise, use the Rigidbody's mass
        }
        solutionMakeup.AddRange(new float[] { percentH2SO4, percentKOH , percentH2O, percentK2SO4, percentAl, percentKAlOH4, percentAl2SO43, percentAlum, percentAlOH3, percentKAlSO42, percentKAlO2, percentCH3CH2OH});
        if (currentVolume_mL > 0){
            calculateDensity();
        }
        if (autoAssignSpillRate)
            maxSpillRate = totalVolume_mL * 0.2f;

        updatePercentages();

        player = GameObject.Find("Player");
        GameObject UI = GameObject.Find("Canvases");
        GameObject InGame = UI.transform.Find("In Game Canvas").gameObject;
        stuffInEyesFilter = InGame.transform.Find("stuffInEyesFilter").gameObject;
    }

    private void Update()
    {
        if (solidinliquideffect != null)
        {
            if (gameObject.transform.name.StartsWith("Beaker")) {

                float percentFull = currentVolume_mL / totalVolume_mL;
                float sludgeY = GetSludgeYPositionBeaker(percentFull);

                if (!hasSpawnedSolid && liquidPercent > 0f && liquidPercent < .95f && currentVolume_mL > 0f)
                {
                    newSolid = Instantiate(solidinliquideffect, transform.position, Quaternion.identity, transform);
                    hasSpawnedSolid = true;
                }

                if (newSolid != null)
                {
                    // Always follow the sludge level
                    newSolid.transform.localPosition = new Vector3(0f, sludgeY, 0f);

                    Renderer rend = newSolid.GetComponent<Renderer>();
                    if (rend != null && rend.material.HasProperty("_Dissolve_Amount"))
                    {
                        float dissolveAmount;

                        if (liquidPercent > 0.95f)
                        {
                            dissolveAmount = 0.1f; // Constant at high fill level
                        }
                        else
                        {
                            float clampedPercent = Mathf.Clamp(liquidPercent, 0.70f, 0.95f);
                            dissolveAmount = Mathf.Lerp(0.08f, -0.4f, Mathf.InverseLerp(0.95f, 0.70f, clampedPercent));
                        }

                        rend.material.SetFloat("_Dissolve_Amount", dissolveAmount);
                    }
                }
            }
            else if (gameObject.transform.name == "Erlenmeyer Flask 500")
            {
                float percentFull = currentVolume_mL / totalVolume_mL;
                float sludgeY = GetSludgeYPositionFlaskLarge(percentFull);

                if (!hasSpawnedSolid && liquidPercent > 0f && liquidPercent < .95f && currentVolume_mL > 0f)
                {
                    newSolid = Instantiate(solidinliquideffect, transform.position, Quaternion.identity, transform);
                    hasSpawnedSolid = true;
                }

                if (newSolid != null)
                {
                    // Always follow the sludge level
                    newSolid.transform.localPosition = new Vector3(0f, sludgeY, 0f);

                    // Scale based on fill amount
                    Vector3 bottomScale = new Vector3(0.15f, 0.002f, 0.15f);
                    Vector3 topScale = new Vector3(0.05f, 0.002f, 0.05f);
                    float curvedFill = Mathf.Pow(percentFull, 0.6f);
                    newSolid.transform.localScale = Vector3.Lerp(bottomScale, topScale, curvedFill);

                    Renderer rend = newSolid.GetComponent<Renderer>();
                    if (rend != null && rend.material.HasProperty("_Dissolve_Amount"))
                    {
                        float dissolveAmount;

                        if (liquidPercent > 0.95f)
                        {
                            dissolveAmount = 0.1f;
                        }
                        else
                        {
                            float clampedPercent = Mathf.Clamp(liquidPercent, 0.70f, 0.95f);
                            dissolveAmount = Mathf.Lerp(0.08f, -0.4f, Mathf.InverseLerp(0.95f, 0.70f, clampedPercent));
                        }

                        rend.material.SetFloat("_Dissolve_Amount", dissolveAmount);
                    }
                }
            }
            else if (gameObject.transform.name.StartsWith("Erlenmeyer")) {
                float percentFull = currentVolume_mL / totalVolume_mL;
                float sludgeY = GetSludgeYPositionFlask(percentFull);

                if (!hasSpawnedSolid && liquidPercent > 0f && liquidPercent < .95f && currentVolume_mL > 0f)
                {
                    newSolid = Instantiate(solidinliquideffect, transform.position, Quaternion.identity, transform);
                    hasSpawnedSolid = true;
                }

                if (newSolid != null)
                {
                    // Always follow the sludge level
                    newSolid.transform.localPosition = new Vector3(0f, sludgeY, 0f);

                    // Scale based on fill amount
                    Vector3 bottomScale = new Vector3(0.15f, 0.002f, 0.15f);
                    Vector3 topScale = new Vector3(0.05f, 0.002f, 0.05f);
                    float curvedFill = Mathf.Pow(percentFull, 0.6f);
                    newSolid.transform.localScale = Vector3.Lerp(bottomScale, topScale, curvedFill);

                    Renderer rend = newSolid.GetComponent<Renderer>();
                    if (rend != null && rend.material.HasProperty("_Dissolve_Amount"))
                    {
                        float dissolveAmount;

                        if (liquidPercent > 0.95f)
                        {
                            dissolveAmount = 0.1f;
                        }
                        else
                        {
                            float clampedPercent = Mathf.Clamp(liquidPercent, 0.70f, 0.95f);
                            dissolveAmount = Mathf.Lerp(0.08f, -0.4f, Mathf.InverseLerp(0.95f, 0.70f, clampedPercent));
                        }

                        rend.material.SetFloat("_Dissolve_Amount", dissolveAmount);
                    }
                }
            }
        }

        // Calculate tilt using the dot product of the beaker's up direction and world up
        dotProduct = Vector3.Dot(transform.up.normalized, Vector3.up);

        // Spill logic
       // if (!isPouring){
       //     if (dotProduct <= 0.25f)
       //     {
       //         float loss = (-0.8f * dotProduct + 0.2f) * maxSpillRate * Time.deltaTime;
       //         currentVolume_mL -= loss;
//
       //         if (gameObject.name.StartsWith("Erlenmeyer") && currentVolume_mL / totalVolume_mL < 0.45f)
       //             currentVolume_mL = 0f;
       //         if (gameObject.name.StartsWith("Beaker") && currentVolume_mL / totalVolume_mL < 0.19f)
       //             currentVolume_mL = 0f;
       //         if (gameObject.name.StartsWith("Paper Cone") && currentVolume_mL / totalVolume_mL < 0.19f)
       //             currentVolume_mL = 0f; 
       //         if (gameObject.name.StartsWith("Graduated") && currentVolume_mL / totalVolume_mL < 0.19f)
       //             currentVolume_mL = 0f; 
       //     }
       // }
        if (transform.Find("Solid In Liquid Effect(Clone)")){
            if (dotProduct <= 0.25f)
            {
                transform.Find("Solid In Liquid Effect(Clone)").gameObject.SetActive(false);
            }
            else if (isCrystalizedAlum)
            {
                transform.Find("Solid In Liquid Effect(Clone)").gameObject.SetActive(false);
            }
            else
            {
                transform.Find("Solid In Liquid Effect(Clone)").gameObject.SetActive(true);
            }
        }
        
        if (isCrystalizedAlum)
        {
            Transform liquidTransform = transform.Find("Liquid");
            if (liquidTransform != null)
            {
                liquidTransform.gameObject.SetActive(false);
            }

            Transform existingCrystalizedLiquid = transform.Find("Crystalized Liquid");
            if (existingCrystalizedLiquid == null)
            {
                if (liquidCrystalPrefab != null)
                {
                    GameObject newCrystallizedLiquid = Instantiate(liquidCrystalPrefab, transform);
                    newCrystallizedLiquid.name = "Crystalized Liquid";

                    // Assign the renderer
                    rend = newCrystallizedLiquid.GetComponent<Renderer>();
                }
            }
            else
            {
                // Assign the renderer from the already existing "Crystalized Liquid"
                rend = existingCrystalizedLiquid.GetComponent<Renderer>();
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
            if (Flask.name.StartsWith("Buchner")){
                if (placeholderFaucet){
                    if (placeholderFaucet.GetComponent<FaucetScript>().FaucetHot || placeholderFaucet.GetComponent<FaucetScript>().FaucetCold){
                        StartCoroutine(handleFiltering(Flask));
                    }
                }  
            }
            else{
                StartCoroutine(handleFiltering(Flask));
            }
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
        if (isViolent && player.GetComponent<interactWithObjects>().gogglesOn == false){
            stuffInEyesFilter.SetActive(true);
        }
        if (BoilingAudioSource){
            if (isBoiling && !BoilingAudioSource.isPlaying)
                if (BoilingAudioSource) BoilingAudioSource.Play();
            if (!isBoiling)
                if (BoilingAudioSource) BoilingAudioSource.Stop();
        }
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

    private float GetSludgeYPositionBeaker(float fillAmount)
    {
        fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
        if (fillAmount <= 0.1f)
        {
            return 0.3113f * fillAmount - .177f;
        }
        else
        {
            return 0.3113f * fillAmount - 0.186f;
        }
    }

    private float GetSludgeYPositionFlask(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);

        if (fillAmount <= 0.5f)
        {
            // 0% to 50%
            return Mathf.Lerp(0.008f, 0.0694f, fillAmount / 0.5f);
        }
        else if (fillAmount <= 0.75f)
        {
            // 50% to 75%
            return Mathf.Lerp(0.0694f, 0.1152f, (fillAmount - 0.5f) / 0.25f);
        }
        else
        {
            // 75% to 100%
            return Mathf.Lerp(0.1152f, 0.18f, (fillAmount - 0.75f) / 0.25f);
        }
    }
    private float GetSludgeYPositionFlaskLarge(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);

        if (fillAmount <= 0.25f)
        {
            // 0% to 25%
            return Mathf.Lerp(0.008f, 0.0503f, fillAmount / 0.25f);
        }
        else if (fillAmount <= 0.5f)
        {
            // 25% to 50%
            return Mathf.Lerp(0.0503f, 0.0903f, (fillAmount - 0.25f) / 0.25f);
        }
        else if (fillAmount <= 0.75f)
        {
            // 50% to 75% 
            return Mathf.Lerp(0.0903f, 0.1304f, (fillAmount - 0.5f) / 0.25f);
        }
        else
        {
            // 75% to 100% 
            return Mathf.Lerp(0.1304f, 0.1864f, (fillAmount - 0.75f) / 0.25f);
        }
    }

    void handleLiquid(){

        
        objectRigidbody.mass = initialObjectMass + currentVolume_mL * densityOfLiquid / 1000f;

        // Liquid Colors
        rend.material.SetColor("_SideColor", surfaceColor);
        rend.material.SetColor("_TopColor", topColor);
        
        // Liquid Volume
        currentVolume_mL = Mathf.Clamp(currentVolume_mL, 0f, totalVolume_mL);

        float percentFull = currentVolume_mL / totalVolume_mL;
        
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

        if (transform.name.StartsWith("Erlenmeyer Flask")){
            
            
            if (totalVolume_mL ==  500f){ // 500 mL Flask
                float renderedFlask500 = 1f;
                if (inRange(percentFull, 0, 0.001f)) 
                    renderedFlask500 = 35.9f;
                if (inRange(percentFull, 0.001f, 0.002f))
                    renderedFlask500 = Mathf.Lerp(35.9f, 17.6f, reScale(percentFull, 0.001f, 0.002f));
                if (inRange(percentFull, 0.002f, 0.004f))
                    renderedFlask500 = Mathf.Lerp(17.6f, 9.4f, reScale(percentFull, 0.002f, 0.004f));
                if (inRange(percentFull, 0.004f, 0.006f))
                    renderedFlask500 = Mathf.Lerp(9.4f, 6.57f, reScale(percentFull, 0.004f, 0.006f));
                if (inRange(percentFull, 0.006f, 0.01f))
                    renderedFlask500 = Mathf.Lerp(6.57f, 4.06f, reScale(percentFull, 0.006f, 0.01f));
                if (inRange(percentFull, 0.01f, 0.015f))
                    renderedFlask500 = Mathf.Lerp(4.06f, 2.94f, reScale(percentFull, 0.01f, 0.015f));
                if (inRange(percentFull, 0.015f, 0.02f))
                    renderedFlask500 = Mathf.Lerp(2.94f, 2.37f, reScale(percentFull, 0.015f, 0.02f));
                if (inRange(percentFull, 0.02f, 0.03f))
                    renderedFlask500 = Mathf.Lerp(2.37f, 1.76f, reScale(percentFull, 0.02f, 0.03f));
                if (inRange(percentFull, 0.03f, 0.04f))
                    renderedFlask500 = Mathf.Lerp(1.76f, 1.6f, reScale(percentFull, 0.03f, 0.04f));
                if (inRange(percentFull, 0.04f, 0.06f))
                    renderedFlask500 = Mathf.Lerp(1.6f, 1.35f, reScale(percentFull, 0.04f, 0.06f));
                if (inRange(percentFull, 0.06f, 0.08f))
                    renderedFlask500 = Mathf.Lerp(1.35f, 1.22f, reScale(percentFull, 0.06f, 0.08f));
                if (inRange(percentFull, 0.08f, 0.1f))
                    renderedFlask500 = Mathf.Lerp(1.22f, 1.14f, reScale(percentFull, 0.08f, 0.1f));
                if (inRange(percentFull, 0.1f, 0.12f))
                    renderedFlask500 = Mathf.Lerp(1.14f, 1.09f, reScale(percentFull, 0.1f, 0.12f));
                if (inRange(percentFull, 0.12f, 0.16f))
                    renderedFlask500 = Mathf.Lerp(1.09f, 1.028f, reScale(percentFull, 0.12f, 0.16f));
                if (inRange(percentFull, 0.16f, 0.2f))
                    renderedFlask500 = Mathf.Lerp(1.028f, 0.99f, reScale(percentFull, 0.16f, 0.2f));
                if (inRange(percentFull, 0.2f, 0.24f))
                    renderedFlask500 = Mathf.Lerp(0.99f, 0.964f, reScale(percentFull, 0.2f, 0.24f));
                if (inRange(percentFull, 0.24f, 0.28f))
                    renderedFlask500 = Mathf.Lerp(0.964f, 0.944f, reScale(percentFull, 0.24f, 0.28f));
                if (inRange(percentFull, 0.28f, 0.32f))
                    renderedFlask500 = Mathf.Lerp(0.944f, 0.932f, reScale(percentFull, 0.28f, 0.32f));
                if (inRange(percentFull, 0.32f, 0.36f))
                    renderedFlask500 = Mathf.Lerp(0.932f, 0.92f, reScale(percentFull, 0.32f, 0.36f));
                if (inRange(percentFull, 0.36f, 0.4f))
                    renderedFlask500 = Mathf.Lerp(0.92f, 0.908f, reScale(percentFull, 0.36f, 0.4f));
                if (inRange(percentFull, 0.4f, 0.5f))
                    renderedFlask500 = Mathf.Lerp(0.908f, 0.849f, reScale(percentFull, 0.4f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.6f))
                    renderedFlask500 = Mathf.Lerp(0.849f, 0.81f, reScale(percentFull, 0.5f, 0.6f));
                if (inRange(percentFull, 0.6f, 0.7f))
                    renderedFlask500 = Mathf.Lerp(0.81f, 0.809f, reScale(percentFull, 0.6f, 0.7f));
                if (inRange(percentFull, 0.7f, 0.8f))
                    renderedFlask500 = Mathf.Lerp(0.809f, 0.805f, reScale(percentFull, 0.7f, 0.8f));
                if (inRange(percentFull, 0.8f, 0.9f))
                    renderedFlask500 = Mathf.Lerp(0.805f, 0.84f, reScale(percentFull, 0.8f, 0.9f));
                if (inRange(percentFull, 0.9f, 0.95f))
                    renderedFlask500 = Mathf.Lerp(0.84f, 0.855f, reScale(percentFull, 0.9f, 0.95f));
                if (inRange(percentFull, 0.95f, 1.0f))
                    renderedFlask500 = Mathf.Lerp(0.855f, 0.865f, reScale(percentFull, 0.95f, 1.0f));
                if (percentFull == 1)
                    renderedFlask500 = 0.865f;

                // renderedFlask500 = testScale;
                // readHere = percentFull;

                readHere =  percentFull * renderedFlask500;
                rend.material.SetFloat("_FillAmount", percentFull * renderedFlask500);
            }

            if (totalVolume_mL ==  250f){ // 250 mL Flask
                float renderedFlask250 = 1f;
                if (inRange(percentFull, 0f, 0.002f))
                    renderedFlask250 = 17f;
                if (inRange(percentFull, 0.002f, 0.004f))
                    renderedFlask250 = Mathf.Lerp(17f, 8.8f, reScale(percentFull, 0.002f, 0.004f));
                if (inRange(percentFull, 0.004f, 0.008f))
                    renderedFlask250 = Mathf.Lerp(8.8f, 4.75f, reScale(percentFull, 0.004f, 0.008f));
                if (inRange(percentFull, 0.008f, 0.014f))
                    renderedFlask250 = Mathf.Lerp(4.75f, 2.94f, reScale(percentFull, 0.008f, 0.014f));
                if (inRange(percentFull, 0.014f, 0.02f))
                    renderedFlask250 = Mathf.Lerp(2.94f, 2.12f, reScale(percentFull, 0.014f, 0.02f));
                if (inRange(percentFull, 0.02f, 0.03f))
                    renderedFlask250 = Mathf.Lerp(2.12f, 1.58f, reScale(percentFull, 0.02f, 0.03f));
                if (inRange(percentFull, 0.03f, 0.04f))
                    renderedFlask250 = Mathf.Lerp(1.58f, 1.33f, reScale(percentFull, 0.03f, 0.04f));
                if (inRange(percentFull, 0.04f, 0.06f))
                    renderedFlask250 = Mathf.Lerp(1.33f, 1.04f, reScale(percentFull, 0.04f, 0.06f));
                if (inRange(percentFull, 0.06f, 0.08f))
                    renderedFlask250 = Mathf.Lerp(1.04f, 0.903f, reScale(percentFull, 0.06f, 0.08f));
                if (inRange(percentFull, 0.08f, 0.1f))
                    renderedFlask250 = Mathf.Lerp(0.903f, 0.82f, reScale(percentFull, 0.08f, 0.1f));
                if (inRange(percentFull, 0.1f, 0.12f))
                    renderedFlask250 = Mathf.Lerp(0.82f, 0.762f, reScale(percentFull, 0.1f, 0.12f));
                if (inRange(percentFull, 0.12f, 0.16f))
                    renderedFlask250 = Mathf.Lerp(0.762f, 0.693f, reScale(percentFull, 0.12f, 0.16f));
                if (inRange(percentFull, 0.16f, 0.2f))
                    renderedFlask250 = Mathf.Lerp(0.693f, 0.65f, reScale(percentFull, 0.16f, 0.2f));
                if (inRange(percentFull, 0.2f, 0.24f))
                    renderedFlask250 = Mathf.Lerp(0.65f, 0.622f, reScale(percentFull, 0.2f, 0.24f));
                if (inRange(percentFull, 0.24f, 0.28f))
                    renderedFlask250 = Mathf.Lerp(0.622f, 0.6021f, reScale(percentFull, 0.24f, 0.28f));
                if (inRange(percentFull, 0.28f, 0.32f))
                    renderedFlask250 = Mathf.Lerp(0.6021f, 0.587f, reScale(percentFull, 0.28f, 0.32f));
                if (inRange(percentFull, 0.32f, 0.4f))
                    renderedFlask250 = Mathf.Lerp(0.587f, 0.562f, reScale(percentFull, 0.32f, 0.4f));
                if (inRange(percentFull, 0.4f, 0.6f))
                    renderedFlask250 = Mathf.Lerp(0.562f, 0.557f, reScale(percentFull, 0.4f, 0.6f));
                if (inRange(percentFull, 0.6f, 0.7f))
                    renderedFlask250 = Mathf.Lerp(0.557f, 0.578f, reScale(percentFull, 0.6f, 0.7f));
                if (inRange(percentFull, 0.7f, 0.8f))
                    renderedFlask250 = Mathf.Lerp(0.578f, 0.595f, reScale(percentFull, 0.7f, 0.8f));
                if (inRange(percentFull, 0.8f, 0.9f))
                    renderedFlask250 = Mathf.Lerp(0.595f, 0.64f, reScale(percentFull, 0.8f, 0.9f));
                if (inRange(percentFull, 0.9f, 1.0f))
                    renderedFlask250 = Mathf.Lerp(0.64f, 0.67f, reScale(percentFull, 0.9f, 1.0f));
                if (percentFull == 1.0f)
                    renderedFlask250 = 0.67f;

                // renderedFlask250 = testScale;
                // readHere = percentFull;

                readHere =  percentFull * renderedFlask250;
                rend.material.SetFloat("_FillAmount", percentFull * renderedFlask250);
            }
        }

        if (transform.name == "Buchner Flask"){
            float renderedFlask250 = 1f;
            if (inRange(percentFull, 0f, 0.002f))
                renderedFlask250 = 17f;
            if (inRange(percentFull, 0.002f, 0.004f))
                renderedFlask250 = Mathf.Lerp(17f, 8.8f, reScale(percentFull, 0.002f, 0.004f));
            if (inRange(percentFull, 0.004f, 0.008f))
                renderedFlask250 = Mathf.Lerp(8.8f, 4.75f, reScale(percentFull, 0.004f, 0.008f));
            if (inRange(percentFull, 0.008f, 0.014f))
                renderedFlask250 = Mathf.Lerp(4.75f, 2.94f, reScale(percentFull, 0.008f, 0.014f));
            if (inRange(percentFull, 0.014f, 0.02f))
                renderedFlask250 = Mathf.Lerp(2.94f, 2.12f, reScale(percentFull, 0.014f, 0.02f));
            if (inRange(percentFull, 0.02f, 0.03f))
                renderedFlask250 = Mathf.Lerp(2.12f, 1.58f, reScale(percentFull, 0.02f, 0.03f));
            if (inRange(percentFull, 0.03f, 0.04f))
                renderedFlask250 = Mathf.Lerp(1.58f, 1.33f, reScale(percentFull, 0.03f, 0.04f));
            if (inRange(percentFull, 0.04f, 0.06f))
                renderedFlask250 = Mathf.Lerp(1.33f, 1.04f, reScale(percentFull, 0.04f, 0.06f));
            if (inRange(percentFull, 0.06f, 0.08f))
                renderedFlask250 = Mathf.Lerp(1.04f, 0.903f, reScale(percentFull, 0.06f, 0.08f));
            if (inRange(percentFull, 0.08f, 0.1f))
                renderedFlask250 = Mathf.Lerp(0.903f, 0.82f, reScale(percentFull, 0.08f, 0.1f));
            if (inRange(percentFull, 0.1f, 0.12f))
                renderedFlask250 = Mathf.Lerp(0.82f, 0.762f, reScale(percentFull, 0.1f, 0.12f));
            if (inRange(percentFull, 0.12f, 0.16f))
                renderedFlask250 = Mathf.Lerp(0.762f, 0.693f, reScale(percentFull, 0.12f, 0.16f));
            if (inRange(percentFull, 0.16f, 0.2f))
                renderedFlask250 = Mathf.Lerp(0.693f, 0.65f, reScale(percentFull, 0.16f, 0.2f));
            if (inRange(percentFull, 0.2f, 0.24f))
                renderedFlask250 = Mathf.Lerp(0.65f, 0.622f, reScale(percentFull, 0.2f, 0.24f));
            if (inRange(percentFull, 0.24f, 0.28f))
                renderedFlask250 = Mathf.Lerp(0.622f, 0.6021f, reScale(percentFull, 0.24f, 0.28f));
            if (inRange(percentFull, 0.28f, 0.32f))
                renderedFlask250 = Mathf.Lerp(0.6021f, 0.587f, reScale(percentFull, 0.28f, 0.32f));
            if (inRange(percentFull, 0.32f, 0.4f))
                renderedFlask250 = Mathf.Lerp(0.587f, 0.562f, reScale(percentFull, 0.32f, 0.4f));
            if (inRange(percentFull, 0.4f, 0.6f))
                renderedFlask250 = Mathf.Lerp(0.562f, 0.557f, reScale(percentFull, 0.4f, 0.6f));
            if (inRange(percentFull, 0.6f, 0.7f))
                renderedFlask250 = Mathf.Lerp(0.557f, 0.578f, reScale(percentFull, 0.6f, 0.7f));
            if (inRange(percentFull, 0.7f, 0.8f))
                renderedFlask250 = Mathf.Lerp(0.578f, 0.595f, reScale(percentFull, 0.7f, 0.8f));
            if (inRange(percentFull, 0.8f, 0.9f))
                renderedFlask250 = Mathf.Lerp(0.595f, 0.64f, reScale(percentFull, 0.8f, 0.9f));
            if (inRange(percentFull, 0.9f, 1.0f))
                renderedFlask250 = Mathf.Lerp(0.64f, 0.67f, reScale(percentFull, 0.9f, 1.0f));
            if (percentFull == 1.0f)
                renderedFlask250 = 0.67f;

            // renderedFlask250 = testScale;
            // readHere = percentFull;

            readHere =  percentFull * renderedFlask250;
            rend.material.SetFloat("_FillAmount", percentFull * renderedFlask250);
        }

        if (transform.name == "Graduated Cylinder") {

            if (totalVolume_mL ==  50f){ // 50 mL Graduated Cylinder
                float renderedGradCylinder50 = 1f;
                if (inRange(percentFull, 0, 0.005f)) 
                    renderedGradCylinder50 = 7.3f;
                if (inRange(percentFull, 0.005f, 0.0075f))
                    renderedGradCylinder50 = Mathf.Lerp(7.3f, 5.19f, reScale(percentFull, 0.005f, 0.0075f));
                if (inRange(percentFull, 0.0075f, 0.01f))
                    renderedGradCylinder50 = Mathf.Lerp(5.19f, 4.2f, reScale(percentFull, 0.0075f, 0.01f));
                if (inRange(percentFull, 0.01f, 0.015f))
                    renderedGradCylinder50 = Mathf.Lerp(4.2f, 3.16f, reScale(percentFull, 0.01f, 0.015f));
                if (inRange(percentFull, 0.015f, 0.02f))
                    renderedGradCylinder50 = Mathf.Lerp(3.16f, 2.73f, reScale(percentFull, 0.015f, 0.02f));
                if (inRange(percentFull, 0.02f, 0.03f))
                    renderedGradCylinder50 = Mathf.Lerp(2.73f, 2.2f, reScale(percentFull, 0.02f, 0.03f));
                if (inRange(percentFull, 0.03f, 0.04f))
                    renderedGradCylinder50 = Mathf.Lerp(2.2f, 1.925f, reScale(percentFull, 0.03f, 0.04f));
                if (inRange(percentFull, 0.04f, 0.05f))
                    renderedGradCylinder50 = Mathf.Lerp(1.925f, 1.76f, reScale(percentFull, 0.04f, 0.05f));
                if (inRange(percentFull, 0.05f, 0.06f))
                    renderedGradCylinder50 = Mathf.Lerp(1.76f, 1.65f, reScale(percentFull, 0.05f, 0.06f));
                if (inRange(percentFull, 0.06f, 0.08f))
                    renderedGradCylinder50 = Mathf.Lerp(1.65f, 1.518f, reScale(percentFull, 0.06f, 0.08f));
                if (inRange(percentFull, 0.08f, 0.1f))
                    renderedGradCylinder50 = Mathf.Lerp(1.518f, 1.437f, reScale(percentFull, 0.08f, 0.1f));
                if (inRange(percentFull, 0.1f, 0.12f))
                    renderedGradCylinder50 = Mathf.Lerp(1.437f, 1.38f, reScale(percentFull, 0.1f, 0.12f));
                if (inRange(percentFull, 0.12f, 0.16f))
                    renderedGradCylinder50 = Mathf.Lerp(1.38f, 1.315f, reScale(percentFull, 0.12f, 0.16f));
                if (inRange(percentFull, 0.16f, 0.2f))
                    renderedGradCylinder50 = Mathf.Lerp(1.315f, 1.275f, reScale(percentFull, 0.16f, 0.2f));
                if (inRange(percentFull, 0.2f, 0.25f))
                    renderedGradCylinder50 = Mathf.Lerp(1.275f, 1.243f, reScale(percentFull, 0.2f, 0.25f));
                if (inRange(percentFull, 0.25f, 0.3f))
                    renderedGradCylinder50 = Mathf.Lerp(1.243f, 1.22f, reScale(percentFull, 0.25f, 0.3f));
                if (inRange(percentFull, 0.3f, 0.35f))
                    renderedGradCylinder50 = Mathf.Lerp(1.22f, 1.205f, reScale(percentFull, 0.3f, 0.35f));
                if (inRange(percentFull, 0.35f, 0.4f))
                    renderedGradCylinder50 = Mathf.Lerp(1.205f, 1.194f, reScale(percentFull, 0.35f, 0.4f));
                if (inRange(percentFull, 0.4f, 0.5f))
                    renderedGradCylinder50 = Mathf.Lerp(1.194f, 1.176f, reScale(percentFull, 0.4f, 0.5f));
                if (inRange(percentFull, 0.5f, 0.6f))
                    renderedGradCylinder50 = Mathf.Lerp(1.176f, 1.166f, reScale(percentFull, 0.5f, 0.6f));
                if (inRange(percentFull, 0.6f, 0.7f))
                    renderedGradCylinder50 = Mathf.Lerp(1.166f, 1.1578f, reScale(percentFull, 0.6f, 0.7f));
                if (inRange(percentFull, 0.7f, 0.8f))
                    renderedGradCylinder50 = Mathf.Lerp(1.1578f, 1.1522f, reScale(percentFull, 0.7f, 0.8f));
                if (inRange(percentFull, 0.8f, 0.9f))
                    renderedGradCylinder50 = Mathf.Lerp(1.1522f, 1.1476f, reScale(percentFull, 0.8f, 0.9f));
                if (inRange(percentFull, 0.9f, 1.0f))
                    renderedGradCylinder50 = Mathf.Lerp(1.1476f, 1.1442f, reScale(percentFull, 0.9f, 1.0f));
                if (percentFull == 1)
                    renderedGradCylinder50 = 1.1442f;

                // renderedGradCylinder50 = testScale;
                // readHere = percentFull;

                readHere =  percentFull * renderedGradCylinder50;
                rend.material.SetFloat("_FillAmount", percentFull * renderedGradCylinder50);
            }
        }
        
        if (transform.name == "Paper Cone"){  // 1 to 1 in this case
            
            float paperConeVerticalScale = 1f;

            if (inRange(percentFull, 0f, 0.05f))
                paperConeVerticalScale = Mathf.Lerp(2.62f, 1.64f, reScale(percentFull, 0f, 0.05f));
            else if (inRange(percentFull, 0.05f, 0.25f))
                paperConeVerticalScale = Mathf.Lerp(1.64f, 0.68f, reScale(percentFull, 0.05f, 0.25f));
            else if (inRange(percentFull, 0.25f, 0.5f))
                paperConeVerticalScale = Mathf.Lerp(0.68f, 0.77f, reScale(percentFull, 0.25f, 0.5f));
            else if (inRange(percentFull, 0.5f, 1f))
                paperConeVerticalScale = Mathf.Lerp(0.77f, 0.51f, reScale(percentFull, 0.5f, 1f));
            if (percentFull == 1)
                paperConeVerticalScale = 0.51f;
            readHere = percentFull;
            // paperConeVerticalScale = testScale;
            // float actualPipetteScale = Mathf.Lerp(sideWaysPipetteScale, uprightPipetteScale, Mathf.Abs(dotProduct));
            
            rend.material.SetFloat("_FillAmount", percentFull * paperConeVerticalScale);
        }

        if (transform.name == "Pipette"){  // 1 to 1 in this case

            float uprightPipetteScale = 1f;
            float sideWaysPipetteScale = 1f;
            
            uprightPipetteScale = Map(percentFull, 0f, 1f, 3f, 1.5f);
            if (inRange(percentFull, 0f, 0.1f))
                sideWaysPipetteScale = Mathf.Lerp(3.5f, 1f, reScale(percentFull, 0f, 0.1f));
            else if (inRange(percentFull, 0.1f, 0.2f))
                sideWaysPipetteScale = Mathf.Lerp(1f, 0.5f, reScale(percentFull, 0.1f, 0.2f));
            else if (inRange(percentFull, 0.2f, 0.6f))
                sideWaysPipetteScale = Mathf.Lerp(0.5f, 0.37f, reScale(percentFull, 0.2f, 0.6f));
            else if (inRange(percentFull, 0.6f, 1f))
                sideWaysPipetteScale = Mathf.Lerp(0.37f, 0.33f, reScale(percentFull, 0.6f, 1f));
            if (percentFull == 1)
                sideWaysPipetteScale = 0.33f;

            float actualPipetteScale = Mathf.Lerp(sideWaysPipetteScale, uprightPipetteScale, Mathf.Abs(dotProduct));
            
            rend.material.SetFloat("_FillAmount", percentFull * actualPipetteScale);
        }

        if (transform.name == "Capilary tube"){
            
            float capillaryTubeScale = 1f;

            capillaryTubeScale = Map(percentFull, 0f, 1f, 0.43f, 0.2f);
            // readHere = percentFull;
            // capillaryTubeScale = testScale;
            // float actualPipetteScale = Mathf.Lerp(sideWaysPipetteScale, uprightPipetteScale, Mathf.Abs(dotProduct));
            
            rend.material.SetFloat("_FillAmount", percentFull * capillaryTubeScale);
        }
        

        

    }

    float Map(float var, float s1, float e1, float s2, float e2)
    {
        return ((var - s1) / (e1 - s1)) * (e2 - s2) + s2;
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
                currentHeat = 263.15f;
        }
        else
        {
            currentHeat = roomTemp;
        }

        // Apply heat transfer equation
        specificHeatCapacity = 0f;
        //Debug.Log(solutionMakeup.Count);
        //Debug.Log(specificHeatCapacities.Count);
        for (int i = 0; i < solutionMakeup.Count; i++){
            specificHeatCapacity += solutionMakeup[i] * specificHeatCapacities[i];
        }

        if (currentHeat < liquidTemperature)
        {
            float ambientCoolingRate = 0.05f; // Adjust for faster cooling
            float coolingLoss = ambientCoolingRate * beakerSurfaceArea * (liquidTemperature - currentHeat) * 1000;
            if ((GetComponent<Rigidbody>().mass * specificHeatCapacity) != 0){
                //liquidTemperature -= coolingLoss / (GetComponent<Rigidbody>().mass * specificHeatCapacity);
            }       
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
        liquidTemperature = Mathf.Lerp(liquidTemperature, currentHeat, Time.deltaTime / 25f);

        // Calculate total mass of the solution (assume mass of liquid is given or available)
        float totalSolutionMass = densityOfLiquid * currentVolume_mL;
        isBoiling = false;
        // Check for evaporation
        for (int i = 0; i < compoundNames.Count; i++)
        {
            float boilingPoint = boilingPoints[i]; // Get the boiling point for the compound
            float latentHeatOfVaporization = latentHeat[i]; // Latent heat of vaporization for each compound

            // If the temperature exceeds the boiling point, calculate evaporation rate
            if (liquidTemperature >= boilingPoint && solutionMakeup[i] > 0.1f)
            {
                isBoiling = true;
                if (!startedBoiling){
                    currentBoilingEffect = Instantiate(boilingEffect, transform);
                    currentBoilingEffect.transform.localPosition = new Vector3(0f, 0f, 0f);
                    currentBoilingEffect.GetComponent<Renderer>().material.color = surfaceColor;
                    startedBoiling = true;
                }

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
            //else if (!isBoiling){
            //    Destroy(currentBoilingEffect);
            //}
        }
        if (!isBoiling){
                startedBoiling = false;
                Destroy(currentBoilingEffect);
            }
        //Beaker Frost Effect
        if (liquidTemperature < 273.15f)
        {
            if (crystallizationTransform == null) // Instantiate if it doesn’t exist
            {
                if (crystallizationPrefab != null)
                {
                    GameObject newCrystallization = Instantiate(crystallizationPrefab, transform);
                    newCrystallization.name = "Crystalization"; 
                    crystallizationTransform = newCrystallization.transform;
                }
            }
            if (crystallizationTransform != null)
            {
                if (!crystallizationTransform.gameObject.activeSelf)
                {
                    crystallizationTransform.gameObject.SetActive(true);
                }

                Renderer crystallizationRenderer = crystallizationTransform.GetComponent<Renderer>();
                if (crystallizationRenderer != null)
                {
                    Material crystallizationMaterial = crystallizationRenderer.material;
                    // Increase freeze progress at a constant rate
                    freezeProgress = Mathf.Max(0.54298f, Mathf.Clamp01(freezeProgress - 0.02f * Time.deltaTime));

                    // Apply to material property
                    crystallizationMaterial.SetFloat("_Growth", freezeProgress);
                }
            }
        }
        else if (liquidTemperature > 273.15f)
        {
            Transform crystallizationTransform = transform.Find("Crystalization");
            if (crystallizationTransform != null)
            {
                Renderer crystallizationRenderer = crystallizationTransform.GetComponent<Renderer>();
                if (crystallizationRenderer != null)
                {
                    Material crystallizationMaterial = crystallizationRenderer.material;

                    // Decrease freeze progress at a constant rate
                    freezeProgress = Mathf.Clamp01(freezeProgress + .07f * Time.deltaTime);

                    // Apply to material property
                    crystallizationMaterial.SetFloat("_Growth", freezeProgress);
                }
            }
        }

        if (transform.Find("red outline")){
            if (liquidTemperature > 322f){
                transform.Find("red outline").gameObject.SetActive(true);
            }
            else{
                transform.Find("red outline").gameObject.SetActive(false);
            }
        }
        
    }

    IEnumerator handleFiltering(Transform Flask)
    {
        isFiltering = true;
        float liquidVolume = 10f;
        List<float> liquidSolution = Enumerable.Repeat(0f, 12).ToList();

        float volumeLeft = 0f;
        if (Flask.name.StartsWith("Erlenmeyer Flask")){
            volumeLeft = currentVolume_mL * 0.05f;
        }
        if (Flask.name.StartsWith("Buchner Flask")){
            if (solutionMakeup[11] > 0.05f){
                volumeLeft = currentVolume_mL * 0.01f;
            }
            else{
                volumeLeft = currentVolume_mL * 0.05f;
            }
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
                if (gameObject.transform.parent.parent.name.StartsWith("Erlenmeyer Flask") || gameObject.transform.parent.parent.name.StartsWith("Buchner Flask"))
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
        percentCH3CH2OH = solutionMakeup[11];

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
            if (percentAl > 0.002f){ 
                //reaction releases 3 mols of H2 which is flamable and makes bubbles
                //releases heat
                //dark gritty material on the top of the solution
                //accelerated by heat
                //CORRECT PATH
                if (percentKOH > 0.002f && percentH2O > 0.06f){
                    currReactionID = 1;
                    Debug.Log("start reaction");
                    List<string> reactants = new List<string> {"Al", "KOH", "H2O"};
                    List<string> products = new List<string> {"KAl(OH)4"};
                    List<float> Rratio = new List<float> {2, 2, 6};
                    List<float> Pratio = new List<float> {2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, .1f, "H2", 3, false));
                }
                //Tested and Working
                if (percentH2SO4 > 0.02f){
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
                    currReactionID = 2;
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
                    currReactionID = 3;
                    step3Done = true;
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
                    currReactionID = 4;
                    step4Done = true;
                    List<string> reactants = new List<string> {"Al2(SO4)3", "K2SO4", "H2O"};
                    List<string> products = new List<string> {"KAl(SO4)2*12H2O"};
                    List<float> Rratio = new List<float> {1, 1, 24};
                    List<float> Pratio = new List<float> {4};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 8f, "none", 0, false));
                }
            }
            if (liquidTemperature < 273.15f)
            {
                if (percentKAlSO42 >= .05f)
                {
                    List<string> reactants = new List<string> { "KAl(SO4)2*12H2O" };
                    List<string> products = new List<string> { "Alum" };
                    List<float> Rratio = new List<float> { 1 };
                    List<float> Pratio = new List<float> { 1 };
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 0.1f, "none", 0, false));
                    isCrystalizedAlum = true;
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
            List<float> solutionMols = Enumerable.Repeat(0f, 12).ToList();

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
            // if (player.GetComponent<doCertainThingWith>().beginStirring)
            // {
            //     duration = duration / 4f;
            // }

            yield return new WaitForSeconds(duration);  // Allow other game logic to continue
        }

        isViolent = false;
        reactionHappening = false;
    }
    public float GetMeltingPoint()
    {
        float meltingPointAvg = 0f;
        for (int i = 0; i < solutionMakeup.Count; i++){
            meltingPointAvg += solutionMakeup[i] * meltingPoints_K[i];
        }

        //float molality = 0f;
        //List<float> Mols = Enumerable.Repeat(0f, 11).ToList();
        //float soluteMols = 0f;
        //float solutionMols = 0f;
        //// Convert percentages to moles for reactants
        //for (int i = 0; i < Mols.Count; i++)
        //{ 
        //    Mols[i] = solutionMakeup[i] * densityOfLiquid / molarMasses[i] * 1000;
        //    if (compoundStates[i] == 'l' || compoundStates[i] == 'a'){
        //        solutionMols += Mols[i];
        //    }
        //    else{
        //        soluteMols += Mols[i];
        //    }
        //}
        //molality = soluteMols / solutionMols;
//
        //float freezingPointDep = 2 * meltingPointAvg * molality;

        float meltingPoint = meltingPointAvg;
        
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