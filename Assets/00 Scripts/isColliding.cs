using Obi;
using UnityEngine;

public class isColliding : MonoBehaviour
{
    public bool isCurrentlyColliding;
    public Transform lockToParent;
    private int collisionCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer != LayerMask.NameToLayer("IgnoreGround")){
            collisionCount++;
            isCurrentlyColliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionCount--;
        if (collisionCount <= 0)
        {
            isCurrentlyColliding = false;
            collisionCount = 0;
        }
    }

    void Update()
    {   
        if (lockToParent){
            transform.position = lockToParent.position;
            transform.localEulerAngles = lockToParent.localEulerAngles;
        }
    }

}