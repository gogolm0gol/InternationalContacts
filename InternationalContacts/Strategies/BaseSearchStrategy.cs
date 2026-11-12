using System.Globalization;
using InternationalContacts.Models;

namespace InternationalContacts.Strategies;

public abstract class BaseSearchStrategy : ISearchStrategy
{
    public abstract List<Researcher> Search(string path, SearchFilter filter);
    public abstract List<string> LoadAttributeValues(string path, string attribute);

    protected static bool IsEmpty(string? s) => string.IsNullOrWhiteSpace(s);

    protected static bool Matches(Researcher r, SearchFilter f)
    {
        if (!IsEmpty(f.FullName) &&
            !r.FullName.Contains(f.FullName!.Trim(), StringComparison.InvariantCultureIgnoreCase)) return false;
        if (!IsEmpty(f.Faculty) && !string.Equals(r.Faculty, f.Faculty, StringComparison.OrdinalIgnoreCase)) return false;
        if (!IsEmpty(f.Department) && !string.Equals(r.Department, f.Department, StringComparison.OrdinalIgnoreCase)) return false;
        if (!IsEmpty(f.Specialty) && !string.Equals(r.Specialty, f.Specialty, StringComparison.OrdinalIgnoreCase)) return false;
        return !f.HasCooperationCriteria || r.Cooperations.Any(c => CooperationMatches(c, f));
    }

    protected static bool CooperationMatches(Cooperation c, SearchFilter f)
    {
        if (f.Type.HasValue && c.Type != f.Type.Value) return false;
        if (f.DateFrom.HasValue && c.EndDate < f.DateFrom.Value) return false;
        if (f.DateTo.HasValue && c.StartDate > f.DateTo.Value) return false;
        return true;
    }

    protected static Cooperation ParseCooperation(string? type, string? start, string? end) =>
        new(Enum.Parse<CooperationType>(type ?? ""))
        {
            StartDate = DateTime.ParseExact(start ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture),
            EndDate = DateTime.ParseExact(end ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture)
        };
}
