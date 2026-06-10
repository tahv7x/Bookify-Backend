namespace Bookify_API.DTOs;

public class SetDispoDto
{
    public int IdPres {get; set;}
    public string Jour {get;set;} = string.Empty;
    public string HeureDebut { get; set; } = string.Empty;
    public string HeureFin   { get; set; } = string.Empty;
    public bool   Disponible { get; set; } = true;
}
