using FFXIVOpcodeWizard.PacketDetection;
using FFXIVOpcodeWizard.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace FFXIVOpcodeWizard
{
    public partial class MainWindow
    {
        private readonly ScannerRegistry scannerRegistry;

        private readonly ScannerRegistryViewModel scannerRegistryViewModel;
        private readonly RegionSelectorViewModel regionSelectorViewModel;
        private readonly CaptureModeSelectorViewModel captureModeSelectorViewModel;
        private readonly NumberFormatSelectorViewModel numberFormatSelectorViewModel;
        private readonly ResultsPanelViewModel resultsPanelViewModel;

        private DetectionProgram detectionProgram;

        public MainWindow()
        {
            InitializeComponent();

            this.scannerRegistry = new ScannerRegistry();
            this.scannerRegistry.AsList()[0].Opcode = 0x74;

            this.scannerRegistryViewModel = new ScannerRegistryViewModel();
            this.scannerRegistryViewModel.Load(this.scannerRegistry);

            this.regionSelectorViewModel = new RegionSelectorViewModel();
            this.regionSelectorViewModel.Load();

            this.captureModeSelectorViewModel = new CaptureModeSelectorViewModel();
            this.captureModeSelectorViewModel.Load();

            this.numberFormatSelectorViewModel = new NumberFormatSelectorViewModel();
            this.numberFormatSelectorViewModel.Load();

            this.resultsPanelViewModel = new ResultsPanelViewModel();
            this.resultsPanelViewModel.Load(this.scannerRegistry, this.numberFormatSelectorViewModel);
        }

        private void Registry_Loaded(object sender, RoutedEventArgs e)
        {
            this.scannerRegistryViewModel.PropertyChanged += RegistryViewModel_PropertyChanged;

            Registry.DataContext = this.scannerRegistryViewModel;
        }

        private void RegistryViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var scanner = this.scannerRegistryViewModel.SelectedScanner;
            var format = this.numberFormatSelectorViewModel.SelectedFormat;

            PacketNameField.Text = scanner.PacketName;
            OpcodeField.Text = Util.NumberToString(scanner.Opcode, format);
            PacketSourceField.Text = scanner.PacketSource.ToString();

            var nextScannerIndex = this.scannerRegistryViewModel.Scanners.IndexOf(this.scannerRegistryViewModel.SelectedScanner) + 1;
            SkipButton.IsEnabled = StopButton.IsEnabled && nextScannerIndex != this.scannerRegistryViewModel.Scanners.Count;
        }

        private void RegionSelector_Loaded(object sender, RoutedEventArgs e)
        {
            RegionSelector.DataContext = this.regionSelectorViewModel;
        }

        private void CaptureModeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            CaptureModeSelector.DataContext = this.captureModeSelectorViewModel;
        }

        private void NumberFormatSelector_Loaded(object sender, RoutedEventArgs e)
        {
            this.numberFormatSelectorViewModel.PropertyChanged += NumberFormatSelectorViewModel_PropertyChanged;

            NumberFormatSelector.DataContext = this.numberFormatSelectorViewModel;
        }

        private void NumberFormatSelectorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var scanner = this.scannerRegistryViewModel.SelectedScanner;
            var format = this.numberFormatSelectorViewModel.SelectedFormat;
            OpcodeField.Text = Util.NumberToString(scanner.Opcode, format);
        }

        private void ResultsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ResultsPanel.DataContext = this.resultsPanelViewModel;
            this.resultsPanelViewModel.UpdateContents();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            foreach (var scanner in this.scannerRegistry.AsList())
            {
                scanner.Opcode = 0;
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            RunButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            this.detectionProgram = new DetectionProgram();
            _ = detectionProgram.Run(new DetectionProgram.Args
            {
                CaptureMode = this.captureModeSelectorViewModel.SelectedCaptureMode,
                Region = this.regionSelectorViewModel.SelectedRegion,
                Registry = this.scannerRegistry,
            }, state =>
            {
                this.scannerRegistryViewModel.SelectedScanner =
                    this.scannerRegistryViewModel.Scanners[state.ScannerIndex];
                TutorialField.Text = state.CurrentTutorial;

                this.resultsPanelViewModel.UpdateContents();
            }, (scanner, paramIndex) =>
            {
                var auxWindow = new AuxInputPrompt(scanner.ParameterPrompts[paramIndex]);
                auxWindow.ShowDialog();
                return (auxWindow.ReturnValue, auxWindow.Skipping);
            });
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            RunButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            SkipButton.IsEnabled = false;

            this.detectionProgram.Stop();
        }

        private void SkipButton_Click(object sender, EventArgs e)
        {
            var nextScannerIndex = this.scannerRegistryViewModel.Scanners.IndexOf(this.scannerRegistryViewModel.SelectedScanner) + 1;
            this.scannerRegistryViewModel.SelectedScanner =
                this.scannerRegistryViewModel.Scanners[nextScannerIndex];

            this.detectionProgram.Skip();
        }
    }
}
