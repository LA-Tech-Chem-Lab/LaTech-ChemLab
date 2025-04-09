using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class TVScript : MonoBehaviour
{   
    public enum VideoType
    {
        NONE,
        PIPETTE,
        BUNSEN_BURNER,
        SCALE,
        PPE
    }

    public int totalVideoCount;
    public int videoIndex;
    public bool isPlaying;
    public bool paused;
    public bool playerWithinRange;

    [Header("Current Video")]
    public VideoType currentVideo;
    public VideoPlayer vidPlayer;
    
    public RenderTexture personalTexture;
    public RenderTexture personalTexture2;


    [Header("Changing Videos")]
    public string vidTitle;
    public GameObject displayCanvas;
    public GameObject changingVidUI;
    public TextMeshProUGUI currentVidText;
    public GameObject pauseIcon;
    public GameObject onIndicator;
    public GameObject offIndicator;


    [Header("Clips")]
    public VideoClip pipetteClip;
    public VideoClip bunsenBurnerClip;
    public VideoClip scaleClip;
    public VideoClip ppeClip;





    GameObject player;
    float distFromPlayer;








    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        totalVideoCount = System.Enum.GetNames(typeof(VideoType)).Length - 1;
        vidPlayer.targetTexture = personalTexture;
        if (gameObject.name == "TV 2")  
            vidPlayer.targetTexture = personalTexture2;
    }


    void Update()
    {
        checkInput();
        
        vidPlayer.gameObject.SetActive(isPlaying);

        distFromPlayer = Vector3.Distance(transform.position, player.transform.position);
        playerWithinRange = distFromPlayer < 6.2f;
        changingVidUI.SetActive(playerWithinRange);

        
    }

    void checkInput(){

        if (playerWithinRange){
            if (Input.mouseScrollDelta.y == 1)
                incrementVideo();
            if (Input.mouseScrollDelta.y == -1)
                decrementVideo();

            if (Input.GetMouseButtonDown(1))
                startPlayingOrPause();
        }
    }

    void incrementVideo(){
        videoIndex ++;

        if (videoIndex > totalVideoCount)
            videoIndex = 0;

        assignNewVid();
    }

    void decrementVideo(){
        videoIndex --;

        if (videoIndex < 0)
            videoIndex = totalVideoCount;
        
        assignNewVid();
    }

    void assignNewVid(){
        isPlaying = true;
        paused = false;
        pauseIcon.SetActive(paused);

        currentVideo = (VideoType)videoIndex;
        vidTitle = currentVideo.ToString();
        currentVidText.text = (currentVideo.ToString() == "NONE") ?
                                 "-----" : currentVideo.ToString();

        displayCanvas.SetActive(currentVideo.ToString() != "NONE");
        onIndicator.SetActive(displayCanvas.activeInHierarchy);
        offIndicator.SetActive(!displayCanvas.activeInHierarchy);

        // Actually Assign the vid part
        vidPlayer.time = 0;
        if (vidTitle == "PIPETTE")
            vidPlayer.clip = pipetteClip; 
        if (vidTitle == "BUNSEN_BURNER")
            vidPlayer.clip = bunsenBurnerClip; 
        if (vidTitle == "SCALE")
            vidPlayer.clip = scaleClip; 
        if (vidTitle == "PPE")
            vidPlayer.clip = ppeClip; 
    }

    void startPlayingOrPause(){
        
        if (!isPlaying)
            isPlaying = true;
        
        else {
            paused = !paused;

            if (paused)
                vidPlayer.Pause();
            else 
                vidPlayer.Play();
        }
        pauseIcon.SetActive(paused);

        if (vidTitle == "NONE"){
            isPlaying = false;
            paused = false;
        }
    }
}
