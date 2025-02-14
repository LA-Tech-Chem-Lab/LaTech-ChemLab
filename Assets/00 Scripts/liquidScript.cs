using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Obi;
using Tripolygon.UModeler.UI.Input;
using Unity.Mathematics;
using Unity.Multiplayer.Center.Common;
using Unity.VisualScripting;
using UnityEngine;
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
    List<float> densities = new List<float> {1.83f, 2.12f, 1f, 2.66f, 2.7f, 1.5f, 2.672f, 1.76f, 2.42f, 1.75f, 1.57f};
    public List<float> molarMasses = new List<float> {98.079f, 56.1056f, 18.01528f, 174.259f, 26.982f, 134.12f, 342.14f, 474.39f, 78f, 258.42f, 98.075f};
    List<Color> liquidColors = new List<Color> {Color.red, Color.green, Color.blue, Color.yellow, new Color(0.6f, 0.6f, 0.6f), Color.yellow, Color.red, Color.green, Color.yellow, Color.green, Color.blue};
    //                                            H2SO4       KOH              H20        K2SO4              Al                  KAl(OH)4
    public bool reactionHappening;

    
    [Header("Spillage")]
    
    public float dotProduct; 
    public float maxSpillRate;
    Renderer rend;
    Rigidbody objectRigidbody;
    float initialObjectMass;

    [Header("Liquid Heating")]
    public float maxHeat = 100f;  // Maximum heat at the center
    public float currentHeat = 0f; // Heat affecting the beaker

    // Use this for initialization
    void Start()
    {
        rend = transform.Find("Liquid").GetComponent<Renderer>();
        objectRigidbody = GetComponent<Rigidbody>();
        initialObjectMass = objectRigidbody.mass;
        solutionMakeup.AddRange(new float[] { percentH2SO4, percentKOH , percentH2O, percentK2SO4, percentAl, percentKAlOH4, percentAl2SO43, percentAlum, percentAlOH3, percentKAlSO42, percentKAlO2});
        if (currentVolume_mL > 0){
            calculateDensity();
        }
        maxSpillRate = totalVolume_mL * 0.2f;
    }

    private void Update()
    {
        
        dotProduct = Vector3.Dot(transform.up.normalized, Vector3.up);
        if (dotProduct <= 0.25f){
            float loss = (-0.8f * dotProduct + 0.2f) * maxSpillRate * Time.deltaTime;
            currentVolume_mL -= loss;
            if (gameObject.name.StartsWith("Erlenmeyer") && currentVolume_mL/totalVolume_mL < 0.45f) // Bad but works for now
                currentVolume_mL = 0f;
            if (gameObject.name.StartsWith("Beaker") && currentVolume_mL/totalVolume_mL < 0.19f) // Bad but works for now
                currentVolume_mL = 0f;
        }

        handleLiquid();

        handleLiquidColor();

        CalculateHeat();

        handleReactions();
        
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

        if (transform.name == "Beaker"){  // 1 to 1 in this case
            
            if (rend.material != null) {
                // Make sure to use the instance material, not the shared one
                rend.material.SetFloat("_FillAmount", scaledPercentRender);
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
        objectRigidbody.mass = initialObjectMass + currentVolume_mL * densityOfLiquid / 1000f;

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
    if (burner == null)
    {
        currentHeat = 0f;
        return;
    }

    Vector3 burnerPos = burner.transform.position;
    Vector3 beakerPos = transform.position;
    float heatRadius = 0.2f;

    // ✅ FIX: Use XZ plane only
    float horizontalDistance = Vector2.Distance(new Vector2(beakerPos.x, beakerPos.z), new Vector2(burnerPos.x, burnerPos.z));

    // Get vertical height difference (beaker must be above)
    float heightDifference = beakerPos.y - burnerPos.y;

    // Check if beaker is within horizontal range and burner is lit
    if (horizontalDistance <= heatRadius && heightDifference > 0 && burner.GetComponent<bunsenBurnerScript>().isLit)
    {
        float heatFactor = 1 - (horizontalDistance / heatRadius);
        currentHeat = maxHeat * heatFactor;
    }
    else
    {
        currentHeat = 0f;
    }
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
        updatePercentages();
    }

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
    }

    void calculateDensity(){
        float totalMass = 0f;
        for (int i = 0; i < densities.Count; i++){
            totalMass += densities[i] * solutionMakeup[i] * currentVolume_mL;
        }
        densityOfLiquid = totalMass / currentVolume_mL;
    }

    public void handleReactions(){
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
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 1f));
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
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 1f));
                }
            }
            if (percentH2SO4 > 0.02f){
                // produces heat
                // fast
                //tested and working
                if (percentKOH > 0.04f){
                    // Reaction: Potassium hydroxide (KOH) + Sulfuric acid (H2SO4)
                    // Produces potassium sulfate (K2SO4) and water (H2O)
                    List<string> reactants = new List<string> {"H2SO4", "KOH"};
                    List<string> products = new List<string> {"K2SO4", "H2O"};
                    List<float> Rratio = new List<float> {1, 2};
                    List<float> Pratio = new List<float> {1, 2};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 20f)); // moderate heat generation
                }
                //GOAL PRODUCT
                //tested and working
                if (percentKAlOH4 > 0.03f){
                    // forms alum as white crystals on the top of the solution
                    List<string> reactants = new List<string> {"H2SO4", "KAl(OH)4"};
                    List<string> products = new List<string> {"Alum"};
                    List<float> Rratio = new List<float> {1, 2};
                    List<float> Pratio = new List<float> {1};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 13f)); // moderate heat, solid white precipitate
                }
                // Potassium aluminate + sulfuric acid
                // Produces potassium alum (KAl(SO4)2) and water (H2O)
                // Fast, produces crystals as the solution cools
                //
                if (percentKAlO2 > 0.02f){
                    // Reaction: Potassium aluminate (KAlO2) + Sulfuric acid (H2SO4)
                    // Produces potassium alum (KAl(SO4)2) and water (H2O)
                    List<string> reactants = new List<string> {"H2SO4", "KAlO2"};
                    List<string> products = new List<string> {"KAl(SO4)2", "H2O"};
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
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 6f)); 
                }
                //aloh3 is a gel like white precipitate which forms almost immediately
                //more dilute -> slower
                if (percentKAlSO42 > 0.02f){
                    List<string> reactants = new List<string> {"KOH", "KAl(SO4)2"};
                    List<string> products = new List<string> {"K2SO4", "Al(OH)3"};
                    List<float> Rratio = new List<float> {3, 1};
                    List<float> Pratio = new List<float> {2, 1};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 30f)); 
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
                   StartCoroutine(react(reactants, Rratio, products, Pratio, 30f));
               }

               if (percentAlOH3 > 0.02f) {
                   // KAlO2 forms as a dry powder when water is removed
                   // Initially appears as a milky, gelatinous fluid before drying
                   // Highly exothermic, strong heat release
                   // Solid product forms upon evaporation, leaving behind a fine white powder
                   List<string> reactants = new List<string> {"KAl(OH)4", "Al(OH)3"};
                   List<string> products = new List<string> {"KAlO2", "H2O"};
                   List<float> Rratio = new List<float> {1, 1};
                   List<float> Pratio = new List<float> {2, 4};
                   StartCoroutine(react(reactants, Rratio, products, Pratio, 5f));
               }
            }
            if (percentAl2SO43 > 0.02f){
                if (percentKOH > 0.02f){
                    List<string> reactants = new List<string> {"Al2(SO4)3", "KOH"};
                    List<string> products = new List<string> {"Al(OH)3", "K2SO4"};
                    List<float> Rratio = new List<float> {1, 6};
                    List<float> Pratio = new List<float> {2, 3};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 5f));
                }

                if (percentKAlO2 > 0.06f && percentH2O > 0.06f){
                    List<string> reactants = new List<string> {"KAlO2", "Al2(SO4)3", "H2O"};
                    List<string> products = new List<string> {"Al(OH)3", "K2SO4", "KOH"};
                    List<float> Rratio = new List<float> {6, 1, 6};
                    List<float> Pratio = new List<float> {2, 3, 6};
                    StartCoroutine(react(reactants, Rratio, products, Pratio, 30f));
                }
            }
        }
    }

    IEnumerator react(List<string> reactants, List<float> Rratio, List<string> products, List<float> Pratio, float reactSpeed)
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

            // Convert masses to new percentages based on the current mass
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                solutionMakeup[i] = solutionMasses[i] / totalMass;
            }
        
            Debug.Log(solutionMakeup.Sum());

            // Update percentages dynamically
            updatePercentages();
            
            yield return new WaitForSeconds(1f / reactSpeed);  // Allow other game logic to continue
        }
        reactionHappening = false;
    }
}
