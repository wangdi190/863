using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;
using MyClassLibrary;
using MyClassLibrary.Share2D;
using MyClassLibrary.Share3D;

namespace MyControlLibrary.Controls3D.CurveSurface3D
{
    ///<summary>曲面数据管理类</summary>
    internal class CurveSurfaceDatas
    {
        public CurveSurfaceDatas()
        {
            para.Limit = new Vector3D(36, 12, 48);
        }



        //=====参数
        ///<summary>曲面场景参数</summary>
        public ScenePara3D para = new ScenePara3D();
        ///<summary>X坐标抽点个数, 首尾自动加入抽点序列</summary>
        public int sampleCount = 10;
        ///<summary>平滑插点个数</summary>
        public int insertCount = 2;
        //public DateTime startDate { get; set; }
        //public DateTime endDate { get; set; }


        private DateTime _startDate;
        ///<summary>曲面数据起始时间</summary>
        public DateTime startDate
        {
            get
            {
                if (_startDate.Year < 1900)
                    _startDate = Datas.Min(p => p.orgPoints.Min(p1 => p1.zDate));
                return _startDate;
            }
            set { _startDate = value; }
        }

        private DateTime _endDate;
        ///<summary>曲面数据结束时间</summary>
        public DateTime endDate
        {
            get
            {
                if (_endDate.Year < 1900)
                    _endDate = Datas.Max(p => p.orgPoints.Max(p1 => p1.zDate));
                return _endDate;
            }
            set { _endDate = value; }
        }

        ///<summary>Y轴方向数据最大值</summary>
        public double maxYValue
        {
            get
            {
                //return Datas.Max(p => p.samplePoints.Max(p1 => p1.zValue))*1.2;
                return Datas.Max(p => p.orgPoints.Max(p1 => p1.zValue)) * 1.2;
            }
        } //最大Y方向值
        //=====数据集
        ///<summary>曲面数据集</summary>
        public List<CurveSurfaceData> Datas = new List<CurveSurfaceData>();
        //=====方法

        ///<summary>3D建模</summary>
        public void createModel()
        {
            foreach (CurveSurfaceData one in Datas)
            {
                one.calSamplePoints();
            }
            foreach (CurveSurfaceData one in Datas)
            {
                one.genModel();
            }
        }
        //=====引用控件
        public void showModel(Model3DGroup mgAll)
        {
            mgAll.Children.Clear();
            foreach (CurveSurfaceData one in Datas)
            {
                if (one.isVisual)
                    mgAll.Children.Add(one.model);
            }
        }
    }

    ///<summary>曲面数据类</summary>
    internal class CurveSurfaceData
    {
        public CurveSurfaceData(CurveSurfaceDatas zparent, string zname)
        {
            parent = zparent;
            name = zname;
        }
        public CurveSurfaceDatas parent;

        public string name { get; set; }
        public bool isVisual { get; set; }

        public List<csDataPoint> orgPoints = new List<csDataPoint>();  //原始数据
        public List<csDataPoint> samplePoints = new List<csDataPoint>();  //抽点数据
        // 可视组件
        public GeometryModel3D model;
        internal Brush curveBrush;

        ///<summary>从原始数据抽点</summary>
        public void calSamplePoints()
        {

            csDataPoint startdp, enddp;
            //首日
            startdp = orgPoints.OrderBy(p => p.zDate).First();
            foreach (csDataPoint one in orgPoints.Where(p => MyFunction.isSameDay(p.zDate, startdp.zDate)))
            {
                samplePoints.Add(new csDataPoint(one.zDate, one.zValue));
            }
            //尾日, 全时段有数据的最后一天
            enddp = orgPoints.OrderBy(p => p.zDate).Last();
            foreach (csDataPoint one in orgPoints.Where(p => MyFunction.isSameDay(p.zDate, enddp.zDate)))
            {
                samplePoints.Add(new csDataPoint(one.zDate, one.zValue));
            }
            //抽点
            if (orgPoints.GroupBy(p => p.zDate.Date).Count() - 2 <= parent.sampleCount)
            {
                foreach (csDataPoint one in orgPoints.Where(p => !MyFunction.isSameDay(p.zDate, startdp.zDate) && !MyFunction.isSameDay(p.zDate, enddp.zDate)))
                    samplePoints.Add(new csDataPoint(one.zDate, one.zValue));

            }
            else
            {
                int step = (int)((orgPoints.GroupBy(p => p.zDate.Date).Count() - 2) / parent.sampleCount);//步长天数
                DateTime tmpdate;
                DateTime sdate = startdp.zDate.AddDays(1).Date;
                DateTime edate;
                double tmpvalue;
                for (int i = 0; i < parent.sampleCount; i++)
                {
                    edate = sdate.AddDays(step - 1).Date;
                    tmpdate = sdate.AddDays((step - 1) / 2);

                    for (int j = 0; j < 24; j++)
                    {
                        tmpvalue = orgPoints.Where(p => p.zDate.Date >= sdate && p.zDate.Date <= edate && p.zDate.Hour == j).Average(p => p.zValue);
                        samplePoints.Add(new csDataPoint(new DateTime(tmpdate.Year, tmpdate.Month, tmpdate.Day, j, 0, 0), tmpvalue));
                    }
                    sdate = edate.AddDays(1).Date;
                }

            }
        }

