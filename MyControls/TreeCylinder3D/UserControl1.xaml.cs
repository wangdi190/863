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

namespace MyControlLibrary.Controls3D.TreeCylinder3D
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UCTreeCylinder3D uc = new UCTreeCylinder3D();
            string sql = @"select t3.Name,t2.Name,SUM(electricity) energy,SUM(electriccharge)/SUM(electricity) price 
	from RegionalElectricityStatistics t1
	join Dic_District t2 on t1.RegionID=t2.ID
	join DIc_ElectricityType t3 on t1.ElectricTypeID=t2.ID
	 where YEAR(time)=2012 group by t2.Name,t3.Name";
            uc.DataSource = DataLayer.DataProvider.getDataTableFromSQL(sql).DefaultView;
            uc.controlTitle = "本年营销收益情况";
            uc.rootTitle = "营销收益";
            uc.heightTitle = "电量";
            uc.sectionTitle = "电价";
            uc.volumeTitle = "电费";
            grdmain.Children.Add(uc);


//            string sql = @"select t3.Name,t2.Name,cast(MONTH(time) as nvarchar(2))+'月' m,SUM(electricity) energy,SUM(electriccharge)/SUM(electricity) price 
//	from RegionalElectricityStatistics t1
//	join Dic_District t2 on t1.RegionID=t2.ID
//	join DIc_ElectricityType t3 on t1.ElectricTypeID=t2.ID
//	 where YEAR(time)=2012 group by MONTH(time),t2.Name,t3.Name";


        }
    }
}
