using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace DiligentWindows.Controls
{
    public sealed partial class SettingsItem : UserControl
    {
        private static Brush WhiteBrush;
        private static Brush BlackBrush;
        private static Brush PressedBrush;

        private bool _isPressed = false;

        #region GlyphIcon

        /// <summary>
        /// GlyphIcon Dependency Property
        /// </summary>
        public static readonly DependencyProperty GlyphIconProperty =
            DependencyProperty.Register("GlyphIcon", typeof(string), typeof(SettingsItem),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the GlyphIcon property. This dependency property 
        /// indicates the Glyph Icon to be displayed.
        /// </summary>
        public string GlyphIcon
        {
            get { return (string)GetValue(GlyphIconProperty); }
            set { SetValue(GlyphIconProperty, value); }
        }

        #endregion

        #region Text

        /// <summary>
        /// Text Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SettingsItem),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the Text property. This dependency property 
        /// indicates the text to be displayed.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        static SettingsItem()
        {
            PressedBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 122, 255));
            WhiteBrush = new SolidColorBrush(Colors.White);
            BlackBrush = new SolidColorBrush(Colors.Black);
        }

        public SettingsItem()
        {
            this.InitializeComponent();

            DataContext = this;
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {
            _isPressed = !_isPressed;

            BgBorder.Background = _isPressed ? PressedBrush : WhiteBrush;
            Logo.Foreground = _isPressed ? WhiteBrush : BlackBrush;
            LogoText.Foreground = _isPressed ? WhiteBrush : BlackBrush;
        }
    }
}
