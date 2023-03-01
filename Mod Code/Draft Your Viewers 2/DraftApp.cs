using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CodeNifty.DraftYourViewers2
{
    // Frontend UI handler for DYV 2. Handles user actions and initial startup.

    // Since the majority of events are user-initiated, the Draft App calls into the Draft Manager. Only 2 callbacks from the Draft Manager are required, on-streamer-authenticated, and on-campaign-loaded.
    public class DraftApp : MonoBehaviour
    {
        private readonly string[] craftDraftFlavorText = {
            "Have a safe trip.",
            "Remember to pack a parachute.",
            "Don't forget the Dr. Kerber!",
            "Hurry up and launch already!",
            "All crew accounted for?"
        };
        private readonly string[] rosterDraftFlavorText = {
            "Best get to training.",
            "Enjoy our state of the art recreation and ameneties.",
            "Try talking to Gene. He gets bored between launches.",
            "Snacks are down the hall to the left.",
            "Watch out for falling rocket parts outside."
        };
        private readonly string[] viewerDrawingFlavorText = {
            "What did you win?",
            "You're so lucky... Or are you unlucky?",
            "Throw the confetti! Dance the night away!",
            "OMG OMG OMG!",
            "Hecc yea! You won!"
        };

        public DraftManager draftManager;

        public GameObject container;
        private GameObject currentPanel;

        // Twitch Authorization
        public GameObject AuthPanel;

        // Home - Drafting / Drawing
        public GameObject HomePanel;
        public TextTyper StatusTextTyper;

        // Config & Save
        public GameObject SettingsPanel;
        public InputField BlockedUsernamesInput;
        public Toggle IncludeDraftedInDrawingsToggle;
        public Toggle IncludeDrawnInDrawingsToggle;
        public Toggle AddKermanToggle;

        // Confirm dialog
        public GameObject ConfirmPanel;
        public Text ConfirmText;
        private Action confirmAction;
        private Action nevermindAction;

        private string streamerDisplayName;

        private void Start()
        {
            currentPanel = AuthPanel;
            draftManager.AppOnStreamerAuthenticated = new Action<string>(OnStreamerAuthenticated);
            draftManager.AppOnCampaignLoaded = new Action<bool>(OnCampaignLoaded);
            draftManager.SetUp();
        }

        public void OnCampaignLoaded(bool isLoaded)
        {
            StatusTextTyper.FullText = $"Ready to draft from: {streamerDisplayName}";
            container.SetActive(isLoaded);
        }

        public void OnAuthorizeButtonClick()
        {
            draftManager.AuthorizeStreamer();
        }

        public void OnStreamerAuthenticated(string displayName)
        {
            streamerDisplayName = displayName;
            StatusTextTyper.FullText = $"Ready to draft from: {streamerDisplayName}";
            currentPanel.SetActive(false);
            HomePanel.SetActive(true);
            currentPanel = HomePanel;
        }

        public void OnDraftViewerIntoActiveCraftButtonClick()
        {
            StatusTextTyper.FullText = "Drafting...";
            draftManager.DraftViewerIntoActiveCraft(
                new Action<string>((viewerDisplayName) =>
                {
                    StatusTextTyper.FullText = $"Drafted {viewerDisplayName} into the craft!\n\n{craftDraftFlavorText[UnityEngine.Random.Range(0, craftDraftFlavorText.Length)]}";
                }),
                new Action<string>((errorText) =>
                {
                    StatusTextTyper.FullText = errorText;
                })
            );
        }

        public void OnDraftViewerIntoRosterButtonClick()
        {
            StatusTextTyper.FullText = "Drafting...";
            draftManager.DraftViewerIntoRoster(
                new Action<string>((viewerDisplayName) =>
                {
                    StatusTextTyper.FullText = $"Drafted {viewerDisplayName} into the roster!\n\n{rosterDraftFlavorText[UnityEngine.Random.Range(0, rosterDraftFlavorText.Length)]}";
                }),
                new Action<string>((errorText) =>
                {
                    StatusTextTyper.FullText = errorText;
                })
            );
        }

        public void OnDrawViewerButtonClick()
        {
            StatusTextTyper.FullText = "Drawing...";
            draftManager.DrawRandomViewer(
                new Action<string>((viewerDisplayName) =>
                {
                    StatusTextTyper.FullText = $"Congratulations, {viewerDisplayName}!\n\n{viewerDrawingFlavorText[UnityEngine.Random.Range(0, viewerDrawingFlavorText.Length)]}";
                }),
                new Action<string>((errorText) =>
                {
                    StatusTextTyper.FullText = errorText;
                })
            );
        }

        public void OnSettingsButtonClick()
        {
            BlockedUsernamesInput.text = string.Join("\n", draftManager.Config.blockedUsernames.ToArray());
            IncludeDraftedInDrawingsToggle.isOn = draftManager.Config.includeDraftedInDrawings;
            IncludeDrawnInDrawingsToggle.isOn = draftManager.Config.includeDrawnInDrawings;
            AddKermanToggle.isOn = draftManager.Config.addKerman;

            currentPanel.SetActive(false);
            SettingsPanel.SetActive(true);
            currentPanel = SettingsPanel;
        }

        public void OnClearSaveButtonClick()
        {
            confirmAction = new Action(() => {
                draftManager.ClearSave();
                currentPanel.SetActive(false);
                SettingsPanel.SetActive(true);
                currentPanel = SettingsPanel;
            });
            nevermindAction = new Action(() =>
            {
                currentPanel.SetActive(false);
                SettingsPanel.SetActive(true);
                currentPanel = SettingsPanel;
            });
            ConfirmText.text = "Are you sure you want to clear this campaign save?";
            currentPanel.SetActive(false);
            ConfirmPanel.SetActive(true);
            currentPanel = ConfirmPanel;
        }

        public void OnClearConfigButtonClick()
        {
            confirmAction = new Action(() => {
                draftManager.ClearConfig();
                streamerDisplayName = "";
                currentPanel.SetActive(false);
                AuthPanel.SetActive(true);
                currentPanel = AuthPanel;
            });
            nevermindAction = new Action(() =>
            {
                currentPanel.SetActive(false);
                SettingsPanel.SetActive(true);
                currentPanel = SettingsPanel;
            });
            ConfirmText.text = "Are you sure you want to clear your global config?\n\nYou will need to reauthorize Draft Your Viewers 2!";
            currentPanel.SetActive(false);
            ConfirmPanel.SetActive(true);
            currentPanel = ConfirmPanel;
        }

        public void OnSaveButtonClick()
        {
            draftManager.Config.blockedUsernames = new List<string>(BlockedUsernamesInput.text.Split('\n'));
            draftManager.Config.includeDraftedInDrawings = IncludeDraftedInDrawingsToggle.isOn;
            draftManager.Config.includeDrawnInDrawings = IncludeDrawnInDrawingsToggle.isOn;
            draftManager.Config.addKerman = AddKermanToggle.isOn;
            FileManager.SaveDraftConfig(draftManager.Config);
            currentPanel.SetActive(false);
            HomePanel.SetActive(true);
            currentPanel = HomePanel;
        }

        public void OnCancelButtonClick()
        {
            currentPanel.SetActive(false);
            HomePanel.SetActive(true);
            currentPanel = HomePanel;
        }

        public void OnConfirmButtonClick()
        {
            confirmAction.Invoke();
        }

        public void OnNevermindButtonClick()
        {
            nevermindAction.Invoke();
        }
    }
}
