#if TARGET_KERBAL
using System;
using KSP.Game;

namespace CodeNifty.DraftYourViewers2
{
    public class KerbalDraftActor : KerbalMonoBehaviour, IDraftActor
    {
        public bool CanHireViewerToRoster()
        {
            return true;
        }

        public bool CanAddViewerToActiveCraft()
        {
            return false;
        }

        public string HireViewerToRoster(string viewer)
        {
            KerbalInfo kerbal = Game.SessionManager.KerbalRosterManager.CreateKerbalByName(viewer);
            return kerbal.NameKey;
        }

        public string AddViewerToActiveCraft(string viewer)
        {
            KerbalInfo kerbal = Game.SessionManager.KerbalRosterManager.CreateKerbalByName(viewer);
            return kerbal.NameKey;

            //this.Game.ViewController.GetActiveSimVessel(true).SimulationObject.PartOwner.CrewableParts
            //Game.SessionManager.KerbalRosterManager.SetKerbalLocation(kerbal, Game.)
        }
    }
}
#endif