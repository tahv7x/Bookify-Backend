using System;
using System.ComponentModel.DataAnnotations;

namespace Bookify_API.DTOs;

public class ProposeAlternativeDto
{
    [Required]
    public DateTime ProposedDate { get; set; }

    public DateTime? ProposedEndDate { get; set; }

    public string? MessageContent { get; set; } // Optional message for the client
}
