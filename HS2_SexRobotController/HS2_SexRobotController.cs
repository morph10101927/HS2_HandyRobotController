using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIChara;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using IllusionUtility.GetUtility;

namespace HS2_HandyController
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [DefaultExecutionOrder(32000)]

    public class HS2_HandyController : BaseUnityPlugin
    {
        public const string pluginGUID = "hs2robotics.HS2SexRobotController";
        public const string pluginName = "HS2_HandyController";
        public const string pluginVersion = "1.48";

        public static HScene hScene;
        public bool inHScene = false;
        public ChaControl[] females;
        public ChaControl[] males;
        public static bool animationChanged = false;
        public static string animationName = "";
        public Transform malePenisBase;
        public Transform malePenisTip;
        public Transform malePenisLeftBall;
        public Transform malePenisRightBall;
        public Transform femaleMouth;
        public Transform femaleMouthLipsUpper;
        public Transform femaleMouthLipsLower;
        public Transform femaleMouthLeft;
        public Transform femaleMouthRight;
        public Transform femaleHip;
        public Transform femaleVagina;
        public Transform femaleAnus;
        public Transform femaleMiddleBreastsLeft;
        public Transform femaleMiddleBreastsRight;
        public Transform femaleBreasts;
        public Transform femaleMiddleFingerLeft;
        public Transform femaleRingFingerLeft;
        public Transform femaleHandLeft;
        public Transform femaleMiddleFingerRight;
        public Transform femaleRingFingerRight;
        public Transform femaleHandRight;
        public Transform femaleFootLeft;
        public Transform femaleFootRight;
        public Transform femaleToesLeft;
        public Transform femaleToesRight;
        public Transform partnerMouth;
        public Transform vibratorPoint;
        public Transform femaleLegLeft;
        public Transform femaleKneeLeft;
        public Transform femaleLegRight;
        public Transform femaleKneeRight;

        public enum femaleTargetType
        {
            VAGINAL,
            ANAL,
            ORAL,
            BREASTS,
            LEFTHAND,
            RIGHTHAND,
            INTERCRURAL,
            VAGINALSWAP,
            ORALSWAP,
            RIGHTHANDSWAP,
            INTERCRURALSWAP,
            NONE,
            KISS,
            HANDVAGINA,
            MOUTHVAGINA,
            MOUTHANUS,
            HANDBREAST,
            HANDHIP,
            VIBRATOR,
            FOOT
        }

        femaleTargetType currentFemaleTargetType;

        public Dictionary<string, femaleTargetType> animationFemaleTargetDictionary = new Dictionary<string, femaleTargetType>
        {
            // Schema: {ANIMATION NAME, FEMALE TARGET(S) TO USE IN REGARD TO MAPPING TO THE MALE'S PENIS TARGET}

            // Honey Select 2 Service HScene Category
            {"Blowjob", femaleTargetType.ORAL},
            {"Handjob", femaleTargetType.LEFTHAND},
            {"Glans Tease", femaleTargetType.ORAL},
            {"Boobjob", femaleTargetType.BREASTS},
            {"Licking Boobjob", femaleTargetType.BREASTS},
            {"Sucking Boobjob", femaleTargetType.BREASTS},
            {"Exhausted Handjob", femaleTargetType.RIGHTHAND},
            {"Exhausted Blowjob", femaleTargetType.ORAL},
            {"Standing Handjob", femaleTargetType.RIGHTHAND},
            {"No-Hands Tip Licking", femaleTargetType.ORAL},
            {"No-Hands Blowjob", femaleTargetType.ORAL},
            {"Deepthroat", femaleTargetType.ORAL},
            {"Standing Boobjob", femaleTargetType.BREASTS},
            {"Stand. Lick. Boobjob", femaleTargetType.BREASTS},
            {"Restrained Blowjob", femaleTargetType.ORAL},
            {"Forced Handjob", femaleTargetType.RIGHTHAND},
            {"Irrumatio", femaleTargetType.ORAL},
            {"Chair Handjob", femaleTargetType.LEFTHAND},
            {"Sit. No-Hand Blowjob", femaleTargetType.ORAL},
            {"Sitting Boobjob", femaleTargetType.BREASTS},
            {"Sitting Licking Boobjob", femaleTargetType.BREASTS},
            {"Wall-Trapped Blowjob", femaleTargetType.ORAL},
            {"Crouching Blowjob", femaleTargetType.ORAL},
            {"Desk Handjob", femaleTargetType.RIGHTHAND},
            {"Behind Handjob", femaleTargetType.LEFTHAND},
            {"Sleeping Boobjob", femaleTargetType.BREASTS},
            {"Wall Irramatio", femaleTargetType.ORAL},
            {"Chair Irramatio", femaleTargetType.ORAL},
            {"Pet Blowjob", femaleTargetType.ORAL},
            {"Forceful BJ", femaleTargetType.ORAL},
            {"Sitting Handjob", femaleTargetType.LEFTHAND},
            {"Titjob", femaleTargetType.BREASTS},
            {"Tip Lick (No Hands)", femaleTargetType.ORAL},
            {"Sitting Titjob", femaleTargetType.BREASTS},
            {"Stockade Femdom BJ", femaleTargetType.ORAL},
            {"Kiss", femaleTargetType.KISS},
            {"Breast Play", femaleTargetType.HANDBREAST},
            {"Fingering", femaleTargetType.HANDVAGINA},
            {"Cunnilingus", femaleTargetType.MOUTHVAGINA},
            {"Rimjob", femaleTargetType.MOUTHANUS},
            {"Anal Vibrator", femaleTargetType.VIBRATOR},
            {"Standing Kiss", femaleTargetType.KISS},
            {"Standing Cunnilingus", femaleTargetType.MOUTHVAGINA},
            {"Breast Massage (Behind)", femaleTargetType.HANDBREAST},
            {"Squatting Breast Massage", femaleTargetType.HANDBREAST},
            {"Kiss Grope", femaleTargetType.HANDBREAST},
            {"Reclining Masturbation", femaleTargetType.HANDVAGINA},
            {"Desk Spank", femaleTargetType.HANDHIP},
            {"Chair Spank", femaleTargetType.HANDHIP},
            {"All-Fours Foreplay", femaleTargetType.HANDVAGINA},
            
            // Honey Select 2 Insert HScene Category
            {"Missionary", femaleTargetType.VAGINAL},
            {"Breast Grope Missionary", femaleTargetType.VAGINAL},
            {"Doggy", femaleTargetType.VAGINAL},
            {"Kneeling Behind", femaleTargetType.VAGINAL},
            {"Spooning", femaleTargetType.VAGINAL},
            {"Cowgirl", femaleTargetType.VAGINAL},
            {"Chest Grope Cowgirl", femaleTargetType.VAGINAL},
            {"Reverse Cowgirl", femaleTargetType.VAGINAL},
            {"Piledriver Missionary", femaleTargetType.VAGINAL},
            {"Bent Missionary", femaleTargetType.VAGINAL},
            {"Double Decker", femaleTargetType.VAGINAL},
            {"Anal Missionary", femaleTargetType.ANAL},
            {"Anal Doggy", femaleTargetType.ANAL},
            {"Floor Bondage Miss.", femaleTargetType.VAGINAL},
            {"Forced Missionary", femaleTargetType.VAGINAL},
            {"Standing", femaleTargetType.VAGINAL},
            {"Standing Behind", femaleTargetType.VAGINAL},
            {"Thrust Behind", femaleTargetType.VAGINAL},
            {"Lifting", femaleTargetType.VAGINAL},
            {"Reverse Lifting", femaleTargetType.VAGINAL},
            {"Thighjob", femaleTargetType.INTERCRURAL},
            {"Stockade", femaleTargetType.VAGINAL},
            {"Wall-Facing Behind", femaleTargetType.VAGINAL},
            {"Wall-Facing Anal", femaleTargetType.ANAL},
            {"Chair Sitting Behind", femaleTargetType.VAGINAL},
            {"Anal Doggy on Chair", femaleTargetType.ANAL},
            {"Desk Missionary", femaleTargetType.VAGINAL},
            {"Desk on Side", femaleTargetType.VAGINAL},
            {"Desk Doggy", femaleTargetType.ANAL},
            {"Against Counter Behind", femaleTargetType.VAGINAL},
            {"Wall-Trapped Doggy", femaleTargetType.VAGINAL},
            {"Crouch Insertion", femaleTargetType.VAGINAL},
            {"Delivery Table Insert", femaleTargetType.VAGINAL},
            {"Hanging", femaleTargetType.VAGINAL},
            {"Tied Up Insertion", femaleTargetType.VAGINAL},
            {"Lying Doggystyle", femaleTargetType.VAGINAL},
            {"Anal Piledriver Miss.", femaleTargetType.ANAL},
            {"Face to Face Sitting", femaleTargetType.VAGINAL},
            {"Chest Grope Sit Behind", femaleTargetType.VAGINAL},
            {"Sitting Hugging", femaleTargetType.VAGINAL},
            {"Restrained Standing", femaleTargetType.VAGINAL},
            {"Clinging Lifting", femaleTargetType.VAGINAL},
            {"Wall-Pressed Behind", femaleTargetType.VAGINAL},
            {"Pet Sex", femaleTargetType.VAGINAL},
            {"Mating Press", femaleTargetType.VAGINAL},
            {"Domination Reverse Cowgirl", femaleTargetType.VAGINAL},
            {"Stockade Femdom Sex", femaleTargetType.VAGINAL},
            {"Side", femaleTargetType.VAGINAL},
            {"Standing Doggy", femaleTargetType.VAGINAL},
            {"Standing Missionary", femaleTargetType.VAGINAL},
            {"Standing Wall Doggy", femaleTargetType.VAGINAL},
            {"Table Missionary", femaleTargetType.VAGINAL},
            {"Missionary Press", femaleTargetType.VAGINAL},
            {"Counter Doggy", femaleTargetType.VAGINAL},
            {"Squatting Sex", femaleTargetType.VAGINAL},
            {"Suspended Sex", femaleTargetType.VAGINAL},
            {"Standing Tied", femaleTargetType.VAGINAL},
            {"Standing Squeeze", femaleTargetType.VAGINAL},

            // Honey Select 2 Woman-led HScene Category
            {"Nipple Licking Blowjob", femaleTargetType.RIGHTHAND},
            {"Rimjob + Handjob", femaleTargetType.RIGHTHAND},
            {"Standing Footjob", femaleTargetType.FOOT},
            {"Sitting Footjob", femaleTargetType.FOOT},
            {"All Fours Handjob", femaleTargetType.RIGHTHAND},
            {"Chair Restraint Footjob", femaleTargetType.FOOT},
            {"Face Sit Cunnilingus", femaleTargetType.MOUTHVAGINA},
            //{"Cowgirl", femaleTargetType.VAGINAL}, // Already in the Dictionary above
            //{"Reverse Cowgirl", femaleTargetType.VAGINAL}, // Already in the Dictionary above
            {"Reverse Piledriver", femaleTargetType.VAGINAL},
            {"Cowgirl Intercrural", femaleTargetType.INTERCRURAL},
            {"Handjob Intercrural", femaleTargetType.INTERCRURAL},
            {"Chair Intercrural", femaleTargetType.INTERCRURAL},
            {"Piledriver Rev. Cowgirl", femaleTargetType.VAGINAL},
            //{"Standing", femaleTargetType.VAGINAL}, // Already in the Dictionary above
            {"Anal Reverse Cowgirl", femaleTargetType.ANAL},
            {"Male Restrained Stand", femaleTargetType.VAGINAL},
            {"Chair Restraint Sitting", femaleTargetType.VAGINAL},

            // Honey Select 2 Female Group HScene Category
            {"W Cowgirl", femaleTargetType.VAGINAL},
            {"W Cowgirl (swap)", femaleTargetType.VAGINALSWAP},
            {"W Blowjob", femaleTargetType.ORAL},
            {"W Blowjob (swap)", femaleTargetType.ORALSWAP},
            {"Intercrural Sandwich", femaleTargetType.INTERCRURAL},
            {"Interc. Sandwich (swap)", femaleTargetType.INTERCRURALSWAP},
            {"Insertion + Fingering", femaleTargetType.VAGINAL},
            {"Insert. + Fing. (swap)", femaleTargetType.VAGINALSWAP},
            {"W Handjob Licking", femaleTargetType.RIGHTHANDSWAP},
            {"W Handjob Lick. (swap)", femaleTargetType.RIGHTHAND},
            {"Sitting+Cunilingus", femaleTargetType.VAGINAL},
            {"Sit+Cunilingus (swap)", femaleTargetType.VAGINALSWAP},

            // Honey Select 2 Special HScene Category
            {"69", femaleTargetType.ORAL}
        };

        private Stopwatch sw = Stopwatch.StartNew();
        private ConfigEntry<KeyboardShortcut> toggleSerialPortConnection { get; set; }
        private ConfigEntry<KeyboardShortcut> strokeLengthMultiplierIncrease { get; set; }
        private ConfigEntry<KeyboardShortcut> strokeLengthMultiplierDecrease { get; set; }
        public ConfigEntry<float> sexRobotUpdateFrequencyConfig;
        public ConfigEntry<bool> serialPortConnected;
        public ConfigEntry<bool> diagnosticsConfig;
        public ConfigEntry<string> handyConnectionKey;
        public ConfigEntry<string> handyStatus;
        public ConfigEntry<bool> handyInvertStroke;
        public ConfigEntry<float> handySendInterval;
        HandyDevice handy;
        readonly List<AnimHold> animHolds = new List<AnimHold>();
        bool loggedAnimDelay;
        string liveFolder;
        float liveRaw = -1f;
        int liveSent = -1;
        float strokeLimited = 0.5f;
        float strokeLimitAt = -1f;
        bool haveStrokeLimit;
        float strokeCenter = 0.5f;
        bool haveStrokeCenter;
        float liveWriteAt;
        float liveMaxDn;
        string surveyScene = "";
        string surveyContact = "";
        float surveyRawMin = 1f;
        float surveyRawMax = 0f;
        int surveySentMin = 100;
        int surveySentMax = 0;
        int surveySamples;
        float surveyDn;
        string liveText = "";
        readonly float[] livePrevNorm = new float[4];
        readonly bool[] liveHavePrev = new bool[4];
        bool suppressConnectionEvent;
        static HS2_HandyController instance;
        ConfigurationManagerAttributes handyConnectAttributes;
        ConfigurationManagerAttributes handyStatusAttributes;
        public ConfigEntry<float> robotL0Multiplier;
        public ConfigEntry<float> robotL0MultiplierStepValue;
        public static bool updateRobotPosition = false;
        public static bool buttonConnectRobotClicked = false;
        public static bool buttonDisconnectRobotClicked = false;
        public static bool buttonStrokeMultiplierIncreaseClicked = false;
        public static bool buttonStrokeMultiplierDecreaseClicked = false;
        public static Transform buttonConnectRobot;
        public static Transform buttonDisconnectRobot;
        public static Transform buttonStrokeMultiplierIncrease;
        public static Transform buttonStrokeMultiplierDecrease;
        public static Text buttonConnectRobotText;
        public static Text buttonDisconnectRobotText;
        public static Text buttonStrokeMultiplierIncreaseText;
        public static Text buttonStrokeMultiplierDecreaseText;

        public void Awake()
        {
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(HS2_HandyController));
            instance = this;
            handy = new HandyDevice(message => Logger.LogInfo(message));
            gameObject.AddComponent<HandyPoseClock>().Owner = this;
            liveFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BepInEx", "plugins", "HS2_HandyController", "live"));
            try
            {
                Directory.CreateDirectory(liveFolder);
                File.WriteAllText(Path.Combine(liveFolder, "status.txt"), pluginVersion + " loaded\r\n");
                File.WriteAllText(Path.Combine(liveFolder, "scenes.txt"), pluginVersion + " scene survey\r\n");
            }
            catch (Exception)
            {
            }

            // Setup config file entries used in the in game menu
            // Creates a config file in BepInEx/config named hs2robotics.HS2SexRobotController.cfg
            strokeLengthMultiplierIncrease = Config.Bind("Sex Robot Limits", "Increase Stroke Multiplier", new KeyboardShortcut(KeyCode.Z));
            strokeLengthMultiplierDecrease = Config.Bind("Sex Robot Limits", "Decrease Stroke Multiplier", new KeyboardShortcut(KeyCode.X));
            robotL0Multiplier = Config.Bind("Sex Robot Limits", "Sex Robot (L0) Stroke Multiplier", 1.0f, new ConfigDescription("Widens the stroke of the body part this scene uses. 1 keeps the measured motion. The result stays between 0 and 1.", new AcceptableValueRange<float>(0.25f, 10.0f)));
            robotL0MultiplierStepValue = Config.Bind("Sex Robot Limits", "Sex Robot (L0) Stroke Multiplier Step Value", 0.25f, new ConfigDescription("Sex Robot (L0) Stroke Multiplier Step Value", new AcceptableValueRange<float>(0.01f, 1.0f)));
            toggleSerialPortConnection = Config.Bind("Sex Robot Connection", "Connect/Disconnect Sex Robot Hotkey", new KeyboardShortcut(KeyCode.S, KeyCode.LeftShift));
            sexRobotUpdateFrequencyConfig = Config.Bind("Sex Robot Connection", "Sex Robot Update Frequency", 30.0f, new ConfigDescription("How often the scene is sampled, in updates per second.", new AcceptableValueRange<float>(1.0f, 120.0f)));
            (serialPortConnected = Config.Bind("Sex Robot Connection", "Connection To Sex Robot Via Serial Port", false, new ConfigDescription("Saves whether the Handy should connect on the next launch.", null, new ConfigurationManagerAttributes { Browsable = false }))).SettingChanged += (s, e) => { UpdateSerialPortConnection(); };
            diagnosticsConfig = Config.Bind("General", "BepInEx Debug Console Output", false);
            handyConnectionKey = Config.Bind("Handy", "Connection Key", "", new ConfigDescription("Device connection key from the Handy app. Saved in BepInEx/config and loaded the next time the game starts.", null, new ConfigurationManagerAttributes { Order = 30 }));
            handyConnectAttributes = new ConfigurationManagerAttributes { CustomDrawer = DrawHandyConnectButton, HideDefaultButton = true, Order = 20 };
            Config.Bind("Handy", "Connect", "", new ConfigDescription("Connect or disconnect the Handy.", null, handyConnectAttributes));
            handyStatusAttributes = new ConfigurationManagerAttributes { ReadOnly = true, Order = 10 };
            handyStatus = Config.Bind("Handy", "Connection Status", "Disconnected", new ConfigDescription("Shows Connected or Disconnected. Updated by the plugin.", null, handyStatusAttributes));
            handyInvertStroke = Config.Bind("Handy", "Invert Stroke", false, "Flip the stroke. Off matches the Handy: 0 is the bottom and 100 is the top, and the game's upward stroke goes to the top.");
            handySendInterval = Config.Bind("Handy", "Send Interval Seconds", 0.25f, new ConfigDescription("The Handy cloud API allows 4 updates a second. The plugin sends at that rate.", new AcceptableValueRange<float>(0.25f, 2.0f)));
            handyConnectionKey.SettingChanged += (s, e) => { OnHandyCredentialChanged(); };

            if (string.IsNullOrEmpty(handyConnectionKey.Value))
            {
                Logger.LogInfo("Handy connection key is empty. Enter it under Handy settings. It is saved for the next launch.");
            }
            else
            {
                Logger.LogInfo("Handy connection key loaded from config.");
            }

            SetConnectionStatus(serialPortConnected.Value ? "Connecting..." : "Disconnected");

            if (serialPortConnected.Value)
            {
                StartCoroutine(ConnectWhenReady());
            }
        }

        IEnumerator ConnectWhenReady()
        {
            int steady = 0;
            float last = Time.realtimeSinceStartup;
            while (steady < 45)
            {
                yield return null;
                float now = Time.realtimeSinceStartup;
                float dt = now - last;
                last = now;
                if (dt > 0.001f && dt < 0.05f)
                {
                    steady++;
                }
                else
                {
                    steady = 0;
                }
            }

            if (serialPortConnected != null && serialPortConnected.Value)
            {
                UpdateSerialPortConnection();
            }
        }

        public void OnDestroy()
        {
            if (handy != null && handy.IsConnected)
            {
                StartCoroutine(handy.Disconnect(handyConnectionKey.Value, OnHandyStatus));
            }
        }

        // Hook method to grab the HScene instance from
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HScene_SetStartVoice(HScene __instance)
        {
            hScene = __instance;
        }

        // Hook method to grab the HScene animation name from
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_PreChangeAnimation(HScene.AnimationListInfo _info)
        {
            animationChanged = true;

            animationName = _info.nameAnimation;

            updateRobotPosition = false;

            if (instance != null)
            {
                instance.ResetHandyForNewAnimation();
            }

            if (instance != null && !string.IsNullOrEmpty(animationName))
            {
                instance.Logger.LogInfo("HScene animation: " + animationName);
            }
        }

        // Hook method to inject UI buttons into the main Honey Select 2 config menu
        [HarmonyPostfix, HarmonyPatch(typeof(Config.ConfigWindow), "Initialize")]
        private static void SetupUIButtons(Config.ConfigWindow __instance, ref Button[] ___buttons)
        {
            // Get main button to instantiate in order to create our new buttons
            Transform btnTitle = __instance.transform.FindLoop("btnTitle").transform;

            // Create connect robot button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonConnectRobot = Instantiate(btnTitle, btnTitle.parent);
            buttonConnectRobot.name = "btnConnectRobot";
            buttonConnectRobotText = buttonConnectRobot.GetComponentInChildren<Text>();
            buttonConnectRobotText.text = "Connect Handy";
            buttonConnectRobotText.fontSize = 18;
            Button newButton = buttonConnectRobot.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonConnectRobotClicked = true;
            });

            // Create disconnect robot button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonDisconnectRobot = Instantiate(btnTitle, btnTitle.parent);
            buttonDisconnectRobot.name = "btnDisconnectRobot";
            buttonDisconnectRobotText = buttonDisconnectRobot.GetComponentInChildren<Text>();
            buttonDisconnectRobotText.text = "Disconnect Handy";
            buttonDisconnectRobotText.fontSize = 18;
            newButton = buttonDisconnectRobot.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonDisconnectRobotClicked = true;
            });

            // Create robot stroke multiplier increase button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonStrokeMultiplierIncrease = Instantiate(btnTitle, btnTitle.parent);
            buttonStrokeMultiplierIncrease.name = "btnStrokeMultiplierIncrease";
            buttonStrokeMultiplierIncreaseText = buttonStrokeMultiplierIncrease.GetComponentInChildren<Text>();
            buttonStrokeMultiplierIncreaseText.text = "Stroke Multiplier +";
            buttonStrokeMultiplierIncreaseText.fontSize = 18;
            newButton = buttonStrokeMultiplierIncrease.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonStrokeMultiplierIncreaseClicked = true;
            });

            // Create robot stroke multiplier decrease button by instantiating main button, changing it's name, text label, and adding a new listener to handle click events
            buttonStrokeMultiplierDecrease = Instantiate(btnTitle, btnTitle.parent);
            buttonStrokeMultiplierDecrease.name = "btnStrokeMultiplierDecrease";
            buttonStrokeMultiplierDecreaseText = buttonStrokeMultiplierDecrease.GetComponentInChildren<Text>();
            buttonStrokeMultiplierDecreaseText.text = "Stroke Multiplier -";
            buttonStrokeMultiplierDecreaseText.fontSize = 18;
            newButton = buttonStrokeMultiplierDecrease.GetComponentInChildren<Button>();
            newButton.onClick = new Button.ButtonClickedEvent();
            newButton.onClick.AddListener(() =>
            {
                buttonStrokeMultiplierDecreaseClicked = true;
            });
        }

        public void UpdateSerialPortConnection()
        {
            if (suppressConnectionEvent || handy == null)
            {
                return;
            }

            if (serialPortConnected.Value)
            {
                StartCoroutine(handy.Connect(handyConnectionKey.Value, OnHandyStatus, () => handySendInterval.Value));
            }
            else
            {
                StartCoroutine(handy.Disconnect(handyConnectionKey.Value, OnHandyStatus));
            }
        }

        public async Task UpdateStrokeMultiplierIncreaseButton()
        {
            await Task.Run(async () =>
            {
                buttonStrokeMultiplierIncreaseText.text = robotL0Multiplier.Value.ToString();
                await Task.Delay(1000);
                buttonStrokeMultiplierIncreaseText.text = "Stroke Multiplier +";
            });
        }

        public async Task UpdateStrokeMultiplierDecreaseButton()
        {
            await Task.Run(async () =>
            {
                buttonStrokeMultiplierDecreaseText.text = robotL0Multiplier.Value.ToString();
                await Task.Delay(1000);
                buttonStrokeMultiplierDecreaseText.text = "Stroke Multiplier -";
            });
        }

        public void Update()
        {
            RefreshHandyButtons();

            // Check if connect robot button was clicked
            if (buttonConnectRobotClicked)
            {
                buttonConnectRobotClicked = false;
                RequestConnect();
            }

            // Check if disconnect robot button was clicked
            if (buttonDisconnectRobotClicked)
            {
                buttonDisconnectRobotClicked = false;
                RequestDisconnect();
            }

            // Check if increase stroke multiplier button was clicked
            if (buttonStrokeMultiplierIncreaseClicked)
            {
                buttonStrokeMultiplierIncreaseClicked = false;

                robotL0Multiplier.Value += robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierIncreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if decrease stroke multiplier button was clicked
            if (buttonStrokeMultiplierDecreaseClicked)
            {
                buttonStrokeMultiplierDecreaseClicked = false;

                robotL0Multiplier.Value -= robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierDecreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if increase stroke multiplier hotkey was pressed
            if (strokeLengthMultiplierIncrease.Value.IsDown())
            {
                robotL0Multiplier.Value += robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierIncreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if decrease stroke multiplier hotkey was pressed
            if (strokeLengthMultiplierDecrease.Value.IsDown())
            {
                robotL0Multiplier.Value -= robotL0MultiplierStepValue.Value;

                Task task = UpdateStrokeMultiplierDecreaseButton();

                Logger.LogInfo("Stroke multiplier: " + robotL0Multiplier.Value);
            }

            // Check if the connect/disconnect hotkey was pressed
            if (toggleSerialPortConnection.Value.IsDown())
            {
                if (IsOutputConnected())
                {
                    if (serialPortConnected.Value)
                    {
                        serialPortConnected.Value = false;
                    }
                    else
                    {
                        UpdateSerialPortConnection();
                    }
                }
                else
                {
                    if (!serialPortConnected.Value)
                    {
                        serialPortConnected.Value = true;
                    }
                    else
                    {
                        UpdateSerialPortConnection();
                    }
                }
            }

            // Return if not in an HScene
            if (hScene == null)
            {
                return;
            }

            // Get ms elapsed since current stopwatch interval
            float msElapsed = sw.ElapsedMilliseconds;

            // If the ms elapsed is greater than the period based on the robot's update frequency then
            // stop the stopwatch, call the robot update function, and restart the stopwatch
            if (msElapsed >= (1000.0 / sexRobotUpdateFrequencyConfig.Value))
            {
                sw.Stop();

                femaleTargetType resolvedTarget;
                if (TryResolveFemaleTarget(animationName, out resolvedTarget))
                {
                        if (animationChanged)
                        {
                            if (resolvedTarget == femaleTargetType.NONE)
                            {
                                StopStroke();
                            }
                            else if (IsPairTarget(resolvedTarget))
                            {
                                SetupPairScene(resolvedTarget);
                            }
                            else
                            {
                            // Find all the female and male chara in the current HScene
                            females = hScene.GetFemales().Where(female => female != null).ToArray();
                        males = hScene.GetMales().Where(male => male != null).ToArray();

                        if (diagnosticsConfig.Value)
                        {
                            Logger.LogInfo("Animation: " + animationName);
                            Logger.LogInfo("Females found: " + females.Length.ToString());
                            Logger.LogInfo("Males found: " + males.Length.ToString());
                        }

                        if (males.Length == 1 && females.Length > 0)
                        {
                            // Find/set all the male transforms needed for the calculations here so it doesn't have to happen each FixedUpdate()
                            // Get the base of the male's penis
                            malePenisBase = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan100_00").FirstOrDefault();

                            // Get the tip of the male's penis
                            malePenisTip = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan109_00").FirstOrDefault();

                            // Get the male's penis left ball
                            malePenisLeftBall = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan_f_L").FirstOrDefault();

                            // Get the male's penis right ball
                            malePenisRightBall = males[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cm_J_dan_f_R").FirstOrDefault();

                            currentFemaleTargetType = resolvedTarget;
                            Logger.LogInfo("Contact bone: " + ContactBoneName(currentFemaleTargetType));

                            if (currentFemaleTargetType == femaleTargetType.VAGINAL)
                            {
                                // Find/set all the female transforms needed for the VAGINAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the base of the female's hip
                                femaleHip = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                // Get the base of the female's vagina
                                femaleVagina = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                // Get the base of the female's anus
                                femaleAnus = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.ANAL)
                            {
                                // Find/set all the female transforms needed for the ANAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the base of the female's hip
                                femaleHip = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                // Get the base of the female's vagina
                                femaleVagina = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                // Get the base of the female's anus
                                femaleAnus = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.ORAL)
                            {
                                // Find/set all the female transforms needed for the ORAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the female's mouth upper lips
                                femaleMouthLipsUpper = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouthup").FirstOrDefault();

                                // Get the female's mouth lower lips
                                femaleMouthLipsLower = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_MouthLow").FirstOrDefault();

                                // Get the female's mouth left
                                femaleMouthLeft = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_L").FirstOrDefault();

                                // Get the female's mouth right
                                femaleMouthRight = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_R").FirstOrDefault();

                                femaleMouth = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_MouthCavity").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.BREASTS)
                            {
                                // Find/set all the female transforms needed for the BREASTS calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the female's middle of the breasts left
                                femaleMiddleBreastsLeft = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mune02_L").FirstOrDefault();

                                // Get the female's middle of the breasts right
                                femaleMiddleBreastsRight = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mune02_R").FirstOrDefault();

                                // Get the female's breasts center on the chest
                                femaleBreasts = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mune00").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.LEFTHAND || currentFemaleTargetType == femaleTargetType.RIGHTHAND)
                            {
                                LoadHands(females[0]);
                            }
                            else if (currentFemaleTargetType == femaleTargetType.FOOT)
                            {
                                LoadFeet(females[0]);
                            }
                            else if (currentFemaleTargetType == femaleTargetType.INTERCRURAL)
                            {
                                // Find/set all the female transforms needed for the INTERCRURAL calculations here so it doesn't have to happen each FixedUpdate()
                                // Get the base of the female's hip
                                femaleHip = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                // Get the base of the female's vagina
                                femaleVagina = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                // Get the base of the female's anus
                                femaleAnus = females[0].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                            }
                            else if (currentFemaleTargetType == femaleTargetType.VAGINALSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    // Find/set all the female transforms needed for the VAGINALSWAP calculations here so it doesn't have to happen each FixedUpdate()
                                    // Get the base of the female's hip
                                    femaleHip = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                    // Get the base of the female's vagina
                                    femaleVagina = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                    // Get the base of the female's anus
                                    femaleAnus = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }
                            else if (currentFemaleTargetType == femaleTargetType.ORALSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    // Find/set all the female transforms needed for the ORALSWAP calculations here so it doesn't have to happen each FixedUpdate()
                                    // Get the female's mouth upper lips
                                    femaleMouthLipsUpper = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouthup").FirstOrDefault();

                                    // Get the female's mouth lower lips
                                    femaleMouthLipsLower = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_MouthLow").FirstOrDefault();

                                    // Get the female's mouth left
                                    femaleMouthLeft = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_L").FirstOrDefault();

                                    // Get the female's mouth right
                                    femaleMouthRight = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Mouth_R").FirstOrDefault();

                                    femaleMouth = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_MouthCavity").FirstOrDefault();
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }
                            else if (currentFemaleTargetType == femaleTargetType.RIGHTHANDSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    LoadHands(females[1]);
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }
                            else if (currentFemaleTargetType == femaleTargetType.INTERCRURALSWAP)
                            {
                                if (females.Length == 2)
                                {
                                    // Find/set all the female transforms needed for the INTERCRURALSWAP calculations here so it doesn't have to happen each FixedUpdate()
                                    // Get the base of the female's hip
                                    femaleHip = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kosi02").FirstOrDefault();

                                    // Get the base of the female's vagina
                                    femaleVagina = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Kokan").FirstOrDefault();

                                    // Get the base of the female's anus
                                    femaleAnus = females[1].GetComponentsInChildren<Transform>().Where(x => x.name == "cf_J_Ana").FirstOrDefault();
                                }
                                else
                                {
                                    Logger.LogInfo("Error: The current HScene (swap) doesn't have 2 females.");
                                }
                            }

                            if (diagnosticsConfig.Value)
                            {
                                Logger.LogInfo("Current animation: " + animationName);
                            }

                            animationChanged = false;

                            updateRobotPosition = true;
                        }
                        else
                        {
                            updateRobotPosition = false;

                            Logger.LogInfo("Error: The current HScene doesn't have 1 male and at least 1 female.");
                        }
                            }

                    }

                    if (updateRobotPosition)
                    {
                        if (IsPairTarget(currentFemaleTargetType))
                        {
                            float pairRaw;
                            if (TryMeasurePair(out pairRaw) && !ShouldDelayVisual())
                            {
                                try
                                {
                                    SendHandyStroke(pairRaw);
                                }
                                catch (Exception e)
                                {
                                    Logger.LogInfo("Error: " + e.ToString());
                                }
                            }
                        }
                        else
                        {
                        // Setup T-code reference coordinate system
                        // X(L0) is up/down in reference to the selected male's penis vector and is positive up
                        // Y(L1) is toward/away orthogonal to the selected male's penis vector and is positive away
                        // Z(L2) is left/right orthogonal to the selected male's penis vector and is positive left
                        // RX(R0) is positive according to the right hand rule around X(L0)
                        // RY(R1) is positive according to the right hand rule around Y(L1)
                        // RZ(R2) is positive according to the right hand rule around Z(L2)

                        // Calculate the center point between the two penis's balls
                        Vector3 malePenisBallsCenterPoint = (malePenisLeftBall.position + malePenisRightBall.position) / 2.0f;

                        // Calculate male's penis length
                        float malePenisLength = Vector3.Distance(malePenisBase.position, malePenisTip.position);

                        // Vector from the selected male's penis's base to tip
                        Vector3 malePenisXAxis = malePenisTip.position - malePenisBase.position;

                        // Use the male's penis's base and the male's penis's balls center point to establish the Z reference axis
                        Vector3 malePenisZAxis = Vector3.Cross(malePenisXAxis, malePenisBallsCenterPoint - malePenisBase.position);
                        malePenisZAxis = (malePenisXAxis.magnitude / malePenisZAxis.magnitude) * malePenisZAxis;

                        // Use the reference X and Z axes to establish the orthogonal Y axis
                        Vector3 malePenisYAxis = Vector3.Cross(malePenisXAxis, malePenisZAxis);
                        malePenisYAxis = (malePenisXAxis.magnitude / malePenisYAxis.magnitude) * malePenisYAxis;

                        Vector3 femaleTargetXAxis = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 femaleTargetZAxis = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 femaleTargetYAxis = new Vector3(0.0f, 0.0f, 0.0f);
                        Vector3 femaleTargetToMalePenisBase = new Vector3(0.0f, 0.0f, 0.0f);

                        if (currentFemaleTargetType == femaleTargetType.VAGINAL || currentFemaleTargetType == femaleTargetType.VAGINALSWAP)
                        {
                            // Vector from the selected female's vagina to hip
                            femaleTargetXAxis = femaleHip.position - femaleVagina.position;

                            // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus.position - femaleVagina.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Z axes to establish the orthogonal Y axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's vagina to the male's penis's base
                            femaleTargetToMalePenisBase = femaleVagina.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.ANAL)
                        {
                            // Vector from the selected female's vagina to hip
                            femaleTargetXAxis = femaleHip.position - femaleVagina.position;

                            // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleAnus.position - femaleVagina.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Z axes to establish the orthogonal Y axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's vagina to the male's penis's base
                            femaleTargetToMalePenisBase = femaleAnus.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.ORAL || currentFemaleTargetType == femaleTargetType.ORALSWAP)
                        {
                            // Calculate the center point between the two lips of the mouth
                            Vector3 femaleMouthLipsCenterPoint = (femaleMouthLipsUpper.position + femaleMouthLipsLower.position) / 2.0f;

                            // Calculate the center point between the left and right sides of the mouth
                            Vector3 femaleMouthCenterPoint = (femaleMouthLeft.position + femaleMouthRight.position) / 2.0f;

                            // Vector from the selected female's mouth lips center point to mouth center point
                            femaleTargetXAxis = femaleMouthCenterPoint - femaleMouthLipsCenterPoint;

                            // Use the female's mouth and lips center points vector and the female's mouth to establish the Y reference axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleMouthRight.position - femaleMouthCenterPoint);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetYAxis);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Blowjobs use one mouth bone against the penis. The lip center is only a fallback.
                            Vector3 mouthPoint = femaleMouth != null ? femaleMouth.position : femaleMouthCenterPoint;
                            femaleTargetToMalePenisBase = mouthPoint - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.BREASTS)
                        {
                            // Calculate the center point between the two middle breasts
                            Vector3 femaleMiddleBreastsCenterPoint = (femaleMiddleBreastsLeft.position + femaleMiddleBreastsRight.position) / 2.0f;

                            // Vector from the selected female's middle breasts to breasts on chest
                            femaleTargetYAxis = femaleBreasts.position - femaleMiddleBreastsCenterPoint;

                            // Use the female's middle breasts and breasts on chest vector and the female's middle breasts right to establish the X reference axis
                            femaleTargetXAxis = Vector3.Cross(femaleTargetYAxis, femaleMiddleBreastsRight.position - femaleMiddleBreastsCenterPoint);
                            femaleTargetXAxis = (femaleTargetYAxis.magnitude / femaleTargetXAxis.magnitude) * femaleTargetXAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetYAxis, femaleTargetXAxis);
                            femaleTargetZAxis = (femaleTargetYAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Vector from the female's breasts center point to the male's penis's base
                            femaleTargetToMalePenisBase = femaleMiddleBreastsCenterPoint - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.LEFTHAND)
                        {
                            // Vector from the selected female's middle and ring fingers
                            femaleTargetXAxis = femaleMiddleFingerLeft.position - femaleRingFingerLeft.position;

                            // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandLeft.position - femaleMiddleFingerLeft.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's hand to the male's penis's base
                            femaleTargetToMalePenisBase = femaleHandLeft.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.RIGHTHAND || currentFemaleTargetType == femaleTargetType.RIGHTHANDSWAP)
                        {
                            // Vector from the selected female's middle and ring fingers
                            femaleTargetXAxis = femaleMiddleFingerRight.position - femaleRingFingerRight.position;

                            // Use the female's middle and ring fingers vector and the female's hand to establish the Y reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHandRight.position - femaleMiddleFingerRight.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Y axes to establish the orthogonal Z axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's hand to the male's penis's base
                            femaleTargetToMalePenisBase = femaleHandRight.position - malePenisBase.position;
                        }
                        else if (currentFemaleTargetType == femaleTargetType.INTERCRURAL || currentFemaleTargetType == femaleTargetType.INTERCRURALSWAP)
                        {
                            // Vector from the selected female's vagina to anus
                            femaleTargetXAxis = femaleVagina.position - femaleAnus.position;

                            // Use the female's vagina and hip vector and the female's anus to establish the Z reference axis
                            femaleTargetZAxis = Vector3.Cross(femaleTargetXAxis, femaleHip.position - femaleVagina.position);
                            femaleTargetZAxis = (femaleTargetXAxis.magnitude / femaleTargetZAxis.magnitude) * femaleTargetZAxis;

                            // Use the reference X and Z axes to establish the orthogonal Y axis
                            femaleTargetYAxis = Vector3.Cross(femaleTargetXAxis, femaleTargetZAxis);
                            femaleTargetYAxis = (femaleTargetXAxis.magnitude / femaleTargetYAxis.magnitude) * femaleTargetYAxis;

                            // Vector from the female's vagina to the male's penis's base
                            femaleTargetToMalePenisBase = femaleVagina.position - malePenisBase.position;
                        }

                        if (IsHandTarget(currentFemaleTargetType))
                        {
                            Transform grip = ClosestGripBone();
                            if (grip != null)
                            {
                                femaleTargetToMalePenisBase = grip.position - malePenisBase.position;
                            }
                        }

                        if (currentFemaleTargetType == femaleTargetType.FOOT)
                        {
                            Transform foot = ClosestFootBone();
                            if (foot != null)
                            {
                                femaleTargetToMalePenisBase = foot.position - malePenisBase.position;
                            }
                        }

                        // Calculate X(L0) for robot based on the reference X axis and the vector from the female's vagina's labia trigger to the male's penis's base collider
                        float robotL0 = Vector3.Dot(malePenisXAxis, femaleTargetToMalePenisBase) / (malePenisXAxis.magnitude * malePenisXAxis.magnitude);
                        float handyRawL0 = robotL0;

                        if (!ShouldDelayVisual())
                        {
                            try
                            {
                                SendHandyStroke(handyRawL0);
                            }
                            catch (Exception e)
                            {
                                Logger.LogInfo("Error: " + e.ToString());
                            }
                            }
                        }
                    }
                }
                else if (animationChanged)
                {
                    StopStroke();
                    Logger.LogInfo("No contact for scene: " + animationName);
                }

                sw = Stopwatch.StartNew();

            }
        }

        public void LateUpdate()
        {
            SampleHandyFromTruePose();
            DelayHandyVisuals();
            WriteLive();
        }

        void SampleHandyFromTruePose()
        {
            if (!updateRobotPosition || !ShouldDelayVisual())
            {
                return;
            }

            float raw;
            if (!TryReadHandyRaw(out raw))
            {
                return;
            }

            try
            {
                SendHandyStroke(raw);
            }
            catch (Exception e)
            {
                Logger.LogInfo("Error: " + e.ToString());
            }
        }

        bool TryReadHandyRaw(out float raw)
        {
            raw = 0f;
            if (IsPairTarget(currentFemaleTargetType))
            {
                return TryMeasurePair(out raw);
            }

            if (malePenisBase == null || malePenisTip == null)
            {
                return false;
            }

            Vector3 axis = malePenisTip.position - malePenisBase.position;
            float denom = axis.sqrMagnitude;
            if (denom < 0.000001f)
            {
                return false;
            }

            Vector3 point;
            if (!TryContactPoint(out point))
            {
                return false;
            }

            raw = Vector3.Dot(axis, point - malePenisBase.position) / denom;
            return true;
        }

        bool TryContactPoint(out Vector3 point)
        {
            point = Vector3.zero;
            if (currentFemaleTargetType == femaleTargetType.VAGINAL || currentFemaleTargetType == femaleTargetType.VAGINALSWAP || currentFemaleTargetType == femaleTargetType.INTERCRURAL || currentFemaleTargetType == femaleTargetType.INTERCRURALSWAP)
            {
                if (femaleVagina == null)
                {
                    return false;
                }

                point = femaleVagina.position;
                return true;
            }

            if (currentFemaleTargetType == femaleTargetType.ANAL)
            {
                if (femaleAnus == null)
                {
                    return false;
                }

                point = femaleAnus.position;
                return true;
            }

            if (currentFemaleTargetType == femaleTargetType.ORAL || currentFemaleTargetType == femaleTargetType.ORALSWAP)
            {
                if (femaleMouth != null)
                {
                    point = femaleMouth.position;
                    return true;
                }

                if (femaleMouthLeft == null || femaleMouthRight == null)
                {
                    return false;
                }

                point = (femaleMouthLeft.position + femaleMouthRight.position) * 0.5f;
                return true;
            }

            if (currentFemaleTargetType == femaleTargetType.BREASTS)
            {
                if (femaleMiddleBreastsLeft == null || femaleMiddleBreastsRight == null)
                {
                    return false;
                }

                point = (femaleMiddleBreastsLeft.position + femaleMiddleBreastsRight.position) * 0.5f;
                return true;
            }

            if (IsHandTarget(currentFemaleTargetType))
            {
                Transform grip = ClosestGripBone();
                if (grip == null)
                {
                    return false;
                }

                point = grip.position;
                return true;
            }

            if (currentFemaleTargetType == femaleTargetType.FOOT)
            {
                Transform foot = ClosestFootBone();
                if (foot == null)
                {
                    return false;
                }

                point = foot.position;
                return true;
            }

            return false;
        }

        bool ShouldDelayVisual()
        {
            return handy != null && handy.IsConnected && hScene != null;
        }

        public void RestoreHandyVisuals()
        {
            if (!ShouldDelayVisual())
            {
                for (int i = 0; i < animHolds.Count; i++)
                {
                    animHolds[i].rewound = false;
                }

                return;
            }

            for (int i = 0; i < animHolds.Count; i++)
            {
                AnimHold hold = animHolds[i];
                if (!hold.rewound || hold.stuck > 6 || hold.animator == null || hold.animator.IsInTransition(0))
                {
                    continue;
                }

                AnimatorStateInfo info = hold.animator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash != hold.stateHash)
                {
                    hold.rewound = false;
                    continue;
                }

                hold.animator.Play(hold.stateHash, 0, hold.trueNormalized);
                hold.animator.Update(0f);
                hold.rewound = false;
            }
        }

        void DelayHandyVisuals()
        {
            if (!ShouldDelayVisual())
            {
                return;
            }

            float delay = handy != null ? handy.VisualDelaySeconds : (HandyDevice.PlaybackDelayMs / 1000f);
            List<Animator> bodies = new List<Animator>();
            CollectBodies(bodies);
            if (!loggedAnimDelay && bodies.Count > 0)
            {
                loggedAnimDelay = true;
                Logger.LogInfo("H scene pose is held back " + delay.ToString("0.00") + "s so it meets the sleeve.");
            }

            for (int i = 0; i < bodies.Count; i++)
            {
                Animator animator = bodies[i];
                if (animator == null || !animator.isActiveAndEnabled || animator.IsInTransition(0))
                {
                    continue;
                }

                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.length < 0.05f)
                {
                    continue;
                }

                AnimHold hold = null;
                for (int h = 0; h < animHolds.Count; h++)
                {
                    if (animHolds[h].animator == animator)
                    {
                        hold = animHolds[h];
                        break;
                    }
                }

                if (hold == null)
                {
                    hold = new AnimHold();
                    hold.animator = animator;
                    animHolds.Add(hold);
                }

                if (hold.havePrev && hold.stateHash == info.shortNameHash && Math.Abs(info.normalizedTime - hold.trueNormalized) < 0.00001f)
                {
                    hold.stuck++;
                }
                else
                {
                    hold.stuck = 0;
                }

                if (hold.stuck > 6)
                {
                    hold.rewound = false;
                    continue;
                }

                hold.stateHash = info.shortNameHash;
                hold.trueNormalized = info.normalizedTime;
                hold.havePrev = true;
                float shown = info.normalizedTime - (delay / info.length);
                if (shown < 0f)
                {
                    shown = 0f;
                }

                animator.Play(info.shortNameHash, 0, shown);
                animator.Update(0f);
                hold.rewound = true;
            }
        }

        void CollectBodies(List<Animator> bodies)
        {
            try
            {
                AddBodies(hScene.GetFemales(), bodies);
                AddBodies(hScene.GetMales(), bodies);
            }
            catch (Exception)
            {
            }
        }

        static void AddBodies(ChaControl[] people, List<Animator> bodies)
        {
            if (people == null)
            {
                return;
            }

            for (int i = 0; i < people.Length; i++)
            {
                if (people[i] == null || people[i].animBody == null)
                {
                    continue;
                }

                bodies.Add(people[i].animBody);
            }
        }

        class AnimHold
        {
            public Animator animator;
            public int stateHash;
            public float trueNormalized;
            public bool rewound;
            public float prevShown;
            public bool havePrev;
            public int stuck;
        }

        [DefaultExecutionOrder(-32000)]
        public class HandyPoseClock : MonoBehaviour
        {
            public HS2_HandyController Owner;

            void Update()
            {
                if (Owner != null)
                {
                    Owner.RestoreHandyVisuals();
                }
            }
        }

        void WriteLive()
        {
            float now = Time.realtimeSinceStartup;
            if (now < liveWriteAt || string.IsNullOrEmpty(liveFolder))
            {
                return;
            }

            liveWriteAt = now + 0.1f;
            string bone = "bone=none";
            if (malePenisBase != null)
            {
                Vector3 p = malePenisBase.position;
                bone = "bone=" + p.x.ToString("0.00") + "," + p.y.ToString("0.00") + "," + p.z.ToString("0.00");
            }

            string bodies = DescribeBodies();
            string handyLine = "handy=off";
            string contact = ContactBoneName(currentFemaleTargetType);
            if (handy != null && handy.IsConnected)
            {
                handyLine = "handy=on x=" + liveSent.ToString() + " raw=" + liveRaw.ToString("0.000") + " q=" + handy.Queued.ToString() + " play=" + (handy.StreamPlaying ? "1" : "0") + " off=" + handy.OffsetMs.ToString();
            }

            liveText = pluginVersion + " " + (string.IsNullOrEmpty(animationName) ? "no-anim" : animationName) + " contact=" + contact + "\n" + handyLine + " " + bone + "\n" + bodies;
            NoteSceneSurvey(animationName, contact, liveRaw, liveSent, liveMaxDn);
            try
            {
                string path = Path.Combine(liveFolder, "status.txt");
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(liveText);
                }

                using (FileStream fs = new FileStream(Path.Combine(liveFolder, "trace.txt"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(now.ToString("0.00") + " " + liveText.Replace("\n", " | "));
                }
            }
            catch (Exception)
            {
            }
        }

        string DescribeBodies()
        {
            if (hScene == null)
            {
                return "pose=none";
            }

            List<Animator> bodies = new List<Animator>();
            CollectBodies(bodies);
            if (bodies.Count == 0)
            {
                return "pose=none";
            }

            StringBuilder sb = new StringBuilder();
            int count = bodies.Count;
            if (count > livePrevNorm.Length)
            {
                count = livePrevNorm.Length;
            }

            liveMaxDn = 0f;
            for (int i = 0; i < count; i++)
            {
                Animator animator = bodies[i];
                if (animator == null)
                {
                    continue;
                }

                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
                float now = info.normalizedTime;
                float dn = liveHavePrev[i] ? now - livePrevNorm[i] : 0f;
                livePrevNorm[i] = now;
                liveHavePrev[i] = true;
                if (Mathf.Abs(dn) > liveMaxDn)
                {
                    liveMaxDn = Mathf.Abs(dn);
                }
                string clip = "?";
                AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
                if (clips != null && clips.Length > 0 && clips[0].clip != null)
                {
                    clip = clips[0].clip.name;
                }

                if (sb.Length > 0)
                {
                    sb.Append(" | ");
                }

                sb.Append(clip);
                sb.Append(" n=");
                sb.Append(now.ToString("0.000"));
                sb.Append(" dn=");
                sb.Append(dn.ToString("0.000"));
            }

            return sb.Length == 0 ? "pose=none" : sb.ToString();
        }

        void NoteSceneSurvey(string scene, string contact, float raw, int sent, float dn)
        {
            if (string.IsNullOrEmpty(scene))
            {
                FlushSceneSurvey();
                return;
            }

            if (scene != surveyScene)
            {
                FlushSceneSurvey();
                surveyScene = scene;
                surveyContact = contact;
                surveyRawMin = raw;
                surveyRawMax = raw;
                surveySentMin = sent;
                surveySentMax = sent;
                surveySamples = 0;
                surveyDn = 0f;
            }

            surveyContact = contact;
            if (raw < surveyRawMin)
            {
                surveyRawMin = raw;
            }

            if (raw > surveyRawMax)
            {
                surveyRawMax = raw;
            }

            if (sent >= 0)
            {
                if (sent < surveySentMin)
                {
                    surveySentMin = sent;
                }

                if (sent > surveySentMax)
                {
                    surveySentMax = sent;
                }
            }

            if (dn > surveyDn)
            {
                surveyDn = dn;
            }

            surveySamples++;
        }

        void FlushSceneSurvey()
        {
            if (surveySamples < 8 || string.IsNullOrEmpty(surveyScene) || string.IsNullOrEmpty(liveFolder))
            {
                surveySamples = 0;
                surveyScene = "";
                return;
            }

            float span = surveyRawMax - surveyRawMin;
            int sentSpan = 0;
            if (surveySentMax >= surveySentMin && surveySentMin <= 100)
            {
                sentSpan = surveySentMax - surveySentMin;
            }

            string verdict = "OK";
            if (surveyDn < 0.01f)
            {
                verdict = "FROZEN";
            }
            else if (span < 0.04f)
            {
                verdict = "FLAT";
            }
            else if (sentSpan < 4)
            {
                verdict = "PEGGED";
            }

            string line = surveyScene + " contact=" + surveyContact + " n=" + surveySamples.ToString() + " raw=" + surveyRawMin.ToString("0.000") + ".." + surveyRawMax.ToString("0.000") + " sent=" + surveySentMin.ToString() + ".." + surveySentMax.ToString() + " dn=" + surveyDn.ToString("0.000") + " " + verdict;
            Logger.LogInfo("Scene survey: " + line);
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(liveFolder, "scenes.txt"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(line);
                }
            }
            catch (Exception)
            {
            }

            surveySamples = 0;
            surveyScene = "";
        }

        void ResetHandyForNewAnimation()
        {
            animHolds.Clear();
            haveStrokeLimit = false;
            haveStrokeCenter = false;
            if (handy != null)
            {
                handy.ResetStream();
            }
        }

        void RequestConnect()
        {
            if (serialPortConnected.Value)
            {
                UpdateSerialPortConnection();
            }
            else
            {
                serialPortConnected.Value = true;
            }
        }

        void RequestDisconnect()
        {
            if (!serialPortConnected.Value)
            {
                UpdateSerialPortConnection();
            }
            else
            {
                serialPortConnected.Value = false;
            }
        }

        static void DrawHandyConnectButton(ConfigEntryBase entry)
        {
            if (instance == null)
            {
                return;
            }

            string status = instance.handyStatus != null ? instance.handyStatus.Value : "Disconnected";
            bool connecting = status != null && status.StartsWith("Connecting");
            string label = "Connect";
            if (instance.IsOutputConnected())
            {
                label = "Disconnect";
            }
            else if (connecting)
            {
                label = "Connecting...";
            }

            if (GUILayout.Button(label, GUILayout.ExpandWidth(true)))
            {
                if (label == "Disconnect" || connecting)
                {
                    instance.RequestDisconnect();
                }
                else
                {
                    instance.RequestConnect();
                }
            }
        }

        bool IsOutputConnected()
        {
            return handy != null && handy.IsConnected;
        }

        void OnHandyCredentialChanged()
        {
            if (serialPortConnected.Value)
            {
                UpdateSerialPortConnection();
            }
        }

        void OnHandyStatus(string status)
        {
            SetConnectionStatus(status);

            if (status != null && status.StartsWith("Disconnected") && serialPortConnected.Value)
            {
                suppressConnectionEvent = true;
                serialPortConnected.Value = false;
                suppressConnectionEvent = false;
            }
        }

        void SetConnectionStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                status = "Disconnected";
            }

            if (handyStatus != null && handyStatus.Value != status)
            {
                handyStatus.Value = status;
            }

            RefreshHandyButtons();
        }

        void RefreshHandyButtons()
        {
            if (buttonConnectRobotText == null || handyStatus == null)
            {
                return;
            }

            string status = handyStatus.Value;
            bool connected = status == "Connected";
            bool connecting = status != null && status.StartsWith("Connecting");
            string connectLabel = "Connect Handy";
            string disconnectLabel = "Disconnected";

            if (connected)
            {
                connectLabel = "Connected";
                disconnectLabel = "Disconnect Handy";
            }
            else if (connecting)
            {
                connectLabel = "Connecting...";
                disconnectLabel = "Disconnect Handy";
            }

            if (buttonConnectRobotText.text != connectLabel)
            {
                buttonConnectRobotText.text = connectLabel;
            }

            if (buttonDisconnectRobotText != null && buttonDisconnectRobotText.text != disconnectLabel)
            {
                buttonDisconnectRobotText.text = disconnectLabel;
            }
        }

        void LoadHands(ChaControl female)
        {
            if (female == null)
            {
                return;
            }

            Transform[] bones = female.GetComponentsInChildren<Transform>();
            femaleMiddleFingerLeft = FindBone(bones, "N_Middle_L");
            femaleRingFingerLeft = FindBone(bones, "N_Ring_L");
            femaleHandLeft = FindBone(bones, "N_Hand_L");
            femaleMiddleFingerRight = FindBone(bones, "N_Middle_R");
            femaleRingFingerRight = FindBone(bones, "N_Ring_R");
            femaleHandRight = FindBone(bones, "N_Hand_R");
        }

        void LoadFeet(ChaControl female)
        {
            if (female == null)
            {
                return;
            }

            Transform[] bones = female.GetComponentsInChildren<Transform>();
            femaleFootLeft = FindBone(bones, "cf_J_Leg03_L");
            femaleFootRight = FindBone(bones, "cf_J_Leg03_R");
            femaleToesLeft = FindBone(bones, "cf_J_Toes_L");
            femaleToesRight = FindBone(bones, "cf_J_Toes_R");
        }

        static Transform FindBone(Transform[] bones, string name)
        {
            if (bones == null)
            {
                return null;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null && bones[i].name == name)
                {
                    return bones[i];
                }
            }

            return null;
        }

        static bool IsHandTarget(femaleTargetType target)
        {
            return target == femaleTargetType.LEFTHAND || target == femaleTargetType.RIGHTHAND || target == femaleTargetType.RIGHTHANDSWAP;
        }

        Transform ClosestGripBone()
        {
            if (malePenisBase == null || malePenisTip == null)
            {
                return null;
            }

            Vector3 axis = malePenisTip.position - malePenisBase.position;
            float lengthSquared = axis.sqrMagnitude;
            Transform best = null;
            float bestDistance = float.MaxValue;
            if (currentFemaleTargetType == femaleTargetType.LEFTHAND)
            {
                ConsiderGrip(femaleMiddleFingerLeft, axis, lengthSquared, ref best, ref bestDistance);
                ConsiderGrip(femaleRingFingerLeft, axis, lengthSquared, ref best, ref bestDistance);
                ConsiderGrip(femaleHandLeft, axis, lengthSquared, ref best, ref bestDistance);
            }
            else if (currentFemaleTargetType == femaleTargetType.RIGHTHAND || currentFemaleTargetType == femaleTargetType.RIGHTHANDSWAP)
            {
                ConsiderGrip(femaleMiddleFingerRight, axis, lengthSquared, ref best, ref bestDistance);
                ConsiderGrip(femaleRingFingerRight, axis, lengthSquared, ref best, ref bestDistance);
                ConsiderGrip(femaleHandRight, axis, lengthSquared, ref best, ref bestDistance);
            }

            return best;
        }

        Transform ClosestFootBone()
        {
            if (malePenisBase == null || malePenisTip == null)
            {
                return null;
            }

            Vector3 axis = malePenisTip.position - malePenisBase.position;
            float lengthSquared = axis.sqrMagnitude;
            Transform best = null;
            float bestDistance = float.MaxValue;
            ConsiderGrip(femaleToesLeft, axis, lengthSquared, ref best, ref bestDistance);
            ConsiderGrip(femaleToesRight, axis, lengthSquared, ref best, ref bestDistance);
            ConsiderGrip(femaleFootLeft, axis, lengthSquared, ref best, ref bestDistance);
            ConsiderGrip(femaleFootRight, axis, lengthSquared, ref best, ref bestDistance);
            return best;
        }

        void ConsiderGrip(Transform bone, Vector3 axis, float lengthSquared, ref Transform best, ref float bestDistance)
        {
            if (bone == null || malePenisBase == null)
            {
                return;
            }

            Vector3 offset = bone.position - malePenisBase.position;
            float along = 0f;
            if (lengthSquared > 0.000001f)
            {
                along = Vector3.Dot(axis, offset) / lengthSquared;
            }

            float clamped = along;
            if (clamped < 0f)
            {
                clamped = 0f;
            }
            else if (clamped > 1f)
            {
                clamped = 1f;
            }

            Vector3 closest = malePenisBase.position + (axis * clamped);
            float distance = (bone.position - closest).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = bone;
            }
        }

        bool TryResolveFemaleTarget(string name, out femaleTargetType target)
        {
            if (!string.IsNullOrEmpty(name) && animationFemaleTargetDictionary.TryGetValue(name, out target))
            {
                return true;
            }

            target = femaleTargetType.VAGINAL;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            string n = name.ToLowerInvariant();
            if (n.Contains("vibrator") || n.Contains("masturbat") || n.Contains("fingering") || n.Contains("cunnilingus") || n.Contains("cunilingus") || n.Contains("spank") || n.Contains("massage") || n.Contains("foreplay") || n.Contains("kiss") || n.Contains("grope") || n.Contains("breast play") || n == "rimjob" || n.Contains("rimjob"))
            {
                if (!(n.Contains("handjob") || n.Contains("blow") || n.Contains(" bj") || n.Contains("missionary") || n.Contains("doggy") || n.Contains("cowgirl") || n.Contains("sex")))
                {
                    target = ForeplayTarget(n);
                    return true;
                }
            }

            bool hand = n.Contains("handjob") || n.Contains("hand job");
            bool foot = n.Contains("footjob") || n.Contains("foot job");
            bool mouth = n.Contains("blow") || n.Contains(" bj") || n.EndsWith(" bj") || n.Contains("irrum") || n.Contains("deepthroat") || n.Contains("tip lick") || n.Contains("glans") || n == "69";
            if (hand)
            {
                target = femaleTargetType.LEFTHAND;
                return true;
            }

            if (foot)
            {
                target = femaleTargetType.FOOT;
                return true;
            }

            if (mouth)
            {
                target = femaleTargetType.ORAL;
                return true;
            }

            if (n.Contains("tit") || n.Contains("boob"))
            {
                target = femaleTargetType.BREASTS;
                return true;
            }

            if (n.Contains("anal"))
            {
                target = femaleTargetType.ANAL;
                return true;
            }

            if (n.Contains("thigh") || n.Contains("intercrural"))
            {
                target = femaleTargetType.INTERCRURAL;
                return true;
            }

            if (n.Contains("missionary") || n.Contains("doggy") || n.Contains("cowgirl") || n.Contains("sex") || n.Contains("spoon") || n == "side" || n.Contains(" side") || n.Contains("squat") || n.Contains("suspend") || n.Contains("tied") || n.Contains("squeeze") || n.Contains("counter") || n.Contains("insertion") || n.Contains("piledriver") || n.Contains("press"))
            {
                target = femaleTargetType.VAGINAL;
                return true;
            }

            return false;
        }

        static bool IsPairTarget(femaleTargetType target)
        {
            return target == femaleTargetType.KISS || target == femaleTargetType.HANDVAGINA || target == femaleTargetType.MOUTHVAGINA || target == femaleTargetType.MOUTHANUS || target == femaleTargetType.HANDBREAST || target == femaleTargetType.HANDHIP || target == femaleTargetType.VIBRATOR;
        }

        static femaleTargetType ForeplayTarget(string n)
        {
            if (n.Contains("vibrator"))
            {
                return femaleTargetType.VIBRATOR;
            }

            if (n.Contains("spank"))
            {
                return femaleTargetType.HANDHIP;
            }

            if (n.Contains("fingering") || n.Contains("masturbat") || n.Contains("foreplay"))
            {
                return femaleTargetType.HANDVAGINA;
            }

            if (n.Contains("rim"))
            {
                return femaleTargetType.MOUTHANUS;
            }

            if (n.Contains("cunnilingus") || n.Contains("cunilingus"))
            {
                return femaleTargetType.MOUTHVAGINA;
            }

            if (n.Contains("breast") || n.Contains("massage") || n.Contains("grope"))
            {
                return femaleTargetType.HANDBREAST;
            }

            if (n.Contains("kiss"))
            {
                return femaleTargetType.KISS;
            }

            return femaleTargetType.NONE;
        }

        void SetupPairScene(femaleTargetType target)
        {
            females = hScene.GetFemales().Where(female => female != null).ToArray();
            males = hScene.GetMales().Where(male => male != null).ToArray();
            if (females.Length == 0)
            {
                updateRobotPosition = false;
                animationChanged = false;
                Logger.LogInfo("Error: no female for " + animationName);
                return;
            }

            ChaControl partner = null;
            if (males.Length > 0)
            {
                partner = males[0];
            }
            else if (females.Length > 1)
            {
                partner = females[1];
            }

            LoadPairBones(females[0], partner);
            currentFemaleTargetType = target;
            vibratorPoint = target == femaleTargetType.VIBRATOR ? FindVibrator(females[0]) : null;
            animationChanged = false;
            updateRobotPosition = true;
            Logger.LogInfo("Contact bone: " + ContactBoneName(target));
        }

        void LoadPairBones(ChaControl female, ChaControl partner)
        {
            Transform[] bones = female.GetComponentsInChildren<Transform>();
            femaleHip = FindBone(bones, "cf_J_Kosi02");
            femaleVagina = FindBone(bones, "cf_J_Kokan");
            femaleAnus = FindBone(bones, "cf_J_Ana");
            femaleMouth = FindBone(bones, "cf_J_MouthCavity");
            femaleMouthLeft = FindBone(bones, "cf_J_Mouth_L");
            femaleMouthRight = FindBone(bones, "cf_J_Mouth_R");
            femaleMiddleBreastsLeft = FindBone(bones, "cf_J_Mune02_L");
            femaleMiddleBreastsRight = FindBone(bones, "cf_J_Mune02_R");
            femaleBreasts = FindBone(bones, "cf_J_Mune00");
            LoadHands(female);
            partnerMouth = partner == null ? null : FindMouth(partner.GetComponentsInChildren<Transform>());
        }

        static Transform FindMouth(Transform[] bones)
        {
            string[] names = { "cf_J_MouthCavity", "cm_J_MouthCavity", "cf_J_Mouth_L", "cm_J_Head", "N_Head", "cf_J_Head" };
            for (int i = 0; i < names.Length; i++)
            {
                Transform bone = FindBone(bones, names[i]);
                if (bone != null)
                {
                    return bone;
                }
            }

            return null;
        }

        Transform FindVibrator(ChaControl female)
        {
            Transform found = FindNamed(female.GetComponentsInChildren<Transform>(true));
            if (found == null && hScene != null)
            {
                found = FindNamed(hScene.GetComponentsInChildren<Transform>(true));
            }

            if (found == null && femaleAnus != null)
            {
                int count = femaleAnus.childCount;
                for (int i = 0; i < count; i++)
                {
                    Transform child = femaleAnus.GetChild(i);
                    if (child != null && !child.name.StartsWith("cf_J_") && !child.name.StartsWith("cm_J_") && !child.name.StartsWith("N_"))
                    {
                        found = child;
                        break;
                    }
                }
            }

            Logger.LogInfo(found == null ? "Vibrator object was not found." : "Vibrator object: " + found.name);
            return found;
        }

        static Transform FindNamed(Transform[] bones)
        {
            if (bones == null)
            {
                return null;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    continue;
                }

                string n = bones[i].name.ToLowerInvariant();
                if (n.StartsWith("cf_j_") || n.StartsWith("cm_j_") || n.StartsWith("n_"))
                {
                    continue;
                }

                if (n.Contains("vib") || n.Contains("denma") || n.Contains("rotor") || n.Contains("vibe"))
                {
                    return bones[i];
                }
            }

            return null;
        }

        bool TryMeasurePair(out float raw)
        {
            raw = 0f;
            Vector3 origin;
            Vector3 mover;
            if (!TryPairPoints(out origin, out mover))
            {
                return false;
            }

            float span = 0.12f;
            if (femaleHip != null && femaleVagina != null)
            {
                float body = Vector3.Distance(femaleHip.position, femaleVagina.position);
                if (body > 0.02f)
                {
                    span = body;
                }
            }

            if (currentFemaleTargetType == femaleTargetType.VIBRATOR && vibratorPoint != null && femaleAnus != null && vibratorPoint.IsChildOf(femaleAnus))
            {
                raw = vibratorPoint.localPosition.magnitude / span;
            }
            else
            {
                raw = Vector3.Distance(mover, origin) / span;
            }

            return true;
        }

        bool TryPairPoints(out Vector3 origin, out Vector3 mover)
        {
            origin = Vector3.zero;
            mover = Vector3.zero;
            Vector3 mouth = MouthPoint();
            if (currentFemaleTargetType == femaleTargetType.KISS)
            {
                if (partnerMouth == null || (femaleMouth == null && femaleMouthLeft == null))
                {
                    return false;
                }

                origin = partnerMouth.position;
                mover = mouth;
                return true;
            }

            if (currentFemaleTargetType == femaleTargetType.HANDVAGINA)
            {
                return HandNear(femaleVagina, out origin, out mover);
            }

            if (currentFemaleTargetType == femaleTargetType.MOUTHVAGINA)
            {
                if (femaleVagina == null || (femaleMouth == null && femaleMouthLeft == null))
                {
                    return false;
                }

                origin = femaleVagina.position;
                mover = mouth;
                return true;
            }

            if (currentFemaleTargetType == femaleTargetType.MOUTHANUS)
            {
                if (femaleAnus == null || (femaleMouth == null && femaleMouthLeft == null))
                {
                    return false;
                }

                origin = femaleAnus.position;
                mover = mouth;
                return true;
            }

            if (currentFemaleTargetType == femaleTargetType.HANDBREAST)
            {
                if (femaleMiddleBreastsLeft == null || femaleMiddleBreastsRight == null)
                {
                    return false;
                }

                return HandNear((femaleMiddleBreastsLeft.position + femaleMiddleBreastsRight.position) * 0.5f, out origin, out mover);
            }

            if (currentFemaleTargetType == femaleTargetType.HANDHIP)
            {
                return HandNear(femaleHip, out origin, out mover);
            }

            if (currentFemaleTargetType == femaleTargetType.VIBRATOR)
            {
                if (femaleAnus == null || vibratorPoint == null)
                {
                    return false;
                }

                origin = femaleAnus.position;
                mover = vibratorPoint.position;
                return true;
            }

            return false;
        }

        bool HandNear(Transform place, out Vector3 origin, out Vector3 mover)
        {
            origin = Vector3.zero;
            mover = Vector3.zero;
            if (place == null)
            {
                return false;
            }

            return HandNear(place.position, out origin, out mover);
        }

        bool HandNear(Vector3 place, out Vector3 origin, out Vector3 mover)
        {
            origin = place;
            mover = Vector3.zero;
            Transform hand = ClosestTo(place, femaleHandLeft, femaleHandRight, femaleMiddleFingerLeft, femaleMiddleFingerRight, femaleRingFingerLeft, femaleRingFingerRight);
            if (hand == null)
            {
                return false;
            }

            mover = hand.position;
            return true;
        }

        Vector3 MouthPoint()
        {
            if (femaleMouth != null)
            {
                return femaleMouth.position;
            }

            if (femaleMouthLeft != null && femaleMouthRight != null)
            {
                return (femaleMouthLeft.position + femaleMouthRight.position) * 0.5f;
            }

            if (femaleMouthLeft != null)
            {
                return femaleMouthLeft.position;
            }

            return Vector3.zero;
        }

        static Transform ClosestTo(Vector3 point, params Transform[] bones)
        {
            Transform best = null;
            float bestDistance = float.MaxValue;
            if (bones == null)
            {
                return null;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    continue;
                }

                float distance = (bones[i].position - point).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = bones[i];
                }
            }

            return best;
        }

        void StopStroke()
        {
            currentFemaleTargetType = femaleTargetType.NONE;
            updateRobotPosition = false;
            animationChanged = false;
            Logger.LogInfo("Contact bone: none");
            if (handy != null)
            {
                handy.ResetStream();
            }
        }

        static string ContactBoneName(femaleTargetType target)
        {
            if (target == femaleTargetType.NONE)
            {
                return "none";
            }

            if (target == femaleTargetType.KISS)
            {
                return "kiss";
            }

            if (target == femaleTargetType.HANDVAGINA)
            {
                return "fingers";
            }

            if (target == femaleTargetType.MOUTHVAGINA || target == femaleTargetType.MOUTHANUS)
            {
                return "mouth";
            }

            if (target == femaleTargetType.HANDBREAST)
            {
                return "breast";
            }

            if (target == femaleTargetType.HANDHIP)
            {
                return "hand";
            }

            if (target == femaleTargetType.VIBRATOR)
            {
                return "vibrator";
            }

            if (target == femaleTargetType.ORAL || target == femaleTargetType.ORALSWAP)
            {
                return "mouth";
            }

            if (target == femaleTargetType.ANAL)
            {
                return "anus";
            }

            if (target == femaleTargetType.BREASTS)
            {
                return "breasts";
            }

            if (target == femaleTargetType.LEFTHAND || target == femaleTargetType.RIGHTHAND || target == femaleTargetType.RIGHTHANDSWAP)
            {
                return "hand";
            }

            if (target == femaleTargetType.FOOT)
            {
                return "foot";
            }

            if (target == femaleTargetType.INTERCRURAL || target == femaleTargetType.INTERCRURALSWAP)
            {
                return "thighs";
            }

            return "vagina";
        }

        void SendHandyStroke(float stroke)
        {
            liveRaw = stroke;
            if (handy == null || !handy.IsConnected)
            {
                return;
            }

            if (stroke < 0.0f)
            {
                stroke = 0.0f;
            }
            else if (stroke > 1.0f)
            {
                stroke = 1.0f;
            }

            float now = Time.realtimeSinceStartup;
            float dt = now - strokeLimitAt;
            if (dt <= 0.0f || dt > 0.05f)
            {
                dt = 0.05f;
            }

            if (!haveStrokeLimit)
            {
                strokeLimited = stroke;
                haveStrokeLimit = true;
            }

            float maxStep = 1.6f * dt;
            float delta = stroke - strokeLimited;
            if (delta > maxStep)
            {
                delta = maxStep;
            }
            else if (delta < -maxStep)
            {
                delta = -maxStep;
            }

            strokeLimited += delta;
            strokeLimitAt = now;
            stroke = strokeLimited;

            if (!haveStrokeCenter)
            {
                strokeCenter = stroke;
                haveStrokeCenter = true;
            }
            else
            {
                float follow = 1.0f - Mathf.Exp(-dt / 1.5f);
                strokeCenter += (stroke - strokeCenter) * follow;
            }

            float gain = robotL0Multiplier != null ? robotL0Multiplier.Value : 1.0f;
            stroke = strokeCenter + ((stroke - strokeCenter) * gain);
            if (stroke < 0.0f)
            {
                stroke = 0.0f;
            }
            else if (stroke > 1.0f)
            {
                stroke = 1.0f;
            }

            if (handyInvertStroke.Value)
            {
                stroke = 1.0f - stroke;
            }

            handy.ReportMotion(stroke * 100.0f);
            liveSent = (int)Math.Round(stroke * 100.0f);
        }

    }
}

