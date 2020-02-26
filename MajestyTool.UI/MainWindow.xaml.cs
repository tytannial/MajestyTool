using System;
using System.Collections.Generic;
using System.IO;
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
using MajestyTool.UI.Common;
using MajestyTool.UI.Model;
using Path = System.IO.Path;

namespace MajestyTool.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using var fs = new FileStream(@"D:\CSProject\王权HD\中文\unpack\textdata.cam\ACTN.STR", FileMode.Open);
            var str = STRLib.Read(fs);
            str.Export(@"D:\CSProject\王权HD\中文\unpack\textdata.cam\ACTN.STRT", true);
        }
    }
}
