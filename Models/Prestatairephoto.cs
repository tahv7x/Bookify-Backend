using System;
using System.Collections.Generic;

namespace Bookify_API.Models;

public partial class Prestatairephoto
{
    public int Id { get; set; }

    public int? PrestataireId { get; set; }

    public string? Url { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Prestataire? Prestataire { get; set; }
}
