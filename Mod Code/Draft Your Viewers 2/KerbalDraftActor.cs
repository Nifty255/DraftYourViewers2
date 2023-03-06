#if TARGET_KERBAL
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.OAB;
using KSP.Sim.impl;
using CodeNifty.DraftYourViewers2.TwitchUtil;

namespace CodeNifty.DraftYourViewers2
{
    public class KerbalDraftActor : KerbalMonoBehaviour, IDraftActor
    {
        public bool KerbalExists(Chatter viewer, string possibleGuid)
        {
            return Game.SessionManager.KerbalRosterManager.GetAllKerbals().Any(kerbal =>
            {
                return possibleGuid == kerbal.Id.ToString() ||
                    viewer.DisplayName == kerbal.NameKey ||
                    viewer.DisplayName == kerbal.Attributes.FirstName;
            });
        }

        public string CanHireViewerToRoster()
        {
            return "";
        }

        public string CanAddViewerToActiveCraft()
        {
            switch (Game.GlobalGameState.GetState())
            {
                case GameState.FlightView:
                    VesselComponent vessel = Game.ViewController.GetActiveSimVessel(true);
                    if (vessel.Situation != VesselSituations.PreLaunch)
                    {
                        return "Can only add kerbals to vessel before launch.";
                    }
                    List<PartComponent> vesselParts = vessel.SimulationObject.PartOwner.CrewableParts;
                    foreach (PartComponent part in vesselParts)
                    {
                        if (part.PartData.crewCapacity < 1)
                        {
                            continue;
                        }
                        int crewInPart = Game.SessionManager.KerbalRosterManager.GetAllKerbalsInSimObject(part.GlobalId).Count;
                        if (crewInPart < part.PartData.crewCapacity)
                        {
                            return "";
                        }
                    }
                    return "The vessel is full.";
                case GameState.VehicleAssemblyBuilder:
                    List<IObjectAssemblyPart> assemblyParts = Game.OAB.Current.Stats.MainAssembly.Parts;
                    foreach(IObjectAssemblyPart part in assemblyParts)
                    {
                        if (part.AvailablePart.CrewCapacity < 1)
                        {
                            continue;
                        }
                        int crewInPart = Game.SessionManager.KerbalRosterManager.GetAllKerbalsInAssemblyPart(Game.SessionManager.KerbalRosterManager.KSCGuid, part).Count;
                        if (crewInPart < part.AvailablePart.CrewCapacity)
                        {
                            return "";
                        }
                    }
                    return "Currently selected assembly is full. Select another assembly or remove some kerbals from this one.";
                default:
                    return "Not in the VAB or flight view.";
            }
        }

        public string HireViewerToRoster(string viewer)
        {
            KerbalInfo kerbal = Game.SessionManager.KerbalRosterManager.CreateKerbalByName(viewer);
            return kerbal.Id.ToString();
        }

        public string AddViewerToActiveCraft(string viewer)
        {
            KerbalInfo kerbal = Game.SessionManager.KerbalRosterManager.CreateKerbalByName(viewer);

            List<PartComponent> crewables = Game.ViewController.GetActiveSimVessel(true).SimulationObject.PartOwner.CrewableParts;
            foreach(PartComponent crewable in crewables)
            {
                int crewInPart = Game.SessionManager.KerbalRosterManager.GetAllKerbalsInSimObject(crewable.GlobalId).Count;
                if (crewInPart < crewable.PartData.crewCapacity)
                {
                    Game.SessionManager.KerbalRosterManager.SetKerbalLocation(kerbal, crewable.SimulationObject, crewInPart);
                    return kerbal.Id.ToString();
                }
            }

            return "";
        }
    }
}
#endif