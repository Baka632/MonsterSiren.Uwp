// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

using Windows.Media.Core;

namespace MonsterSiren.Uwp.Views;

public sealed partial class CodecInfoDialog : ContentDialog
{
    public IEnumerable<CodecInfo> CodecInfos { get; }

    public CodecInfoDialog(IEnumerable<CodecInfo> codecInfos)
    {
        CodecInfos = codecInfos ?? throw new ArgumentNullException(nameof(codecInfos));
        this.InitializeComponent();
    }
}
