using STranslate.Plugin;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace STranslate.Core;

internal static class OcrWordBuilder
{
    public static ObservableCollection<OcrWord> CreateFromOcrContents(IEnumerable<OcrContent>? contents)
    {
        if (contents == null)
            return [];

        var words = new List<OcrWord>();
        foreach (var content in contents)
        {
            if (string.IsNullOrEmpty(content.Text) ||
                content.BoxPoints == null ||
                content.BoxPoints.Count == 0)
                continue;

            var boundingBox = CalculateBoundingBox(content.BoxPoints);
            if (boundingBox.IsEmpty || boundingBox.Width <= 0 || boundingBox.Height <= 0)
                continue;

            var avgCharWidth = boundingBox.Width / Math.Max(content.Text.Length, 1);
            for (var i = 0; i < content.Text.Length; i++)
            {
                words.Add(new OcrWord
                {
                    Text = content.Text[i].ToString(),
                    BoundingBox = new Rect(
                        boundingBox.Left + avgCharWidth * i,
                        boundingBox.Top,
                        avgCharWidth,
                        boundingBox.Height)
                });
            }
        }

        return CreateIndexedCollection(words);
    }

    public static IReadOnlyList<OcrWord> CreateFromFormattedText(
        string text,
        FormattedText formattedText,
        Point origin,
        Rect clipRect,
        double scaleFactor)
    {
        if (string.IsNullOrEmpty(text) ||
            clipRect.IsEmpty ||
            clipRect.Width <= 0 ||
            clipRect.Height <= 0 ||
            scaleFactor <= 0)
        {
            return [];
        }

        var words = new List<OcrWord>();
        for (var i = 0; i < text.Length; i++)
        {
            var geometry = formattedText.BuildHighlightGeometry(origin, i, 1);
            var bounds = geometry?.Bounds ?? Rect.Empty;
            if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
                continue;

            var clippedBounds = Rect.Intersect(bounds, clipRect);
            if (clippedBounds.IsEmpty || clippedBounds.Width <= 0 || clippedBounds.Height <= 0)
                continue;

            words.Add(new OcrWord
            {
                Text = text[i].ToString(),
                BoundingBox = ScaleRect(clippedBounds, scaleFactor)
            });
        }

        return words;
    }

    public static ObservableCollection<OcrWord> CreateIndexedCollection(IEnumerable<OcrWord> words)
    {
        var sortedWords = words
            .Where(word => !string.IsNullOrEmpty(word.Text) &&
                           !word.BoundingBox.IsEmpty &&
                           word.BoundingBox.Width > 0 &&
                           word.BoundingBox.Height > 0)
            .OrderBy(word => word.BoundingBox.Top)
            .ThenBy(word => word.BoundingBox.Left)
            .ToList();

        var currentIndex = 0;
        foreach (var word in sortedWords)
        {
            word.StartIndexInFullText = currentIndex;
            currentIndex += word.Text.Length;
        }

        return new ObservableCollection<OcrWord>(sortedWords);
    }

    private static Rect CalculateBoundingBox(IReadOnlyCollection<BoxPoint> boxPoints)
    {
        var minX = boxPoints.Min(point => point.X);
        var minY = boxPoints.Min(point => point.Y);
        var maxX = boxPoints.Max(point => point.X);
        var maxY = boxPoints.Max(point => point.Y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private static Rect ScaleRect(Rect rect, double scaleFactor) =>
        new(
            rect.Left * scaleFactor,
            rect.Top * scaleFactor,
            rect.Width * scaleFactor,
            rect.Height * scaleFactor);
}
