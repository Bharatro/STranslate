using System.Drawing;
using System.Windows;

using WpfRect = System.Windows.Rect;

namespace STranslate.Core;

internal static class ImageTranslateCompactWindowPlacement
{
    internal static Rectangle CreateForImageBounds(
        Rectangle imageBounds,
        double dpiScaleX,
        double dpiScaleY,
        double minWidthDip,
        double minImageHeightDip,
        double toolbarHeightDip)
    {
        var width = Math.Max(ToPhysicalPixels(minWidthDip, dpiScaleX), imageBounds.Width);
        var imageHeight = Math.Max(ToPhysicalPixels(minImageHeightDip, dpiScaleY), imageBounds.Height);
        var height = imageHeight + ToPhysicalPixels(toolbarHeightDip, dpiScaleY);

        return new Rectangle(imageBounds.Left, imageBounds.Top, width, height);
    }

    internal static Rectangle CreateCenteredOnWorkArea(
        Rectangle workArea,
        System.Drawing.Size bitmapSize,
        double dpiScaleX,
        double dpiScaleY,
        double minWidthDip,
        double minImageHeightDip,
        double toolbarHeightDip,
        double maxWidthRatio,
        double maxHeightRatio)
    {
        var toolbarHeight = ToPhysicalPixels(toolbarHeightDip, dpiScaleY);
        var minWidth = ToPhysicalPixels(minWidthDip, dpiScaleX);
        var minImageHeight = ToPhysicalPixels(minImageHeightDip, dpiScaleY);
        var maxWidth = Math.Max(minWidth, (int)Math.Round(workArea.Width * maxWidthRatio));
        var maxImageHeight = Math.Max(minImageHeight, (int)Math.Round(workArea.Height * maxHeightRatio) - toolbarHeight);
        var width = Clamp(bitmapSize.Width, minWidth, maxWidth);
        var imageHeight = Clamp(bitmapSize.Height, minImageHeight, maxImageHeight);
        var height = imageHeight + toolbarHeight;
        var left = workArea.Left + (workArea.Width - width) / 2;
        var top = workArea.Top + (workArea.Height - height) / 2;

        return new Rectangle(left, top, width, height);
    }

    /// <summary>
    /// 将物理像素矩形换算为 WPF 逻辑像素(DIP)矩形，供同步 WPF 的 Left/Top/Width/Height 使用。
    /// 对齐 ScreenGrab 的 ScaledBounds：物理坐标 ÷ DPI 缩放。
    /// </summary>
    internal static WpfRect ToDipBounds(Rectangle physicalBounds, double dpiScaleX, double dpiScaleY) =>
        new(
            physicalBounds.Left / dpiScaleX,
            physicalBounds.Top / dpiScaleY,
            Math.Max(1d, physicalBounds.Width / dpiScaleX),
            Math.Max(1d, physicalBounds.Height / dpiScaleY));

    private static int ToPhysicalPixels(double dip, double dpiScale) =>
        Math.Max(1, (int)Math.Round(dip * dpiScale));

    private static int Clamp(int value, int min, int max) =>
        Math.Min(Math.Max(value, min), Math.Max(min, max));
}
