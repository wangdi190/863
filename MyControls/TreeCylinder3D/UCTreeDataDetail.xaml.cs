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
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyClassLibrary.Share3D;

namespace MyControlLibrary.Controls3D.TreeCylinder3D
{
    /// <summary>
    /// UCSettleDetail.xaml 的交互逻辑
    /// </summary>
    public partial class UCTreeDataDetail : UserControl
    {
        public UCTreeDataDetail()
        {
            InitializeComponent();
        }


        private string _heightTitle = "高度";
        public string heightTitle
        {
            get { return _heightTitle; }
            set { _heightTitle = value; }
        }


        private string _sectionTitle = "截面";
        public string sectionTitle
        {
            get { return _sectionTitle; }
            set { _sectionTitle = value; }
        }



        private string _volumeTitle = "体积";
        public string volumeTitle
        {
            get { return _volumeTitle; }
            set { _volumeTitle = value; }
        }
      

        private void addCube(SettleNode node)
        {
            GeometryModel3D model;
            MeshGeometry3D mesh;
            MaterialGroup mat;
            Transform3DGroup tg;
            ////底
            //mesh = Share.Model3DHelper.genCubePlaneMesh(4);
            //mat = (MaterialGroup)Application.Current.FindResource("matTransWhite");
            //model = new GeometryModel3D(mesh, mat);
            //tg = new Transform3DGroup();
            //tg.Children.Add(new ScaleTransform3D(1, 1, 1, -1, 0, 0));
            //tg.Children.Add(new TranslateTransform3D(-1, -1, 0));
            //model = new GeometryModel3D(mesh, mat);
            //model.Transform = tg;
            //mgLeft.Children.Add(model);
            ////顶
            //mesh = Share.Model3DHelper.genCubePlaneMesh(4);
            //mat = (MaterialGroup)Application.Current.FindResource("matTransWhite");
            //model = new GeometryModel3D(mesh, mat);
            //tg = new Transform3DGroup();
            //tg.Children.Add(new ScaleTransform3D(1, 1, 1, -1, 0, 0));
            //tg.Children.Add(new TranslateTransform3D(-1, 1, 0));
            //model = new GeometryModel3D(mesh, mat);
            //model.Transform = tg;
            //mgLeft.Children.Add(model);

            //主体
            mat = (MaterialGroup)Application.Current.FindResource("matTransRed");
            mesh = (MeshGeometry3D)Application.Current.FindResource("meshCyl");
            tg = new Transform3DGroup();
            tg.Children.Add(new ScaleTransform3D(1, 2, 1, 0, -1, 0));
            tg.Children.Add(new TranslateTransform3D(0, 1, 0));
            model = new GeometryModel3D(mesh, mat);
            model.Transform = tg;
            mgLeft.Children.Add(model);

            

        }

        Color[] darkc = new Color[12];
        private void initSource() //初始化资源
        {
            darkc[0] = Colors.Red;
            darkc[1] = Colors.Blue;
            darkc[2] = Colors.DarkGreen;
            darkc[3] = Color.FromRgb(0x66, 0x00, 0xFF);
            darkc[4] = Color.FromRgb(0x99, 0x66, 0x00);
            darkc[5] = Colors.Red;
            darkc[6] = Colors.Gold;
            darkc[7] = Color.FromRgb(0x00, 0x00, 0xFF);
            darkc[8] = Colors.Green;
            darkc[9] = Color.FromRgb(0x66, 0x00, 0x66);
            darkc[10] = Color.FromRgb(0xFF, 0x99, 0x00);
            darkc[11] = Color.FromRgb(0xFF, 0xCC, 0xFF);
            // 设置资源
            Color[] ac = new Color[12];
            ac[0] = Color.FromRgb(0xFF, 0x66, 0x66);
            ac[1] = Color.FromRgb(0x00, 0x66, 0xFF);
            ac[2] = Color.FromRgb(0x66, 0xCC, 0x00);
            ac[3] = Color.FromRgb(0xCC, 0x00, 0xFF);
            ac[4] = Color.FromRgb(0x99, 0x66, 0x00);
            ac[5] = Colors.Pink;
            ac[6] = Color.FromRgb(0xFF, 0xFF, 0x00);
            ac[7] = Color.FromRgb(0x00, 0x00, 0xFF);
            ac[8] = Colors.Green;
            ac[9] = Color.FromRgb(0x66, 0x00, 0x66);
            ac[10] = Color.FromRgb(0xFF, 0x99, 0x00);
            ac[11] = Color.FromRgb(0xFF, 0xCC, 0xFF);
            MaterialGroup matg;
            DiffuseMaterial mat;
            for (int i = 0; i < 12; i++)
            {
                matg = new MaterialGroup();
                matg.Children.Add(new DiffuseMaterial(new SolidColorBrush(ac[i])));
                matg.Children.Add(new SpecularMaterial(Brushes.White, 90));
                if (this.TryFindResource("matslice" + i.ToString()) == null)
                    this.Resources.Add("matslice" + i.ToString(), matg);
                ac[i].A = 128;
                mat = new DiffuseMaterial(new LinearGradientBrush(ac[i], Colors.Transparent, 90));
                mat.Freeze();
                if (this.TryFindResource("matcone" + i.ToString()) == null)
                this.Resources.Add("matcone" + i.ToString(), mat);
            }
            //MeshGeometry3D cubep = Model3DHelper.genTextPlaneMesh();
            //cubep.Freeze();
            //this.Resources.Add("meshText", cubep);
        }

