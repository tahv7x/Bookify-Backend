using System;
using System.ComponentModel.DataAnnotations;

namespace Bookify_API.DTOs;

public class AcceptProposalDto
{
    [Required]
    public DateTime ProposedDate { get; set; }

    public DateTime? ProposedEndDate { get; set; }
}
