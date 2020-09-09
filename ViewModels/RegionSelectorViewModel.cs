using FFXIVOpcodeWizard.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FFXIVOpcodeWizard.ViewModels
{
    public class RegionSelectorViewModel
    {
        public ObservableCollection<RadioItem> Regions { get; set; }

        public Region SelectedRegion => GetCheckedRegion();

        public void Load()
        {
            Regions = new ObservableCollection<RadioItem>
            {
                new RadioItem { Text = "Global", IsChecked = true },
                new RadioItem { Text = "Korea" },
                new RadioItem { Text = "China" },
            };
        }

        private Region GetCheckedRegion()
        {
            var checkedRadioItem = Regions.First(radioItem => radioItem.IsChecked);
            return (Region)Enum.Parse(typeof(Region), checkedRadioItem.Text);
        }
    }
}