using STranslate.Core;
using System.Drawing;
using WpfRect = System.Windows.Rect;

namespace STranslate.Tests;

public class ImageTranslateCompactWindowPlacementTests
{
    [Fact]
    public void CreateForImageBoundsUsesTargetDpiForToolbarHeight()
    {
        var actual = ImageTranslateCompactWindowPlacement.CreateForImageBounds(
            new Rectangle(-120, 80, 640, 360),
            dpiScaleX: 1.25,
            dpiScaleY: 1.25,
            minWidthDip: 1,
            minImageHeightDip: 1,
            toolbarHeightDip: 64);

        Assert.Equal(new Rectangle(-120, 80, 640, 440), actual);
    }

    [Fact]
    public void CreateForImageBoundsKeepsTinyImageAboveMinimumPhysicalSize()
    {
        var actual = ImageTranslateCompactWindowPlacement.CreateForImageBounds(
            new Rectangle(10, 20, 80, 40),
            dpiScaleX: 2,
            dpiScaleY: 1.5,
            minWidthDip: 120,
            minImageHeightDip: 60,
            toolbarHeightDip: 64);

        Assert.Equal(new Rectangle(10, 20, 240, 186), actual);
    }

    [Fact]
    public void CreateCenteredOnWorkAreaClampsToPhysicalWorkArea()
    {
        var actual = ImageTranslateCompactWindowPlacement.CreateCenteredOnWorkArea(
            new Rectangle(100, 50, 1000, 800),
            new Size(2000, 2000),
            dpiScaleX: 1.25,
            dpiScaleY: 1.25,
            minWidthDip: 320,
            minImageHeightDip: 180,
            toolbarHeightDip: 64,
            maxWidthRatio: 0.85,
            maxHeightRatio: 0.85);

        Assert.Equal(new Rectangle(175, 110, 850, 680), actual);
    }

    [Fact]
    public void ToDipBoundsScalesPhysicalPixelsByDpi()
    {
        var actual = ImageTranslateCompactWindowPlacement.ToDipBounds(
            new Rectangle(-120, 80, 640, 440),
            dpiScaleX: 1.25,
            dpiScaleY: 1.25);

        Assert.Equal(new WpfRect(-96, 64, 512, 352), actual);
    }

    [Fact]
    public void ToDipBoundsClampsTinySizeToOneDip()
    {
        var actual = ImageTranslateCompactWindowPlacement.ToDipBounds(
            new Rectangle(10, 20, 0, 0),
            dpiScaleX: 2,
            dpiScaleY: 1.5);

        Assert.Equal(new WpfRect(5, 13.333333333333334, 1, 1), actual);
    }
}
