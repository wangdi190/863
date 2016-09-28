using System;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyClassLibrary.Share3D;
using MyClassLibrary.Share2D;


namespace MyControlLibrary.Controls3D.TreeCylinder3D
{
    /// <summary>
    /// 用以表现有树结构特征的乘积数据呈现，如电费（体积）=电量（高）*电价（截面）
    /// 统计计算约定：高度数据具有累加特性，截面数据具有平均特性，即sum(height*section)/sum(height)
    /// 数据源传递约定：树层次字段为从0到n-3，高度数据为n-2，截面数据为n-1
    /// </summary>
    public partial class UCTreeCylinder3D : UserControl, IDisposable
    {
        public UCTreeCylinder3D()
        {
            InitializeComponent();
        }

        #region 模型定义


        [CategoryAttribute("数据"), Description("数据源视图，约定，前面字段为逐级分层字段，最后两个字段为值字段，分别代表柱体高和截面积")]
        public DataView DataSource
        {
            get { return (DataView)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(DataView), typeof(UCTreeCylinder3D), new UIPropertyMetadata(null, new PropertyChangedCallback(OnDataSourceChanged)));
        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UCTreeCylinder3D Sender = (UCTreeCylinder3D)d;
            Sender.updateData();
        }

        ///<summary>数据源更新，重构数据类</summary>
        private void updateData()
        {
            SettleNode root = new SettleNode(_datas);
            root.nodename = rootTitle;
            root.mbrush = new SolidColorBrush(Colors.OrangeRed);
            root.splitDir = SettleNode.ESplitDir.z方向;
        
            genNode(root, DataSource.Table.AsEnumerable(), 0);

            _datas.root = root;

        }


        void genNode(SettleNode pnode, EnumerableRowCollection<DataRow> rows, int colidx)
        {
            int colcount = DataSource.Table.Columns.Count;
            int idx = 0;
            foreach (string sortname in rows.GroupBy(p => p.Field<string>(colidx)).OrderBy(p=>p.Key).Select(p => p.Key))
            {
                SettleNode node = pnode.addNode(sortname);
                if (colidx==0)
                    {node.mbrush = MediaHelper.getSolidBrush((EMaterailColor)idx, EMaterialColorDeep.浅色).Clone(); idx++;}
                else
                    node.mbrush = pnode.mbrush.Clone();
                node.splitDir = colidx % 2 != 0 ? SettleNode.ESplitDir.z方向 : SettleNode.ESplitDir.x方向;
                if (colcount - 3 == colidx)
                {
                    node.energy = rows.Where(p => p.Field<string>(colidx) == sortname).Sum(p =>double.Parse(p[colcount-2].ToString()));
                    node.fee = rows.Where(p => p.Field<string>(colidx) == sortname).Sum(p => double.Parse(p[colcount - 2].ToString()) * double.Parse(p[colcount - 1].ToString()));
                }
                else
                {
                    EnumerableRowCollection<DataRow> subrows = rows.Where(p => p.Field<string>(colidx) == sortname);
                    genNode(node, subrows, colidx + 1);
                }
            }


        }

        
        private string _controlTitle;
        [CategoryAttribute("外观"), Description("控件标题")]
        public string controlTitle
        {
            get { return _controlTitle; }
            set { _controlTitle = value; title.Text = value; }
        }
      


        private string _rootTitle="根标题";
        [CategoryAttribute("外观"), Description("根名称")]
        public string rootTitle
        {
            get { return _rootTitle; }
            set {
                _rootTitle = value;
                if (_datas.root!=null)
                _datas.root.nodename = value;
            }
        }


        
        private string _heightTitle="高度";
        [CategoryAttribute("外观"), Description("代表高度数据的含义字串")]
        public string heightTitle
        {
            get { return _heightTitle; }
            set { _heightTitle = value; UCDetail.heightTitle = value; }
        }

        
        private string _sectionTitle="截面";
        [CategoryAttribute("外观"), Description("代表截面数据的含义字串")]
        public string sectionTitle
        {
            get { return _sectionTitle; }
            set { _sectionTitle = value; UCDetail.sectionTitle = value; }
        }



        private string _volumeTitle = "体积";
        [CategoryAttribute("外观"), Description("代表体积数据的含义字串")]
        public string volumeTitle
        {
            get { return _volumeTitle; }
            set { _volumeTitle = value; UCDetail.volumeTitle = value; }
        }
      

        #endregion

        bool isInitialized;
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }


        SettleDatas _datas = new SettleDatas();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //controller3D.mainModel = Model0;
            controller3D.mainViewport3D = mainViewport3D;
            ground.GridSize = new Size(0.1, 0.1);
            //controller3D.groundGeometryModel = ground.GroundSurfaceModel;
            //controller3D.groundTransformGroup = ground.Transform as Transform3DGroup;

            controller3D.showDetail = showDetail;
            controller3D.showTooltips = showTooltips;
            _datas.camera = camera;

