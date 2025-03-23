using Obi;
using UnityEngine;

public class FaucetScript : MonoBehaviour
{
    public ObiEmitter LFaucetEmitter;
    public bool FaucetCold; float coldWaterFlow = 3f;
    faucetHandleScript LFaucetColdScript;
    public bool FaucetHot; float hotWaterFlow = 3f;
    faucetHandleScript LFaucetHotScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LFaucetColdScript = transform.Find("Hinge L2").GetChild(0).GetComponent<faucetHandleScript>();
        LFaucetHotScript  = transform.Find("Hinge L1").GetChild(0).GetComponent<faucetHandleScript>();
    }

    // Update is called once per frame
    void Update()
    {
        FaucetCold = !LFaucetColdScript.faucetIsOff;
        FaucetHot  = !LFaucetHotScript.faucetIsOff;
        
        LFaucetEmitter.speed = (FaucetCold ? coldWaterFlow : 0f) + (FaucetHot ? hotWaterFlow : 0f); 
    }
}
