namespace AbaDeskBack.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int InAnalysisTickets { get; set; }
    public int WaitingUserTickets { get; set; }
    public int ResolvedTickets { get; set; }
    
    public int PendingMyAction { get; set; }
    public double ResolutionRate { get; set; }
}
