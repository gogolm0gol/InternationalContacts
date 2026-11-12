namespace InternationalContacts.Models;
public class SearchFilter
{
    public string? FullName { get; set; }
    public string? Faculty { get; set; }
    public string? Department { get; set; }
    public string? Specialty { get; set; }
    public CooperationType? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public bool HasCooperationCriteria => Type.HasValue || DateFrom.HasValue || DateTo.HasValue;
}
