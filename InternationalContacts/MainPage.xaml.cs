using InternationalContacts.ViewModels;

namespace InternationalContacts;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }
}
