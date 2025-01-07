using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class teacherScript : NetworkBehaviour
{      
    
    public bool IsWeirdWalter;
    public GameObject[] playerList = new GameObject[2];

    public bool playerNearby = false;
    public float timeAlone; 
    float timeOutTime = 3f; // was 15
    Animator teacherAnimator;
    Transform lookAt; float lookSpeed = 5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teacherAnimator = GetComponent<Animator>();
        if (IsWeirdWalter){
            var p = transform.position; p.y = 0f; transform.position = p;
        }
        lookAt = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        playerList = GameObject.FindGameObjectsWithTag("Player");

        checkPlayersWithinRange();

        if (playerNearby) lookAt.position = Vector3.Slerp(lookAt.position, findClosestPlayer().GetChild(0).position, Time.deltaTime * lookSpeed);
        else lookAt.localPosition = Vector3.Slerp(lookAt.localPosition, new Vector3(0f, 1.5f, 1f), Time.deltaTime * lookSpeed);
        // lookAtClosestPlayer();
    }

    void checkPlayersWithinRange(){
        playerNearby = false;

        foreach (GameObject player in playerList){
            if (Vector3.Distance(transform.position, player.transform.position) < 6.5f){
                playerNearby = true;

                if (timeAlone > timeOutTime){
                    WaveToPlayer();
                }
            }    
        }

        if (playerNearby)
            timeAlone = 0f;
        else
            timeAlone += Time.deltaTime;
        
    }

    void lookAtClosestPlayer(){
        transform.LookAt(findClosestPlayer());
        var rot = transform.localEulerAngles; rot.x = 0f;
        transform.localEulerAngles = rot;
    }

    void WaveToPlayer(){
        
        teacherAnimator.SetTrigger("Wave");
        UpdateAnimationTriggerServerRpc("Wave");
    }

    Transform findClosestPlayer(){
        float min = 999;
        GameObject closest = null;
        foreach (GameObject player in playerList){
            if (Vector3.Distance(transform.position, player.transform.position) < min){
                closest = player; min = Vector3.Distance(transform.position, player.transform.position);
            }    
        }
        return closest.transform;
    }

    [ServerRpc]
    private void UpdateAnimationTriggerServerRpc(string parameter)
    {
        // Set the trigger on the server-side Animator
        teacherAnimator.SetTrigger(parameter);

        // Propagate the trigger to all other clients
        UpdateAnimationTriggerClientRpc(parameter);
    }

    [ClientRpc]
    private void UpdateAnimationTriggerClientRpc(string parameter)
        {
            if (!IsOwner) // Prevent re-triggering on the owning client
            {
                teacherAnimator.SetTrigger(parameter);
            }
        }
}
