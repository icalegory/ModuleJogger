using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ModuleJogger
{
    /// <summary>
    /// Interaction logic for ErrorResultWindow.xaml
    /// </summary>
    public partial class ErrorResultWindow : Window
    {
        public ErrorResultWindow()
        {
            InitializeComponent();
        }

        public ErrorResultWindow(List<string> errorList)
        {
            InitializeComponent();
            foreach (string error in errorList)
                textBox.Text += error + "\n\r";
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Windows_ContentRendered(object sender, EventArgs e)
        {
            textBox.SelectAll();
            textBox.Focus();
        }
    }
}
