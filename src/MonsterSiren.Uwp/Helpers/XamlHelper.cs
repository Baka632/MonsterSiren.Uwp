namespace MonsterSiren.Uwp.Helpers;

public static class XamlHelper
{
    public static bool ReverseBoolean(bool value) => !value;

    public static Visibility ReverseVisibility(Visibility value)
    {
        return value switch
        {
            Visibility.Visible => Visibility.Collapsed,
            _ => Visibility.Visible,
        };
    }
    
    public static Visibility ReverseVisibility(bool value)
    {
        return value switch
        {
            true => Visibility.Collapsed,
            false => Visibility.Visible,
        };
    }
    
    public static Visibility ToVisibility(bool? value)
    {
        return value switch
        {
            true => Visibility.Collapsed,
            false => Visibility.Visible,
            null => Visibility.Collapsed
        };
    }
    
    public static Visibility ToVisibility(bool? value, Visibility defaultVisibilityForNull = Visibility.Collapsed)
    {
        return value switch
        {
            true => Visibility.Collapsed,
            false => Visibility.Visible,
            null => defaultVisibilityForNull
        };
    }

    public static string DateTimeOffsetToFormatedString(DateTimeOffset value)
    {
        return value.ToString("yyyy年M月d日");
    }
}
