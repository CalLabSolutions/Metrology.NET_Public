using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoA_Editor.Components
{
    public class AlphabetTextBox : TextBox
    {
        private static readonly Regex regex = new Regex("^[a-zA-Z]+$");

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }

            base.OnPreviewTextInput(e);
        }
    }
}
