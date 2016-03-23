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
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DiligentWindows.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LockScreenMobilePage : Page
    {
        private bool _isSwipingUp = false;

        private CanvasBitmap _background;
        private CanvasBitmap _userImg;
        bool _loadedUserImg = false;
        CanvasImageBrush _userImgBrush;
        private CanvasRenderTarget _bgRenderTarget;
        private GaussianBlurEffect _bgBlur;

        #region BlurAmount

        /// <summary>
        /// BlurAmount Dependency Property
        /// </summary>
        public static readonly DependencyProperty BlurAmountProperty =
            DependencyProperty.Register("BlurAmount", typeof(double), typeof(LockScreenMobilePage),
                new PropertyMetadata(0.0, OnBlurAmountChanged));

        /// <summary>
        /// Gets or sets the BlurAmount property. This dependency property 
        /// indicates the blur amount.
        /// </summary>
        public double BlurAmount
        {
            get { return (double)GetValue(BlurAmountProperty); }
            set { SetValue(BlurAmountProperty, value); }
        }

        /// <summary>
        /// Handles changes to the BlurAmount property.
        /// </summary>
        /// <param name="d">MainPage</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnBlurAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (LockScreenMobilePage)d;
            var oldBlurAmount = (double)e.OldValue;
            var newBlurAmount = target.BlurAmount;
            target.OnBlurAmountChanged(oldBlurAmount, newBlurAmount);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the BlurAmount property.
        /// </summary>
		/// <param name="oldBlurAmount">Old Value</param>
		/// <param name="newBlurAmount">New Value</param>
        void OnBlurAmountChanged(double oldBlurAmount, double newBlurAmount)
        {
            if (_bgBlur != null)
            {
                _bgBlur.BlurAmount = (float)newBlurAmount;
                LSBackground.Invalidate();
            }
        }

        #endregion

        public LockScreenMobilePage()
        {
            this.InitializeComponent();
            //ResetBtn.IsEnabled = false;
            LSLogin.IsHitTestVisible = false;
            var dt = DateTime.Now;
            timeTB.Text = dt.ToString("HH:mm");
            dateTB.Text = dt.ToString("dddd, MMMM dd");
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
            var scalefactor = pixelWidth / _userImg.Size.Width;
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
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Speed
            };
        }

        private void OnLSBackgroundDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(_bgBlur);
        }

        private CanvasGeometry _userImgGeometry;
        private async void OnUserImgCreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            //LOAD IMAGE
            _userImg = await CanvasBitmap.LoadAsync(sender.Device, new Uri("ms-appx:///Assets/Images/WorkingAnt.png"));

            var canvasSize = new Vector2((float)(sender as CanvasControl).Width, (float)(sender as CanvasControl).Height);
            var imgSize = new Vector2((float)_userImg.Bounds.Width, (float)_userImg.Bounds.Height);
            var scale = new Vector2(canvasSize.X / imgSize.X, canvasSize.X / imgSize.X);
            var scaleEffect = new ScaleEffect()
            {
                Source = _userImg,
                Scale = scale,
                BorderMode = EffectBorderMode.Hard
            };

            _userImgBrush = new CanvasImageBrush(sender.Device, scaleEffect)
            {
                //ExtendX = CanvasEdgeBehavior.Clamp,
                //ExtendY = CanvasEdgeBehavior.Wrap,
                SourceRectangle = new Rect(0, 0, _userImg.Bounds.Width, _userImg.Bounds.Height)
            };
            //END LOAD IMAGE

            var center = new Vector2(canvasSize.X / 2, canvasSize.Y / 2);
            var radiusX = canvasSize.X / 2;
            var radiusY = canvasSize.Y / 2;

            _userImgGeometry = CanvasGeometry.CreateEllipse(sender.Device, center, radiusX, radiusY);

            _loadedUserImg = true;

            sender.Invalidate();

        }

        private void OnUserImgDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_loadedUserImg)
            {

                //// ADD BACKGROUND
                //session.Units = CanvasUnits.Pixels;
                //double displayScaling = DisplayInformation.GetForCurrentView().LogicalDpi / 96.0;
                //double pixelWidth = sender.ActualWidth * displayScaling;
                //var scalefactor = pixelWidth / _userImg.Size.Width;
                //var scaleEffect = new ScaleEffect();
                //scaleEffect.Source = _userImg;
                //scaleEffect.Scale = new Vector2()
                //{
                //    X = (float)scalefactor,
                //    Y = (float)scalefactor
                //};

                //var blurEffect = new GaussianBlurEffect();
                //blurEffect.Source = _userImg;
                //blurEffect.BlurAmount = 10;

                ////session.DrawImage(blurEffect, 0.0f, 0.0f);

                //// END ADD BACKGROUND


                //session.FillGeometry(CanvasGeometry.CreateEllipse(sender, 128, 128, 128, 128), _userImgBrush, _userImgOpacityMask);

                //.DrawCachedGeometry(_userImgCachedGeometry, _userImgBrush);
                args.DrawingSession.FillGeometry(_userImgGeometry, _userImgBrush);
            }

        }

        private void OnAddPassword(object sender, RoutedEventArgs e)
        {
            passwordBox.Password += (sender as Button).Content;
        }

        private void OnDeletePassword(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password.Length > 0)
            {
                passwordBox.Password = passwordBox.Password.Substring(0, passwordBox.Password.Length - 1);
            }
        }

        private void OnLockscreenPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!_isSwipingUp)
            {
                LSLogin.IsHitTestVisible = true;
                _isSwipingUp = true;
                var height = LSInfo.ActualHeight;
                if (!Double.IsNaN(height))
                {
                    SwipeUpAnim.To = -height;
                    SwipeUpSB.Begin();
                    ExplanationSB.Begin();
                }
                //ResetBtn.IsEnabled = true;
            }
        }

        private void OnResetLockscreen(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnResetLockscreenTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_isSwipingUp)
            {
                LSLogin.IsHitTestVisible = false;
                SwipeDownSB.Begin();
                ResetLockScreenSB.Begin();
                _isSwipingUp = false;
                passwordBox.Password = string.Empty;
                //ResetBtn.IsEnabled = false;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ConceptShowSB.Begin();
            base.OnNavigatedTo(e);
        }
    }
}
