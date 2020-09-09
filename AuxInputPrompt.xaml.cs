using System;

namespace FFXIVOpcodeWizard
{
    /// <summary>
    /// Interaction logic for AuxInputPrompt.xaml
    /// </summary>
    public partial class AuxInputPrompt
    {
        public string ReturnValue { get; private set; }

        public AuxInputPrompt(string tutorialText)
        {
            InitializeComponent();

            TutorialField.Text = tutorialText;
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            ReturnValue = ValueField.Text;
            Close();
        }
    }
}
