using System;
using System.ComponentModel.DataAnnotations;

namespace AbaDeskBack.DTOs.Tickets;

public class AssignTicketRequest
{
    [Required]
    public Guid UserId { get; set; }
}
