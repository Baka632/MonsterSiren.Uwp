using System.Diagnostics;
using System.Globalization;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示应用支持的语言。
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct AppLanguage : IEquatable<AppLanguage>
{
    public static readonly AppLanguage SystemLanguage = new()
    {
        Name = string.Empty,
        DisplayName = "UseSystemLanguage".GetLocalized()
    };

    /// <summary>
    /// 构造 <see cref="AppLanguage"/> 的新实例。
    /// </summary>
    /// <param name="name">形如“en-US”的语言名称。</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> 为 <see langword="null"/> 或空白。</exception>
    public AppLanguage(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"“{nameof(name)}”不能为 null 或空白。", nameof(name));
        }

        Name = name;
        DisplayName = new CultureInfo(name).NativeName;
    }

    /// <summary>
    /// 形如“en-US”的语言名称。
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 语言的显示名称。
    /// </summary>
    public string DisplayName { get; private set; }

    public override readonly bool Equals(object obj)
    {
        return obj is AppLanguage language && Equals(language);
    }

    public readonly bool Equals(AppLanguage other)
    {
        return Name == other.Name;
    }

    public override readonly int GetHashCode()
    {
        return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
    }

    public static bool operator ==(AppLanguage left, AppLanguage right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AppLanguage left, AppLanguage right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return this == SystemLanguage
            ? DisplayName
            : $"{DisplayName} [{Name}]";
    }
}
