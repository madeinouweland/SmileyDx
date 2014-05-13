using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using SharpDX;
using Color = Windows.UI.Color;

namespace SmileyDx
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            Init();
            WavePlayer.Instance.AddWave("surfsup", "Assets/surfsup.wav");
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var width = (int)ActualWidth;
            var height = (int)ActualHeight;
            _source = new MySurfaceImageSource(width, height, true);
            SurfaceImage.Source = _source;
            CompositionTarget.Rendering += (s, a) =>
            {
                Update();
                Draw();
            };
            WavePlayer.Instance.PlayWave("surfsup");
        }

        private MySurfaceImageSource _source;
        private readonly Random _random = new Random();

        private readonly List<Point> _smileys = new List<Point>();
        private readonly byte[] _rgb = new byte[3];
        private readonly int[] _rgbDelta = new int[3];
        private Color _backColor;

        private void Init()
        {
            for (int i = 0; i < 12; i++)
            {
                _smileys.Add(new Point(100, 100));
            }
            _random.NextBytes(_rgb);
            _backColor = new Color { R = _rgb[0], G = _rgb[1], B = _rgb[2], A = 255 };
            _rgbDelta[0] = 1;
            _rgbDelta[1] = 2;
            _rgbDelta[2] = 3;
        }

        private long _ticks1 = Environment.TickCount;
        private long _ticks2 = Environment.TickCount;

        private long _time = 0;
        private void Update()
        {
            var ticks1 = Environment.TickCount;
            var ticks2 = Environment.TickCount;
            if (ticks1 > _ticks1 + 10)
            {
                _ticks1 = ticks1;

                var r = (int)_rgb[0];
                r += _rgbDelta[0];
                _rgb[0] = (byte)r;

                if (r > 250 || r < 10)
                {
                    _rgbDelta[0] = _rgbDelta[0] * -1;
                }

                var g = (int)_rgb[1];
                g += _rgbDelta[1];
                _rgb[1] = (byte)g;

                if (g > 250 || g < 10)
                {
                    _rgbDelta[1] = _rgbDelta[1] * -1;
                }

                var b = (int)_rgb[2];
                b += _rgbDelta[2];
                _rgb[2] = (byte)b;

                if (b > 250 || b < 10)
                {
                    _rgbDelta[2] = _rgbDelta[2] * -1;
                }

                _backColor = new Color { R = _rgb[0], G = _rgb[1], B = _rgb[2], A = 255 };
            }
            _time++;
            if (ticks2 > _ticks2 + 10)
            {
                _ticks2 = ticks2;

                for (int i = 0; i < 12; i++)
                {
                    var x = Math.Cos((_time + i * i / 3) / 20d);
                    var y = Math.Sin(2 * (_time + i * i / 3) / 20d) / 2;
                    var xx = ((int)(Math.Round(x * 400) + ActualHeight));
                    var yy = ((int)(Math.Round(y * 200) + ActualHeight / 2));
                    _smileys[i] = new Point(xx, yy);
                }
            }
        }

        private void Draw()
        {
            _source.BeginDraw();
            _source.Clear(_backColor);

            for (int i = 0; i < 12; i++)
            {
                _source.DrawSmiley(_smileys[i]);
            }

            _source.EndDraw();
        }
    }
}
