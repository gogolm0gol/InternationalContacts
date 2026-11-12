namespace InternationalContacts.Models;

public class Cooperation
{
    public CooperationType Type { get; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public Cooperation(CooperationType type) => Type = type;

    public override string ToString() =>
        $"{Type.ToUkrainian()}: {StartDate:dd.MM.yyyy} – {EndDate:dd.MM.yyyy}";
}
