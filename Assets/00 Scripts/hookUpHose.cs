using UnityEngine;

public class hookUpHose : MonoBehaviour
{   

    public Transform intakeTip;
    public Transform endOfTube;
    public bunsenBurnerScript theBunsenBurnerAttatchedHere;
    
    public ParticleSystem bunsenBurnerFlame;
    public gasHandleScript gasValve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        theBunsenBurnerAttatchedHere = intakeTip.parent.parent.parent.GetComponent<bunsenBurnerScript>();
        bunsenBurnerFlame = theBunsenBurnerAttatchedHere.transform.Find("Flame").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        endOfTube.position = intakeTip.position;
        

        if (gasValve.faucetIsOff){
            theBunsenBurnerAttatchedHere.isLit = false;
            if (bunsenBurnerFlame.isPlaying) bunsenBurnerFlame.Stop();
        }
    }
}