        private void createPie(SettleNode node)
        {
            GeometryModel3D model, labelmodel;
            MeshGeometry3D mesh;
            MaterialGroup mat;
            Model3DGroup mgmodel = new Model3DGroup();
            Model3DGroup mglabel = new Model3DGroup();
            double sum = node.nodes.Sum(p => p.fee);
            double startAngle = 0, endAngle;
            double radius = 1;
            double holeradius = 0.5 * radius, heightratio = 1;
            int segcount = 1;
            int coloridx = 0;

            foreach (SettleNode e0 in node.nodes)
            {
                endAngle = startAngle + e0.fee / sum * 360;
                segcount = (int)Math.Abs((endAngle - startAngle) * 4);
                mesh = Model3DHelper.genPieSlice(radius, holeradius, heightratio, startAngle, endAngle, segcount,Model3DHelper.EPieType.棱形面);
                mat = (MaterialGroup)this.FindResource("matslice" + (coloridx % 12).ToString());
                model = new GeometryModel3D(mesh, mat);
                mgmodel.Children.Add(model);

                Vector3D vec = new Vector3D();
                vec.X = 1.1 * radius * Math.Cos((startAngle + endAngle) / 2 / 180 * Math.PI);
                vec.Y = model.Bounds.SizeY ;/// 2;
                vec.Z = -1.1 * radius * Math.Sin((startAngle + endAngle) / 2 / 180 * Math.PI);
                mesh = (MeshGeometry3D)this.FindResource("meshText");
                string text = e0.nodename + ":" + (e0.fee / sum).ToString("p1");
                labelmodel = Model3DHelper.genTextPanel(mesh, Brushes.White, new SolidColorBrush(darkc[coloridx%12]), 32, text, rotationY, 0.2, vec, 0.5, 0.38);
                mglabel.Children.Add(labelmodel);
                //重新绑定旋转
                AxisAngleRotation3D axis3d = new AxisAngleRotation3D();
                axis3d.Axis = new Vector3D(0, 1, 0);
                Binding anglebind = new Binding();
                anglebind.Source = rotationY;
                anglebind.Path = new PropertyPath(AxisAngleRotation3D.AngleProperty);
                anglebind.Mode = BindingMode.OneWay;
                anglebind.Converter = new ConverterNegative();
                BindingOperations.SetBinding(axis3d, AxisAngleRotation3D.AngleProperty, anglebind);
                ((labelmodel.Transform as Transform3DGroup).Children[1] as RotateTransform3D).Rotation = axis3d;

                startAngle = endAngle;
                coloridx++;
            }

            mgRight.Children.Add(mgmodel);
            mgRight.Children.Add(mglabel);
        }

     
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initSource();

            DoubleAnimation da = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(15), FillBehavior.Stop);
            Storyboard.SetTargetName(da, "rotationY");
            Storyboard.SetTargetProperty(da, new PropertyPath(AxisAngleRotation3D.AngleProperty));
            storyboard.Children.Add(da);
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Begin(this, true);

