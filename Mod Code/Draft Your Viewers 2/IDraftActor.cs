namespace CodeNifty.DraftYourViewers2
{
    public interface IDraftActor
    {
        bool CanHireViewerToRoster();
        bool CanAddViewerToActiveCraft();
        string HireViewerToRoster(string viewer);
        string AddViewerToActiveCraft(string viewer);

    }
}
