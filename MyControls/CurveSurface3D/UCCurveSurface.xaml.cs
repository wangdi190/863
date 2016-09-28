using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyClassLibrary.Share2D;
using MyClassLibrary.Share3D;
using System.ComponentModel;

namespace MyControlLibrary.Controls3D.CurveSurface3D
{
    /// <summary>
    /// UCCurveSurface.xaml 的交互逻辑
    /// </summary>
    public partial class UCCurveSurface : UserControl, IDisposable
    {
        public UCCurveSurface()
        {
            InitializeComponent();
            //_data = ucdata;
        }
        #region 初始化
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            controller.mainViewport3D = mainViewport3D;

        }
        bool isInitialized;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //_pcenter = new Point3D(_data.para.Limit.X / 2, _data.para.Limit.Y / 2, -_data.para.Limit.Z / 2);
            //rotateXOperate(20);
            //rotateYOperate(60);
            //========

            //_data.createModel();
            //initControls();
            //Model3DHelper.addControlCube(mainViewport3D, 200);
            //_data.Datas[0].isVisual = true;
            //_data.showModel(modelgroup);

            //drawGrid();

            //圆角框裁剪
            brdQM.Clip = recQM.RenderedGeometry;
            brdQX.Clip = recQX.RenderedGeometry;

            if (!isInitialized)
            {
                showModel();
                isInitialized = true;
            }

