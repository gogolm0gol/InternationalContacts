namespace InternationalContacts.Models;

public enum CooperationType
{
    BusinessTrip,
    Lecturing,
    SendingInfoAbroad,
    ReceivingInfoAbroad,
    SeminarConducting
}

public static class CooperationTypeExtensions
{
    public static string ToUkrainian(this CooperationType t) => t switch
    {
        CooperationType.BusinessTrip => "Наукове відрядження",
        CooperationType.Lecturing => "Читання лекцій",
        CooperationType.SendingInfoAbroad => "Передача інформації за кордон",
        CooperationType.ReceivingInfoAbroad => "Одержання інформації з-за кордону",
        CooperationType.SeminarConducting => "Проведення семінару",
        _ => t.ToString()
    };
}
