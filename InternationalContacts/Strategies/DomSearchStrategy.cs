using System.Globalization;
using System.Text;
using System.Xml;
using InternationalContacts.Models;

namespace InternationalContacts.Strategies;
public class DomSearchStrategy : BaseSearchStrategy
{
    private const string Alphabet = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string Upper = Alphabet;
    private static readonly string Lower = Alphabet.ToLowerInvariant();

    public string LastXPath { get; private set; } = "";

    public override List<Researcher> Search(string path, SearchFilter filter)
    {
        var doc = new XmlDocument();
        doc.Load(path);

        LastXPath = BuildXPath(filter);
        var result = new List<Researcher>();

        foreach (XmlNode node in doc.SelectNodes(LastXPath)!)
        {
            var r = new Researcher();
            foreach (XmlAttribute a in node.Attributes!) r.SetAttribute(a.Name, a.Value);
            foreach (XmlNode c in node.SelectNodes("cooperation")!)
            {
                r.Cooperations.Add(ParseCooperation(
                    c.Attributes?["Type"]?.Value,
                    c.Attributes?["Start"]?.Value,
                    c.Attributes?["End"]?.Value));
            }
            result.Add(r);
        }
        return result;
    }

    public override List<string> LoadAttributeValues(string path, string attribute)
    {
        var doc = new XmlDocument();
        doc.Load(path);
        return doc.SelectNodes($"//employee/@{attribute}")!
            .Cast<XmlNode>()
            .Select(n => n.Value ?? "")
            .Where(v => v.Length > 0)
            .Distinct()
            .OrderBy(v => v)
            .ToList();
    }

    private static string BuildXPath(SearchFilter f)
    {
        var sb = new StringBuilder("//employee");

        if (!IsEmpty(f.FullName))
        {
            var needle = f.FullName!.Trim().ToLowerInvariant();
            sb.Append($"[contains(translate(@FullName,'{Upper}','{Lower}'),{Literal(needle)})]");
        }
        if (!IsEmpty(f.Faculty)) sb.Append($"[@Faculty={Literal(f.Faculty!)}]");
        if (!IsEmpty(f.Department)) sb.Append($"[@Department={Literal(f.Department!)}]");
        if (!IsEmpty(f.Specialty)) sb.Append($"[@Specialty={Literal(f.Specialty!)}]");

        if (f.HasCooperationCriteria)
        {
            var coop = new StringBuilder();
            if (f.Type.HasValue) coop.Append($"[@Type='{f.Type.Value}']");
            if (f.DateFrom.HasValue)
                coop.Append($"[number(translate(@End,'-',''))>={f.DateFrom.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}]");
            if (f.DateTo.HasValue)
                coop.Append($"[number(translate(@Start,'-',''))<={f.DateTo.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}]");
            sb.Append($"[cooperation{coop}]");
        }
        return sb.ToString();
    }

    private static string Literal(string s)
    {
        if (!s.Contains('\'')) return $"'{s}'";
        if (!s.Contains('"')) return $"\"{s}\"";
        return "concat('" + s.Replace("'", "',\"'\",'") + "')";
    }
}
