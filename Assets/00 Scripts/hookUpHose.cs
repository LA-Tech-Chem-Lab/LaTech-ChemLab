using UnityEngine;

public class hookUpHose : MonoBehaviour
{   

    public Transform intakeTip;
    public Transform endOfTube;
    public Transform theBunsenBurnerAttatchedHere;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // theBunsenBurnerAttatchedHere = GetComponentInParent<bunsenBurnerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        endOfTube.position = intakeTip.position;
    }
}
