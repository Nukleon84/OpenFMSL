using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Shell.Helper
{
    public static class DialogHelper
    {
        public static bool ShowErrorMessageBox(string title, string text)
        {
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return true;
        }

        public static bool ShowWarningMessageBox(string title, string text)
        {
            var result = MessageBox.Show(text, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            return result == MessageBoxResult.OK;
        }

        public static bool ShowInfoMessageBox(string title, string text)
        {
            var result = MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        public static bool ShowInputDialog(string title, string text, out string input)
        {
            throw new NotImplementedException();
        }

        public static bool ShowOpenFileDialog(string extension, string filter, out string path)
        {
            path = "";
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = extension;
            dlg.Filter = filter;
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                path = dlg.FileName;
                return true;
            }

            return false;
        }

        public static bool ShowSaveFileDialog(string extension, string filter, out string path)
        {
            path = "";
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = extension;
            dlg.Filter = filter;
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                path = dlg.FileName;
                return true;
            }

            return false;
        }
    }
}
