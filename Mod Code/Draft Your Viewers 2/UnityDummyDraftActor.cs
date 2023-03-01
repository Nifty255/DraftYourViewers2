#if TARGET_UNITY
using System;
using System.IO;
using UnityEngine;

namespace CodeNifty.DraftYourViewers2
{
    public class UnityDummyDraftActorController : MonoBehaviour
    {
        private static char slash = Path.DirectorySeparatorChar;

        public bool allowHireViewerToRoster;
        public bool allowAddViewerToActiveCraft;
        public bool pretendKerbalExists;
        public string fauxGuid;

        public string fauxCampaignPath;

        public DraftManager draftManager;

        private void Awake()
        {
            fauxCampaignPath = $"{Application.persistentDataPath}{slash}Saves{slash}SinglePlayer{slash}Default";
        }

        public void OnLoadCampaign()
        {
            draftManager.OnCampaignLoaded(fauxCampaignPath, (string kerbalGuid) => pretendKerbalExists);
        }

        public void OnUnloadCampaign()
        {
            draftManager.OnCampaignUnloaded();
        }
    }
    public class UnityDummyDraftActor : MonoBehaviour, IDraftActor
    {
        public UnityDummyDraftActorController controller;

        private void Start()
        {
            controller.draftManager.draftActor = this;
        }
        public bool CanHireViewerToRoster()
        {
            Logger.LogInfo("CALLED CanHireViewerToRoster");
            return controller.allowHireViewerToRoster;
        }

        public bool CanAddViewerToActiveCraft()
        {
            Logger.LogInfo("CALLED CanHireViewerToRoster");
            return controller.allowAddViewerToActiveCraft;
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