using AbaDeskBack.DTOs.Ai;

namespace AbaDeskBack.Services.Interfaces;

public interface IAiService
{
    Task<AiChatResponse> SendMessageAsync(AiChatRequest request, CancellationToken cancellationToken = default);
}
