using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class WeightScale : MonoBehaviour
{
    public List<GameObject> allObjectsContainingLiquids = new List<GameObject>();

    float forceToMass;
    public TextMeshProUGUI massText; 

    public float combinedForce;
    public float calculatedMass;

    public int registeredRigidbodies;
    private float tareTracker;

    Dictionary<Rigidbody, float> impulsePerRigidBody = new Dictionary<Rigidbody, float>();

    float currentDeltaTime;
    float lastDeltaTime;

    private void Awake()
    {
        forceToMass = 1f / Physics.gravity.magnitude;
    }

    void UpdateWeight()
    {
        //calculates sum of forces on scale
        registeredRigidbodies = impulsePerRigidBody.Count;
        combinedForce = 0;

        foreach (var force in impulsePerRigidBody.Values)
        {
            combinedForce += force;
        }

        calculatedMass = (float)(combinedForce * forceToMass) - tareTracker; //calculates mass from force and tares scale appropriately

        // sets scale display in grams with a max of 5 kg. 
        if (calculatedMass < 5)
        {
            massText.text = (calculatedMass * 1000).ToString("F2") + " g"; 
        }
        else 
        {
            massText.text = "ERR";
        }
    }

    private void FixedUpdate()
    {
        lastDeltaTime = currentDeltaTime;
        currentDeltaTime = Time.deltaTime;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            if (collision.transform.Find("Liquid"))
                allObjectsContainingLiquids.Add(collision.gameObject);
            


            if (impulsePerRigidBody.ContainsKey(collision.rigidbody))
                impulsePerRigidBody[collision.rigidbody] = collision.impulse.y / lastDeltaTime;
            else
                impulsePerRigidBody.Add(collision.rigidbody, collision.impulse.y / lastDeltaTime);

            UpdateWeight();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            if (impulsePerRigidBody.ContainsKey(collision.rigidbody))
                impulsePerRigidBody[collision.rigidbody] = collision.impulse.y / lastDeltaTime;
            else
                impulsePerRigidBody.Add(collision.rigidbody, collision.impulse.y / lastDeltaTime);

            UpdateWeight();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            if (collision.transform.Find("Liquid") && !allObjectsContainingLiquids.Exists(c => c.gameObject == collision.gameObject))
            {
                // If "Liquid" is found and it's not already in the list, add it.
                allObjectsContainingLiquids.Add(collision.gameObject);
            }
            else
            {
                // Remove from list if it's already present
                allObjectsContainingLiquids.Remove(collision.gameObject);
            }
            impulsePerRigidBody.Remove(collision.rigidbody);
            UpdateWeight();
        }
    }

    public void Tare()
    {
        //calculates total mass on scale
        registeredRigidbodies = impulsePerRigidBody.Count;
        combinedForce = 0;

        foreach (var force in impulsePerRigidBody.Values)
        {
            combinedForce += force;
        }

        calculatedMass = (float)(combinedForce * forceToMass);

        // updates the tare tracker
        tareTracker = calculatedMass;
        UpdateWeight();
    }

}

