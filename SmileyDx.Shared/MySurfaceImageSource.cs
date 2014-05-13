using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = SharpDX.Color;
using Device = SharpDX.Direct3D11.Device;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;

namespace SmileyDx
{
    public sealed class MySurfaceImageSource : SurfaceImageSource
    {
        private Device _d3DDevice;
        private SharpDX.Direct2D1.Device _d2DDevice;
        private SharpDX.Direct2D1.DeviceContext _d2DContext;
        private readonly int _width;
        private readonly int _height;

        public MySurfaceImageSource(int pixelWidth, int pixelHeight, bool isOpaque)
            : base(pixelWidth, pixelHeight, isOpaque)
        {
            _width = pixelWidth;
            _height = pixelHeight;

            CreateDeviceResources();

            Application.Current.Suspending += OnSuspending;
        }

        private void CreateDeviceResources()
        {
            Utilities.Dispose(ref _d3DDevice);
            Utilities.Dispose(ref _d2DDevice);
            Utilities.Dispose(ref _d2DContext);

            var creationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= DeviceCreationFlags.Debug;
#endif

            FeatureLevel[] featureLevels =
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
                FeatureLevel.Level_9_3,
                FeatureLevel.Level_9_2,
                FeatureLevel.Level_9_1,
            };

            _d3DDevice = new Device(DriverType.Hardware, creationFlags, featureLevels);

            using (var dxgiDevice = _d3DDevice.QueryInterface<SharpDX.DXGI.Device>())
            {
                _d2DDevice = new SharpDX.Direct2D1.Device(dxgiDevice);
                _d2DContext = new SharpDX.Direct2D1.DeviceContext(_d2DDevice, DeviceContextOptions.None);
                using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
                {
                    sisNative.Device = dxgiDevice;
                }
            }
        }

        public void BeginDraw()
        {
            BeginDraw(new Windows.Foundation.Rect(0, 0, _width, _height));
        }

        public void BeginDraw(Windows.Foundation.Rect updateRect)
        {
            var updateRectNative = new Rectangle
            {
                Left = (int)updateRect.Left,
                Top = (int)updateRect.Top,
                Right = (int)updateRect.Right,
                Bottom = (int)updateRect.Bottom
            };

            using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
            {
                Point offset;
                using (var surface = sisNative.BeginDraw(updateRectNative, out offset))
                {
                    using (var bitmap = new Bitmap1(_d2DContext, surface))
                    {
                        _d2DContext.Target = bitmap;
                    }

                    _d2DContext.BeginDraw();

                    _d2DContext.PushAxisAlignedClip(
                        new RectangleF(
                            (offset.X),
                            (offset.Y),
                            (offset.X + (float)updateRect.Width),
                            (offset.Y + (float)updateRect.Height)
                            ),
                        AntialiasMode.Aliased
                        );

                    _d2DContext.Transform = Matrix3x2.Translation(offset.X, offset.Y);
                }
            }
        }

        public void EndDraw()
        {
            _d2DContext.Transform = Matrix3x2.Identity;
            _d2DContext.PopAxisAlignedClip();

            _d2DContext.EndDraw();
            _d2DContext.Target = null;

            using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
            {
                sisNative.EndDraw();
            }
        }

        public void Clear(Windows.UI.Color color)
        {
            _d2DContext.Clear(ConvertToColorF(color));
        }

        public void DrawSmiley(Point point)
        {
            var center = new Vector2(point.X, point.Y);
            var yellowBrush = new SolidColorBrush(_d2DContext, ConvertToColorF(Colors.Yellow));
            var blackBrush = new SolidColorBrush(_d2DContext, ConvertToColorF(Colors.Black));

            var ellipse = new Ellipse(center, 100, 100);
            _d2DContext.FillEllipse(ellipse, yellowBrush);
            _d2DContext.DrawEllipse(ellipse, blackBrush, 5f);

            var oog1 = new Ellipse(new Vector2(center.X - 20, center.Y - 30), 10, 15);
            _d2DContext.FillEllipse(oog1, blackBrush);

            var oog2 = new Ellipse(new Vector2(center.X + 20, center.Y - 30), 10, 15);
            _d2DContext.FillEllipse(oog2, blackBrush);

            var geometry = new PathGeometry(_d2DContext.Factory);
            GeometrySink sink = geometry.Open();
            sink.BeginFigure(new Vector2(center.X - 50, center.Y + 30), new FigureBegin());

            var arc = new ArcSegment();
            arc.Point = new Vector2(center.X + 50, center.Y + 30);
            arc.Size = new Size2F(60f, 60f);
            sink.AddArc(arc);
            sink.EndFigure(new FigureEnd());
            sink.Close();

            _d2DContext.DrawGeometry(geometry, blackBrush, 5);
        }

        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            using (var dxgiDevice = _d3DDevice.QueryInterface<Device3>())
            {
                dxgiDevice.Trim();
            }
        }

        private static Color ConvertToColorF(Windows.UI.Color color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
    }
}
