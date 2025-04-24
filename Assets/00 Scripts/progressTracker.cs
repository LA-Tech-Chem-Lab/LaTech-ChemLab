using System.Collections;
using Unity.VisualScripting;

//using Tripolygon.UModeler.UI.Controls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LabProgress : MonoBehaviour
{
    // Current state of the lab progress
    public enum LabState
    {
        safetyCheck, 
        Step1, 
        Step2, 
        Step3, 
        Step4, 
        Step5, 
        Step6, 
        Step7, 
        Step8,
        Step9, 
        Step10,
        Finished
    }
    private LabState currentState;
    public Button nextButton;
    private bool nextButtonClicked = false; 
    public GameObject popUpPanel;
    public TextMeshProUGUI content;
    public GameObject player;
    public GameObject scrollContent;
    public GameObject step1Erlenmeyer; 
    public GameObject Pause;
    public GameObject InGame;

    // Initialize the first state
    private void Start()
    {
        GameObject Canvases = GameObject.Find("Canvases");
        InGame = Canvases.transform.Find("In Game Canvas").gameObject;
        popUpPanel = InGame.transform.Find("Pop Up Panel").gameObject;
        nextButton = popUpPanel.transform.Find("Next Button").GetComponent<Button>();
        nextButton.onClick.AddListener(nextButtonClick);
        content = popUpPanel.transform.Find("content").GetComponent<TextMeshProUGUI>();
        player = GameObject.Find("Player");
        Pause = Canvases.transform.Find("Pause Canvas").gameObject;
        GameObject Notebook = Pause.transform.Find("Notebook").gameObject;
        GameObject Scroll = Notebook.transform.Find("Scroll View").gameObject;
        GameObject View = Scroll.transform.Find("Viewport").gameObject;
        scrollContent = View.transform.Find("Content").gameObject;

        currentState = LabState.safetyCheck;
        DisplayCurrentState();
        StartCoroutine(Intro());
    }

    // Transition to the next state
    public void TransitionToNextState()
    {
        switch (currentState)
        {
            case LabState.safetyCheck:
                GameObject title = scrollContent.transform.Find("Safety Check").gameObject;
                GameObject check = title.transform.Find("Check").gameObject;
                check.SetActive(true);
                StartCoroutine(Step1());
                currentState = LabState.Step1;
                break;

            case LabState.Step1:
                GameObject title1 = scrollContent.transform.Find("Step 1 Title").gameObject;
                GameObject check1 = title1.transform.Find("Check").gameObject;
                check1.SetActive(true);
                StartCoroutine(Step2());
                currentState = LabState.Step2;
                break;

            case LabState.Step2:
                GameObject title2 = scrollContent.transform.Find("Step 2 Title").gameObject;
                GameObject check2 = title2.transform.Find("Check").gameObject;
                check2.SetActive(true);
                StartCoroutine(Step3());
                currentState = LabState.Step3;
                break;

            case LabState.Step3:
                GameObject title3 = scrollContent.transform.Find("Step 3 Title").gameObject;
                GameObject check3 = title3.transform.Find("Check").gameObject;
                check3.SetActive(true);
                StartCoroutine(Step4());
                currentState = LabState.Step4;
                break;

            case LabState.Step4:
                GameObject title4 = scrollContent.transform.Find("Step 4 Title").gameObject;
                GameObject check4 = title4.transform.Find("Check").gameObject;
                check4.SetActive(true);
                StartCoroutine(Step5());
                currentState = LabState.Step5;
                break;

            case LabState.Step5:
                GameObject title5 = scrollContent.transform.Find("Step 5 Title").gameObject;
                GameObject check5 = title5.transform.Find("Check").gameObject;
                check5.SetActive(true);
                StartCoroutine(Step6());
                currentState = LabState.Step6;
                break;

            case LabState.Step6:
                GameObject title6 = scrollContent.transform.Find("Step 6 Title").gameObject;
                GameObject check6 = title6.transform.Find("Check").gameObject;
                check6.SetActive(true);
                StartCoroutine(Step7());
                currentState = LabState.Step7;
                break;

            case LabState.Step7:
                GameObject title7 = scrollContent.transform.Find("Step 7 Title").gameObject;
                GameObject check7 = title7.transform.Find("Check").gameObject;
                check7.SetActive(true);
                currentState = LabState.Step8;
                break;

            case LabState.Step8:
                GameObject title8 = scrollContent.transform.Find("Step 8 Title").gameObject;
                GameObject check8 = title8.transform.Find("Check").gameObject;
                check8.SetActive(true);
                currentState = LabState.Step9;
                break;

            case LabState.Step9:
                GameObject title9 = scrollContent.transform.Find("Step 9 Title").gameObject;
                GameObject check9 = title9.transform.Find("Check").gameObject;
                check9.SetActive(true);
                currentState = LabState.Step10;
                break;

            case LabState.Step10:
                GameObject title10 = scrollContent.transform.Find("Step 10 Title").gameObject;
                GameObject check10 = title10.transform.Find("Check").gameObject;
                check10.SetActive(true);
                currentState = LabState.Finished;
                break;

            case LabState.Finished:
                Debug.Log("Lab is complete!");
                return; // Do not transition further
        }

        DisplayCurrentState();
    }

    // Display the current state in the Unity Console
    private void DisplayCurrentState()
    {
        Debug.Log($"Current State: {currentState}");
        // You can also update UI elements here based on the state, such as changing text or images
    }

    void Update()
    {
        PerformStateActions();
        if (Pause.activeInHierarchy || popUpPanel.activeInHierarchy){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else{
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // You could implement additional methods to perform actions in each state
    private void PerformStateActions()
    {
        switch (currentState)
        {
            //check if safety goggles have been donned
            case LabState.safetyCheck:
                if (player.GetComponent<interactWithObjects>().gogglesOn){
                    TransitionToNextState();
                }
                break;

            //check if 1 g of aluminum has been placed alone in a 250 mL Erlenmeyer
            case LabState.Step1:
                GameObject[] liquidHolders = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders)
                {
                    if (obj.transform.name.StartsWith("Erlenmeyer Flask 250")){
                        if (obj.GetComponent<liquidScript>().currentVolume_mL < 0.375f && obj.GetComponent<liquidScript>().currentVolume_mL > 0.365f && obj.GetComponent<liquidScript>().percentAl >= 0.95f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                break;

            case LabState.Step2:
                GameObject[] liquidHolders1 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders1)
                {
                    if (obj.transform.name.StartsWith("Erlenmeyer Flask 250")){
                        if(obj.GetComponent<liquidScript>().currentVolume_mL > 25f && obj.GetComponent<liquidScript>().currentVolume_mL < 26f && obj.GetComponent<liquidScript>().percentAl > 0.2f && obj.GetComponent<liquidScript>().percentAl < 0.25f && obj.GetComponent<liquidScript>().percentKOH > 0.1f && obj.GetComponent<liquidScript>().percentKOH < 0.15f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                
                break;

            case LabState.Step3:
                GameObject[] liquidHolders2 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders2)
                {
                    if (obj.transform.name.StartsWith("Erlenmeyer Flask 250")){
                        if (obj.GetComponent<liquidScript>().liquidTemperature > 343.15f && obj.GetComponent<liquidScript>().currentVolume_mL > 24.5f && obj.GetComponent<liquidScript>().percentKAlOH4 > 0.36f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                break;

            case LabState.Step4:
                GameObject[] liquidHolders3 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders3)
                {
                    if (obj.transform.name.StartsWith("Erlenmeyer Flask")){
                        if (obj.GetComponent<liquidScript>().liquidTemperature <= 295.15f && obj.GetComponent<liquidScript>().currentVolume_mL > 20f && obj.GetComponent<liquidScript>().percentKAlOH4 > 0.43f && obj.GetComponent<liquidScript>().percentAl <= 0.01f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                break;

            case LabState.Step5:
                GameObject[] liquidHolders4 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders4)
                {
                    if (obj.GetComponent<liquidScript>().currentVolume_mL > 45f && obj.GetComponent<liquidScript>().percentKAlSO42 > 0.3f){ //&& player.GetComponent<doCertainThingWith>().beginStirring add this when stirring is good to go
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step6:
                GameObject[] liquidHolders5 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders5)
                {
                    if (obj.GetComponent<liquidScript>().liquidTemperature > 330f && obj.GetComponent<liquidScript>().currentVolume_mL > 45f && obj.GetComponent<liquidScript>().percentKAlSO42 > 0.3f){ 
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step7:
                GameObject[] liquidHolders6 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders6)
                {
                    if (obj.GetComponent<liquidScript>().liquidPercent > 0.95f && obj.GetComponent<liquidScript>().currentVolume_mL > 45f && obj.GetComponent<liquidScript>().percentKAlSO42 > 0.3f){ 
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step8:
                break;

            case LabState.Step9:
                break;

            case LabState.Step10:
                break;

            case LabState.Finished:
                // Mark the lab as completed, maybe show results or feedback
                break;
        }
    }

    IEnumerator Intro(){
        // give them a couple of seconds to view the lab
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Hello There! My name is Walter. Welcome to the Synthesis of Alum Lab! If you have any questions, please press T on your keyboard and I would be happy to assist you.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "You can use WASD or the arrow keys to move throughout the lab. You can use left click to pick up or drop objects or right click to use the objects in your hand. If you press tab or escape, you will be able to veiw the menu where you can check out your lab book, settings, ask questions or exit the game.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Check that you are wearing proper atire and that you remove any rings on your fingers. Before starting any lab, make sure that you put on your safety goggles! You can find them hanging on the wall in the lab.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step1(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Congratulations! Your eyes are now protected from hazardous chemicals. Now to begin the experiment. First, you will need to measure out 1 gram of aluminum pellets using the scoopula, weight boat and scale. Don't forget to tere the weigh boat first so that you can tell how much aluminum you are weighing out.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step2(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        float aluminumVol = step1Erlenmeyer.GetComponent<liquidScript>().currentVolume_mL;
        float aluminumGrams = aluminumVol * 2.7f;
        content.text = "Nice Work! You have measured out " + aluminumGrams + " grams of aluminum into the 250 mL Erlenmeyer Flask. Record this number for later use. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "The next step is to measure out 25 mL of potassium hydroxide or KOH into the flask containing the aluminum. The KOH can be found in one of the hoods. You can pick up the beakers and hold right click to see what is in them.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "In order to measure the correct amount of potassium hydroxide (KOH), you can use the graduated cylinder. This is a tool used for measuring precise volumes of liquid. While holding the graduated cylinder, you can hold down right click to view the precice volume of its contents. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Once you have poured an approximate volume from the KOH beaker into the graduated cylinder, you can use the pipette to move small amounts of liquid and get the exact volume that you want. Then you can record this volume and pour its contents into the flask containing the aluminum. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "CAUTION: KOH is a caustic material and is harmful to skin! Immediately rinse affected area with plenty of water if skin comes in contact with KOH.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step3()
    {
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        Debug.Log(step1Erlenmeyer.transform.name);
        float KOHvol = (step1Erlenmeyer.GetComponent<liquidScript>().percentKOH + step1Erlenmeyer.GetComponent<liquidScript>().percentH2O + step1Erlenmeyer.GetComponent<liquidScript>().percentKAlOH4) * step1Erlenmeyer.GetComponent<liquidScript>().currentVolume_mL + 5f;
        content.text = "Awesome! You have measured out " + KOHvol + " mL of potassium hydroxide (KOH) into the 250 mL Erlenmeyer Flask with the aluminum. Record this number for later use. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Now it's time to let it react. In the IESB labs, this would take around 15 minutes, but time moves differently here. Let it react for a few minutes and then use a bunsen burner to raise the activation energy of the reactants and drive the remainder of the reaction forwards. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "WARNING: This is an exothermic reaction meaning that it produces heat. This may cause the glass to be hot. You may want to use tongs to transport it from place to place. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "CAUTION: Hydrogen Gas (H2) is evolved in this reaction. This gas is highly flamable and can cause fires and explosions. You should allow this reaction to take place under the vents. Manipulate the vents so that they are in the desired position and then use the lever handle to turn them on. This will evacuate the Hydrogen gas (H2). Make sure you are also using the vents anytime that you are using the bunsen burner. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step4(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Great! The reaction has proceded as expected. Now it's time to filter out the remaining solid waste. When you are trying to filter out solids to get a liquid product, gravity filtering is the prefered method. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "In order to use the gravity filter, you insert the glass funnel into an empty Erlenmeyer Flask. Then you get the paper filter cone and insert it into the funnel/flask combination. From here, you can slowly pour or pipette the solution that you wish to filter into the gravity filter you jsut assembled. After you are finished filtering, you can take the paper cone and put it in the trash and remove the glass funnel. The solution remaining in the flask below is a filtered liquid solution. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Be careful of hot glassware. Make sure that you allow the solution to cool before attempting to use the filter. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step5(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Good work! The filtering looks like it went well. Next, you need to add 30 mL of Sulfuric Acid (H2SO4) to the solution. Use the stir rod to accelerate and ensure a complete reaction. Since the flasks are too narrow for stirring, you may want to transfer the solution to a beaker first.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step6(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Nice Job! The reaction proceeded as expected. As a precaution, gently heat the reaction to ensure that the Aluminum Hydroxide is dissolved.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step7(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "You're really in your element! It looks like there may be some solid impurities in your solution. Use the gravity filter again with a clean peice of filter paper to remove these impurities.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    private void nextButtonClick()
    {
        // Set the flag to true when the button is clicked
        nextButtonClicked = true;
    }
}