        ///<summary>获取指定数据点的3D坐标</summary>
        internal Point3D getPoint3DFromDateValue(csDataPoint datapoint)
        {
            double maxX3D = parent.para.Limit.X, maxY3D = parent.para.Limit.Y, maxZ3D = parent.para.Limit.Z;  // 3D场景最大坐标
            int zcount = 24, xcountData;
            xcountData = samplePoints.GroupBy(p => p.zDate.Date).Count();


            double devy = maxY3D / parent.maxYValue, devz = maxZ3D / (zcount);

            Point3D pnt = new Point3D((double)((datapoint.zDate - parent.startDate).TotalDays - 1) / ((parent.endDate - parent.startDate).TotalDays - 1) * maxX3D,
                devy * (double)datapoint.zValue,
                -devz * datapoint.zDate.Hour);

            return pnt;
        }

        Point3D[,] allpoint;
        ///<summary>生成3D模型</summary>
        public void genModel()
        {
            //数据初始化
            //定义参数
            double maxX3D = parent.para.Limit.X, maxY3D = parent.para.Limit.Y, maxZ3D = parent.para.Limit.Z;  // 3D场景最大坐标
            int xcount = 12, zcount = 24, xcountData, zcountData = 25;
            // x y 方向数目，x为月数，z为小时数,其中，xcount表征月分，xcountData为数据的x方向个数
            //zcount为0-23点，但zcountData为0-24点，多一个数据，同理，xcount中，增加一个数，全年下为12.31日
            xcountData = samplePoints.GroupBy(p => p.zDate.Date).Count();

            MeshGeometry3D mesh = new MeshGeometry3D();

            double devx = maxX3D / (xcount), devy = maxY3D / parent.maxYValue, devz = maxZ3D / (zcount);

            allpoint = new Point3D[xcountData, zcountData];

            double tx, tz;
            int di, dj;
            di = 0;
            DateTime olddate = samplePoints.OrderBy(p => p.zDate).First().zDate.Date;
            foreach (var e in samplePoints.OrderBy(p => p.zDate))
            {
                //di = e.zDate.Month - 1;
                if (e.zDate.Date != olddate)
                {
                    di++;
                    olddate = e.zDate.Date;
                }

                dj = e.zDate.Hour;

                allpoint[di, dj] = new Point3D((double)((e.zDate - parent.startDate).TotalDays - 1) / ((parent.endDate - parent.startDate).TotalDays - 1) * maxX3D, devy * (double)e.zValue, -devz * dj);
                if (dj == 0) //赋第24点，以本日0代替
                    allpoint[di, zcountData - 1] = new Point3D((double)((e.zDate - parent.startDate).TotalDays - 1) / ((parent.endDate - parent.startDate).TotalDays - 1) * maxX3D, devy * (double)e.zValue, -devz * (zcountData - 1));
            }


            allpoint = insertZPoint(allpoint, 2); //z插点
            allpoint = insertXPoint(allpoint, 2); //x插点



            for (int i = 0; i < allpoint.GetLength(0); i++)  //加positions
            {
                for (int j = 0; j < allpoint.GetLength(1); j++)
                {
                    mesh.Positions.Add(allpoint[i, j]);
                    tx = 1.0 * i / allpoint.GetLength(0);
                    tz = 1.0 * j / allpoint.GetLength(1);
                    mesh.TextureCoordinates.Add(new Point(tx, tz));

                }
            }

            for (int i = 0; i < allpoint.GetLength(0) - 1; i++) // 加三角索引
            {
                for (int j = 0; j < allpoint.GetLength(1) - 1; j++)
                {


                    mesh.TriangleIndices.Add(j + i * allpoint.GetLength(1));
                    mesh.TriangleIndices.Add(j + (i + 1) * allpoint.GetLength(1));
                    mesh.TriangleIndices.Add(j + 1 + i * allpoint.GetLength(1));
                    mesh.TriangleIndices.Add(j + 1 + i * allpoint.GetLength(1));
                    mesh.TriangleIndices.Add(j + (i + 1) * allpoint.GetLength(1));
                    mesh.TriangleIndices.Add(j + 1 + (i + 1) * allpoint.GetLength(1));

                }
            }


            //===============  曲面材质
            Canvas panelLine = new Canvas();
            panelLine.Width = devx * (parent.endDate - parent.startDate).TotalDays;
            panelLine.Height = devz * 23 * 30;
            //色变化说明：最高255,0,0最低0,255,0；从低到高变化顺序1:R 0->255,2:G 255->0;

            double x, y, maxy = 10, miny = 0, maxh = panelLine.Height, maxw = panelLine.Width;
            double w = maxw / (allpoint.GetLength(0) - 1), h = maxh / (allpoint.GetLength(1) - 1);
            double v;
            Color c1, c2;
            byte cr, cg, cb = 0;
            maxy = 0; miny = 10000;
            for (int i = 0; i < allpoint.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < allpoint.GetLength(1) - 1; j++)
                {
                    if (allpoint[i, j].Y > maxy) { maxy = allpoint[i, j].Y; }
                    if (allpoint[i, j].Y < miny) { miny = allpoint[i, j].Y; }
                }
            }

