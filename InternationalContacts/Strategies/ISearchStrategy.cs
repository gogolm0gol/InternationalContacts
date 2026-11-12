using InternationalContacts.Models;

namespace InternationalContacts.Strategies;

public interface ISearchStrategy
{
    List<Researcher> Search(string path, SearchFilter filter);
    List<string> LoadAttributeValues(string path, string attribute);
}
