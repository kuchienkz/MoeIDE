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
                IsLiveMode = newSettings.MainSetting.IsLive;
                if (IsLiveMode)
                {
                    liveImageControl.ChangeFolder(newSettings.MainSetting.Folderpath);
                    liveImageControl.Stretch = newSettings.MainSetting.Stretch;
                    liveImageControl.Interval = newSettings.MainSetting.Interval;
                    liveImageControl.TransitionDuration = newSettings.MainSetting.TransitionDuration;
                    liveImageControl.HorizontalAlignment = newSettings.MainSetting.HorizontalAlignment;
                    liveImageControl.VerticalAlignment = newSettings.MainSetting.VerticalAlignment;

                    liveImageControl.Wake();
                }
                else
                {
                    liveImageControl.Hibernate();

                    var imagesource = BitmapFrame.Create(new Uri(newSettings.MainSetting.Filename), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    imagesource.Freeze();
                    staticImageControl.Source = imagesource;
                    staticImageControl.Stretch = newSettings.MainSetting.Stretch;
                    staticImageControl.HorizontalAlignment = newSettings.MainSetting.HorizontalAlignment;
                    staticImageControl.VerticalAlignment = newSettings.MainSetting.VerticalAlignment;
                }
                
                var br = new SolidColorBrush(newSettings.MainSetting.BackColor);
                br.Freeze();
                parentBorder.Background = br;

                liveImageControl.Opacity = newSettings.MainSetting.Opacity;
                staticImageControl.Opacity = newSettings.MainSetting.Opacity;

                double blur = newSettings.MainSetting.Blur;
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
                liveImageControl.Hibernate();
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
