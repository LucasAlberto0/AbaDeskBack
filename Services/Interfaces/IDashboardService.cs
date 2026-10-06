using AbaDeskBack.DTOs.Dashboard;
using AbaDeskBack.Enums;
using System;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId, Role userRole);
}
