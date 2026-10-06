namespace AbaDeskBack.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    
    // Specific for Attendant/Admin
    public int InAnalysisTickets { get; set; }
    public int InDevelopmentTickets { get; set; }
    public int InTestTickets { get; set; }
    
    public int WaitingHomologationTickets { get; set; }
    public int ResolvedTickets { get; set; }
    
    public int PendingMyAction { get; set; }
    public double ResolutionRate { get; set; }
}
