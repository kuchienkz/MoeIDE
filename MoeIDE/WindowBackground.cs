using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Meowtrix.MoeIDE
{
    public class WindowBackground
    {
        private readonly Border parentBorder = new Border();
        private readonly ImageSwitcher liveImageControl = null;
        private readonly Image staticImageControl = new Image();
        private bool IsLiveMode = false;

        public WindowBackground(Window window)
        {
            if (window.IsLoaded) Window_Loaded(window, null);
            window.Loaded += Window_Loaded;
            SettingsManager.SettingsUpdated += SettingsUpdated;
            liveImageControl = new ImageSwitcher(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        }

        private void SettingsUpdated(SettingsModel oldSettings, SettingsModel newSettings)
        {
            try
            {
                IsLiveMode = newSettings.MainBackground.IsLive;
                if (IsLiveMode)
                {
                    liveImageControl.ChangeFolder(newSettings.MainBackground.Folderpath);
                    liveImageControl.Stretch = newSettings.MainBackground.Stretch;
                    liveImageControl.Interval = newSettings.MainBackground.Interval;
                    liveImageControl.TransitionDuration = newSettings.MainBackground.TransitionDuration;
                    liveImageControl.HorizontalAlignment = newSettings.MainBackground.HorizontalAlignment;
                    liveImageControl.VerticalAlignment = newSettings.MainBackground.VerticalAlignment;

                    liveImageControl.Wake();
                }
                else
                {
                    liveImageControl.Hibernate();

                    var imagesource = BitmapFrame.Create(new Uri(newSettings.MainBackground.Filename), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    imagesource.Freeze();
                    staticImageControl.Source = imagesource;
                    staticImageControl.Stretch = newSettings.MainBackground.Stretch;
                    staticImageControl.HorizontalAlignment = newSettings.MainBackground.HorizontalAlignment;
                    staticImageControl.VerticalAlignment = newSettings.MainBackground.VerticalAlignment;
                }
                
                
                var br = new SolidColorBrush(newSettings.MainBackground.BackColor);
                br.Freeze();
                parentBorder.Background = br;

                liveImageControl.Opacity = newSettings.MainBackground.Opacity;
                staticImageControl.Opacity = newSettings.MainBackground.Opacity;

                double blur = newSettings.MainBackground.Blur;
                if (blur == 0.0)
                {
                    liveImageControl.Effect = null;
                    staticImageControl.Effect = null;
                }
                else
                {
                    liveImageControl.Effect = new BlurEffect { Radius = blur };
                    staticImageControl.Effect = new BlurEffect { Radius = blur };
                }

                parentBorder.Child = IsLiveMode ? (UIElement)liveImageControl : staticImageControl;
            }
            catch
            {
                liveImageControl.SwitchTo(null);
                staticImageControl.Source = null;
                parentBorder.Child = staticImageControl;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var mainwindow = (Window)sender;
            
            var cache = new BitmapCache { SnapsToDevicePixels = true };
            cache.Freeze();
            parentBorder.CacheMode = cache;
            Grid.SetRowSpan(parentBorder, 4);
            var rootgrid = (Grid)mainwindow.Template.FindName("RootGrid", mainwindow);
            rootgrid.Children.Insert(0, parentBorder);
        }
    }
}
