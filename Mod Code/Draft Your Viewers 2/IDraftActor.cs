using CodeNifty.DraftYourViewers2.TwitchUtil;

namespace CodeNifty.DraftYourViewers2
{
    public interface IDraftActor
    {
        bool KerbalExists(Chatter viewer, string possibleGuid);
        string CanHireViewerToRoster();
        string CanAddViewerToActiveCraft();
        string HireViewerToRoster(string viewer);
        string AddViewerToActiveCraft(string viewer);

    }
}
