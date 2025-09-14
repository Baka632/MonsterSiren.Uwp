using Windows.Globalization;
using Windows.System.UserProfile;

namespace MonsterSiren.Uwp.Helpers;

public static class LanguageHelper
{
    public static IReadOnlyList<AppLanguage> SupportLanguages { get; } = [.. ApplicationLanguages.ManifestLanguages.Select(langName => new AppLanguage(langName))];

    public static bool IsAppSupportUserPreferredLanguage
    {
        get => SupportLanguages.Any(lang => GlobalizationPreferences.Languages.Contains(lang.Name));
    }

    public static void SetAppLanguage(AppLanguage language)
    {
        if (language == AppLanguage.SystemLanguage)
        {
            ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
        }
        else
        {
            ApplicationLanguages.PrimaryLanguageOverride = language.Name;
        }
    }

    public static AppLanguage GetCurrentAppLanguage()
    {
        return string.IsNullOrWhiteSpace(ApplicationLanguages.PrimaryLanguageOverride)
            ? AppLanguage.SystemLanguage
            : new AppLanguage(ApplicationLanguages.PrimaryLanguageOverride);
    }
}
