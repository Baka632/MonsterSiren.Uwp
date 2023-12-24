using Microsoft.Graphics.Canvas;
using Microsoft.Toolkit.Uwp;
using Windows.Storage.Streams;
using Windows.UI;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为获取图像主题色提供帮助方法的类
/// </summary>
public static class ImageColorHelper
{
    private static readonly CanvasDevice device = new();
    private static readonly Color defaultColor = (Color)Application.Current.Resources["SystemAccentColorDark2"];

    /// <summary>
    /// 通过一个 <see cref="Uri"/> 获取主题色
    /// </summary>
    /// <param name="uri">图像 Uri</param>
    /// <returns>表示为 <see cref="Color"/> 的主题色</returns>
    public static async Task<Color> GetPaletteColor(Uri uri)
    {
        try
        {
            using CanvasBitmap bimap = await CanvasBitmap.LoadAsync(device, uri);
            Color[] colors = bimap.GetPixelColors();
            Color color = GetThemeColor(colors);

            return color;
        }
        catch (FileNotFoundException)
        {
            return defaultColor;
        }
    }

    /// <summary>
    /// 通过一个 <see cref="IRandomAccessStream"/> 获取主题色
    /// </summary>
    /// <param name="stream"><see cref="IRandomAccessStream"/> 流</param>
    /// <returns>表示为 <see cref="Color"/> 的主题色</returns>
    public static async Task<Color> GetPaletteColor(IRandomAccessStream stream)
    {
        try
        {
            using CanvasBitmap bimap = await CanvasBitmap.LoadAsync(device, stream);
            Color[] colors = bimap.GetPixelColors();
            Color color = GetThemeColor(colors);
            return color;
        }
        catch (FileNotFoundException)
        {
            return defaultColor;
        }
    }

    private static Color GetThemeColor(Color[] colors)
    {
        Color color;

        //饱和度 黑色多
        double sumS = 0;
        //明亮度 白色多
        double sumV = 0;
        double sumHue = 0;
        //颜色中最大亮度
        double maxV = 0;
        //颜色中最大饱和度
        double maxS = 0;
        //颜色中最大色相
        double maxH = 0;
        double count = 0;
        List<Color> notBlackWhite = [];
        foreach (Color item in colors)
        {
            //将 rgb 转换成 hsv 对象
            HsvColor hsv = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToHsv(item);

            //先将黑色和白色剔除掉
            if (hsv.V < 0.3 || hsv.S < 0.2)
            {
                continue;
            }
            //找出最大饱和度
            maxS = hsv.S > maxS ? hsv.S : maxS;
            //找出最大亮度度
            maxV = hsv.V > maxV ? hsv.V : maxV;
            //找出最大色相
            maxH = hsv.H > maxH ? hsv.H : maxH;
            //色相总和
            sumHue += hsv.H;
            //亮度总和
            sumS += hsv.S;
            //饱和度总和
            sumV += hsv.V;
            count++;
            notBlackWhite.Add(item);
        }

        double avgH = sumHue / count;
        double avgV = sumV / count;
        double avgS = sumS / count;
        double maxAvgV = maxV / 2;
        double maxAvgS = maxS / 2;
        double maxAvgH = maxH / 2;

        //计算各个值，用来做判断用
        double h = Math.Max(maxAvgV, avgV);
        double s = Math.Min(maxAvgS, avgS);
        double hue = Math.Min(maxAvgH, avgH);

        //aveS = aveS ;
        double R = 0;
        double G = 0;
        double B = 0;
        count = 0;

        foreach (Color item in notBlackWhite)
        {
            HsvColor hsv = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToHsv(item);

            if (hsv.H >= hue + 10 && hsv.V >= h && hsv.S >= s)
            {
                R += item.R;
                G += item.G;
                B += item.B;
                count++;
            }
        }

        double r = R / count;
        double g = G / count;
        double b = B / count;

        color = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        return color;
    }
}
