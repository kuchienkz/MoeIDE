using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Meowtrix.MoeIDE
{
    /// <summary>
    /// Interaction logic for ImageSwitcher.xaml
    /// </summary>
    public partial class ImageSwitcher : UserControl
    {
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ImageSwitcher), new PropertyMetadata(Stretch.UniformToFill));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly DependencyProperty IntervalProperty =
           DependencyProperty.Register("Interval", typeof(double), typeof(ImageSwitcher), new PropertyMetadata(120000.0));

        public double Interval
        {
            get { return (double)GetValue(IntervalProperty); }
            set
            {
                if ((double)GetValue(IntervalProperty) != value)
                {
                    if (backgroundChanger != null)
                    {
                        backgroundChanger.Stop();
                        backgroundChanger.Interval = TimeSpan.FromMilliseconds(value);
                        if (!Hibernating)
                        {
                            backgroundChanger.Start();
                        }
                    }
                    SetValue(IntervalProperty, value);
                }
            }
        }

        public enum Transitions
        {
            [Description("Smooth fade between 2 images")]FADE, // Implemented :D
            [Description("Fade-out old image, fade-in new image")] FADE_IN_OUT, // todo: later
            [Description("Slide new image to the left")] SLIDE_TO_LEFT, // todo: later
            [Description("Slide new image to the right")] SLIDE_TO_RIGHT, //todo: later
        }

        private DispatcherTimer backgroundChanger;
        private Queue<string> imageFiles;
        private Storyboard story = new Storyboard();
        private bool Hibernating = true;

        public double TransitionDuration;
        public Transitions TransitionType;
        public Image ActiveImage;
        
        public ImageSwitcher(string mustExistsFolderPath)
        {
            TransitionType = Transitions.FADE;
            Interval = 30000.0;
            Stretch = Stretch.UniformToFill;
            TransitionDuration = 2000.0;
            var files = Directory.EnumerateFiles(mustExistsFolderPath).Where(f => f.EndsWith(".jpg") || f.EndsWith(".png"));
            imageFiles = new Queue<string>(files);
            backgroundChanger = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Interval) // 2 minutes | todo: implement on setting Page
            };
            backgroundChanger.Tick += BackgroundChanger_Tick;

            InitializeComponent();
        }

        public void Hibernate()
        {
            backgroundChanger.Stop();
            Hibernating = true;
        }

        public void Wake()
        {
            if (!Hibernating)
            {
                if (!backgroundChanger.IsEnabled)
                {
                    backgroundChanger.Start();
                }
                return;
            }

            if (imageFiles.Count > 0)
            {
                var t = imageFiles.Dequeue();
                SwitchTo(LoadImage(t));
                imageFiles.Enqueue(t);
            }
            if (imageFiles.Count > 1)
            {
                backgroundChanger.Start();
            }
            Hibernating = false;
        }

        private void BackgroundChanger_Tick(object sender, EventArgs e)
        {
            backgroundChanger.Stop();

            var t = imageFiles.Dequeue();
            ImageSource newImg = LoadImage(t);
            imageFiles.Enqueue(t);
            SwitchTo(newImg, TransitionType);

            backgroundChanger.Start();
        }

        public void SwitchTo(ImageSource newImage)
        {
            SwitchTo(newImage, TransitionType);
        }

        public void SwitchTo(ImageSource newImage, Transitions transition)
        {
            if (newImage == null)
            {
                return;
            }
            switch (transition)
            {
                default:
                case Transitions.FADE:
                    if (ActiveImage == behind)
                    {
                        front.Source = newImage;
                        ActiveImage = front;
                        front.BeginAnimation(OpacityProperty, 
                            new DoubleAnimation(1, 
                            new Duration(TimeSpan.FromMilliseconds(TransitionDuration))));
                    }
                    else
                    {
                        behind.Source = newImage;
                        ActiveImage = behind;
                        front.BeginAnimation(OpacityProperty,
                            new DoubleAnimation(0,
                            new Duration(TimeSpan.FromMilliseconds(TransitionDuration))));
                    }
                    break;
                case Transitions.FADE_IN_OUT:
                    break;
                case Transitions.SLIDE_TO_LEFT:
                    break;
                case Transitions.SLIDE_TO_RIGHT:
                    break;
            }
        }

        private BitmapFrame LoadImage(string filename)
        {
            var imagesource = BitmapFrame.Create(new Uri(filename), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            imagesource.Freeze();
            return imagesource;
        }

        public void ChangeFolder(string folderPath)
        {
            if (imageFiles.Count > 0 && imageFiles.Peek().Contains(folderPath))
            {
                return;
            }
            backgroundChanger.Stop();

            var files = Directory.EnumerateFiles(folderPath).Where(f => f.EndsWith(".jpg") || f.EndsWith(".png"));
            imageFiles = new Queue<string>(files);
            if (!Hibernating)
            {
                if (imageFiles.Count > 0)
                {
                    var t = imageFiles.Dequeue();
                    SwitchTo(LoadImage(t), TransitionType);
                    imageFiles.Enqueue(t);
                }
                if (imageFiles.Count > 1)
                {
                    backgroundChanger.Start();
                }
            }
        }

        private void behind_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            ActiveImage = behind;
        }

        private void front_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            ActiveImage = front;
        }
    }
}