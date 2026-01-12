using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;

namespace PSMultiTools.Classes
{
    public class LogTextMarker : IBackgroundRenderer, IVisualLineTransformer
    {

        public readonly TextDocument _document;
        public readonly List<TextMarker> _markers = [];

        public LogTextMarker(TextDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _document.Changed += _document_Changed;
        }

        public void _document_Changed(object? sender, DocumentChangeEventArgs e)
        {
            var delta = e.InsertionLength - e.RemovalLength;
            if (delta == 0) return;

            for (int i = _markers.Count - 1; i >= 0; i--)
            {
                var m = _markers[i];
                if (e.Offset <= m.StartOffset)
                {
                    m.Shift(delta);
                }
                else if (e.Offset < m.StartOffset + m.Length)
                {
                    _markers.RemoveAt(i);
                }
            }
        }

        public KnownLayer Layer => KnownLayer.Selection;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView == null || drawingContext == null) return;

            foreach (var m in _markers)
            {
                var seg = new TextSegment { StartOffset = m.StartOffset, Length = m.Length };
                foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, seg))
                {
                    if (m.Background != null)
                        drawingContext.FillRectangle(m.Background, r);
                }
            }
        }

        public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements)
        {
            if (context == null || elements == null) return;

            foreach (var element in elements)
            {
                var segStart = context.VisualLine.FirstDocumentLine.Offset + element.RelativeTextOffset;
                var segLength = element.DocumentLength;

                var elemStart = segStart;
                var elemEnd = segStart + segLength;

                foreach (var m in _markers)
                {
                    var markerStart = m.StartOffset;
                    var markerEnd = m.StartOffset + m.Length;

                    if (markerEnd <= elemStart || markerStart >= elemEnd)
                        continue;

                    if (m.Foreground != null)
                    {
                        var trp = element.TextRunProperties;
                        trp?.SetForegroundBrush(m.Foreground);
                    }

                    break;
                }
            }
        }

        public TextMarker Create(int start, int length, IBrush? background = null, IBrush? foreground = null)
        {
            var m = new TextMarker(start, length, background!, foreground!);
            _markers.Add(m);
            return m;
        }

        public TextMarker Create(int start, int length, IBrush foreground)
            => Create(start, length, null, foreground);

        public void RemoveAll()
        {
            _markers.Clear();
        }

        public class TextMarker(int start, int length, IBrush background, IBrush foreground)
        {
            public int StartOffset { get; set; } = start;
            public int Length { get; set; } = length;
            public IBrush Background { get; set; } = background;
            public IBrush Foreground { get; set; } = foreground;

            public void Shift(int delta) => StartOffset += delta;
            public bool IsValid(int docLength) => StartOffset >= 0 && StartOffset < docLength && Length > 0;
        }

    }
}
