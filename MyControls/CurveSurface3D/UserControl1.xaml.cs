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

namespace MyControlLibrary.Controls3D.CurveSurface3D
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

        Random rd = new Random();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UCCurveSurface uc = new UCCurveSurface();
            uc.labelValueFormat = "{0:f2}万KW";
            uc.unit = "万KW";
            string sql = "select '发电出力' as sort, cast(convert(char(13),time,120)+':00:00' as datetime) as ymd,GLoad_00 as value from SysProductionLoad where ElectricityId=30121 and year(time)=2012 order by ymd";
            //string sql = "select '年用电负荷' as sort,cast(convert(char(13),dtime,120)+':00:00' as datetime) as ymd,ISNULL(actual,0) as value from (select tb.dtime,ld.actual  from (select cast(convert(VARCHAR(10),L.dtime,120)+' '+convert(VARCHAR(8),R.htime,108) as Datetime) as dtime from (SELECT distinct DATEADD (dd,sv.number-datepart(dd,GETDATE()),DATEADD(mm,-11,GETDATE()) ) dtime FROM MASTER.dbo.spt_values sv WHERE sv.number BETWEEN 1 AND 365) L,(SELECT distinct DATEADD (mi,sv.number*60,'00:00') htime FROM MASTER.dbo.spt_values sv WHERE sv.number BETWEEN 1 AND 24) R) tb left join (SELECT [actual],[dtime] FROM [DMSDB].[dbo].[dat_loadofarea]) ld on tb.dtime=ld.dtime ) tt order by dtime";
            uc.SortMember = "sort";
            uc.TimeMember = "ymd";
            uc.ValueMember = "value";
            uc.DataSource = DataLayer.DataProvider.getDataTableFromSQL(sql).DefaultView;
            grdmain.Children.Add(uc);



        }

        class SimDP
        { 
            public string sort="测试";
            public DateTime ymd;
            public double value;
        }
    }
}