            _datas.mgAll = mgAll;
            if (!isInitialized)
            {
                if (DataSource != null)
                {
                    _datas.init();

                    _datas.expandToLayer(0);
                    _datas.root.showModel();

                    initLegend();
                    initExpandCollapsed();
                    isInitialized = true;
                }
            }
        }

        #region 回调处理细节显示和tooltips
        /// <summary>
        /// 显示细节回调函数
        /// </summary>
        /// <param name="ray3DResult">射线测试结果</param>
        /// <param name="mousePoint">点击的屏幕坐标点</param>
        /// <returns>是否命中模型显示了要细节</returns>
        private bool showDetail(RayMeshGeometry3DHitTestResult ray3DResult, Point mousePoint)
        {
            SettleNode one = ray3DResult.ModelHit.GetValue(ToolTipService.ToolTipProperty) as SettleNode;
            //=======  注：点击打开细节视图
            if (one != null)
            {
                toolTips.IsOpen = false;
                //isclick = true;

                UCDetail.showNodeDetail(one, mousePoint - new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
                return true;
            }
            else
            {
                if (UCDetail.Visibility == Visibility.Visible)
                    UCDetail.closeDetail();
                return false;
            }
        }

        /// <summary>
        /// 显示Tooltips回调函数
        /// </summary>
        /// <param name="ray3DResult">射线测试结果</param>
        /// <param name="position">鼠标移动的屏幕坐标点</param>
        /// <returns>是否命中模型显示了Tooltips</returns>
        private bool showTooltips(RayMeshGeometry3DHitTestResult ray3DResult, Point position)
        {
            SettleNode one = ray3DResult.ModelHit.GetValue(ToolTipService.ToolTipProperty) as SettleNode;
            //===== tooltips
            if (one != null)
            {
                //填充信息
                ttContent.Text = String.Format("{0}:{1:f0}\r\n{2}:{3:f0}\r\n{4}:{5:f4}", volumeTitle, one.fee, heightTitle, one.energy, sectionTitle, one.price);
                double ToolTipOffset = 5;
                toolTips.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                toolTips.PlacementTarget = mainViewport3D;
                toolTips.HorizontalOffset = position.X + ToolTipOffset;
                toolTips.VerticalOffset = position.Y + ToolTipOffset;
                toolTips.IsOpen = true;
                return true;
            }
            else
            {
                toolTips.IsOpen = false;
                return false;
            }
        }
        #endregion

        #region 图例处理
        Legend3D legendView;
        ///<summary>初始化图例视图</summary>
        private void initLegend()
        {
            //初始化图例视图
            Legend3DData legendData;
            legendData = new Legend3DData();
            LegendUnit lg;
            lg = legendData.addNewModelLegend(EModelType.平滑园柱);
            lg.LegendName = rootTitle;
            lg.SizeXName = sectionTitle;
            lg.SizeYName = heightTitle;
            lg.mat = My3DHelper.getSolidMaterial(EMaterailColor.红, EMaterialColorDeep.浅色);
            legendView = new Legend3D(legendData);
            legendView.Visibility = Visibility.Hidden;
            grdMain.Children.Add(legendView);
            //附加图例按钮
            Image img = new Image() { Width = 48, Height = 48, Margin = new Thickness(0, 60, 0, 5), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center };
            img.ToolTip = "图例说明";
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,,/TreeCylinder3D;component/Images/legend.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            img.Source = bi;
            img.MouseLeftButtonDown += new MouseButtonEventHandler(imgLegend_MouseLeftButtonDown);
            controller3D.addControl(img);
        }
        void imgLegend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (legendView.isShow)
                legendView.closeLegend();
            else
                legendView.showLegend(e.GetPosition(grdMain) - new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
        }

        #endregion
        ///<summary>初始化工具栏</summary>
        private void initExpandCollapsed()
        {
            Image img = new Image() { Width = 48, Height = 48, Margin = new Thickness(0, 5, 0, 5), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center };
            img.ToolTip = "扩展";
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,,/TreeCylinder3D;component/Images/expand.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            if (bi.CanFreeze)
                bi.Freeze();
            img.Source = bi;
            img.MouseLeftButtonDown += new MouseButtonEventHandler(imgExpand_MouseLeftButtonDown);
            controller3D.addControl(img);

            img = new Image() { Width = 48, Height = 48, Margin = new Thickness(0, 5, 0, 5), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center };
            img.ToolTip = "收缩";
            bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,,/TreeCylinder3D;component/Images/collapsed.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            if (bi.CanFreeze)
                bi.Freeze();

            img.Source = bi;
            img.MouseLeftButtonDown += new MouseButtonEventHandler(imgCollapsed_MouseLeftButtonDown);
            controller3D.addControl(img);
        }

        void imgExpand_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _datas.expand();
        }
        void imgCollapsed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _datas.fold();
        }
        public void SplitAction(bool expand)
        {
            if (expand)
                _datas.expand();
            else
                _datas.fold();
        }

                #region Dispose相关

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    controller3D.ClearaddControl();
                    legendView = null;

                    _datas.camera = null;
                    _datas.mgFlag.Children.Clear();
                    _datas.mgLabel.Children.Clear();
                    _datas.mgModel.Children.Clear();
                    _datas.mgAll.Children.Clear();
                    _datas.root.clearNode();
                    _datas.root = null;
                    
                    _datas.mgAll = null;
                    _datas = null;


                    controller3D.showTooltips = null;
                    controller3D.showDetail = null;
                    controller3D.mainViewport3D = null;
                    controller3D.clearDynControl();

                    controller3D = null;
                    GC.Collect();
                    GC.WaitForFullGCComplete();
                }



                disposed = true;

            }
        }

        ~UCTreeCylinder3D()
        {

            Dispose(false);
        }

        #endregion


    }
}
