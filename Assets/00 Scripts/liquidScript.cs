using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class liquidScript : MonoBehaviour
{
    public float totalVolume_mL;
    public float currentVolume_mL;
    public float scaleDown = 1f;
    public Color surfaceColor;
    public Color topColor;

    [Header("Wobble")]
    public float MaxWobble = 0.0003f; float initialMax;
    public float WobbleSpeed = 0.9f;
    public float Recovery = 1.4f;
    
    


    float wobbleAmountX; float wobbleAmountToAddX;
    float wobbleAmountZ; float wobbleAmountToAddZ;
    float pulse;
    Renderer rend;
    Vector3 lastPos;
    Vector3 velocity;
    float time = 0.5f;
    
    // Use this for initialization
    void Start()
    {
        rend = transform.Find("Liquid").GetComponent<Renderer>();
        initialMax = MaxWobble;
    }
    private void Update()
    {
        handleLiquid();

        handleWobble();
        
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


    }



    void handleWobble(){
        time += Time.deltaTime;

        // decrease wobble over time
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * (Recovery));
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * (Recovery));
 
        // make a sine wave of the decreasing wobble
        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);
 
        // send it to the shader
        rend.material.SetFloat("_WobbleX", wobbleAmountX);
        rend.material.SetFloat("_WobbleZ", wobbleAmountZ);
 
        // velocity
        velocity = (lastPos - transform.position) / Time.deltaTime;
        float percentFull = currentVolume_mL / totalVolume_mL;
        MaxWobble = initialMax * (-Mathf.Pow(2*percentFull-1, 2)+1);
 
        // add clamped velocity to wobble
        wobbleAmountToAddX += Mathf.Clamp((velocity.x) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z) * MaxWobble, -MaxWobble, MaxWobble);
 
        // keep last position
        lastPos = transform.position;
    }
 
 
}