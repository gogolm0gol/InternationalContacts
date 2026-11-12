using InternationalContacts.Models;
using InternationalContacts.Strategies;

namespace InternationalContacts.Services;
public class SearchContext
{
    private ISearchStrategy? _strategy;

    public void SetStrategy(ISearchStrategy strategy) => _strategy = strategy;

    public List<Researcher> Search(string path, SearchFilter filter) =>
        Strategy.Search(path, filter);

    public List<string> LoadAttributeValues(string path, string attribute) =>
        Strategy.LoadAttributeValues(path, attribute);

    private ISearchStrategy Strategy =>
        _strategy ?? throw new InvalidOperationException("Стратегію пошуку не задано.");
}
