#if TARGET_UNITY
using System;
using System.IO;
using UnityEngine;
using CodeNifty.DraftYourViewers2.TwitchUtil;

namespace CodeNifty.DraftYourViewers2
{
    public class UnityDummyDraftActorController : MonoBehaviour, ISavePathGetter
    {
        private static char slash = Path.DirectorySeparatorChar;

        public bool allowHireViewerToRoster;
        public bool allowAddViewerToActiveCraft;
        public bool pretendKerbalExists;
        public string fauxGuid;

        public string fauxCampaignPath;

        public string CurrentSavePath { get { return fauxCampaignPath; } }

        public DraftManager draftManager;

        private void Awake()
        {
            fauxCampaignPath = $"{Application.persistentDataPath}{slash}Saves{slash}SinglePlayer{slash}Default";
            draftManager.savePathGetter = this;
        }

        public void OnLoadCampaign()
        {
            draftManager.OnCampaignLoaded();
        }

        public void OnUnloadCampaign()
        {
            draftManager.OnCampaignUnloaded();
        }
    }
    public class UnityDummyDraftActor : MonoBehaviour, IDraftActor
    {
        public UnityDummyDraftActorController controller;

        public bool KerbalExists(Chatter viewer, string possibleGuid)
        {
            return controller.pretendKerbalExists;
        }

        private void Start()
        {
            controller.draftManager.draftActor = this;
        }
        public string CanHireViewerToRoster()
        {
            Logger.LogInfo("CALLED CanHireViewerToRoster");
            return controller.allowHireViewerToRoster ? "" : "Dummy controller said no sry.";
        }

        public string CanAddViewerToActiveCraft()
        {
            Logger.LogInfo("CALLED CanHireViewerToRoster");
            return controller.allowAddViewerToActiveCraft ? "" : "Dummy controller said no sry.";
        }

        public string HireViewerToRoster(string viewer)
        {
            Logger.LogInfo("CALLED HireViewerToRoster");
            if (controller.fauxGuid != "")
            {
                return controller.fauxGuid;
            }
            return Guid.NewGuid().ToString();
        }

        public string AddViewerToActiveCraft(string viewer)
        {
            Logger.LogInfo("CALLED AddViewerToActiveCraft");
            if (controller.fauxGuid != "")
            {
                return controller.fauxGuid;
            }
            return Guid.NewGuid().ToString();
        }
    }
}
#endif