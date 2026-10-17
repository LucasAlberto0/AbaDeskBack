using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AbaDeskBack.Configuration;
using AbaDeskBack.DTOs.Ai;
using AbaDeskBack.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AbaDeskBack.Services.Implementations;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;
    private readonly ILogger<AiService> _logger;

    public AiService(HttpClient httpClient, IOptions<OpenAiOptions> options, ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiChatResponse> SendMessageAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("Chave da API da OpenAI não configurada.");
            throw new InvalidOperationException("O serviço de inteligência artificial não está devidamente configurado no servidor.");
        }

        if (string.IsNullOrWhiteSpace(_options.AgentId))
        {
            _logger.LogError("Agent ID da OpenAI não configurado.");
            throw new InvalidOperationException("O identificador da assistente não está configurado no servidor.");
        }

        string sessionId = request.SessionId ?? string.Empty;
        string? previousAssistantMessageId = null;

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            sessionId = await CreateSessionAsync(request.Message.Trim(), cancellationToken);
        }
        else
        {
            try
            {
                var previousMessage = await GetLatestAssistantMessageAsync(sessionId, cancellationToken);
                previousAssistantMessageId = previousMessage.Id;

                await SendSessionMessageAsync(sessionId, request.Message.Trim(), cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                sessionId = await CreateSessionAsync(request.Message.Trim(), cancellationToken);
            }
        }

        var assistantText = await WaitForAssistantResponseAsync(sessionId, previousAssistantMessageId, cancellationToken);

        return new AiChatResponse
        {
            Response = assistantText,
            SessionId = sessionId
        };
    }

    private async Task<string> CreateSessionAsync(string message, CancellationToken cancellationToken)
    {
        var payload = new
        {
            agent_id = _options.AgentId,
            environment = new { type = "none" },
            input = message,
            stream = false
        };

        using var httpRequest = CreateRequest(HttpMethod.Post, "https://api.openai.com/v1/agents/sessions", payload);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Falha ao criar sessão do agente OpenAI. Status: {StatusCode}, Detalhe: {Error}", response.StatusCode, errorContent);
            throw new InvalidOperationException("Não foi possível iniciar a sessão com a assistente.");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
        {
            return idProp.GetString()!;
        }

        throw new InvalidOperationException("Não foi possível inicializar a sessão do agente.");
    }

    private async Task SendSessionMessageAsync(string sessionId, string message, CancellationToken cancellationToken)
    {
        var payload = new
        {
            events = new[]
            {
                new
                {
                    type = "agent.session.input.message",
                    input = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new[]
                            {
                                new
                                {
                                    type = "input_text",
                                    text = message
                                }
                            }
                        }
                    }
                }
            }
        };

        using var httpRequest = CreateRequest(HttpMethod.Post, $"https://api.openai.com/v1/agents/sessions/{Uri.EscapeDataString(sessionId)}/events", payload);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Falha ao enviar mensagem para sessão OpenAI. Status: {StatusCode}, Detalhe: {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Erro ao enviar mensagem para a assistente: {response.StatusCode}", null, response.StatusCode);
        }
    }

    private async Task<string> WaitForAssistantResponseAsync(string sessionId, string? previousMessageId, CancellationToken cancellationToken)
    {
        var maxAttempts = 40;
        var delay = 750;

        for (int i = 0; i < maxAttempts; i++)
        {
            using var httpRequest = CreateRequest(HttpMethod.Get, $"https://api.openai.com/v1/agents/sessions/{Uri.EscapeDataString(sessionId)}");
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("status", out var statusProp))
                {
                    var status = statusProp.GetString();
                    if (status == "failed" || status == "cancelled")
                    {
                        throw new InvalidOperationException("A execução da assistente foi interrompida.");
                    }

                    if (status == "idle" || status == "completed")
                    {
                        var latest = await GetLatestAssistantMessageAsync(sessionId, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(latest.Text) && (previousMessageId == null || latest.Id != previousMessageId))
                        {
                            return latest.Text;
                        }
                    }
                }
            }

            await Task.Delay(delay, cancellationToken);
        }

        throw new TimeoutException("O tempo limite para obter a resposta da assistente foi excedido.");
    }

    private async Task<(string? Id, string? Text)> GetLatestAssistantMessageAsync(string sessionId, CancellationToken cancellationToken)
    {
        using var httpRequest = CreateRequest(HttpMethod.Get, $"https://api.openai.com/v1/agents/sessions/{Uri.EscapeDataString(sessionId)}/items?limit=10");
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Falha ao obter itens da sessão OpenAI. Status: {StatusCode}, Detalhe: {Error}", response.StatusCode, errorContent);
            throw new InvalidOperationException("Não foi possível obter a resposta da assistente.");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in dataProp.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "message" &&
                    item.TryGetProperty("role", out var roleProp) && roleProp.GetString() == "assistant" &&
                    item.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Array)
                {
                    string? id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

                    foreach (var part in contentProp.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var textProp))
                        {
                            var text = textProp.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                return (id, text);
                            }
                        }
                    }
                }
            }
        }

        return (null, null);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, object? payload = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.Add("OpenAI-Beta", "agents=v1");

        if (payload != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        }

        return request;
    }
}
