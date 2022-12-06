using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Reactive.Bindings;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Controls
{
    public partial class SvgImage : ContentView
    {
        private SKPicture _picture;

        public SvgImage()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source), typeof(string), typeof(SvgImage), default(string), propertyChanged: OnSourcePropertyChanged);

        public static readonly BindableProperty LayersProperty = BindableProperty.Create(
            nameof(Layers), typeof(List<SvgLayer>), typeof(SvgImage), new List<SvgLayer>(), BindingMode.OneWayToSource);

        public static readonly BindableProperty CanHandleClickOnLayersProperty =
            BindableProperty.Create(nameof(CanHandleClickOnLayers), typeof(bool), typeof(SvgImage), false);

        public static readonly BindableProperty LayersTappedCommandProperty =
            BindableProperty.Create(nameof(LayersTappedCommand), typeof(ReactiveCommand<SvgLayer>), typeof(SvgImage), null);

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public List<SvgLayer> Layers
        {
            get => (List<SvgLayer>)GetValue(LayersProperty);
            set => SetValue(LayersProperty, value);
        }

        public bool CanHandleClickOnLayers
        {
            get => (bool)GetValue(CanHandleClickOnLayersProperty);
            set => SetValue(CanHandleClickOnLayersProperty, value);
        }

        public ReactiveCommand<SvgLayer> LayersTappedCommand
        {
            get => (ReactiveCommand<SvgLayer>)GetValue(LayersTappedCommandProperty);
            set => SetValue(LayersTappedCommandProperty, value);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == HeightProperty.PropertyName || propertyName == WidthProperty.PropertyName)
                canvasView.InvalidateSurface();
        }

        private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is SvgImage control) || newValue == null) return;

            control.CreateLayers();
            control.LoadPicture();
            control.canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (_picture == null) return;

            var canvas = args.Surface.Canvas;
            var info = args.Info;
            canvas.Clear();

            canvas.Save();
            canvas.Translate(info.Width / 2f, info.Height / 2f);

            var bounds = _picture.CullRect;
            var maxHeight = Math.Max(info.Height, bounds.Height);
            var minHeight = Math.Min(info.Height, bounds.Height);
            var minWidth = Math.Min(info.Width, bounds.Width);
            var maxWidth = Math.Max(info.Width, bounds.Width);

            var ratio = bounds.Width >= bounds.Height
                 ? info.Width > bounds.Width ? maxWidth / minWidth : minWidth / maxWidth
                 : info.Height > bounds.Height ? maxHeight / minHeight : minHeight / maxHeight;

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);
            canvas.DrawPicture(_picture);
            canvas.Restore();
        }

        private void CreateLayers()
        {
            Layers.Clear();
            var path = $"{typeof(SvgImage).Assembly.GetName().Name}.{Source}";
            using (var stream = GetType().Assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    throw new Exception($"Error rendering the SVG icon '{path}'. Make sure it's build action is set to 'Embedded resource' and it is present at the specified path");
                }

                using (var reader = XmlReader.Create(stream))
                {
                    reader.MoveToContent();

                    var root = (XElement)XNode.ReadFrom(reader);

                    Layers = new List<SvgLayer>(root.Elements()
                        .Where(x => x.Name.LocalName == "g")
                        .Select(CreateSvgLayer));
                }
            }

            SvgLayer CreateSvgLayer(XElement layerElement)
            {
                var id = layerElement.Attribute("id")?.Value;
                var pathBounds = GetSkPath(layerElement);
                var display = layerElement.Attribute("display");
                var isVisible = display == null || display.Value != "none";

                return new SvgLayer(id, isVisible, OnLayerVisibilityChanged, pathBounds);
            }

            IEnumerable<SKPath> GetSkPath(XElement layerElement)
            {
                return layerElement.Elements()
                    .Where(x => x.Name.LocalName == "path")
                    .Select(x => SKPath.ParseSvgPathData(x.Attribute("d").Value));
            }
        }

        private void OnLayerVisibilityChanged()
        {
            LoadPicture();
            canvasView.InvalidateSurface();
        }

        private void LoadPicture()
        {
            var path = $"{typeof(SvgImage).Assembly.GetName().Name}.{Source}";

            var svg = new SkiaSharp.Extended.Svg.SKSvg();
            using (var stream = GetType().Assembly.GetManifestResourceStream(path))
            {
                if (Layers != null)
                {
                    using (var filteredStream = FilterLayers(stream))
                    {
                        svg.Load(filteredStream);
                    }
                }
                else
                {
                    svg.Load(stream);
                }
            }

            _picture = svg.Picture;
        }

        private Stream FilterLayers(Stream stream)
        {
            using (var reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();

                if (!(XNode.ReadFrom(reader) is XElement root)) return null;

                HideInvisibleLayers(root);
                ShowVisibleLayers(root);

                var ms = new MemoryStream();

                root.Save(ms);
                ms.Position = 0;
                return ms;
            }

            void HideInvisibleLayers(XElement root)
            {
                var invisibleLayersIds = Layers.Where(x => !x.IsVisible).Select(x => x.Id);

                var layersToMakeInvisible = root.Elements()
                    .Where(x => x.Name.LocalName == "g" &&
                                invisibleLayersIds.Contains(x.Attribute("id")?.Value))
                    .ToList();

                foreach (var layer in layersToMakeInvisible)
                {
                    layer.SetAttributeValue("display", "none");
                }
            }

            void ShowVisibleLayers(XElement root)
            {
                var visibleLayersIds = Layers.Where(x => x.IsVisible).Select(x => x.Id);

                var layersToMakeVisible = root.Elements()
                    .Where(x => x.Name.LocalName == "g" &&
                                visibleLayersIds.Contains(x.Attribute("id")?.Value))
                    .ToList();

                foreach (var layer in layersToMakeVisible)
                {
                    layer.SetAttributeValue("display", "inline");
                }
            }
        }
    }
}