            daOpen.Completed += new EventHandler(daOpen_Completed);
            daClose.Completed += new EventHandler(daClose_Completed);
        }

        void daClose_Completed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            this.Visibility = Visibility.Hidden;
        }

        void daOpen_Completed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            storyboard.Resume(this);
        }

        Storyboard storyboard = new Storyboard();
        DoubleAnimation daOpen = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
        DoubleAnimation daClose = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
        Vector vectranslate;
        internal void showNodeDetail(SettleNode node, Vector vec_translate)
        {
            vectranslate = vec_translate;
            ScaleTransform3D mscale;//,scale;
            //TranslateTransform3D translate;
            title.Text = node.nodename;

            if (mgLeft.Children.Count == 0) addCube(node);
            //主体
            mscale = (((mgLeft as Model3DGroup).Children[0] as GeometryModel3D).Transform as Transform3DGroup).Children[0] as ScaleTransform3D;
            mscale.ScaleX = node.scaleVec.X / node.layerScale * 3;
            mscale.ScaleY = node.scaleVec.Y / node.layerScale;
            mscale.ScaleZ = node.scaleVec.Z / node.layerScale * 3;
            ////顶
            //scale = (((mgLeft as Model3DGroup).Children[1] as GeometryModel3D).Transform as Transform3DGroup).Children[0] as ScaleTransform3D;
            //scale.ScaleZ = node.scaleVec.Z / node.layerScale * 3;
            //translate = (((mgLeft as Model3DGroup).Children[1] as GeometryModel3D).Transform as Transform3DGroup).Children[1] as TranslateTransform3D;
            //translate.OffsetY = mscale.ScaleY*2-1.001;
            ////底
            //scale = (((mgLeft as Model3DGroup).Children[0] as GeometryModel3D).Transform as Transform3DGroup).Children[0] as ScaleTransform3D;
            //scale.ScaleZ = node.scaleVec.Z / node.layerScale * 3;

            //电量标志
            grdFlag.Children.Clear();
            Point ptop, pbottom, pcenter, pfronttop, pfrontbottom;
            GeneralTransform3DTo2D gtransform = modelMain.TransformToAncestor(grdmain);
            ptop = gtransform.Transform(new Point3D(mgLeft.Bounds.X, mgLeft.Bounds.Y + mgLeft.Bounds.SizeY, 0));
            pbottom = gtransform.Transform(new Point3D(mgLeft.Bounds.X, mgLeft.Bounds.Y, 0));
            pcenter = gtransform.Transform(new Point3D(mgLeft.Bounds.X + mgLeft.Bounds.SizeX / 2, mgLeft.Bounds.Y, 0));
            pfronttop = gtransform.Transform(new Point3D(mgLeft.Bounds.X + mgLeft.Bounds.SizeX / 2, mgLeft.Bounds.Y + mgLeft.Bounds.SizeY, mgLeft.Bounds.Z + mgLeft.Bounds.SizeZ));
            pfrontbottom = gtransform.Transform(new Point3D(mgLeft.Bounds.X + mgLeft.Bounds.SizeX / 2, mgLeft.Bounds.Y, mgLeft.Bounds.Z + mgLeft.Bounds.SizeZ));
            Path path;
            path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
            PathGeometry geo = new PathGeometry();
            PathFigure fig = new PathFigure();
            fig.StartPoint = ptop;
            fig.Segments.Add(new LineSegment(new Point(ptop.X - 10, ptop.Y), true));
            fig.Segments.Add(new LineSegment(new Point(ptop.X - 10, pbottom.Y), true));
            fig.Segments.Add(new LineSegment(new Point(pbottom.X, pbottom.Y), true));
            geo.Figures.Add(fig);
            fig = new PathFigure();
            fig.StartPoint = new Point(ptop.X - 10, (ptop.Y + pbottom.Y) / 2);
            fig.Segments.Add(new LineSegment(new Point(ptop.X - 45, (ptop.Y + pbottom.Y) / 2), true));
            fig.Segments.Add(new LineSegment(new Point(ptop.X - 65, (ptop.Y + pbottom.Y) / 2 - 10), true));
            geo.Figures.Add(fig);
            path.Data = geo;
            grdFlag.Children.Add(path);
            path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
            path.Data = new EllipseGeometry(new Point(ptop.X - 65, (ptop.Y + pbottom.Y) / 2 - 10), 5, 5);
            grdFlag.Children.Add(path);
            TextBlock txt = new TextBlock() { Foreground = Brushes.Yellow, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            txt.Text = heightTitle+"：" + node.energy.ToString("f0");
            txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            txt.Margin = new Thickness(ptop.X - 65 - txt.DesiredSize.Width / 2, (ptop.Y + pbottom.Y) / 2 - 10 - txt.DesiredSize.Height - 5, 0, 0);
            grdFlag.Children.Add(txt);
            //电价标志
            path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
            geo = new PathGeometry();
            fig = new PathFigure();
            fig.StartPoint = new Point(pcenter.X, ptop.Y);
            fig.Segments.Add(new LineSegment(new Point(pcenter.X, ptop.Y - 20), true));
            fig.Segments.Add(new LineSegment(new Point(pcenter.X - 50, ptop.Y - 40), true));
            geo.Figures.Add(fig);
            path.Data = geo;
            grdFlag.Children.Add(path);
            path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
            path.Data = new EllipseGeometry(new Point(pcenter.X - 50, ptop.Y - 40), 5, 5);
            grdFlag.Children.Add(path);
            txt = new TextBlock() { Foreground = Brushes.Yellow, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            txt.Text = sectionTitle+"：" + node.price.ToString("f4");
            txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            txt.Margin = new Thickness(pcenter.X - 50 - txt.DesiredSize.Width / 2, ptop.Y - 40 - txt.DesiredSize.Height - 5, 0, 0);
            grdFlag.Children.Add(txt);

            //费用标志
            path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
            geo = new PathGeometry();
            fig = new PathFigure();
            fig.StartPoint = new Point(pcenter.X, pfronttop.Y + (pfrontbottom.Y - pfronttop.Y) * 0.25);
            fig.Segments.Add(new LineSegment(new Point(pcenter.X + 60, pfronttop.Y + (pfrontbottom.Y - pfronttop.Y) * 0.25), true));
            fig.Segments.Add(new LineSegment(new Point(pcenter.X + 80, pfronttop.Y + (pfrontbottom.Y - pfronttop.Y) * 0.25 - 20), true));
            geo.Figures.Add(fig);
            path.Data = geo;
            grdFlag.Children.Add(path);
            path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
            path.Data = new EllipseGeometry(new Point(pcenter.X + 80, pfronttop.Y + (pfrontbottom.Y - pfronttop.Y) * 0.25 - 20), 5, 5);
            grdFlag.Children.Add(path);
            txt = new TextBlock() { Foreground = Brushes.Yellow, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            txt.Text = volumeTitle+"：" + node.fee.ToString("f0");
            txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            txt.Margin = new Thickness(pcenter.X + 60, pfronttop.Y + (pfrontbottom.Y - pfronttop.Y) * 0.25 - 20 - txt.DesiredSize.Height - 5, 0, 0);
            grdFlag.Children.Add(txt);


            mgRight.Children.Clear();
            createPie(node);

            //动画放大
            this.Visibility = Visibility.Visible;
            thisScale.BeginAnimation(ScaleTransform.ScaleXProperty, daOpen);
            thisScale.BeginAnimation(ScaleTransform.ScaleYProperty, daOpen);

            DoubleAnimation tax = new DoubleAnimation(vectranslate.X, 0, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            DoubleAnimation tay = new DoubleAnimation(vectranslate.Y, 0, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            thisTranslate.BeginAnimation(TranslateTransform.XProperty, tax);
            thisTranslate.BeginAnimation(TranslateTransform.YProperty, tay);
            
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            closeDetail();
        }

        public void closeDetail()
        {
            storyboard.Pause(this);
            thisScale.BeginAnimation(ScaleTransform.ScaleXProperty, daClose);
            thisScale.BeginAnimation(ScaleTransform.ScaleYProperty, daClose);

            DoubleAnimation tax = new DoubleAnimation(0, vectranslate.X, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            DoubleAnimation tay = new DoubleAnimation(0, vectranslate.Y, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            thisTranslate.BeginAnimation(TranslateTransform.XProperty, tax);
            thisTranslate.BeginAnimation(TranslateTransform.YProperty, tay);
        }
    }




    //===== 转换返回*-1
    [ValueConversion(typeof(double), typeof(double))]
    internal class ConverterNegative : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return -(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return -(double)value;
        }
    }
}
