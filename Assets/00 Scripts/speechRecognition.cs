using OpenAI;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Whisper
{
    public class speechRecognition : MonoBehaviour
    {
        public bool enabled;


        private readonly string fileName = "output.wav";
        private readonly int duration = 13;
        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();
        private int sampleWindow = 64;
        public float micSensitivity = 0.0002f;
        public float timeInSilence;
        private bool inSilence = false;
        public float silenceTimeOut = 2f;
        public bool hasSpokenYet = false;
        public ChatGPTManager cgpt;

        [Header("UI")]
        [SerializeField] private Dropdown dropdown;










        private void Start()
        {

            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
            #endif
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        private void StartRecording()
        {
            isRecording = true;
            hasSpokenYet = false;

            var index = PlayerPrefs.GetInt("user-mic-device-index");

            #if !UNITY_WEBGL
            // Initialize the audio clip from the selected microphone
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
            #endif

            // Ensure clip is not null
            if (clip == null)
            {
                Debug.LogError("Failed to start microphone recording.");
                return;
            }
        }

        public float getLoudnessFromMicrophone()
        {
            // Ensure the clip is not null before proceeding
            if (clip == null)
            {
                Debug.LogError("Microphone clip is not initialized.");
                return 0f; // Return a default value to prevent errors
            }

            // Get the current microphone position and compute loudness
            return getLoudnessFromAudioClip(Microphone.GetPosition(Microphone.devices[0]), clip);
        }

        public float getLoudnessFromAudioClip(int clipPosition, AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("Audio clip is null in getLoudnessFromAudioClip.");
                return 0f; // Return default value
            }

            int startPos = clipPosition - sampleWindow;

            // Ensure we are not trying to read outside the bounds of the clip
            if (startPos < 0)
            {
                startPos = 0; // Adjust the starting position to ensure valid access
            }

            float[] waveData = new float[sampleWindow];
            clip.GetData(waveData, startPos);

            float totalLoudness = 0f;
            for (int i = 0; i < sampleWindow; i++)
            {
                totalLoudness += Mathf.Abs(waveData[i]);
            }

            return totalLoudness / sampleWindow;
        }

        private async void EndRecording()
        {   
            inSilence = false;
            Debug.Log("Transcripting...");

            #if !UNITY_WEBGL
            Microphone.End(null);
            #endif

            // Ensure the clip is not null before trying to save the data
            if (clip == null)
            {
                Debug.LogError("Failed to end recording: clip is null.");
                return;
            }

            byte[] data = SaveWav.Save(fileName, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);
    
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Debug.Log($"Sending Message to ChatGPT:   {res.Text}");
            cgpt.AskChatGPT(res.Text);
        }

        private void Update()
        {
            // Ensure microphone is recording before accessing loudness
            if (Input.GetMouseButtonDown(1) && enabled)
            {
                StartRecording();
            }


            if (isRecording)
            {
                if (clip != null){
                    float sensitivity = getLoudnessFromMicrophone();

                    if (!hasSpokenYet) hasSpokenYet = sensitivity >= micSensitivity;
                    inSilence = sensitivity < micSensitivity;
                    if (inSilence)
                        timeInSilence += Time.deltaTime;
                    else
                        timeInSilence = 0f;
                    
                    if (timeInSilence > silenceTimeOut && hasSpokenYet){
                        isRecording = false;
                        timeInSilence = 0f;
                        EndRecording();
                    }
                }

                time += Time.deltaTime;

                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }
        }


    }
}
