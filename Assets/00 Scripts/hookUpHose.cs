using UnityEngine;

public class hookUpHose : MonoBehaviour
{   

    public Transform intakeTip;
    public Transform endOfTube;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        endOfTube.position = intakeTip.position;
    }
}
