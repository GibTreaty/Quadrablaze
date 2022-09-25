using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DG {
    using Util;
    using TrelloAPI;
    using Quadrablaze;
    using UnityEngine.Events;
    using Steamworks;
    public class UsageExample : Singleton<UsageExample> {
        [Header("Personal Trello Information")]
        public string yourKey = "Your Key";
        public string yourToken = "Your Token";
        public string currentBoard = "Your Trello Board";

        [Space(15)]
        [Tooltip("Places new uploaded cards on top of the list")]
        public bool newCardsOnTop = true;

        [Space(15)]
        [Header("Setup report types to appear in the dropdown here")]
        public List<Dropdown.OptionData> reportTypes;

        [Space(15)]
        [Header("UI Objects")]
        public GameObject inProgressUI;
        public GameObject successUI;
        public GameObject fillInFormMessageUI;

        //Singleton instance
        //public static UsageExample instance;

        // Trello API obj
        private Trello trello;

        // Platform dependent Log Path
        private string logPath {
            get {

#if UNITY_STANDALONE_WIN
                var absolutePath = "%USERPROFILE%/AppData/LocalLow/" + Application.companyName + "/" + Application.productName + "/Player.log";
                //var absolutePath = "%USERPROFILE%/AppData/LocalLow/" + Application.companyName + "/" + Application.productName + "/output_log.txt";
                var filePath = System.Environment.ExpandEnvironmentVariables(absolutePath);
                return filePath;
                //Old windows log path
                //return System.Diagnostics.Process.GetCurrentProcess().ProcessName + "_Data/output_log.txt";

#elif UNITY_STANDALONE_LINUX
                return "~/.config/unity3d/" + Application.companyName + "/" + Application.productName + "/Player.log";

#elif UNITY_STANDALONE_OSX
                return "~/Library/Logs/Unity/Player.log";
#else
                return "";
#endif
            }
        }
        // Platform dependent Log Path copy
        private string logPathCopy {
            get {
#if UNITY_STANDALONE_WIN
                var absolutePath = "%USERPROFILE%/AppData/LocalLow/" + Application.companyName + "/" + Application.productName + "/output_logCopy.txt";
                var filePath = System.Environment.ExpandEnvironmentVariables(absolutePath);
                return filePath;
                //Old windows log path
                //return System.Diagnostics.Process.GetCurrentProcess().ProcessName + "_Data/output_log2.txt";

#elif UNITY_STANDALONE_LINUX
                return "~/.config/unity3d/" + Application.companyName + "/" + Application.productName + "/PlayerCopy.log";

#elif UNITY_STANDALONE_OSX
               return "~/Library/Logs/Unity/PlayerCopy.log";
#else
                return "";
#endif
            }
        }

        public IEnumerator Start() {
            //Checks if we are already connected
            if(trello != null && trello.IsConnected()) {
                Debug.Log("Connection with Trello server succesful");
                yield break;
            }

            // Creates our trello Obj with our key and token
            trello = new Trello(yourKey, yourToken);

            // gets the boards of the current user
            yield return trello.PopulateBoardsRoutine();
            trello.SetCurrentBoard(currentBoard);

            // gets the lists on the current board
            yield return trello.PopulateListsRoutine();

            // check if our reportType match the lists in your trello board
            // otherwise it creates new lists and uploads them
            for(int i = 0; i < reportTypes.Count; i++) {
                if(!trello.IsListCached(reportTypes[i].text)) {
                    var optionList = trello.NewList(reportTypes[i].text);
                    yield return trello.UploadListRoutine(optionList);
                }
            }

            // caches the new lists created (if any)
            yield return trello.PopulateListsRoutine();
        }

        public IEnumerator SendReportRoutine(TrelloCard card, List<Texture2D> screenshots) {
            fillInFormMessageUI.SetActive(false);

            // Shows the "in progress" text
            inProgressUI.SetActive(true);

            // We upload the card with an async custom coroutine that will return the card ID
            // Once it has been uploaded.
            CustomCoroutine cC = new CustomCoroutine(this, trello.UploadCardRoutine(card));
            yield return cC.coroutine;

            // The uploaded card ID
            string cardID = (string)cC.result;

            int i = 0;
            foreach(Texture2D screenshot in screenshots) {
                i++;
                // We can now attach the screenshot to the card given its ID.
                yield return trello.SetUpAttachmentInCardRoutine(cardID, "ScreenShot" + i + ".png", screenshot);
            }

#if UNITY_STANDALONE
            // We make sure the log exists before trying to retrieve it.
            if(System.IO.File.Exists(logPath)) {
                // We make a copy of the log since the original is being used by Unity.
                System.IO.File.Copy(logPath, logPathCopy, true);

                // We attach the Unity log file to the card.
                yield return trello.SetUpAttachmentInCardFromFileRoutine(cardID, "Player.log", logPathCopy);
            }
#endif
            // this one is meant to be replaced with relevant data about your game
            //string relevantData = "Deaths: 100, coins: 50, respect: 0.5";
            string relevantData = "Game Version:" + GameManager.Current.GameVersion;

            if(SteamManager.Initialized)
                relevantData += '\n' + "Steam Username: " + SteamFriends.GetPersonaName();

            yield return trello.SetUpAttachmentInCardRoutine(cardID, "GameData.txt", relevantData);

            /**
            *
            *   Attach more convenient data to the card here
            *
            **/

            // Wait for one extra second to let the player read that his issue is being processed
            yield return new WaitForSeconds(1);

            // Since we are done we can deactivate the in progress canvas
            inProgressUI.SetActive(false);
            fillInFormMessageUI.SetActive(false);

            // Now we show the success text to let the user know the action has been completed
            StartCoroutine(SetActiveForSecondsRoutine(successUI, 2));
        }

        // Sets gameObject active or inactive for timeInSeconds
        public IEnumerator SetActiveForSecondsRoutine(GameObject gameObject, float timeInSeconds, bool setActive = true) {
            gameObject.SetActive(setActive);
            yield return new WaitForSeconds(timeInSeconds);
            gameObject.SetActive(!setActive);
        }

        public Coroutine SendReport(string title, string description, string listName, List<Texture2D> screenshots) {
            if(title.Trim() == "" || description.Trim() == "") {
                StartCoroutine(SetActiveForSecondsRoutine(fillInFormMessageUI, 2));
                return null;
            }

            TrelloCard card = trello.NewCard(title, description, listName);
            return StartCoroutine(SendReportRoutine(card, screenshots));
        }

        public Coroutine SendReport(string title, string description, string listName, Texture2D screenshot) {
            List<Texture2D> screenshots = new List<Texture2D> { screenshot };
            return SendReport(title, description, listName, screenshots);
        }
    }
}