            Point3D lastpoint = allpoint[allpoint.GetLength(0) - 1, allpoint.GetLength(1) - 1];
            for (int i = 0; i < allpoint.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < allpoint.GetLength(1) - 1; j++)
                {
                    x = allpoint[i, j].X / lastpoint.X * maxw;
                    y = allpoint[i, j].Z / lastpoint.Z * maxh;

                    w = (allpoint[i + 1, j].X - allpoint[i, j].X) / lastpoint.X * maxw;

                    double temp = 1;// .2;//新疆怪需求，人为加大最大值，以避免纯红
                    v = (allpoint[i, j].Y - miny) / (maxy * temp - miny) * 511;
                    cr = v > 255 ? (byte)255 : (byte)v;
                    cg = v > 255 ? (byte)(255 - (v - 256)) : (byte)255;
                    c1 = Color.FromRgb(cr, cg, cb);
                    v = (allpoint[i + 1, j + 1].Y - miny) / (maxy * temp - miny) * 511;
                    cr = v > 255 ? (byte)255 : (byte)v;
                    cg = v > 255 ? (byte)(255 - (v - 256)) : (byte)255;
                    c2 = Color.FromRgb(cr, cg, cb);

                    RectangleGeometry rect = new RectangleGeometry(new Rect(x, y, w, h));
                    rect.Freeze();
                    Path pr = new Path();
                    pr.Data = rect;
                    LinearGradientBrush cbrush = new LinearGradientBrush(c1, c2, new Point(0, 0), new Point(1, 1));
                    pr.Fill = cbrush;
                    pr.StrokeThickness = 0;

                    panelLine.Children.Add(pr);

                }
            }
            panelLine.Measure(new System.Windows.Size(h, w));
            panelLine.Arrange(new Rect(0, 0, h, w));



            System.Windows.Media.Imaging.RenderTargetBitmap renderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap((int)panelLine.Width, (int)panelLine.Height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(panelLine);
            renderTarget.Freeze();
            curveBrush = new ImageBrush(renderTarget);
            DiffuseMaterial mat = new DiffuseMaterial(curveBrush);
            mat.Freeze();

            model = new GeometryModel3D(mesh, mat);

            model.BackMaterial = mat;
        }

