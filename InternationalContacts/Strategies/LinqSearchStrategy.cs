using System.Xml.Linq;
using InternationalContacts.Models;

namespace InternationalContacts.Strategies;

public class LinqSearchStrategy : BaseSearchStrategy
{
    public override List<Researcher> Search(string path, SearchFilter filter)
    {
        var doc = XDocument.Load(path);
        IEnumerable<XElement> query = doc.Descendants("employee");

        if (!IsEmpty(filter.FullName))
            query = query.Where(e => ((string?)e.Attribute("FullName") ?? "")
                .Contains(filter.FullName!.Trim(), StringComparison.InvariantCultureIgnoreCase));
        if (!IsEmpty(filter.Faculty))
            query = query.Where(e => (string?)e.Attribute("Faculty") == filter.Faculty);
        if (!IsEmpty(filter.Department))
            query = query.Where(e => (string?)e.Attribute("Department") == filter.Department);
        if (!IsEmpty(filter.Specialty))
            query = query.Where(e => (string?)e.Attribute("Specialty") == filter.Specialty);
        if (filter.HasCooperationCriteria)
            query = query.Where(e => e.Elements("cooperation").Any(c =>
                CooperationMatches(ParseCooperation(
                    (string?)c.Attribute("Type"),
                    (string?)c.Attribute("Start"),
                    (string?)c.Attribute("End")), filter)));

        return query.Select(ToResearcher).ToList();
    }

    public override List<string> LoadAttributeValues(string path, string attribute) =>
        XDocument.Load(path)
            .Descendants("employee")
            .Attributes(attribute)
            .Select(a => a.Value)
            .Where(v => v.Length > 0)
            .Distinct()
            .OrderBy(v => v)
            .ToList();

    private static Researcher ToResearcher(XElement e)
    {
        var r = new Researcher();
        foreach (var a in e.Attributes()) r.SetAttribute(a.Name.LocalName, a.Value);
        foreach (var c in e.Elements("cooperation"))
            r.Cooperations.Add(ParseCooperation(
                (string?)c.Attribute("Type"),
                (string?)c.Attribute("Start"),
                (string?)c.Attribute("End")));
        return r;
    }
}
