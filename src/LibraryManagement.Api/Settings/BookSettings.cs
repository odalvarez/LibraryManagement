namespace LibraryManagement.Api.Settings;

public class BookSettings
{
    public const string SectionName = "BookSettings";
    public int MaxAllowed { get; set; } = 10;
}
