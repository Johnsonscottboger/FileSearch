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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileSerach.Model;
using FileSerach.ViewModel;

namespace FileSerach
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //this.searchTextBox.Populating += new PopulatingEventHandler(AutoCompleteBox_Populating);
            this.searchTextBox.SelectionChanged += new SelectionChangedEventHandler(SearchTextBox_SelectionChanged);
        }

        void SearchTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = this.searchTextBox.SelectedItem as SearchHistory;
            if (model != null)
            {
                this.searchTextBox.Text = model.KeyWord;
            }
        }


        private void AutoCompleteBox_Populating(object sender, PopulatingEventArgs e)
        {
            e.Cancel = true;
            //this.searchTextBox.ItemsSource = data;
            this.searchTextBox.PopulateComplete();
        }
    }
}
