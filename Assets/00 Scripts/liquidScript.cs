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

    void handleLiquid(){

        // Liquid Colors
        rend.material.SetColor("_SideColor", surfaceColor);
        rend.material.SetColor("_TopColor", topColor);
        
        // Liquid Volume
        currentVolume_mL = Mathf.Clamp(currentVolume_mL, 0f, totalVolume_mL);

        float percentFull = currentVolume_mL / totalVolume_mL;
        float scaledPercentRender = percentFull / scaleDown;
        
        // Now differentiate between flasks beakers and pipettes

        if (transform.name == "Beaker" || transform.name == "Paper Cone" || transform.name == "graduated Cylinder" || transform.name == "Ice Bucket"){  // 1 to 1 in this case
            
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
            if (H2Released > 0.1f && !exploded){
                exploded = true;
                explode(); 
                Debug.Log("Boom");
                for (int i = 0; i < 5; i++){
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
    else{
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
        float coolingLoss = ambientCoolingRate * beakerSurfaceArea * (liquidTemperature - roomTemp) * 1000;
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
            //Debug.LogWarning("Filtering stopped: Solution too low to continue.");
            return;
        }

        // Ensure we don't try to filter more than available
        if (volume > currentVolume_mL)
        {
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
                if (percentKOH > 0.02f && percentH2O > 0.06f){
                    List<string> reactants = new List<string> {"Al", "KOH", "H2O"};
                    List<string> products = new List<string> {"KAl(OH)4"};
                    List<float> Rratio = new List<float> {2, 2, 6};
                    List<float> Pratio = new List<float> {2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, .1f, "H2", 3));
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
                    StartCoroutine(react(reactants, Rratio, products, Pratio, .1f, "H2", 3));
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
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 5f)); // moderate heat generation
                }
                //GOAL PRODUCT
                //tested and working
                //split up into steps
                //if h2so4 goes in to fast solid will clump and reaction will slow significanlty
                //hat solves this problem or mixing
                if (percentKAlOH4 > 0.03f){
                    // forms alum as white crystals on the top of the solution
                    List<string> reactants = new List<string> {"H2SO4", "KAl(OH)4"};
                    List<string> products = new List<string> {"Alum"};
                    List<float> Rratio = new List<float> {1, 2};
                    List<float> Pratio = new List<float> {1};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 6f)); // moderate heat, solid white precipitate
                }
                // Potassium aluminate + sulfuric acid
                // Produces potassium alum (KAl(SO4)2) and water (H2O)
                // Fast, produces crystals as the solution cools
                //
                if (percentKAlO2 > 0.02f){
                    // Reaction: Potassium aluminate (KAlO2) + Sulfuric acid (H2SO4)
                    // Produces potassium alum (KAl(SO4)2) and water (H2O)
                    List<string> reactants = new List<string> {"H2SO4", "KAlO2"};
                    List<string> products = new List<string> {"Al(OH)3", "K2SO4"};
                    List<float> Rratio = new List<float> {2, 3};
                    List<float> Pratio = new List<float> {1, 2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 10f)); // exothermic, forms crystals over time
                }
                // Reaction: Aluminum hydroxide (Al(OH)3) + Sulfuric acid (H2SO4)
                // Produces aluminum sulfate (Al2(SO4)3) and water (H2O)

                // **Physical Manifestations:**
                // - **Exothermic:** Generates moderate heat, causing a noticeable temperature rise in the solution.
                // - **Dissolution:** The gelatinous, white Al(OH)3 precipitate dissolves upon contact with the acid.
                // - **Clarity Change:** Initially cloudy solution becomes clear as Al(OH)3 dissolves.
                // - **Reaction Speed:** Moderate (3/30 scale), occurs within seconds to minutes
                if (percentAlOH3 > 0.02f){
                    List<string> reactants = new List<string> {"Al(OH)3", "H2SO4"};
                    List<string> products = new List<string> {"Al2(SO4)3", "H2O"};
                    List<float> Rratio = new List<float> {2, 3};
                    List<float> Pratio = new List<float> {1, 6};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 3f)); 
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
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 15f)); 
                }
                //aloh3 is a gel like white precipitate which forms almost immediately
                //more dilute -> slower
                if (percentKAlSO42 > 0.02f){
                    List<string> reactants = new List<string> {"KOH", "KAl(SO4)2"};
                    List<string> products = new List<string> {"K2SO4", "Al(OH)3"};
                    List<float> Rratio = new List<float> {3, 1};
                    List<float> Pratio = new List<float> {2, 1};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 6f)); 
                }
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
                   StartCoroutine(react(reactants, Rratio, products, Pratio, 8f));
               }
            }
            if (percentAl2SO43 > 0.02f){
                if (percentKOH > 0.02f){
                    List<string> reactants = new List<string> {"Al2(SO4)3", "KOH"};
                    List<string> products = new List<string> {"Al(OH)3", "K2SO4"};
                    List<float> Rratio = new List<float> {1, 6};
                    List<float> Pratio = new List<float> {2, 3};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 8f));
                }
            }
        }
    }

    IEnumerator react(List<string> reactants, List<float> Rratio, List<string> products, List<float> Pratio, float reactSpeed, string gasType = "none", int gasMols = 0)
    {
        reactionHappening = true;
        limreactnum = 1f;

        // Gradually process the reaction
        while (limreactnum > 0.01f)
        {
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