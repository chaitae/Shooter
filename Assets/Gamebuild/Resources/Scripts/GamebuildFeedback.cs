using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gamebuild.Feedback
{
    public class GamebuildFeedback : MonoBehaviour
    {
        [Header("Your Settings")]
        [Header("Enable this to ignore game sessions in Unity Editor.")]
        public bool PublishedBuildsOnly;
        
        [Header("If you want to manage input yourself, disable the field below.")]
        [Tooltip("Enable this script to manage player input to pull up the feedback panel")]
        public bool EnableInputFromGamebuildScipt;
        [Tooltip("Key users press to open the feedback panel.")]
        public KeyCode cornerPopUpInputKey;
        //[Tooltip("Toggles the cursor lock and screen lock.")]
        //public bool ToggleScreenLock;
        [Tooltip("Your gamebuild token for the project, can be found at gamebuild.io")]
        public string gamebuildToken;

        

        [Space(10)]

        [Header("Audio")]
        [Tooltip("Turn on chimes for the feedback panels")]
        public bool playChime;
        public AudioClip chimeSound;
        public AudioSource chimeAudioSource;

        private FeedbackPopUpController feedbackPopUpController;
        private static GamebuildPopUpController gamebuildPopUpController;
        [HideInInspector]
        public bool panelShowing = false;
        private bool showingFeedbackPanel;
        private static Image backgroundImage;
        [HideInInspector]
        public bool inputFieldFocused = false;

        private static GameBuildData gameBuildData;
        
        [SerializeField]
        private GameObject canvasUI;

        public enum QuestionType
        {
            Reaction = 0,
            Linear = 1,
        }

        public bool IgnoreEditorSessions()
        {
            if (PublishedBuildsOnly && Application.isEditor)
            {
                return true;
            }
            return false;

        }

        // Initialize references
        private void Awake()
        {
            //canvasUI.SetActive(true);
            //if (IgnoreEditorSessions())
            //{
            //    gameObject.SetActive(false);
            //    return;
            //}


            ////Check if game object exists, if so destroy this one
            //if (GameObject.Find("Gamebuild Feedback Canvas") != null && GameObject.Find("Gamebuild Feedback Canvas") != gameObject)
            //{
            //    Destroy(gameObject);
            //    return;
            //}
            
            ////Get GameBuildData and enable it
            //gameBuildData = GetComponentInChildren<GameBuildData>();
            //gameBuildData.enabled = true;
            
            //transform.GetChild(0).gameObject.SetActive(true);
            //DontDestroyOnLoad(gameObject);
            //feedbackPopUpController = GetComponentInChildren<FeedbackPopUpController>();
            //feedbackPopUpController.gamebuildFeedback = this;
            //gamebuildPopUpController = GetComponentInChildren<GamebuildPopUpController>();
            
        }

        public static void CaptureMoment(string moment)
        {
            //Wait until gamebuilddata initalizedSuccessfully is true
            if (!gameBuildData.initalizedSuccessfully)
            {
                Debug.Log("Gamebuild data not initialized, waiting...");
                return;
            }

            string json = " { \"moment\": \"" + moment + "\" }";
            gameBuildData.CaptureDataAndSend(json, false);
        }

        public static void GameStartMoment()
        {

        }
        
        // Handle input and panel visibility
        void Update()
        {
            if (IgnoreEditorSessions())
            {
                return;
            }
            HandleInput();
            UpdatePanelVisibility();
        }

        private void HandleInput()
        {
            if (UnityEngine.Input.GetKeyDown(cornerPopUpInputKey))
            {
                Debug.Log("Showing Feedback Popup");
                ToggleFeedbackPanel();
            }
        }

        private void UpdatePanelVisibility()
        {
            if (feedbackPopUpController == null)
            {
                return;
            }
            
            panelShowing = feedbackPopUpController.feedbackPanelShowing ||
                           gamebuildPopUpController.customFeedbackPanelShowing;
        }

        // Toggle the feedback panel
        public void ToggleFeedbackPanel()
        {
            if (!IsAnyInputFieldFocused())
            {
                showingFeedbackPanel = !showingFeedbackPanel;
                panelShowing = showingFeedbackPanel;

                feedbackPopUpController.gameObject.SetActive(showingFeedbackPanel);

                if (showingFeedbackPanel)
                {
                    feedbackPopUpController.ShowPopUp();
                }
                else
                {
                    feedbackPopUpController.HidePopUp();
                }
            }
        }

        private bool IsAnyInputFieldFocused()
        {
            return feedbackPopUpController.inputField.isFocused ||
                   gamebuildPopUpController.questionInputField.isFocused;
        }


        // Darken the background
        public static void DarkenBackground()
        {
            backgroundImage.DOFade(0.45f, 1f);
        }

        // Lighten the background
        public static void LightenBackground()
        {
            backgroundImage.DOFade(0f, 1f);
        }

        // Show a custom popup with the specified question and options
        public static void Prompt(string question, QuestionType questionType, bool showInputField)
        {
            if (gamebuildPopUpController == null)
            {
                Debug.Log("Gamebuild PopUp Controller not found");
                return;
            }
            
            gamebuildPopUpController.ShowPlayerPopUp(question, questionType, showInputField);
        }

        public static void Prompt(string question, QuestionType questionType, bool showInputField, List<string> customFeedback)
        {
            gamebuildPopUpController.ShowPlayerPopUp(question, questionType, showInputField, customFeedback);
        }

        public static void Prompt(string question, QuestionType questionType, bool showInputField, List<string> customFeedback, Action runBeforePopUp, Action runAfterPopUp)
        {
            gamebuildPopUpController.ShowPlayerPopUp(question, questionType, showInputField, customFeedback, runAfterPopUp);
        }

        public static void Prompt(string question, QuestionType questionType, bool showInputField, Action runBeforePopUp, Action runAfterPopUp)
        {
            runBeforePopUp?.Invoke();

            gamebuildPopUpController.ShowPlayerPopUp(question, questionType, showInputField, runAfterPopUp);
        }
    }
}
