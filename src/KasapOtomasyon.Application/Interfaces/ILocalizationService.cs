namespace KasapOtomasyon.Application.Interfaces;

public class LanguageInfo
{
    public string Code { get; set; } = "tr-TR"; // tr-TR, en-GB, pl-PL
    public string DisplayName { get; set; } = "Türkçe";
    public string FlagEmoji { get; set; } = "🇹🇷";
    public string CultureName { get; set; } = "tr-TR";
}

public interface ILocalizationService
{
    string CurrentLanguageCode { get; }
    LanguageInfo CurrentLanguage { get; }
    IReadOnlyList<LanguageInfo> SupportedLanguages { get; }
    
    void SetLanguage(string languageCode);
    string Get(string key, params object[] args);
    string this[string key] { get; }

    event EventHandler<string>? LanguageChanged;
}
