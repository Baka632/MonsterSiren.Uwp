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
    private static readonly Color defaultColor = (Color)Application.Current.Resources["SystemAccentColor"];

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

    #region Code from Windows Community Toolkit
    /*
     License

     Windows Community Toolkit
     Copyright © .NET Foundation and Contributors

     All rights reserved.

     MIT License (MIT)
     Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

     The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

     THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
     */

    /// <summary>
    /// Lightens the color by the given percentage.
    /// </summary>
    /// <param name="color">Source color.</param>
    /// <param name="amount">Percentage to lighten. Value should be between 0 and 1.</param>
    /// <returns>Color</returns>
    public static Color LighterBy(this Color color, float amount)
    {
        return color.Lerp(Colors.White, amount);
    }

    /// <summary>
    /// Darkens the color by the given percentage.
    /// </summary>
    /// <param name="color">Source color.</param>
    /// <param name="amount">Percentage to darken. Value should be between 0 and 1.</param>
    /// <returns>Color</returns>
    public static Color DarkerBy(this Color color, float amount)
    {
        return color.Lerp(Colors.Black, amount);
    }

    /// <summary>
    /// Calculates the linear interpolated value based on the given values.
    /// </summary>
    /// <param name="start">Starting value.</param>
    /// <param name="end">Ending value.</param>
    /// <param name="amount">Weight-age given to the ending value.</param>
    /// <returns>Linear interpolated value.</returns>
    public static float Lerp(this float start, float end, float amount)
    {
        return start + ((end - start) * amount);
    }

    /// <summary>
    /// Calculates the linear interpolated Color based on the given Color values.
    /// </summary>
    /// <param name="colorFrom">Source Color.</param>
    /// <param name="colorTo">Target Color.</param>
    /// <param name="amount">Weightage given to the target color.</param>
    /// <returns>Linear Interpolated Color.</returns>
    public static Color Lerp(this Color colorFrom, Color colorTo, float amount)
    {
        // Convert colorFrom components to lerp-able floats
        float sa = colorFrom.A,
            sr = colorFrom.R,
            sg = colorFrom.G,
            sb = colorFrom.B;

        // Convert colorTo components to lerp-able floats
        float ea = colorTo.A,
            er = colorTo.R,
            eg = colorTo.G,
            eb = colorTo.B;

        // lerp the colors to get the difference
        byte a = (byte)Math.Max(0, Math.Min(255, sa.Lerp(ea, amount))),
            r = (byte)Math.Max(0, Math.Min(255, sr.Lerp(er, amount))),
            g = (byte)Math.Max(0, Math.Min(255, sg.Lerp(eg, amount))),
            b = (byte)Math.Max(0, Math.Min(255, sb.Lerp(eb, amount)));

        // return the new color
        return Color.FromArgb(a, r, g, b);
    }
    #endregion
}
