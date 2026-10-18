using System;

namespace AbaDeskBack.DTOs.Tickets;

public class TicketAttachmentDto
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "image/jpeg";
    public string FileName { get; set; } = "anexo.jpg";
}
