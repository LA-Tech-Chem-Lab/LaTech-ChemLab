using System.Collections;
using System.Collections.Generic;
using Tripolygon.UModeler.UI.Input;
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
    public List<string> compoundNames = new List<string> {"H2SO4", "KOH", "H2O", "K2SO4"};
    public List<float> solutionMakeup = new List<float>();
    List<float> densities = new List<float> {1.83f, 2.12f, 1f, 2.66f};
    List<float> molarMasses = new List<float> {98.079f, 56.1056f, 18.01528f, 174.259f};
    List<Color> liquidColors = new List<Color> {Color.red, Color.green, Color.blue, Color.yellow};


    
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
        solutionMakeup.AddRange(new float[] { percentH2SO4, percentKOH , percentH2O, percentK2SO4});
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
        


    }

    //adds a vollume of a given solution to the current solution
    public void addSolution(List<float> solutionToAdd, float volume){
        for (int i = 0; i < solutionMakeup.Count; i++){
            //formula for computing new percentage of each substance
            solutionMakeup[i] = ((solutionMakeup[i] * currentVolume_mL) + (solutionToAdd[i] * volume)) / (currentVolume_mL + volume);
            updatePercentages();
          //  handleReactions();
        }
    }

    public void updatePercentages(){
        percentH2SO4 = solutionMakeup[0];
        percentKOH = solutionMakeup[1];
        percentH2O = solutionMakeup[2];
        percentK2SO4 = solutionMakeup[3];

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
        float molH2SO4 = percentH2SO4 * currentVolume_mL * densityOfLiquid / molarMasses[compoundNames.IndexOf("H2SO4")];
        float molKOH = percentKOH * currentVolume_mL * densityOfLiquid / molarMasses[compoundNames.IndexOf("KOH")];
        float molK2SO4 = percentK2SO4 * currentVolume_mL * densityOfLiquid / molarMasses[compoundNames.IndexOf("K2SO4")];
        float molH2O = percentH2O * currentVolume_mL * densityOfLiquid / molarMasses[compoundNames.IndexOf("H2O")];

        float H2SO4Molarity = percentH2SO4 * densityOfLiquid / molarMasses[compoundNames.IndexOf("H2S04")];
        float KOHMolarity = percentKOH * densityOfLiquid / molarMasses[compoundNames.IndexOf("KOH")];

        //checks to see if KOH and H2SO4 are in safe concentrations to proceed
        //if (((KOHMolarity <= 1) && (H2SO4Molarity <= 1)) && ((KOHMolarity >= 0.1) && (H2SO4Molarity >= 0.1))){
            float limitingReactNum = Mathf.Min(molH2SO4 / 1, molKOH / 2);
            molH2SO4 -= 1 * limitingReactNum;
            molKOH -= 2 * limitingReactNum;
            molK2SO4 += 1 * limitingReactNum;
            molH2O += 2 * limitingReactNum;
            
            solutionMakeup[0] = molH2SO4 / currentVolume_mL / densityOfLiquid;
            solutionMakeup[1] = molKOH / currentVolume_mL / densityOfLiquid;
            solutionMakeup[2] = molH2O / currentVolume_mL / densityOfLiquid;
            solutionMakeup[3] = molK2SO4 / currentVolume_mL / densityOfLiquid;

            updatePercentages();
        
    }
 
}