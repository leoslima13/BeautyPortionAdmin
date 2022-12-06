using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace BeautyPortionAdmin.Controls
{
    public class SvgLayer
    {
        private readonly Action _notifyIsVisibleChanged;
        private bool _isVisible = true;
        public IEnumerable<SKPath> _pathBounds;

        public SvgLayer(string id, bool isVisible, Action notifyIsVisibleChanged, IEnumerable<SKPath> pathBounds)
        {
            Id = id;
            _isVisible = isVisible;
            _pathBounds = pathBounds;
            _notifyIsVisibleChanged = notifyIsVisibleChanged;
        }

        public string Id { get; set; }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _notifyIsVisibleChanged?.Invoke();
            }
        }

        public bool ContainsPoint(double x, double y)
        {
            return _pathBounds.Any(p => p.Contains(Convert.ToInt64(x), Convert.ToInt64(y)));
        }
    }
}