        ///<summary>贝塞尔平滑插点计算，Z方向</summary>
        private Point3D[,] insertZPoint(Point3D[,] allpoint, int insertcount) //z方向插点
        {
            // bezier平滑插值
            Point[] ap, cp1, cp2;
            Point calpoint;
            Point3D[,] resultpoint = new Point3D[allpoint.GetLength(0), (allpoint.GetLength(1) - 1) * (insertcount + 1) + 1];
            Point[] cp = new Point[4];
            for (int i = 0; i < allpoint.GetLength(0); i++)
            {
                ap = new Point[allpoint.GetLength(1)];
                for (int j = 0; j < allpoint.GetLength(1); j++)
                {
                    ap[j].X = allpoint[i, j].Z;
                    ap[j].Y = allpoint[i, j].Y;
                }
                MyGeometryHelper.GetCurveControlPoints(ap, out cp1, out cp2);
                for (int j = 0; j < allpoint.GetLength(1) - 1; j++)
                {
                    resultpoint[i, j * (insertcount + 1)] = allpoint[i, j]; //原有的点
                    cp[0] = ap[j]; cp[1] = cp1[j]; cp[2] = cp2[j]; cp[3] = ap[j + 1];
                    for (int k = 0; k < insertcount; k++)
                    {
                        calpoint = MyGeometryHelper.PointOnBezier(cp, 1.0 / (insertcount + 1) * (k + 1));
                        resultpoint[i, j * (insertcount + 1) + 1 + k] = new Point3D(allpoint[i, j].X, calpoint.Y, calpoint.X); //插入的点
                    }
                }
                resultpoint[i, (allpoint.GetLength(1) - 1) * (insertcount + 1)] = allpoint[i, allpoint.GetLength(1) - 1]; //原有的终点
            }
            return resultpoint;
        }
        ///<summary>贝塞尔平滑插点计算，X方向</summary>
        private Point3D[,] insertXPoint(Point3D[,] allpoint, int insertcount) //x方向插点
        {
            // bezier平滑插值
            Point[] ap, cp1, cp2;
            Point calpoint;
            Point3D[,] resultpoint = new Point3D[(allpoint.GetLength(0) - 1) * (insertcount + 1) + 1, allpoint.GetLength(1)];
            Point[] cp = new Point[4];
            for (int i = 0; i < allpoint.GetLength(1); i++)
            {
                ap = new Point[allpoint.GetLength(0)];
                for (int j = 0; j < allpoint.GetLength(0); j++)
                {
                    ap[j].X = allpoint[j, i].X;
                    ap[j].Y = allpoint[j, i].Y;
                }
                MyGeometryHelper.GetCurveControlPoints(ap, out cp1, out cp2);
                for (int j = 0; j < allpoint.GetLength(0) - 1; j++)
                {
                    resultpoint[j * (insertcount + 1), i] = allpoint[j, i]; //原有的点
                    cp[0] = ap[j]; cp[1] = cp1[j]; cp[2] = cp2[j]; cp[3] = ap[j + 1];
                    for (int k = 0; k < insertcount; k++)
                    {
                        calpoint = MyGeometryHelper.PointOnBezier(cp, 1.0 / (insertcount + 1) * (k + 1));
                        resultpoint[j * (insertcount + 1) + 1 + k, i] = new Point3D(calpoint.X, calpoint.Y, allpoint[j, i].Z); //插入的点
                    }
                }
                resultpoint[(allpoint.GetLength(0) - 1) * (insertcount + 1), i] = allpoint[allpoint.GetLength(0) - 1, i]; //原有的终点
            }
            return resultpoint;
        }


        #region ========== 值截面相关 ==========

        Contour con;
        List<ValueDot> dots = new List<ValueDot>();

        internal Geometry getValueGeometry(double zvalue, double vWidth, double vHeight)
        {
            if (con == null)
            {
                int idx = 0;

                //用allpoint来插点
                double devy = parent.para.Limit.Y / parent.maxYValue;
                double tmpy = devy * zvalue;
                Point3D lastpoint = allpoint[allpoint.GetLength(0) - 1, allpoint.GetLength(1) - 1];
                for (int i = 0; i < allpoint.GetLength(0) - 1; i++)
                {
                    for (int j = 0; j < allpoint.GetLength(1) - 1; j++)
                    {
                        double tmpx = allpoint[i, j].X / lastpoint.X ;
                        double tmpz = allpoint[i, j].Z / lastpoint.Z ;
                        //allpoint[di, dj] = new Point3D((double)((e.zDate - parent.startDate).TotalDays - 1) / ((parent.endDate - parent.startDate).TotalDays - 1) * maxX3D, devy * (double)e.zValue, -devz * dj);

                        dots.Add(new ValueDot()
                        {
                            id = idx.ToString(),
                            location = new Point(vWidth * tmpz, vHeight * tmpx),
                            value = allpoint[i,j].Y >= tmpy ? 2 : 0
                        });
                        idx++;
                    }
                }
           
                //设置计算参数
                con = new Contour();
                con.dots = dots;
                con.opacityType = Contour.EOpacityType.正坡形;
                con.canvSize = new Size(vWidth, vHeight);
                con.gridXCount = 100;
                con.gridYCount = 50;
                con.Span = 2;
                con.maxvalue = 2;
                con.minvalue = 0;
                con.isDrawGrid = false;
                con.isDrawLine = false;
                con.isFillLine = true;
                con.GenGeometry();
            }
            else
            {
                int idx = 0;

                double devy = parent.para.Limit.Y / parent.maxYValue;
                double tmpy = devy * zvalue;
                Point3D lastpoint = allpoint[allpoint.GetLength(0) - 1, allpoint.GetLength(1) - 1];
                for (int i = 0; i < allpoint.GetLength(0) - 1; i++)
                {
                    for (int j = 0; j < allpoint.GetLength(1) - 1; j++)
                    {
                        dots[idx].value = allpoint[i, j].Y >= tmpy ? 2 : 0;
                        idx++;
                    }
                }
                con.ReGenGeometry();
            }

            return con.geometry;
        }

    


        #endregion


    }

    internal class csDataPoint
    {
        public csDataPoint(DateTime zdate, double zvalue)
        {
            zDate = zdate;
            zValue = zvalue;
        }
        public DateTime zDate { get; set; }
        public double zValue { get; set; }
    }

}
