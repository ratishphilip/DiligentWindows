using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DiligentWindows.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DiligentWindows.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuickActionsPage : Page
    {
        private CanvasBitmap _background;
        private CanvasRenderTarget _bgRenderTarget;
        private GaussianBlurEffect _bgBlur;
        List<string> _glyphIcons;
        List<string> _glyphIconText;
        private bool _isCollapsed = true;

        public QuickActionsPage()
        {
            this.InitializeComponent();
            var dt = DateTime.Now;
            TimeTB.Text = dt.ToString("HH:mm");
            DateTB.Text = dt.ToString("M/yy");

            _glyphIcons = new List<string>()
            {
                "\xEBDE", "\xEBB6", "\xE754", "\xE70B",
                "\xE705", "\xE1C4", "\xE709", "\xE114",
                "\xE870", "\xE704", "\xE708", "\xE706",
                "\xE701", "\xE702", "\xE755", "\xE115"
            };

            _glyphIconText = new List<string>()
            {
                "Connect", "Battery saver", "Flashlight", "Note",
                "VPN", "Location", "Airplane mode", "Camera",
                "T-Mobile", "Mobile hotspot", "Quiet hours", "Automatic",
                "WiFi", "Bluetooth", "Rotation lock", "All Settings"
            };

            var items = new List<SettingsItem>();
            for (var i = 0; i < 16; i++)
            {
                var c = new SettingsItem()
                {
                    Width = 123,
                    Height = 90,
                    GlyphIcon = _glyphIcons[i],
                    Text = _glyphIconText[i]
                };

                items.Add(c);
            }

            panel.ItemsSource = items;
        }

        private void OnCreateLSBackgroundResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateLSBackgroundResources(sender).AsAsyncAction());
        }

        private async Task CreateLSBackgroundResources(CanvasControl sender)
        {
            _background = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/Win10_Port.jpg"));

            CreateOffscreenBackground(sender);

            sender.Invalidate();
        }

        private void CreateOffscreenBackground(CanvasControl sender)
        {
            double displayScaling = DisplayInformation.GetForCurrentView().LogicalDpi / 96.0;
            double pixelWidth = sender.ActualWidth * displayScaling;
            var scalefactor = pixelWidth / _background.Size.Width;
            var scaleEffect = new ScaleEffect();
            scaleEffect.Source = _background;
            scaleEffect.Scale = new Vector2()
            {
                X = (float)scalefactor,
                Y = (float)scalefactor
            };

            _bgRenderTarget = new CanvasRenderTarget(sender.Device, (float)sender.ActualWidth, (float)sender.ActualHeight, 96);
            using (CanvasDrawingSession ds = _bgRenderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.Black);
                ds.DrawImage(_background, new Rect(0, 0, sender.ActualWidth, sender.ActualHeight));
            }

            _bgBlur = new GaussianBlurEffect
            {
                Source = _bgRenderTarget,
                BlurAmount = 10f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Speed
            };
        }

        private void OnLSBackgroundDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(_bgBlur);
        }

        private void OnExpanderTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_isCollapsed)
            {
                _isCollapsed = false;
                SwipeDownSB.Begin();
            }
            else
            {
                _isCollapsed = true;
                SwipeUpSB.Begin();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ConceptShowSB.Begin();
            base.OnNavigatedTo(e);
        }
    }
}
