using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obi;
using Tripolygon.UModeler.UI.Input;
using Unity.Multiplayer.Center.Common;
using Unity.VisualScripting;
using UnityEngine;
 
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
    public List<float> solutionMakeup = new List<float>();
    public List<string> compoundNames = new List<string> {"H2SO4", "KOH", "H2O", "K2SO4", "Al", "KAl(OH)4"};
    List<float> densities = new List<float> {1.83f, 2.12f, 1f, 2.66f, 2.7f, 1.5f};
    List<float> molarMasses = new List<float> {98.079f, 56.1056f, 18.01528f, 174.259f, 26.982f, 134.12f};
    List<Color> liquidColors = new List<Color> {Color.red, Color.green, Color.blue, Color.yellow, new Color(0.6f, 0.6f, 0.6f), Color.yellow};
    //                                            H2SO4       KOH              H20        K2SO4              Al                  KAl(OH)4

    
    [Header("Spillage")]
    
    public float dotProduct; 
    public float maxSpillRate;
    Renderer rend;
    Rigidbody objectRigidbody;
    float initialObjectMass;

    // Use this for initialization
    void Start()
    {
        rend = transform.Find("Liquid").GetComponent<Renderer>();
        objectRigidbody = GetComponent<Rigidbody>();
        initialObjectMass = objectRigidbody.mass;
        solutionMakeup.AddRange(new float[] { percentH2SO4, percentKOH , percentH2O, percentK2SO4, percentAl, percentKAlOH4});
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
    solutionMakeup[2] += error; // Adjust the first element to compensate for rounding
    updatePercentages();
    handleReactions();
}

    public void updatePercentages(){
        percentH2SO4 = solutionMakeup[0];
        percentKOH = solutionMakeup[1];
        percentH2O = solutionMakeup[2];
        percentK2SO4 = solutionMakeup[3];
        percentAl = solutionMakeup[4];
        percentKAlOH4 = solutionMakeup[5];

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
        if (percentH2SO4 > 0.02f && percentKOH > 0.02f && percentH2O > 0.2f){
            List<string> reactants = new List<string> {"H2SO4", "KOH"};
            List<string> products = new List<string> {"K2SO4", "H2O"};
            List<float> Rratio = new List<float> {1, 2};
            List<float> Pratio = new List<float> {1, 2};
            StartCoroutine(react(reactants, Rratio, products, Pratio, 3f));
        }
        if (percentAl > 0.02f && percentKOH > 0.02f && percentH2O > 0.1f){
            List<string> reactants = new List<string> {"Al", "KOH", "H2O"};
            List<string> products = new List<string> {"KAl(OH)4"};
            List<float> Rratio = new List<float> {2, 2, 6};
            List<float> Pratio = new List<float> {2};
            StartCoroutine(react(reactants, Rratio, products, Pratio, 20f));
        }
    }

IEnumerator react(List<string> reactants, List<float> Rratio, List<string> products, List<float> Pratio, float reactSpeed)
{
    // Convert percentages to mols for reactants and products
    Debug.Log("starting reaction");
    List<float> reactantMols = new List<float>();
    List<float> productMols = new List<float>();
    
    for (int i = 0; i < reactants.Count; i++)
    {
        reactantMols.Add(solutionMakeup[compoundNames.IndexOf(reactants[i])] * currentVolume_mL * densityOfLiquid / molarMasses[compoundNames.IndexOf(reactants[i])]);
    }
    for (int i = 0; i < products.Count; i++)
    {
        productMols.Add(solutionMakeup[compoundNames.IndexOf(products[i])] * currentVolume_mL * densityOfLiquid / molarMasses[compoundNames.IndexOf(products[i])]);
    }

    // Find the limiting reactant
    List<float> limreactfinder = new List<float>();
    for (int i = 0; i < reactants.Count; i++)
    {
        limreactfinder.Add(reactantMols[i] / Rratio[i]);
    }
    float limreactnum = limreactfinder.Min();

    // Check for size mismatches
    if (reactantMols.Count != Rratio.Count)
    {
        Debug.LogError("Mismatch: reactantMols.Count != Rratio.Count");
        yield break;  // Exit coroutine if there's an error
    }
    if (productMols.Count != Pratio.Count)
    {
        Debug.LogError("Mismatch: productMols.Count != Pratio.Count");
        yield break;
    }

    // Gradually process the reaction
    float progress = 0f;
    while (progress < 1)
    {
        for (int i = 0; i < reactantMols.Count; i++)
        {
            reactantMols[i] -= Rratio[i] * limreactnum * Time.deltaTime / reactSpeed;
        }
        for (int i = 0; i < productMols.Count; i++)
        {
            productMols[i] += Pratio[i] * limreactnum * Time.deltaTime / reactSpeed;
        }
            float totalMass = 0f;

        // Convert mols to masses
        List<float> reactMasses = new List<float>();
        for (int i = 0; i < reactantMols.Count; i++)
        {
            reactMasses.Add(reactantMols[i] * molarMasses[compoundNames.IndexOf(reactants[i])]);
            totalMass += reactMasses[i];
        }
        List<float> prodMasses = new List<float>();
        for (int i = 0; i < productMols.Count; i++)
        {
            prodMasses.Add(productMols[i] * molarMasses[compoundNames.IndexOf(products[i])]);
            totalMass += prodMasses[i];
        }

        // Convert masses to percentages
        for (int i = 0; i < reactants.Count; i++)
        {
            solutionMakeup[compoundNames.IndexOf(reactants[i])] = reactMasses[i] / totalMass;
        }
        for (int i = 0; i < products.Count; i++)
        {
            solutionMakeup[compoundNames.IndexOf(products[i])] = prodMasses[i] / totalMass;
        }

        updatePercentages();
        progress += Time.deltaTime / reactSpeed;
        Debug.Log(progress);
        yield return null;  // Allow other game logic to continue
    }
}

}