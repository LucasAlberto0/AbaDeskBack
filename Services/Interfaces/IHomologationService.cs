using AbaDeskBack.DTOs.Homologations;
using AbaDeskBack.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbaDeskBack.Services.Interfaces;

public interface IHomologationService
{
    Task<HomologationResponse> CreateHomologationAsync(Guid ticketId, CreateHomologationRequest request, Guid userId, Role userRole);
    Task<List<HomologationResponse>> GetHomologationsAsync(Guid ticketId, Guid userId, Role userRole);
}
