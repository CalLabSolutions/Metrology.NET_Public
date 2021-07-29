﻿using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoA_Editor.Views
{
    /// <summary>
    /// Interaction logic for InputParameterRangeDialogView.xaml
    /// </summary>
    public partial class InputParameterRangeDialogView : UserControl
    {
        public InputParameterRangeDialogView()
        {
            InitializeComponent();
        }

        private void TextMin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                e.Handled = false;
                return;
            }
            Regex regex = new Regex(@"^[0-9]([\.\,][0-9]{1,3})?$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TextMax_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                e.Handled = false;
                return;
            }
            Regex regex = new Regex(@"^[0-9]([\.\,][0-9]{1,3})?$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}