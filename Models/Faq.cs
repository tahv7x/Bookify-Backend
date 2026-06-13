using System;

namespace Bookify_API.Models;

public partial class Faq
{
    public int IdFaq { get; set; }

    public string Question { get; set; } = null!;

    public string Reponse { get; set; } = null!;

    public DateTime? DateCreation { get; set; }
}
