using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace PatientFinder.View
{
    /// <summary>
    /// Interaction logic for PatientFinderBirthDate.xaml
    /// </summary>
    public partial class PatientFinderBirthDate : UserControl
    {
        public PatientFinderBirthDate()
        {
            InitializeComponent();
        }

        private void BirthDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);    // e.Handled = true --> stops character from being addded to display
        }

        // private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        //private static readonly Regex _regex = new Regex(@"[0-9]{2}/[0-9]{2}/[0-9]{4}"); // This is ALLOWED text for full DATE pattern. This can NOT be used since it expects
                                                                                           // the full pattern to be present--but characters are typed one at a time.

        private static readonly Regex _regex = new Regex(@"[a-zA-Z]");  // regex that matches disallowed text. Text is any lowercase or uppercase letter.

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

    }
}
