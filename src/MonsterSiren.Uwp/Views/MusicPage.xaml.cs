using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views
{
    /// <summary>
    /// 音乐展示页
    /// </summary>
    public sealed partial class MusicPage : Page
    {
        public MusicViewModel ViewModel { get; } = new MusicViewModel();

        public MusicPage()
        {
            this.InitializeComponent();
            ViewModel.Initialize();
        }
    }
}
