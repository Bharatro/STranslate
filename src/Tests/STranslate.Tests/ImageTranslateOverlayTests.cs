using STranslate.Controls;
using STranslate.Core;
using STranslate.Helpers;
using STranslate.Plugin;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STranslate.Tests;

public class ImageTranslateOverlayTests
{
    [Fact]
    public void CreateTranslatedOverlayWithoutLocationsReturnsEmptyDocument()
    {
        var block = new OcrLayoutBlock { Text = "translated text" };

        var document = ImageTranslateRenderer.CreateTranslatedOverlay(
            [block],
            ImageTranslateOverlayTheme.Light);

        Assert.True(document.IsEmpty);
        Assert.Empty(document.Items);
        Assert.Empty(document.SelectableWords);
    }

    [Fact]
    public void CreateTranslatedOverlayKeepsSelectableWordsInSourceCoordinates()
    {
        var block = CreateBlock("ABC", left: 100, top: 50, width: 180, height: 40);

        var document = ImageTranslateRenderer.CreateTranslatedOverlay(
            [block],
            ImageTranslateOverlayTheme.Light);

        var item = Assert.Single(document.Items);
        Assert.False(document.IsEmpty);
        Assert.Equal("ABC", string.Concat(document.SelectableWords.Select(word => word.Text)));
        Assert.All(document.SelectableWords.Where(word => !word.BoundingBox.IsEmpty), word =>
        {
            Assert.True(word.BoundingBox.Left >= item.Plan.TextClipRect.Left);
            Assert.True(word.BoundingBox.Top >= item.Plan.TextClipRect.Top);
            Assert.True(word.BoundingBox.Right <= item.Plan.TextClipRect.Right);
            Assert.True(word.BoundingBox.Bottom <= item.Plan.TextClipRect.Bottom);
        });
    }

    [Fact]
    public void CreateTranslatedOverlayPreservesThemeColors()
    {
        var block = CreateBlock("译文", left: 10, top: 20, width: 160, height: 40);

        var light = ImageTranslateRenderer.CreateTranslatedOverlay(
            [block],
            ImageTranslateOverlayTheme.Light);
        var dark = ImageTranslateRenderer.CreateTranslatedOverlay(
            [block],
            ImageTranslateOverlayTheme.Dark);

        var lightItem = Assert.Single(light.Items);
        var darkItem = Assert.Single(dark.Items);
        Assert.Equal(Colors.Black, lightItem.Plan.ForegroundColor);
        Assert.Equal(Colors.White, darkItem.Plan.ForegroundColor);
        Assert.NotEqual(lightItem.Plan.OverlayBackgroundColor, darkItem.Plan.OverlayBackgroundColor);
        Assert.True(lightItem.BackgroundBrush.IsFrozen);
        Assert.True(darkItem.BackgroundBrush.IsFrozen);
    }

    [Fact]
    public void OverlayRendersDocumentAndBecomesTransparentWhenCleared()
    {
        RunOnStaThread(() =>
        {
            var document = ImageTranslateRenderer.CreateTranslatedOverlay(
                [CreateBlock("ABC", left: 20, top: 20, width: 160, height: 40)],
                ImageTranslateOverlayTheme.Light);
            var overlay = new ImageTranslateOverlay
            {
                Width = 220,
                Height = 100,
                Document = document
            };

            Assert.True(RenderHasVisiblePixels(overlay, 220, 100));

            overlay.Document = null;
            Assert.False(RenderHasVisiblePixels(overlay, 220, 100));
        });
    }

    private static bool RenderHasVisiblePixels(FrameworkElement element, int width, int height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        var pixels = new byte[width * height * 4];
        bitmap.CopyPixels(pixels, width * 4, 0);
        return pixels.Where((_, index) => index % 4 == 3).Any(alpha => alpha != 0);
    }

    private static OcrLayoutBlock CreateBlock(
        string text,
        double left,
        double top,
        double width,
        double height)
    {
        var box = Box(left, top, width, height);
        return new OcrLayoutBlock
        {
            Text = text,
            BoxPoints = box,
            LineBoxPoints = [box]
        };
    }

    private static List<BoxPoint> Box(double left, double top, double width, double height) =>
    [
        new((float)left, (float)top),
        new((float)(left + width), (float)top),
        new((float)(left + width), (float)(top + height)),
        new((float)left, (float)(top + height))
    ];

    private static void RunOnStaThread(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
            ExceptionDispatchInfo.Capture(exception).Throw();
    }
}
