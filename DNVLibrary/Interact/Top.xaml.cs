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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DNVLibrary.Interact
{
    /// <summary>
    /// Top.xaml 的交互逻辑
    /// </summary>
    public partial class Top : UserControl
    {
        public Top()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            List<PrjClass> projects = new List<PrjClass>() ;
            projects.Add(new PrjClass() {prjID="aaa",prjName="基础方案" });
            projects.Add(new PrjClass() { prjID = "bbb", prjName = "高可靠性方案" });
            projects.Add(new PrjClass() { prjID = "ccc", prjName = "经济性方案" });
            cmbProjects.ItemsSource = projects;
            cmbProjects.SelectedIndex = 1;
        }
    }

    public class PrjClass
    {
        public string prjName { get; set; }
        public string prjID { get; set; }
    }
}
