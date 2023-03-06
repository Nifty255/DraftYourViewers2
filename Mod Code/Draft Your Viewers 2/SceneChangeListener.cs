#if TARGET_KERBAL
using System;
using System.IO;
using UnityEngine;
using KSP.Game;
using KSP.Messages;

namespace CodeNifty.DraftYourViewers2
{
    class SceneChangeListener : KerbalMonoBehaviour, ISavePathGetter
    {
        private static char slash = Path.DirectorySeparatorChar;

        public DraftManager draftManager;

        private SubscriptionHandle gameStateChangedHandler;
        private bool subscribed;
        private bool sceneChanged;

        public string CurrentSavePath { get; private set; }

        private void FixedUpdate()
        {
            if (!subscribed && Game != null && Game.Messages != null)
            {
                SubscribeToGameStateChanges();
            }

            if (sceneChanged && Game.SaveLoadManager.IsLoaded)
            {
                sceneChanged = false;
                CurrentSavePath = $"{Application.persistentDataPath}{slash}Saves{slash}{Game.SessionManager.ActiveCampaignType}{slash}{Game.SessionManager.ActiveCampaignName}";
                draftManager.OnCampaignLoaded();
            }
        }

        private void OnDestroy()
        {
            Game.Messages.Unsubscribe(ref gameStateChangedHandler);
        }

        private void SubscribeToGameStateChanges()
        {
            subscribed = true;
            gameStateChangedHandler = Game.Messages.Subscribe<GameStateChangedMessage>(new Action<MessageCenterMessage>(OnGameStateChanged));
            Logger.LogInfo("Subscribed to game state changes.");
        }

        public void OnGameStateChanged(MessageCenterMessage message)
        {
            if ((message is GameStateChangedMessage stateChangedMessage))
            {
                switch(stateChangedMessage.CurrentState)
                {
                    case GameState.MainMenu:
                        if (stateChangedMessage.PreviousState != GameState.WarmUpLoading)
                        {
                            draftManager.OnCampaignUnloaded();
                        }
                        break;
                    case GameState.Invalid:
                    case GameState.WarmUpLoading:
                        break;
                    default:
                        sceneChanged = true;
                        break;
                }
            }
        }
    }
}
#endif