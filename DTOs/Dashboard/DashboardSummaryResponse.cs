namespace AbaDeskBack.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int WaitingHomologationTickets { get; set; }
    public int ResolvedTickets { get; set; }
    
    // Indica chamados que estão dependendo da ação do usuário logado (ex: homologação pendente, ou em dev pra ele)
    public int PendingMyAction { get; set; }
}
