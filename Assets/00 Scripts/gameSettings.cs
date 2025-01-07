using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class gameSettings : MonoBehaviour
{   
    public GameObject VolumeProfiles;

    [Header("Volume Settings")]
    bool cloudsEnabled = true;
    public GameObject cloudCheck;
    bool fogEnabled = false;
    public GameObject fogCheck;
    bool terrainEnabled = true;
    
    [Header("Terrain Settings")]
    public GameObject terrain;
    public GameObject terrainCheck;
    

    public void toggleClouds(){
        cloudsEnabled = !cloudsEnabled;
        cloudCheck.SetActive(cloudsEnabled);

        DetermineVolumeProfile();
    }

    public void toggleFog(){
        fogEnabled = !fogEnabled;
        fogCheck.SetActive(fogEnabled);

        DetermineVolumeProfile();
    }

    public void toggleTerrain(){
        terrainEnabled = !terrainEnabled;
        terrain.SetActive(terrainEnabled);
        terrainCheck.SetActive(terrainEnabled);
    }







    void DetermineVolumeProfile(){

        foreach (Transform child in VolumeProfiles.transform)
        {
            child.gameObject.SetActive(false); // Disable all child GameObjects
        }

        if (!fogEnabled)
            VolumeProfiles.transform.Find("No Fog").gameObject.SetActive(true);
        
        if (!cloudsEnabled)
            VolumeProfiles.transform.Find("No Clouds").gameObject.SetActive(true);
            
    }
}
