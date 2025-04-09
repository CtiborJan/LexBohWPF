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

namespace LexBohWPF
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
        }

        private void CmdLoad_Click(object sender, RoutedEventArgs e)
        {
            clsWordDocument doc = new clsWordDocument(@"H:\převodník.docx");
            clsWPFRenderer rend = new clsWPFRenderer(rtf);
            foreach (clsWordDocumentEntry ent in doc.entries)
            {
                lst.Items.Add(ent.lemma);
            }
            clsDocument d = new clsDocument(doc.entries[0]);
            rtf.FontFamily = new FontFamily("Times New Roman");
            rend.set_data((from s in d.readRuns.segments select s).Cast<clsDocument.intSegment>().ToList());

            rend.render();
        }
    }
}
