using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NVIDIA;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game.
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks.


        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.

        private bool HeavyDutyTravelerAchievement = false; // Flag to keep track of if the "Heavy Duty Traveler" achievement has been achieved.

        private void Start()
        {
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);

            SpawnAllTanks();
            SetCameraTargets();

            SetupHighlights();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }


        private void SpawnAllTanks()
        {
            // For all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... create them, set their player number and references needed for control.
                m_Tanks[i].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }


        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            Transform[] targets = new Transform[m_Tanks.Length];

            // For each of these transforms...
            for (int i = 0; i < targets.Length; i++)
            {
                // ... set it to the appropriate tank transform.
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow.
            m_CameraControl.m_Targets = targets;
        }


        // This is called from start and will run each phase of the game one after another.
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                // Show highlights summary after game win
                ShowHighlightsSummary();

                // If there is a game winner, restart the level.
                SceneManager.LoadScene(0);
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine(GameLoop());
            }
        }


        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks();
            DisableTankControl();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
            {
                // Trigger the 'Heavy Duty Traveler' highlight if the tank has covered a specified distance
                if (!HeavyDutyTravelerAchievement && TraveledAGoodDistance())
                {
                    // Create a screenshot highlight 'Heavy Duty Traveler' and associate it with the 'Misc Group' group.
                    Highlights.ScreenshotHighlightParams shp = new Highlights.ScreenshotHighlightParams();
                    shp.groupId = "MISC_GROUP";
                    shp.highlightId = "HEAVY_DUTY_TRAVELER";
                    Highlights.SetScreenshotHighlight(shp, Highlights.DefaultSetScreenshotCallback);

                    HeavyDutyTravelerAchievement = true;
                }

                // ... return on the next frame.
                yield return null;
            }

            // Trigger 'Kaboom' highlight every other round
            if (m_RoundNumber % 2 == 0)
            {
                // Create a screenshot highlight 'Kaboom' and associate it with the 'Shot Highlight Group' group.
                Highlights.ScreenshotHighlightParams Shp = new Highlights.ScreenshotHighlightParams();
                Shp.groupId = "SHOT_HIGHLIGHT_GROUP";
                Shp.highlightId = "KABOOM";
                Highlights.SetScreenshotHighlight(Shp, Highlights.DefaultSetScreenshotCallback);
            }

            // Trigger 'Hurt Me Plenty' video highlight after the third round
            if (m_RoundNumber == 3)
            {
                // Create a video highlight 'Hurt Me Plenty' and associate it with the 'Shot Highlight Group' group.
                Highlights.VideoHighlightParams vhp = new Highlights.VideoHighlightParams();
                vhp.groupId = "SHOT_HIGHLIGHT_GROUP";
                vhp.highlightId = "HURT_ME_PLENTY";
                // Provide start and end times for the video being captured. Negative values indicate events in the past. Unit is milliseconds
                vhp.startDelta = -3000;
                vhp.endDelta = 2000;
                Highlights.SetVideoHighlight(vhp, Highlights.DefaultSetVideoCallback);
            }
        }


        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            DisableTankControl();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end.
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter.
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }


        // This function is to find out if there is a winner of the round.
        // This function is called with the assumption that 1 or fewer tanks are currently active.
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }

        // Performs all the necessary setup for the Highlights feature. It includes
        // creating the SDK, configuring all the highlights needed for the game, requesting
        // permissions and opening all highlight groups that would be used in the session.
        void SetupHighlights()
        {
            // Log message handler. Must be called before CreateHighlightsSDK.
            Highlights.AttachLogListener(Highlights.DefaultLogListener);

            // Create Highlights SDK
            Highlights.HighlightScope[] RequiredScopes = new Highlights.HighlightScope[3]
            {
                Highlights.HighlightScope.Highlights,
                Highlights.HighlightScope.HighlightsRecordVideo,
                Highlights.HighlightScope.HighlightsRecordScreenshot
            };
            System.String AppName = "Tanks";
            if (Highlights.CreateHighlightsSDK(AppName, RequiredScopes) != Highlights.ReturnCode.SUCCESS)
            {
                Debug.LogError("Failed to initialize Highlights");
                return;
            }

            int x = Highlights.PeekCallbackId();

            // Request Permissions
            Highlights.RequestPermissions(Highlights.DefaultRequestPermissionsCallback);

            // Configure Highlights
            Highlights.HighlightDefinition[] HighlightDefinitions = new Highlights.HighlightDefinition[3];

            HighlightDefinitions[0].Id = "HURT_ME_PLENTY";
            HighlightDefinitions[0].HighlightTags = Highlights.HighlightType.Achievement;
            HighlightDefinitions[0].Significance = Highlights.HighlightSignificance.Good;
            HighlightDefinitions[0].UserDefaultInterest = true;
            HighlightDefinitions[0].NameTranslationTable = new Highlights.TranslationEntry[]
            {
                new Highlights.TranslationEntry("en-US", "Hurt me plenty"),
            };

            HighlightDefinitions[1].Id = "KABOOM";
            HighlightDefinitions[1].HighlightTags = Highlights.HighlightType.Incident;
            HighlightDefinitions[1].Significance = Highlights.HighlightSignificance.Good;
            HighlightDefinitions[1].UserDefaultInterest = true;
            HighlightDefinitions[1].NameTranslationTable = new Highlights.TranslationEntry[]
            {
                new Highlights.TranslationEntry("en-US", "Kaboom!"),
            };

            HighlightDefinitions[2].Id = "HEAVY_DUTY_TRAVELER";
            HighlightDefinitions[2].HighlightTags = Highlights.HighlightType.Achievement;
            HighlightDefinitions[2].Significance = Highlights.HighlightSignificance.Good;
            HighlightDefinitions[2].UserDefaultInterest = true;
            HighlightDefinitions[2].NameTranslationTable = new Highlights.TranslationEntry[]
            {
                new Highlights.TranslationEntry("en-US", "Heavy duty traveler"),
            };

            Highlights.ConfigureHighlights(HighlightDefinitions, "en-US", Highlights.DefaultConfigureCallback);

            // Open Groups
            Highlights.OpenGroupParams Ogp1 = new Highlights.OpenGroupParams();
            Ogp1.Id = "SHOT_HIGHLIGHT_GROUP";
            Ogp1.GroupDescriptionTable = new Highlights.TranslationEntry[]
            {
                new Highlights.TranslationEntry("en-US", "Shot highlight group"),
            };
            Highlights.OpenGroup(Ogp1, Highlights.DefaultOpenGroupCallback);

            Highlights.OpenGroupParams Ogp2 = new Highlights.OpenGroupParams();
            Ogp2.Id = "MISC_GROUP";
            Ogp2.GroupDescriptionTable = new Highlights.TranslationEntry[]
            {
                new Highlights.TranslationEntry("en-US", "Misc group"),
            };
            Highlights.OpenGroup(Ogp2, Highlights.DefaultOpenGroupCallback);

        }

        private void Update()
        {
            Highlights.UpdateLog();
        }

        private void OnDestroy()
        {
            Highlights.ReleaseHighlightsSDK();
        }

        // This function checks if the player tank has traveled a certain distance
        private bool TraveledAGoodDistance()
        {
            const float TriggerDistance = 200.0f;
            if (m_Tanks.Length > 0)
            {
                // ... and if traveled TriggerDistance
                if (m_Tanks[0].DistanceTraveled() > TriggerDistance)
                    return true;
            }

            return false;
        }

        void ShowHighlightsSummary()
        {
            Highlights.GroupView[] groupViews = new Highlights.GroupView[2];
            Highlights.GroupView gv1 = new Highlights.GroupView();
            gv1.GroupId = "SHOT_HIGHLIGHT_GROUP";
            gv1.SignificanceFilter = Highlights.HighlightSignificance.Good;
            gv1.TagFilter = Highlights.HighlightType.Achievement;
            groupViews[0] = gv1;

            Highlights.GroupView gv2 = new Highlights.GroupView();
            gv2.GroupId = "MISC_GROUP";
            gv2.SignificanceFilter = Highlights.HighlightSignificance.Good;
            gv2.TagFilter = Highlights.HighlightType.Achievement;
            groupViews[1] = gv2;

            Highlights.OpenSummary(groupViews, Highlights.DefaultOpenSummaryCallback);


            Highlights.GetNumberOfHighlights(gv1, Highlights.DefaultGetNumberOfHighlightsCallback);
            Highlights.GetNumberOfHighlights(gv2, Highlights.DefaultGetNumberOfHighlightsCallback);
            Highlights.GetUserSettings(Highlights.DefaultGetUserSettingsCallback);
            Highlights.GetUILanguage(Highlights.DefaultGetUILanguageCallback);
        }
    }
}
