using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModuleJogger
{
    /// <summary>
    /// Interaction logic for CustomizeFileExtensionsWindow.xaml
    /// </summary>
    public partial class CustomizeFileExtensionsWindow : Window
    {
        //List<string> _extensions;
        StringCollection _extensions;

        public CustomizeFileExtensionsWindow()
        {
            InitializeComponent();
        }

        //public CustomizeFileExtensionsWindow(string language, List<string> extensions)
        public CustomizeFileExtensionsWindow(string language, StringCollection extensions)
        {
            InitializeComponent();
            _extensions = extensions;
            labelTitle.Content += language;
            foreach (string extension in extensions)
                listBoxExtensions.Items.Add(extension);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxExtension.Focus();
        }

        // This method is called when a selection change event in the listBoxExtensions component occurs.
        // If there is something selected, the delete button is enabled.
        void DeleteButtonCheck(object sender, SelectionChangedEventArgs args)
        {
            //ListBoxItem lbi = ((sender as System.Windows.Forms.ListBox).SelectedItem as ListBoxItem);
            if (listBoxExtensions.SelectedItems.Count > 0)
                buttonDelete.IsEnabled = true;
            else
                buttonDelete.IsEnabled = false;
        }

        // This is the Remove Row button event.
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = listBoxExtensions.SelectedItems.Cast<Object>().ToArray();
            foreach (var item in selected)
                listBoxExtensions.Items.Remove(item);
        }

        private void AddExtension()
        {
            if (textBoxExtension.Text.Length > 0)
            {
                string extensionToAdd = textBoxExtension.Text;
                listBoxExtensions.Items.Add(extensionToAdd);
                listBoxExtensions.Items.SortDescriptions.Add(
                            new System.ComponentModel.SortDescription("",
                            System.ComponentModel.ListSortDirection.Ascending));
                _extensions.Add(extensionToAdd);
                textBoxExtension.Text = string.Empty;
            }
        }

        // This is the add extension button event.
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddExtension();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // System.Windows.Input.KeyEventArgs is NOT the correct one:
        private void textBoxExtension_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                AddExtension();
            }
        }


    }
}