            aniload.EasingFunction = new ElasticEase() { Springiness = 8 };
            aniload.AccelerationRatio = 0.9;
            transformani.BeginAnimation(ScaleTransform3D.ScaleYProperty, aniload);
        }




        DoubleAnimation aniload = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(2), From = 0.1, To = 1, BeginTime = TimeSpan.FromSeconds(1) };



        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        #endregion
        #region 模型定义


        [CategoryAttribute("数据"), Description("数据视图")]
        ///<summary>曲面数据源</summary>
        public DataView DataSource
        {
            get { return (DataView)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(DataView), typeof(UCCurveSurface), new UIPropertyMetadata(null, new PropertyChangedCallback(OnDataSourceChanged)));
        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UCCurveSurface Sender = (UCCurveSurface)d;
            Sender.updateData();
        }
        ///<summary>数据源更新，重构数据类</summary>
        private void updateData()
        {
            if (string.IsNullOrWhiteSpace(SortMember) || string.IsNullOrWhiteSpace(TimeMember) || string.IsNullOrWhiteSpace(ValueMember)) { isDataInitialized = false; return; }

            _data.Datas.Clear();

            foreach (string zsort in DataSource.Table.AsEnumerable().GroupBy(p => p.Field<string>(SortMember)).Select(p => p.Key))
            {
                CurveSurfaceData cs1 = new CurveSurfaceData(_data, zsort);
                _data.Datas.Add(cs1);

                foreach (DataRow item in DataSource.Table.AsEnumerable().Where(p => p.Field<string>(SortMember) == zsort))
                {
                    //cs1.orgPoints.Add(new csDataPoint(item.Field<DateTime>(TimeMember), item.Field<double>(ValueMember)));
                    cs1.orgPoints.Add(new csDataPoint(item.Field<DateTime>(TimeMember), double.Parse(item[ValueMember].ToString())));
                }
            }

            isDataInitialized = true;
            showModel();
        }

        private string _SortMember;
        [CategoryAttribute("数据"), Description("分类字段名")]
        ///<summary>分类字段名</summary>
        public string SortMember
        {
            get { return _SortMember; }
            set { _SortMember = value; }
        }

        private string _TimeMember;
        [CategoryAttribute("数据"), Description("时间字段名")]
        ///<summary>时间字段名</summary>
        public string TimeMember
        {
            get { return _TimeMember; }
            set { _TimeMember = value; }
        }


        private string _ValueMember;
        [CategoryAttribute("数据"), Description("值字段名")]
        ///<summary>值字段名</summary>
        public string ValueMember
        {
            get { return _ValueMember; }
            set { _ValueMember = value; }
        }


        private DateTime _startTime;
        [CategoryAttribute("数据"), Description("起始时间")]
        ///<summary>起始时间</summary>
        public DateTime startTime
        {
            get { return _startTime; }
            set { _startTime = value; _data.startDate = value; }
        }


        private DateTime _endTime;
        [CategoryAttribute("数据"), Description("终止时间")]
        ///<summary>终止时间</summary>
        public DateTime endTime
        {
            get { return _endTime; }
            set { _endTime = value; _data.endDate = value; }
        }

        [CategoryAttribute("外观"), Description("相机初始变换参数")]
        ///<summary>相机初始变换参数</summary>
        public Matrix3D cameraMatrix
        {
            get
            { return cameratransform.Matrix; }
            set
            {
                cameratransform.Matrix = value;
            }
        }

        [CategoryAttribute("引用"), Description("日期标题控件")]
        ///<summary>日期标题控件</summary>
        public TextBlock DateControl
        {
            get { return lblDate; }
        }


        private bool _isShowLabel = true;
        ///<summary>是否显示标签</summary>
        public bool isShowLabel
        {
            get { return _isShowLabel; }
            set
            {
                _isShowLabel = value;
                showLabel(value);
            }
        }

        ///<summary>读取或设置，mouse超过3D图面的多少时，操作为X轴旋转，而非移动，缺省值0.8</summary>
        public double XRotationScale { get { return controller.XRotationScale; } set { controller.XRotationScale = value; } }


        ///<summary>标签日期格式串，例{0:yyyy-MM-dd HH:mm}：</summary>
        public string labelDateFormat = "{0:yyyy-MM-dd HH:mm}：";
        ///<summary>标签值格式串，例{0:f2}万KW</summary>
        public string labelValueFormat = "{0:f2}";

        public string unit;
        #endregion

        bool isDataInitialized;

        ///<summary>呈现曲面</summary>
        private void showModel()
        {
            if (isDataInitialized && this.IsLoaded)
            {
                _pcenter = new Point3D(_data.para.Limit.X / 2, _data.para.Limit.Y / 2, -_data.para.Limit.Z / 2);
                //rotateXOperate(30);
                //rotateYOperate(60);
                //scaleOperate(1.4, _pcenter);
                cameraMatrix = Matrix3D.Parse("0.677591759338397,-1.0842021724855E-18,-1.22109854544041,0,-0.433844021639299,1.30538696558336,-0.240741531466645,0,1.14142937695003,0.496162392856479,0.633383065253878,0,23.3313019071457,10.1124301146577,5.11460614784044,1");


                _data.createModel();
                initControls();

                _data.Datas[0].isVisual = true;
                curCurve = _data.Datas[0];
                showLabel(isShowLabel);
                initTraceValue();
                _data.showModel(modelgroup);

                drawGrid();

                //圆角框裁剪
                brdQM.Clip = recQM.RenderedGeometry;
                brdQX.Clip = recQX.RenderedGeometry;

                setFHQXds(_data.startDate);



            }
        }

        CurveSurfaceDatas _data = new CurveSurfaceDatas();

        ///<summary>初始化曲面控件</summary>
        private void initControls()
        {

            //=========初始化选择radio
            //<RadioButton Content="负荷总量" Height="16" HorizontalAlignment="Left" Margin="6,6,0,0" Name="rdoSum" VerticalAlignment="Top" GroupName="changeMode" IsChecked="True" Foreground="White" />
            grdRdo.Children.Clear();
            int idx = 0;
            foreach (CurveSurfaceData one in _data.Datas)
            {
                RadioButton rdo = new RadioButton() { Content = one.name, Height = 24, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Foreground = Brushes.White };
                if (idx == 0)
                    rdo.IsChecked = true;
                rdo.Checked += new RoutedEventHandler(rdo_Checked);
                grdRdo.Children.Add(rdo);
                idx++;
            }
            //=========初始化slider
            //sliderDay.Minimum = 0;
            //sliderDay.Maximum = (_data.endDate - _data.startDate).TotalDays;


            //=======================底板
            double maxZ3D = _data.para.Limit.Z, maxY3D = _data.para.Limit.Y, maxX3D = _data.para.Limit.X;
            int zcount = 24, xcount = 12; Plane3D panel = new Plane3D(); planebuttom = panel;
            panel.Material = new DiffuseMaterial(Brushes.White);
            panel.BackMaterial = (MaterialGroup)this.FindResource("matTransparent");
            Transform3DGroup tg = new Transform3DGroup();
            tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90), new Point3D(0, 0, 0)));
            tg.Children.Add(new TranslateTransform3D(1, -1, -1));
            tg.Children.Add(new ScaleTransform3D(new Vector3D(maxX3D / 2, 0.1, maxZ3D / 2), new Point3D(0, 0, 0)));
            panel.Transform = tg;
            modelMain.Children.Add(panel);

            //=========左边板

            MyVisualHost vh = new MyVisualHost();
            panelLeft.Width = maxZ3D * 30;
            panelLeft.Height = maxY3D * 30;
            double devx2d = panelLeft.Width / zcount;
            double fontsizeZ = devx2d * 0.25 * 2;
            for (int i = 0; i < zcount; i++)
            {
                vh.CreateDrawingVisualLine(new Pen(Brushes.Black, 0.5), new Point(i * devx2d, 0), new Point(i * devx2d, panelLeft.Height));
                vh.CreateDrawingVisualText(i.ToString(), null, fontsizeZ, Brushes.Black, new Point((i + 0.25) * devx2d, 20));
            }
            panelLeft.Children.Add(vh);
            VisualBrush vbrush = new VisualBrush();
            Binding anglebind = new Binding();
            anglebind.Source = panelLeft;
            anglebind.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(vbrush, VisualBrush.VisualProperty, anglebind);

            panel = new Plane3D(); planeleft = panel;
            panel.Material = new DiffuseMaterial(vbrush);
            panel.BackMaterial = (MaterialGroup)this.FindResource("matTransparent");
            tg = new Transform3DGroup();
            tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90), new Point3D(0, 0, 0)));
            tg.Children.Add(new TranslateTransform3D(-1, 1, -1));
            tg.Children.Add(new ScaleTransform3D(new Vector3D(0.1, maxY3D / 2, maxZ3D / 2), new Point3D(0, 0, 0)));
            panel.Transform = tg;
            modelMain.Children.Add(panel);
            //========背景板
            vh = new MyVisualHost();
            panelBack.Width = maxX3D * 30;
            panelBack.Height = maxY3D * 30;
            devx2d = panelBack.Width / xcount;
            fontsizeZ = devx2d * 0.2 * 2;
            for (int i = 0; i < xcount; i++)
            {
                int tmp = _data.startDate.AddMonths(i).Month;
                vh.CreateDrawingVisualLine(new Pen(Brushes.Black, 0.5), new Point(i * devx2d, 0), new Point(i * devx2d, panelBack.Height));
                vh.CreateDrawingVisualText(tmp.ToString() + "月", null, fontsizeZ, Brushes.Black, new Point((i + 0.2) * devx2d, 20));

                //vh.CreateDrawingVisualLine(new Pen(Brushes.Black, 0.5), new Point(i * devx2d, 0), new Point(i * devx2d, panelBack.Height));
                //vh.CreateDrawingVisualText((i + 1).ToString() + "月", null, fontsizeZ, Brushes.Black, new Point((i + 0.2) * devx2d, 20));
            }
            panelBack.Children.Add(vh);
            vbrush = new VisualBrush();
            anglebind = new Binding();
            anglebind.Source = panelBack;
            anglebind.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(vbrush, VisualBrush.VisualProperty, anglebind);

            panel = new Plane3D(); planeback = panel;
            panel.Material = new DiffuseMaterial(vbrush);
            panel.BackMaterial = (MaterialGroup)this.FindResource("matTransparent");
            tg = new Transform3DGroup();
            tg.Children.Add(new ScaleTransform3D(new Vector3D(maxX3D / 2, maxY3D / 2, 0.1), new Point3D(-1, -1, 1)));
            tg.Children.Add(new TranslateTransform3D(1, 1, -maxZ3D - 1));
            panel.Transform = tg;
            modelMain.Children.Add(panel);

            //初始化滑动面
            movehour.OffsetZ = -maxZ3D - 1.001;
            movemonth.OffsetX = -1 - 0.001;
            moveValue.OffsetY = -1 - 0.001;

            panelHourScale.ScaleY = (maxY3D - 2) / 2;
            panelHourScale.ScaleX = maxX3D / 2;

            panelMonthScale.ScaleY = (maxY3D - 2) / 2;
            panelMonthScale.ScaleZ = maxZ3D / 2;

            panelValueScale.ScaleX = maxX3D / 2;
            panelValueScale.ScaleZ = maxZ3D / 2;

        }

        void rdo_Checked(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            foreach (CurveSurfaceData one in _data.Datas)
            {
                if ((sender as RadioButton).Content.ToString() == one.name)
                {
                    one.isVisual = true;
                    curCurve = one;
                    if (isShowLabel)
                    {
                        updateLabelContent();
                        updateLabelPosition();
                    }
                    initTraceValue();
                }
                else
                    one.isVisual = false;
            }
            _data.showModel(modelgroup);
        }


        Plane3D planeleft, planeback, planebuttom;


        CurveSurfaceData curCurve;
        csDataPoint curMin, curMax;
        Microsoft.Expression.Controls.Callout labelMax, labelMin;
        TextBlock txtMax, txtMin, txtMaxDate, txtMinDate;
        ///<summary>创建标签</summary>
        void createLabel()
        {
            labelMax = new Microsoft.Expression.Controls.Callout() { AnchorPoint = new Point(0.05, 1.5), Stroke = Brushes.Black, Width = 200, Height = 30, Fill = Brushes.LightYellow, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = System.Windows.VerticalAlignment.Top, IsHitTestVisible = false, StrokeThickness = 0.5 };
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
            labelMax.Content = sp;
            txtMaxDate = new TextBlock();
            txtMax = new TextBlock() { Foreground = Brushes.Blue };
            sp.Children.Add(txtMaxDate);
            sp.Children.Add(txtMax);

            labelMin = new Microsoft.Expression.Controls.Callout() { AnchorPoint = new Point(0.05, 1.5), Stroke = Brushes.Black, Width = 200, Height = 30, Fill = Brushes.LightYellow, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = System.Windows.VerticalAlignment.Top, IsHitTestVisible = false, StrokeThickness = 0.5 };
            sp = new StackPanel() { Orientation = Orientation.Horizontal };
            labelMin.Content = sp;
            txtMinDate = new TextBlock();
            txtMin = new TextBlock() { Foreground = Brushes.Blue };
            sp.Children.Add(txtMinDate);
            sp.Children.Add(txtMin);
        }
        ///<summary>更新标签内容</summary>
        void updateLabelContent()
        {
            IOrderedEnumerable<csDataPoint> tmp = curCurve.orgPoints.OrderBy(p => p.zValue);
            curMin = tmp.ElementAt(0);
            curMax = tmp.ElementAt(tmp.Count() - 1);

            txtMinDate.Text = string.Format(labelDateFormat, curMin.zDate);
            txtMin.Text = string.Format(labelValueFormat, curMin.zValue);
            txtMaxDate.Text = string.Format(labelDateFormat, curMax.zDate);
            txtMax.Text = string.Format(labelValueFormat, curMax.zValue);
        }
        ///<summary>更新标签位置</summary>
        void updateLabelPosition()
        {
            Point3D p3d;
            Point pnt, pnt2;
            GeneralTransform3DTo2D tran = Model0.TransformToAncestor(grdCurve);
            p3d = curCurve.getPoint3DFromDateValue(curMin);
            pnt = tran.Transform(p3d);
            //AnchorPoint = new Point(0.05, 1.4), Stroke = Brushes.Cyan, Width = 200, Height = 30
            pnt2 = new Point(pnt.X - labelMin.Width * labelMin.AnchorPoint.X, pnt.Y - labelMin.Height * labelMin.AnchorPoint.Y);
            labelMin.Margin = new Thickness(pnt2.X, pnt2.Y, 0, 0);

            p3d = curCurve.getPoint3DFromDateValue(curMax);
            pnt = tran.Transform(p3d);
            pnt2 = new Point(pnt.X - labelMax.Width * labelMax.AnchorPoint.X, pnt.Y - labelMax.Height * labelMax.AnchorPoint.Y);
            labelMax.Margin = new Thickness(pnt2.X, pnt2.Y, 0, 0);

        }

        ///<summary>显示标签</summary>
        void showLabel(bool isshow)
        {
            // <blend:Callout AnchorPoint="0.05,1.4" Stroke="#FFFF0A0A" Width="200" Height="100" Fill="#FFF29E9E" >
            //    <StackPanel>
            //        <TextBlock Text="skldf"/>
            //        <TextBlock Text="skldf"/>
            //    </StackPanel>
            //</blend:Callout>
            if (isshow)
            {
                if (labelMax == null)
                {
                    createLabel();
                    grdCurve.Children.Add(labelMin);
                    grdCurve.Children.Add(labelMax);
                }

                updateLabelContent();
                updateLabelPosition();

                labelMax.Visibility = System.Windows.Visibility.Visible;
                labelMin.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                if (labelMax != null)
                {
                    labelMax.Visibility = System.Windows.Visibility.Collapsed;
                    labelMin.Visibility = System.Windows.Visibility.Collapsed;
                }
            }



        }




        #region 交互控制
        private string _controlStatus = "";
        private Point _oldPoint;
        private Nullable<Point3D> _pcenter;
        private Nullable<Point3D> _pcenterScale;  //触摸缩放操作中心点
        private Nullable<Point3D> _pcenterRotation;  //触摸旋转操作中心点
        private double _distcenter;
        Point3D panelCenter = new Point3D(0, 0, 0);
        private void mainViewport3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _pcenter = null;
            calPCenter(e.GetPosition(mainViewport3D));
            scaleOperate(e.Delta > 0 ? 1.05 : 0.95, _pcenter);

            //controlpanel.zoomValue = controlpanel.zoomValue * (e.Delta > 0 ? 0.95 : 1.05);
        }

        private void mainViewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isSlide = false;
            Point newPoint = e.GetPosition(mainViewport3D);
            isSlide = hitTestPanel(newPoint); //点击测试时间侧板
            if (isSlide)
                e.Handled = true;
            //if (!isSlide)  //非点中滑动面(即小时或月的滑动)
            //{
            //    if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
            //    {
            //        _controlStatus = "move";
            //    }
            //    else if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
            //    {
            //        _controlStatus = "rotationY";
            //    }
            //    else if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Pressed)
            //    {
            //        _controlStatus = "rotationX";
            //    }



            //    _oldPoint = e.GetPosition(sender as Viewport3D);
            //}
        }



        private void mainViewport3D_MouseLeave(object sender, MouseEventArgs e)
        {
            _controlStatus = "";
            _pcenter = null;
        }

        private void mainViewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _controlStatus = "";
            _pcenter = null;
        }

        private void mainViewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            Point newPoint = e.GetPosition(mainViewport3D);
            _pcenter = null;
            if (_controlStatus == "drag")
                hitTestPanel(newPoint); //点击测试时间侧板

            else if (_controlStatus == "move")
            {
                Point3D? porg, pobj;
                calPCenter(_oldPoint);
                porg = _pcenter;
                calPCenter(newPoint);
                pobj = _pcenter;

                if (porg != null && pobj != null)
                {
                    Vector3D v3 = (Vector3D)(porg - pobj);
                    traslateOperate(v3);
                }
            }
            else if (_controlStatus == "rotationX")
            {
                calPCenter(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
                if (_pcenter != null)
                {
                    Point mp = new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
                    Vector vecold = _oldPoint - mp;
                    Vector vecnew = newPoint - mp;
                    double dev = Vector.AngleBetween(vecold, vecnew);
                    rotateXOperate(dev);
                }
            }
            else if (_controlStatus == "rotationY")
            {
                calPCenter(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
                if (_pcenter != null)
                {
                    Point mp = Model0.TransformToAncestor(mainViewport3D).Transform((Point3D)_pcenter);
                    Vector vecold = _oldPoint - mp;
                    Vector vecnew = newPoint - mp;
                    double dev = Vector.AngleBetween(vecold, vecnew);
                    rotateYOperate(dev);
                }
            }
            else
            {
                HitTestResult result = VisualTreeHelper.HitTest(mainViewport3D, newPoint);
                if (result is RayMeshGeometry3DHitTestResult)
                {
                    RayMeshGeometry3DHitTestResult ray3DResult = (RayMeshGeometry3DHitTestResult)result;

                    // SettleNode one = ray3DResult.ModelHit.GetValue(ToolTipService.ToolTipProperty) as SettleNode;
                    //if (one != null) //tooltips
                    //{

                    //填充信息
                    //ttContent.Text = "电费：" + one.fee.ToString("f0") + "\r\n" + "电量：" + one.energy.ToString("f0") + "\r\n" + "电价：" + one.price.ToString("f4");
                    //double ToolTipOffset = 5;
                    //Point position = e.GetPosition(mainViewport3D);
                    //toolTips.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                    //toolTips.PlacementTarget = mainViewport3D;
                    //toolTips.HorizontalOffset = position.X + ToolTipOffset;
                    //toolTips.VerticalOffset = position.Y + ToolTipOffset;
                    //toolTips.IsOpen = true;
                    //}
                    //else
                    //{
                    //    //toolTips.IsOpen = false;
                    //}
                }

            }


            _oldPoint = newPoint;
        }

        private void calPCenter(Point p2d)
        {
            PointHitTestParameters pointparams = new PointHitTestParameters(p2d);

            VisualTreeHelper.HitTest(mainViewport3D, null, HTResult, pointparams);
        }
        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {

                if (rayResult.ModelHit.Equals((planebuttom.Content as Model3DGroup).Children[0]))
                {
                    _pcenter = rayResult.PointHit;
                    //_pcenter = tgpanel.Transform(rayResult.PointHit);
                    _pcenter = (planebuttom.Transform as Transform3DGroup).Transform(rayResult.PointHit);
                    _distcenter = rayResult.DistanceToRayOrigin;
                    return HitTestResultBehavior.Stop;
                }

            }

            return HitTestResultBehavior.Continue;
        }







        //==== 操作
        ///<summary>缩放操作处理</summary>
        public void scaleOperate(double dev, Point3D? pcenter)
        {
            if (pcenter == null) return;
            Matrix3D matrix = cameratransform.Matrix;
            matrix.ScaleAt(new Vector3D(dev, dev, dev), (Point3D)pcenter);
            cameratransform.Matrix = matrix;
        }

        ///<summary>位移操作处理</summary>
        private void traslateOperate(Vector3D vec)
        {

            Matrix3D matrix = cameratransform.Matrix;
            matrix.Translate(vec);
            cameratransform.Matrix = matrix;


        }
        ///<summary>X轴旋转处理</summary>
        private void rotateXOperate(double dev)
        {
            Matrix3D matrix = cameratransform.Matrix;
            matrix.RotateAt(new Quaternion(matrix.Transform(new Vector3D(1, 0, 0)), -dev), (Point3D)_pcenter);
            cameratransform.Matrix = matrix;

        }
        ///<summary>Y轴旋转处理</summary>
        private void rotateYOperate(double dev)
        {
            Matrix3D matrix = cameratransform.Matrix;
            matrix.RotateAt(new Quaternion(new Vector3D(0, 1, 0), dev), (Point3D)_pcenter);
            cameratransform.Matrix = matrix;

        }

        public void rotateXOperate(double dev, Point3D? pcenter)
        {
            if (pcenter == null) return;
            Matrix3D matrix = cameratransform.Matrix;
            matrix.RotateAt(new Quaternion(matrix.Transform(new Vector3D(1, 0, 0)), -dev), (Point3D)pcenter);
            cameratransform.Matrix = matrix;
        }
        public void rotateYOperate(double dev, Point3D? pcenter)
        {
            if (pcenter == null) return;
            Matrix3D matrix = cameratransform.Matrix;
            matrix.RotateAt(new Quaternion(new Vector3D(0, 1, 0), dev), (Point3D)pcenter);
            cameratransform.Matrix = matrix;
        }



        #endregion 交互控制

        #region 负荷曲线
        ///<summary>设置小时轴截面</summary>
        private void setFHQXds(int hour)
        {
            mycanv.Children.Clear();

            foreach (CurveSurfaceData one in _data.Datas)
            {
                if (one.isVisual)
                {
                    Point[] pnts = new Point[one.orgPoints.Where(p => p.zDate.Hour == hour).Count()];
                    int i = 0;
                    foreach (csDataPoint e0 in one.orgPoints.Where(p => p.zDate.Hour == hour).OrderBy(p => p.zDate))
                    {
                        pnts[i].X = (e0.zDate - _data.startDate).TotalDays;
                        pnts[i].Y = (double)e0.zValue;
                        i++;
                    }
                    drawSpline(pnts, false);
                }
            }


            gridcanv12.Visibility = Visibility.Visible;
            gridcanv24.Visibility = Visibility.Hidden;
            gridcanvM.Visibility = Visibility.Hidden;
            rectValue.Visibility = Visibility.Hidden;
            mycanv.Visibility = Visibility.Visible;

        }
        ///<summary>设置日期轴截面</summary>
        private void setFHQXds(DateTime selDate)
        {
            mycanv.Children.Clear();

            foreach (CurveSurfaceData one in _data.Datas)
            {
                if (one.isVisual)
                {
                    Point[] pnts = new Point[one.orgPoints.Where(p => p.zDate.Date == selDate.Date).Count()];
                    int i = 0;
                    foreach (csDataPoint e0 in one.orgPoints.Where(p => p.zDate.Date == selDate.Date).OrderBy(p => p.zDate.Hour))
                    {
                        pnts[i].X = e0.zDate.Hour;
                        pnts[i].Y = (double)e0.zValue;
                        i++;
                    }
                    drawSpline(pnts, true);
                }
            }
            gridcanv12.Visibility = Visibility.Hidden;
            gridcanv24.Visibility = Visibility.Visible;
            gridcanvM.Visibility = Visibility.Hidden;
            rectValue.Visibility = Visibility.Hidden;
            mycanv.Visibility = Visibility.Visible;
        }
        ///<summary>设置值截面</summary>
        private void setFHQXds(double value)
        {
            Brush cbrush = curCurve.curveBrush.Clone();
            TransformGroup ctg = new TransformGroup();
            ctg.Children.Add(new RotateTransform(90, 0.5, 0.5));
            ctg.Children.Add(new ScaleTransform(-1, 1, 0.5, 0.5));
            cbrush.RelativeTransform = ctg;
            rectValue.Fill = cbrush;
            double checkvalue = traceValue.Value * maxValue / 50;
            if (checkvalue > minValue * 1.08)
            {
                Geometry geo = curCurve.getValueGeometry(checkvalue, rectValue.ActualWidth, rectValue.ActualHeight);
                rectValue.Clip = geo;
            }
            else
                rectValue.Clip = null;

            string txtinfo = "高于{0:f0}{1}（共{2:f0}小时，{3:p0}）";
            double bfb = 1.0f * curCurve.orgPoints.Count(p => p.zValue >= checkvalue) / curCurve.orgPoints.Count;
            double hours = 1.0f * bfb * (_data.endDate - _data.startDate).TotalHours;

            lblDate.Text = string.Format(txtinfo, checkvalue, unit, hours, bfb);

            gridcanv12.Visibility = Visibility.Hidden;
            gridcanv24.Visibility = Visibility.Visible;
            gridcanvM.Visibility = Visibility.Visible;
            rectValue.Visibility = Visibility.Visible;
            mycanv.Visibility = Visibility.Hidden;

            moveValue.OffsetY = -1 + _data.para.Limit.Y * checkvalue / _data.maxYValue;
            if (checkvalue==0)
                moveValue.OffsetY = -1 -0.001;
        }

        double gridMaxValue;//, gridMinValue;

        string saveQX = "小时";
        int hour = 0;
        DateTime selDate;

        #endregion 负荷曲线



        #region 交互负荷曲线

        GeometryModel3D hitPanel;
        Point3D hitPoint;
        ///<summary>面板点击测试</summary>
        private bool hitTestPanel(Point p2d)
        {
            bool result = false;
            PointHitTestParameters pointparams = new PointHitTestParameters(p2d);
            hitPanel = null;
            VisualTreeHelper.HitTest(mainViewport3D, null, HTPanelResult, pointparams);
            if (hitPanel != null)
            {
                if (hitPanel.Equals((planeback.Content as Model3DGroup).Children[0])) //月
                {
                    GeneralTransform3D trans = Model0.TransformToDescendant(planeback);
                    Point3D p = trans.Transform(hitPoint);
                    p = planeback.Transform.Transform(p);
                    double wz = (p.X - hitPanel.Bounds.X) / hitPanel.Bounds.SizeX;
                    int days = (int)(wz * (_data.endDate - _data.startDate).TotalDays);
                    selDate = _data.startDate.AddDays(days);
                    lblDate.Text = string.Format("{0:yyyy年MM月dd日}", selDate);

                    setFHQXds(selDate);
                    saveQX = "日期";
                    movemonth.OffsetX = _data.para.Limit.X * wz - 1;
                    _controlStatus = "drag";
                    result = true;
                }
                else if (hitPanel.Equals((planeleft.Content as Model3DGroup).Children[0])) //小时
                {
                    GeneralTransform3D trans = Model0.TransformToDescendant(planeleft);
                    Point3D p = trans.Transform(hitPoint);
                    p = planeleft.Transform.Transform(p);
                    double wz = (p.X - hitPanel.Bounds.X) / hitPanel.Bounds.SizeX;
                    hour = (int)(wz * 24);
                    lblDate.Text = string.Format("{0}点", hour);
                    setFHQXds(hour);
                    saveQX = "小时";
                    movehour.OffsetZ = -1 - _data.para.Limit.Z * wz;
                    _controlStatus = "drag";
                    result = true;
                }
            }
            return result;
        }
        private HitTestResultBehavior HTPanelResult(System.Windows.Media.HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                if (rayResult.ModelHit.Equals((planeback.Content as Model3DGroup).Children[0]) || rayResult.ModelHit.Equals((planeleft.Content as Model3DGroup).Children[0]))
                {
                    hitPanel = (GeometryModel3D)rayResult.ModelHit;
                    hitPoint = rayResult.PointHit;
                    return HitTestResultBehavior.Stop;
                }

            }

            return HitTestResultBehavior.Continue;
        }

        #endregion


        #region 备用

        private void drawSpline(Point[] ap, bool isDay)
        {

            double w;
            if (!isDay)
                w = mycanv.ActualWidth * (_data.Datas.Max(p => p.orgPoints.Max(p2 => p2.zDate)) - _data.startDate).TotalDays / (_data.endDate - _data.startDate).TotalDays;
            else
                w = mycanv.ActualWidth;
            double h = mycanv.ActualHeight;

            int len = 0;
            for (int i = ap.Count() - 1; i > -1; i--)
            {
                if (ap[i].Y > 0)
                {
                    len = i + 1;
                    break;
                }
            }

            Point[] np = new Point[len];

            for (int i = 0; i < len; i++)
            {
                np[i].X = ap[i].X / (ap.Count() - 1) * w;
                np[i].Y = (gridMaxValue - ap[i].Y) / gridMaxValue * h;

            }



            // Get Bezier Spline Control Points.
            Point[] cp1, cp2;
            if ((np.Count() > 2))
            {
                MyGeometryHelper.GetCurveControlPoints(np, out cp1, out cp2);
                // Draw curve by Bezier.
                PathSegmentCollection lines = new PathSegmentCollection();
                for (int m = 0; m < cp1.Length; ++m)
                {
                    lines.Add(new BezierSegment(cp1[m], cp2[m], np[m + 1], true));
                }
                PathFigure f = new PathFigure(np[0], lines, false);
                PathGeometry g = new PathGeometry(new PathFigure[] { f });
                Path path = new Path()
                {
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 1,
                    Data = g
                };
                mycanv.Children.Add(path);
            }

        }

        private void drawGrid()  //绘网格
        {
            gridcanv12.Children.Clear();
            gridcanv24.Children.Clear();
            gridcanvV.Children.Clear();
            gridcanvM.Children.Clear();

            double w = mycanv.ActualWidth, h = mycanv.ActualHeight;

            PathGeometry g;
            Path path;
            //24点竖线
            g = new PathGeometry();
            for (int m = 1; m < 23; ++m)
            {
                PathFigure f = new PathFigure();
                f.StartPoint = new Point(w * m / 23, 0);
                f.Segments.Add(new LineSegment(new Point(w * m / 23, h), true));
                g.Figures.Add(f);

                TextBlock tx = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16 };
                tx.Text = m.ToString();
                Canvas.SetLeft(tx, w * m / 23);
                Canvas.SetTop(tx, h - 36);
                gridcanv24.Children.Add(tx);

            }
            path = new Path() { Stroke = new SolidColorBrush(Colors.Gainsboro), StrokeThickness = 0.5, Data = g };
            gridcanv24.Children.Add(path);
            //12点竖线
            int xcount = 12;
            g = new PathGeometry();
            for (int m = 0; m < xcount; ++m)
            {
                PathFigure f = new PathFigure();
                f.StartPoint = new Point(w * m / 12, 0);
                f.Segments.Add(new LineSegment(new Point(w * m / 12, h), true));
                g.Figures.Add(f);

                int tmp = _data.startDate.AddMonths(m).Month;
                TextBlock tx = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16 };
                tx.Text = tmp.ToString() + "月";
                Canvas.SetLeft(tx, w * m / 12);
                Canvas.SetTop(tx, h - 36);
                gridcanv12.Children.Add(tx);
            }
            path = new Path() { Stroke = new SolidColorBrush(Colors.Gainsboro), StrokeThickness = 0.5, Data = g };
            gridcanv12.Children.Add(path);

            //横线值
            int tmp1 = ((int)_data.maxYValue).ToString().Length;

            int tmp2 = int.Parse(((int)_data.maxYValue).ToString().Substring(0, 2)) + 1;
            gridMaxValue = double.Parse(tmp2.ToString().PadRight(tmp1, '0'));

            g = new PathGeometry();
            for (int m = 0; m < 4; ++m)
            {
                PathFigure f = new PathFigure();
                f.StartPoint = new Point(0, h * m / 4);
                f.Segments.Add(new LineSegment(new Point(w, h * m / 4), true));
                g.Figures.Add(f);
            }
            path = new Path() { Stroke = new SolidColorBrush(Colors.Gainsboro), StrokeThickness = 0.5, Data = g };
            gridcanvV.Children.Add(path);
            TextBlock txv = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16 };
            txv.Text = gridMaxValue.ToString();
            Canvas.SetLeft(txv, 5);
            gridcanvV.Children.Add(txv);
            txv = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16 };
            txv.Text = (gridMaxValue / 2).ToString();
            Canvas.SetLeft(txv, 5);
            Canvas.SetTop(txv, h / 2);
            gridcanvV.Children.Add(txv);


            //横线月
            tmp1 = ((int)_data.maxYValue).ToString().Length;

            tmp2 = int.Parse(((int)_data.maxYValue).ToString().Substring(0, 2)) + 1;

            g = new PathGeometry();
            for (int m = 0; m < 4; ++m)
            {
                PathFigure f = new PathFigure();
                f.StartPoint = new Point(0, h * m / 4);
                f.Segments.Add(new LineSegment(new Point(w, h * m / 4), true));
                g.Figures.Add(f);
            }
            path = new Path() { Stroke = new SolidColorBrush(Colors.Gainsboro), StrokeThickness = 0.5, Data = g };
            gridcanvM.Children.Add(path);
            txv = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16, Text = "1月" };
            Canvas.SetLeft(txv, rectValue.ActualWidth - 30);
            gridcanvM.Children.Add(txv);
            txv = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16, Text = "4月" };
            Canvas.SetLeft(txv, rectValue.ActualWidth - 30);
            Canvas.SetTop(txv, h * 0.25);
            gridcanvM.Children.Add(txv);
            txv = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16, Text = "7月" };
            Canvas.SetLeft(txv, rectValue.ActualWidth - 30);
            Canvas.SetTop(txv, h * 0.5);
            gridcanvM.Children.Add(txv);
            txv = new TextBlock() { Foreground = Brushes.DeepSkyBlue, FontSize = 16, Text = "10月" };
            Canvas.SetLeft(txv, rectValue.ActualWidth - 30);
            Canvas.SetTop(txv, h * 0.75);
            gridcanvM.Children.Add(txv);
        }





        #endregion

        #region 多点触摸

        Point startpoint, savepoint;
        int starttime, savetime;
        int clicknum = 0;
        Rect controlbouns;
        private void mainViewport3D_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = mainViewport3D;
            //单点操作设置
            e.IsSingleTouchEnabled = true;
            Point pc = new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
            //pc=mainViewport3D.TranslatePoint(pc,grdMain);
            e.Pivot = new ManipulationPivot(pc, 200);

            e.Handled = true;
        }

        private void mainViewport3D_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            hitTestPanel(e.ManipulationOrigin);


            //存最初触点
            startpoint = e.ManipulationOrigin;
            starttime = e.Timestamp;
            getCrossPoint3D(startpoint);
            _pcenterScale = _pcenter;
            Point pc = new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
            getCrossPoint3D(pc);
            _pcenterRotation = _pcenter;
            //存最初判断移动或旋转的矩形区域
            Rect rect0 = new Rect(0, 0, mainViewport3D.ActualWidth, mainViewport3D.ActualHeight);
            Rect rect1 = new Rect(mainViewport3D.ActualWidth / 4, mainViewport3D.ActualHeight / 4, mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
            controlbouns = rect1;// rect3;

        }
        private Rect get3DRect()  //获得3D模型的2D投影rect
        {
            Rect rect = modelMain.TransformToAncestor(mainViewport3D).TransformBounds(modelMain.Content.Bounds);
            rect.Union(planeback.TransformToAncestor(mainViewport3D).TransformBounds(planeback.getBouns()));
            rect.Union(planeleft.TransformToAncestor(mainViewport3D).TransformBounds(planeleft.getBouns()));
            rect.Union(planebuttom.TransformToAncestor(mainViewport3D).TransformBounds(planebuttom.getBouns()));
            return rect;
        }


        private void mainViewport3D_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {

            e.TranslationBehavior.DesiredDeceleration = 5.0 * 96.0 / (1000.0 * 1000.0);
            //e.ExpansionBehavior.DesiredDeceleration = 0.10 * 96 / (1000.0 * 1000.0);
            e.RotationBehavior.DesiredDeceleration = 360 / (1000.0 * 1000.0);
            e.Handled = true;
        }

        private void mainViewport3D_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_controlStatus == "drag")
            {
                hitTestPanel(e.ManipulationOrigin);
            }
            else
            {

                //===== 缩放
                double dev;
                dev = 1.0 / e.DeltaManipulation.Scale.X;
                //_pcenter = null;
                //calPCenter(startpoint);
                scaleOperate(dev, _pcenterScale);

                if (controlbouns.Contains(startpoint))
                {

                    //===== 移动
                    Point3D? porg, pobj;
                    getCrossPoint3D(e.ManipulationOrigin);
                    porg = _pcenter;
                    getCrossPoint3D(e.ManipulationOrigin + e.DeltaManipulation.Translation);
                    pobj = _pcenter;
                    if (porg != null && pobj != null)
                    {
                        Vector3D v3 = (Vector3D)(porg - pobj);
                        traslateOperate(v3);
                    }
                }
                else
                {
                    //===== 旋转
                    dev = e.DeltaManipulation.Rotation;
                    //x旋转
                    if (Math.Abs(e.DeltaManipulation.Translation.X) < Math.Abs(e.DeltaManipulation.Translation.Y))
                    {
                        rotateXOperate(dev, _pcenterRotation);
                    }   //y旋转
                    else
                    {
                        rotateYOperate(dev, _pcenterRotation);
                    }
                }
            }
        }

        private void mainViewport3D_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _controlStatus = "none";
        }


        /// <summary>
        /// 获取屏幕点与地平面Y=0的交点
        /// </summary>
        /// <param name="p">viewport3D上的屏幕点</param>
        private void getCrossPoint3D(Point p)
        {
            Matrix3D matrix2, matrixc;
            Vector3D v1, v2;
            Vector3D raxis; double rangle;
            //v1 = new Vector3D(0, 0, -1);
            PerspectiveCamera camera = mainViewport3D.Camera as PerspectiveCamera;
            matrixc = (camera.Transform as MatrixTransform3D).Matrix;

            Point3D pline = camera.Position;
            pline = matrixc.Transform(pline);
            Vector3D vline = camera.LookDirection;
            vline = matrixc.Transform(vline);
            double angx = Math.Atan((p.X - mainViewport3D.ActualWidth / 2) / (mainViewport3D.ActualWidth / 2) * Math.Tan(camera.FieldOfView / 2 / 180 * Math.PI));
            double angy = Math.Atan((-p.Y + mainViewport3D.ActualHeight / 2) / (mainViewport3D.ActualWidth / 2) * Math.Tan(camera.FieldOfView / 2 / 180 * Math.PI));
            v1 = new Vector3D(0, 0, -1);
            v2 = new Vector3D(Math.Tan(angx), Math.Tan(angy), -1);
            raxis = Vector3D.CrossProduct(v1, v2);
            raxis.Normalize();
            raxis = matrixc.Transform(raxis);
            raxis.Normalize();
            rangle = Vector3D.AngleBetween(v1, v2);
            if (rangle != 0)
            {
                matrix2 = new Matrix3D();
                matrix2.Rotate(new Quaternion(raxis, rangle));
                vline = matrix2.Transform(vline);
                vline.Normalize();
            }
            _pcenter = Model3DHelper.calFlatXLine(new Vector3D(0, 1, 0), new Point3D(10000, 0, 10000), vline, pline);

        }


        #endregion

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isDataInitialized && IsLoaded)
            {
                brdQM.Clip = recQM.RenderedGeometry;
                brdQX.Clip = recQX.RenderedGeometry;
                drawGrid();
                if (saveQX == "小时")
                    setFHQXds(hour);
                else
                    setFHQXds(selDate);
            }
        }


        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isSlide = false;
            Point newPoint = e.GetPosition(mainViewport3D);
            isSlide = hitTestPanel(newPoint); //点击测试时间侧板
            if (isSlide)
                e.Handled = true;

        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_controlStatus == "drag")
            {
                hitTestPanel(e.GetPosition(mainViewport3D));
                e.Handled = true;
            }

        }

        private void Grid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _controlStatus = "none";
        }

        private void controller_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            bool isSlide = hitTestPanel(e.ManipulationOrigin);
            if (isSlide)
                e.Handled = true;
            else
            {
                controller.ManipulationStartedMothod(sender, e);
                e.Handled = true;
            }
        }

        private void controller_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_controlStatus == "drag")
                hitTestPanel(e.ManipulationOrigin);
            else
                controller.ManipulationDeltaMothod(sender, e);
            e.Handled = true;
        }

        private void controller_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_controlStatus == "drag")
                _controlStatus = "none";
            else
                controller.ManipulationCompletedMothod(sender, e);
            e.Handled = true;
        }




        #region 公开方法
        /// <summary>
        /// 移动到指定日期
        /// </summary>
        /// <param name="date">指定日期</param>
        public void moveToDate(DateTime date)
        {
            double wz = ((double)(date - _data.startDate).Days / (_data.endDate - _data.startDate).Days);
            lblDate.Text = string.Format("{0:yyyy年MM月dd日}", date);
            setFHQXds(date);
            movemonth.OffsetX = _data.para.Limit.X * wz - 1;
        }

        /// <summary>
        /// 移动到指定小时
        /// </summary>
        /// <param name="hour">时点，支持小数</param>
        public void moveToHour(double hour)
        {
            double wz = (double)hour / 24;
            lblDate.Text = string.Format("{0}点", hour);
            setFHQXds((int)hour);
            movehour.OffsetZ = -1 - _data.para.Limit.Z * wz;
        }


        #endregion



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

                    foreach (RadioButton item in grdRdo.Children)
                    {
                        item.Checked -= rdo_Checked;
                    }
                    grdRdo.Children.Clear();
                    modelMain.Children.Clear();

                    foreach (CurveSurfaceData item in _data.Datas)
                    {
                        item.samplePoints.Clear();
                        item.orgPoints.Clear();
                        item.model = null;
                    }

                    _data.Datas.Clear();
                    modelgroup.Children.Clear();
                    _data = null;


                    controller.showTooltips = null;
                    controller.showDetail = null;
                    controller.mainViewport3D = null;
                    controller.clearDynControl();
                    modelgroup = null;

                    controller = null;
                    GC.Collect();
                    GC.WaitForFullGCComplete();
                }



                disposed = true;

            }
        }

        ~UCCurveSurface()
        {

            Dispose(false);
        }

        #endregion

        private void controller_CameraChanged(object sender, EventArgs e)
        {
            if (isShowLabel)
                updateLabelPosition();
        }


        #region =========== 值截面相关 ==========

        double maxValue = 1;
        double minValue = 0;
        void initTraceValue()
        {
            maxValue = curCurve.orgPoints.Max(p => p.zValue);
            minValue = curCurve.orgPoints.Min(p => p.zValue);
            traceValue.Maximum = 50;
            traceValue.LargeStep = 5;
            traceValue.SmallStep = 0.1;
            traceValue.Value = 0;
        }


        ///<summary>值改变</summary>
        private void traceValue_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            double checkvalue = traceValue.Value * maxValue / 50;
            setFHQXds(checkvalue);
        }



        #endregion



    }



}
