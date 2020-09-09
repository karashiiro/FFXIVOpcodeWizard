using FFXIVOpcodeWizard.PacketDetection;
using FFXIVOpcodeWizard.ViewModels;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace FFXIVOpcodeWizard
{
    public partial class MainWindow
    {
        private readonly ScannerRegistry scannerRegistry;

        private ScannerRegistryViewModel scannerRegistryViewModel;
        private RegionSelectorViewModel regionSelectorViewModel;
        private CaptureModeSelectorViewModel captureModeSelectorViewModel;
        private NumberFormatSelectorViewModel numberFormatSelectorViewModel;
        private ResultsPanelViewModel resultsPanelViewModel;

        private DetectionProgram detectionProgram;

        private string NumberToString(int input)
        {
            var format = this.numberFormatSelectorViewModel.SelectedFormat;

            var formatString = format switch
            {
                NumberDisplayFormat.Decimal => "",
                NumberDisplayFormat.HexadecimalUppercase => "X4",
                NumberDisplayFormat.HexadecimalLowercase => "x4",
                _ => throw new NotImplementedException(),
            };

            return !string.IsNullOrEmpty(formatString) ? $"0x{input.ToString(formatString)}" : input.ToString();
        }

        public MainWindow()
        {
            InitializeComponent();

            this.scannerRegistry = new ScannerRegistry();
        }

        private void Registry_Loaded(object sender, RoutedEventArgs e)
        {
            this.scannerRegistryViewModel = new ScannerRegistryViewModel();
            this.scannerRegistryViewModel.Load(this.scannerRegistry);

            this.scannerRegistryViewModel.PropertyChanged += RegistryViewModel_PropertyChanged;

            Registry.DataContext = this.scannerRegistryViewModel;
        }

        private void RegistryViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var scanner = this.scannerRegistryViewModel.SelectedScanner;

            PacketNameField.Text = scanner.PacketName;
            OpcodeField.Text = NumberToString(scanner.Opcode);
            PacketSourceField.Text = scanner.PacketSource.ToString();

            var nextScannerIndex = this.scannerRegistryViewModel.Scanners.IndexOf(this.scannerRegistryViewModel.SelectedScanner) + 1;
            SkipButton.IsEnabled = StopButton.IsEnabled && nextScannerIndex != this.scannerRegistryViewModel.Scanners.Count;
        }

        private void RegionSelector_Loaded(object sender, RoutedEventArgs e)
        {
            this.regionSelectorViewModel = new RegionSelectorViewModel();
            this.regionSelectorViewModel.Load();

            RegionSelector.DataContext = this.regionSelectorViewModel;
        }

        private void CaptureModeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            this.captureModeSelectorViewModel = new CaptureModeSelectorViewModel();
            this.captureModeSelectorViewModel.Load();

            CaptureModeSelector.DataContext = this.captureModeSelectorViewModel;
        }

        private void NumberFormatSelector_Loaded(object sender, RoutedEventArgs e)
        {
            this.numberFormatSelectorViewModel = new NumberFormatSelectorViewModel();
            this.numberFormatSelectorViewModel.Load();

            this.numberFormatSelectorViewModel.PropertyChanged += NumberFormatSelectorViewModel_PropertyChanged;

            NumberFormatSelector.DataContext = this.numberFormatSelectorViewModel;
        }

        private void NumberFormatSelectorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var scanner = this.scannerRegistryViewModel.SelectedScanner;
            OpcodeField.Text = NumberToString(scanner.Opcode);
        }

        private void ResultsPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.resultsPanelViewModel = new ResultsPanelViewModel();
            this.resultsPanelViewModel.Load();

            ResultsPanel.DataContext = this.resultsPanelViewModel;
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

                this.resultsPanelViewModel.Contents = BuildResults();
            });
        }

        private string BuildResults()
        {
            var sb = new StringBuilder();

            var affix = this.resultsPanelViewModel.Affix;
            foreach (var scanner in this.scannerRegistry.AsList())
            {
                if (scanner.Opcode != 0)
                {
                    sb.AppendLine($"{scanner.PacketName} = {NumberToString(scanner.Opcode)},{affix}");
                }
            }

            return sb.ToString();
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
