using System;
using System.Collections.ObjectModel;
using System.Linq;
using FFXIVOpcodeWizard.Models;
using static Machina.TCPNetworkMonitor;

namespace FFXIVOpcodeWizard.ViewModels
{
    public class CaptureModeSelectorViewModel
    {
        public ObservableCollection<RadioItem> CaptureModes { get; set; }

        public NetworkMonitorType SelectedCaptureMode => GetSelectedCaptureMode();

        public void Load()
        {
            CaptureModes = new ObservableCollection<RadioItem>
            {
                new RadioItem { Text = "WinPCap", IsChecked = true },
                new RadioItem { Text = "Sockets", IsEnabled = Util.Elevated() },
            };
        }

        private NetworkMonitorType GetSelectedCaptureMode()
        {
            var selection = CaptureModes.First(cm => cm.IsChecked).Text;
            return selection switch
            {
                "WinPCap" => NetworkMonitorType.WinPCap,
                "Sockets" => NetworkMonitorType.RawSocket,
                _ => throw new NotImplementedException(),
            };
        }
    }
}