using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace Autoprofile
{
    /// <summary>
    /// Interaction logic for Analysis_Window.xaml
    /// </summary>
    public partial class Analysis_Window : Window
    {
        bool isSaved = false;
        public Analysis_Window()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Brush temp = ((WPFPsi.Window1)Application.Current.MainWindow).drawingSurface.Background;
            if (temp as ImageBrush != null)
                analysed_image.Source = ((ImageBrush)temp).ImageSource;
            else
                return;            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isSaved != true)
            {
                try
                {
                    if (MessageBox.Show("Желаете ли да запазите анализа?", "Запази", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        SaveFileDialog saveFile = new SaveFileDialog();
                        saveFile.Filter = "RichText Files (*.rtf)|*.rtf";

                        if (saveFile.ShowDialog() == true)
                        {
                            TextRange txtRange = new TextRange(docViewer.Document.ContentStart, docViewer.Document.ContentEnd);

                            using (FileStream fs = File.Create(saveFile.FileName))
                            {
                                txtRange.Save(fs, DataFormats.Rtf);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(string.Format("Информация за грешка:\n{0}\n{1}\n{2}\n{3}\n{4}", exc.InnerException.ToString(),exc.Message, exc.Source, exc.StackTrace, exc.TargetSite));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            docViewer.Print();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "RichText Files (*.rtf)|*.rtf";

            try
            {
                if (saveFile.ShowDialog() == true)
                {
                    TextRange txtRange = new TextRange(docViewer.Document.ContentStart, docViewer.Document.ContentEnd);

                    using (FileStream fs = File.Create(saveFile.FileName))
                    {
                        txtRange.Save(fs, DataFormats.Rtf);
                    }
                }

                isSaved = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(string.Format("Информация за грешка:\n{0}\n{1}\n{2}\n{3}\n{4}", exc.InnerException.ToString(), exc.Message, exc.Source, exc.StackTrace, exc.TargetSite));
            }
        }
    }
}
