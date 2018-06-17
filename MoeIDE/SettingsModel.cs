using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Meowtrix.MoeIDE
{
    [Serializable, XmlRoot("Settings")]
    public class SettingsModel
    {
        [Serializable]
        public class SettingPack
        {
            public string Filename { get; set; }
            public bool IsLive { get; set; } = false;
            public string Folderpath { get; set; }
            public double Interval { get; set; } = 30000.0;
            public double TransitionDuration { get; set; } = 2000.0;
            public Stretch Stretch { get; set; } = Stretch.UniformToFill;
            public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;
            public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
            public Color BackColor { get; set; } = Colors.Transparent;
            public double Opacity { get; set; } = 1.0;
            public double Blur { get; set; } = 0.0;
            public SettingPack Clone() => (SettingPack)MemberwiseClone();
        }
        public SettingPack MainSetting { get; set; } = new SettingPack();
        public SettingsModel Clone() => new SettingsModel
        {
            MainSetting = this.MainSetting.Clone()
        };
    }
}
