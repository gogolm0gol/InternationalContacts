using System.Xml;
using InternationalContacts.Models;

namespace InternationalContacts.Strategies;

public class SaxSearchStrategy : BaseSearchStrategy
{
    private static XmlReaderSettings Settings => new() { IgnoreWhitespace = true, IgnoreComments = true };

    public override List<Researcher> Search(string path, SearchFilter filter)
    {
        var result = new List<Researcher>();
        Researcher? current = null;

        using var reader = XmlReader.Create(path, Settings);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "employee")
                {
                    current = new Researcher();
                    if (reader.MoveToFirstAttribute())
                    {
                        do current.SetAttribute(reader.Name, reader.Value);
                        while (reader.MoveToNextAttribute());
                        reader.MoveToElement();
                    }
                }
                else if (reader.Name == "cooperation" && current != null)
                {
                    current.Cooperations.Add(ParseCooperation(
                        reader.GetAttribute("Type"),
                        reader.GetAttribute("Start"),
                        reader.GetAttribute("End")));
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "employee" && current != null)
            {
                if (Matches(current, filter)) result.Add(current);
                current = null;
            }
        }
        return result;
    }

    public override List<string> LoadAttributeValues(string path, string attribute)
    {
        var values = new SortedSet<string>();
        using var reader = XmlReader.Create(path, Settings);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "employee")
            {
                var v = reader.GetAttribute(attribute);
                if (!string.IsNullOrEmpty(v)) values.Add(v);
            }
        }
        return values.ToList();
    }
}
