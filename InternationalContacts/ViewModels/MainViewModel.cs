using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternationalContacts.Models;
using InternationalContacts.Services;
using InternationalContacts.Strategies;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InternationalContacts.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string All = "(усі)";

    private readonly SearchContext _context = new();
    private readonly HtmlConverter _converter = new();
    private readonly ISearchStrategy[] _strategies =
        { new SaxSearchStrategy(), new DomSearchStrategy(), new LinqSearchStrategy() };

    private string _xmlPath = "";
    private string _xslPath = "";

    public string[] StrategyNames { get; } =
        { "SAX API (XmlReader)", "DOM API (XmlDocument + XPath)", "LINQ to XML (XDocument)" };

    public List<string> CooperationTypeNames { get; } =
        new[] { All }.Concat(Enum.GetValues<CooperationType>().Select(t => t.ToUkrainian())).ToList();

    public ObservableCollection<string> Faculties { get; } = new();
    public ObservableCollection<string> Departments { get; } = new();
    public ObservableCollection<string> Specialties { get; } = new();
    public ObservableCollection<Researcher> Results { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectXmlCommand))]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private int selectedStrategyIndex = -1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(TransformCommand))]
    private string xmlFileName = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TransformCommand))]
    private string xslFileName = "";

    [ObservableProperty] private string fullNameText = "";
    [ObservableProperty] private string? selectedFaculty;
    [ObservableProperty] private string? selectedDepartment;
    [ObservableProperty] private string? selectedSpecialty;
    [ObservableProperty] private int selectedTypeIndex;
    [ObservableProperty] private bool useDates;
    [ObservableProperty] private DateTime dateFrom = new(2024, 1, 1);
    [ObservableProperty] private DateTime dateTo = new(2024, 12, 31);
    [ObservableProperty] private string statusText = "Крок 1: оберіть метод пошуку.";
    [ObservableProperty] private HtmlWebViewSource? htmlSource;

    private bool HasStrategy => SelectedStrategyIndex is >= 0 and < 3;
    private bool HasXml => !string.IsNullOrEmpty(XmlFileName);
    private bool CanSearch => HasStrategy && HasXml;
    private bool HasXmlAndXsl => HasXml && !string.IsNullOrEmpty(XslFileName);

    private ISearchStrategy CurrentStrategy =>
        HasStrategy ? _strategies[SelectedStrategyIndex]
                    : throw new InvalidOperationException("Спочатку оберіть метод пошуку.");


    [RelayCommand(CanExecute = nameof(HasStrategy))]
    private async Task SelectXmlAsync()
    {
        try
        {
            var path = await PickToCacheAsync("Оберіть XML-файл", ".xml");
            if (path == null) return;

            _xmlPath = path;
            XmlFileName = Path.GetFileName(path);

            _context.SetStrategy(CurrentStrategy);
            Fill(Faculties, await Task.Run(() => _context.LoadAttributeValues(_xmlPath, "Faculty")));
            Fill(Departments, await Task.Run(() => _context.LoadAttributeValues(_xmlPath, "Department")));
            Fill(Specialties, await Task.Run(() => _context.LoadAttributeValues(_xmlPath, "Specialty")));
            SelectedFaculty = SelectedDepartment = SelectedSpecialty = All;

            await SearchAsync();
        }
        catch (Exception ex) { StatusText = "Помилка: " + ex.Message; }
    }

    [RelayCommand]
    private async Task SelectXslAsync()
    {
        try
        {
            var path = await PickToCacheAsync("Оберіть XSL-файл", ".xsl", ".xslt");
            if (path == null) return;
            _xslPath = path;
            XslFileName = Path.GetFileName(path);
            StatusText = "XSL обрано: " + XslFileName;
        }
        catch (Exception ex) { StatusText = "Помилка: " + ex.Message; }
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync()
    {
        try
        {
            _context.SetStrategy(CurrentStrategy);
            var filter = BuildFilter();

            var sw = Stopwatch.StartNew();
            var found = await Task.Run(() => _context.Search(_xmlPath, filter));
            sw.Stop();

            Results.Clear();
            foreach (var r in found) Results.Add(r);

            StatusText = $"Метод: {StrategyNames[SelectedStrategyIndex]}. " +
                         $"Знайдено: {found.Count}. Час: {sw.Elapsed.TotalMilliseconds:F1} мс";
        }
        catch (Exception ex) { StatusText = "Помилка: " + ex.Message; }
    }

    [RelayCommand(CanExecute = nameof(HasXmlAndXsl))]
    private async Task TransformAsync()
    {
        try
        {
            var htmlPath = Path.Combine(FileSystem.AppDataDirectory, "contacts.html");
            await Task.Run(() => _converter.Convert(_xmlPath, _xslPath, htmlPath));
            HtmlSource = new HtmlWebViewSource { Html = await File.ReadAllTextAsync(htmlPath) };
            StatusText = "HTML збережено: " + htmlPath;
        }
        catch (Exception ex) { StatusText = "Помилка: " + ex.Message; }
    }

    [RelayCommand]
    private void Clear()
    {
        FullNameText = "";
        SelectedFaculty = SelectedDepartment = SelectedSpecialty = All;
        SelectedTypeIndex = 0;
        UseDates = false;
        Results.Clear();
        HtmlSource = null;
        StatusText = "Поля очищено.";
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        bool yes = await Shell.Current.DisplayAlert(
            "Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
        if (yes) Application.Current?.Quit();
    }


    private SearchFilter BuildFilter() => new()
    {
        FullName = Norm(FullNameText),
        Faculty = Norm(SelectedFaculty),
        Department = Norm(SelectedDepartment),
        Specialty = Norm(SelectedSpecialty),
        Type = SelectedTypeIndex > 0 ? (CooperationType)(SelectedTypeIndex - 1) : null,
        DateFrom = UseDates ? DateFrom : null,
        DateTo = UseDates ? DateTo : null
    };

    private static string? Norm(string? s) =>
        string.IsNullOrWhiteSpace(s) || s == All ? null : s.Trim();

    private static void Fill(ObservableCollection<string> target, IEnumerable<string> values)
    {
        target.Clear();
        target.Add(All);
        foreach (var v in values) target.Add(v);
    }

    private static async Task<string?> PickToCacheAsync(string title, params string[] extensions)
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = title });
        if (file == null) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!extensions.Contains(ext))
            throw new InvalidOperationException($"Очікувався файл {string.Join(" / ", extensions)}");

        var target = Path.Combine(FileSystem.CacheDirectory, file.FileName);
        await using var src = await file.OpenReadAsync();
        await using var dst = File.Create(target);
        await src.CopyToAsync(dst);
        return target;
    }
}
