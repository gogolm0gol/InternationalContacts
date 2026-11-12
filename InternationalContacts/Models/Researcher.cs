namespace InternationalContacts.Models;

public class Researcher
{
    public string FullName { get; set; }
    public string Faculty { get; set; } = "";
    public string Department { get; set; } = "";
    public string Specialty { get; set; } = "";
    public Dictionary<string, string> ExtraAttributes { get; } = new();
    public List<Cooperation> Cooperations { get; } = new();

    public Researcher(string fullName = "") => FullName = fullName;

    public void SetAttribute(string name, string value)
    {
        switch (name)
        {
            case "FullName": FullName = value; break;
            case "Faculty": Faculty = value; break;
            case "Department": Department = value; break;
            case "Specialty": Specialty = value; break;
            default: ExtraAttributes[name] = value; break;
        }
    }
    public string ExtraText => string.Join(", ", ExtraAttributes.Select(kv => $"{kv.Key}: {kv.Value}"));
    public string CooperationsText => string.Join("\n", Cooperations.Select(c => c.ToString()));
}
