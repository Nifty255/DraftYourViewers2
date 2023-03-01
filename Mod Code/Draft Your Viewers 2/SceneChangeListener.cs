#if TARGET_KERBAL
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using KSP.Game;
using KSP.Messages;

namespace CodeNifty.DraftYourViewers2
{
    class SceneChangeListener : KerbalMonoBehaviour
    {
        private static char slash = Path.DirectorySeparatorChar;

        public DraftManager draftManager;

        private SubscriptionHandle gameStateChangedHandler;
        private bool subscribed;

        private void Update()
        {
            if (Game == null || Game.Messages == null || subscribed) { return; }
            subscribed = true;
            gameStateChangedHandler = Game.Messages.Subscribe<GameStateChangedMessage>(new Action<MessageCenterMessage>(OnGameStateChanged));
        }

        private void OnDestroy()
        {
            Game.Messages.Unsubscribe(ref gameStateChangedHandler);
        }

        public void OnGameStateChanged(MessageCenterMessage message)
        {
            if ((message is GameStateChangedMessage stateChangedMessage))
            {
                if (
                    stateChangedMessage.CurrentState == GameState.MainMenu &&
                    stateChangedMessage.PreviousState != GameState.WarmUpLoading
                )
                {
                    draftManager.OnCampaignUnloaded();
                } else if (
                    stateChangedMessage.PreviousState == GameState.MainMenu &&
                    stateChangedMessage.CurrentState != GameState.Invalid &&
                    stateChangedMessage.CurrentState != GameState.WarmUpLoading
                )
                {
                    draftManager.OnCampaignLoaded(
                        $"{Application.persistentDataPath}{slash}Saves{Game.SessionManager.ActiveCampaignType}{slash}{Game.SessionManager.ActiveCampaignName}",
                        (string id) =>
                        {
                            return Game.SessionManager.KerbalRosterManager.GetAllKerbals().Any(kerbal => kerbal.Id.ToString() == id);
                        }
                    );
                }
            }
        }
    }
}

#endif