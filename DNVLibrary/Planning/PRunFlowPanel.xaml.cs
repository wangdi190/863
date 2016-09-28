using System;
using System.Data;
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
using WpfEarthLibrary;
using DistNetLibrary;

namespace DNVLibrary.Planning
{

    /// <summary>
    /// PFlowPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PRunFlowPanel : UserControl, BaseIPanel
    {
        public PRunFlowPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }
        public void load()
        {

            root.earth.config.tooltipMoveEnable = true;

            if (UCDNV863.EDISTNET == UCDNV863.EDistnet.亦庄16)
            {
                #region ----- gis16数据模拟或载入 -----
                //逐项填写运行数据
                //载入潮流
                List<PowerBasicObject> objs = root.distnet.dbdesc["基础数据"].DictSQLS["导线段"].batchLoadRunData(root.distnet, false);
                foreach (var item in objs)
                {
                    DNLineSeg obj = item as DNLineSeg;
                    obj.isInverse = obj.thisRunData.activePower < 0; //校验方向
                    obj.tooltipMoveTemplate = "PlanningLineTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }
                //载入变电站
                objs = root.distnet.dbdesc["基础数据"].DictSQLS["变电站"].batchLoadRunData(root.distnet, false);
                foreach (var obj in objs)
                {
                    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }
                //载入主变压器
                objs = root.distnet.dbdesc["基础数据"].DictSQLS["主变压器"].batchLoadRunData(root.distnet, false);
                foreach (var obj in objs)
                {
                    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }

                #endregion
            }
            else if (UCDNV863.EDISTNET == UCDNV863.EDistnet.亦庄new)
            {
                #region ----- 新亦庄数据模拟或载入 -----
                //逐项填写运行数据
                //载入潮流
                List<PowerBasicObject> objs = root.distnet.dbdesc["基础数据"].DictSQLS["线路"].batchLoadRunData(root.distnet, false);
                foreach (var item in objs)
                {
                    DNACLine obj = item as DNACLine;
                    obj.isInverse = obj.thisRunData.activePower < 0; //校验方向
                    obj.tooltipMoveTemplate = "PlanningLineTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }
                //载入变电站
                objs = root.distnet.dbdesc["基础数据"].DictSQLS["变电站"].batchLoadRunData(root.distnet, false);
                foreach (var obj in objs)
                {
                    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }
                //载入主变压器
                objs = root.distnet.dbdesc["基础数据"].DictSQLS["主变2卷"].batchLoadRunData(root.distnet, false);
                foreach (var obj in objs)
                {
                    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }

                #endregion
            }
            else
            {

                #region ------ gis15数据模拟域载入 ------

                //逐项填写运行数据
                //载入潮流
                string id; PowerBasicObject obj;
                DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select f_id id,f_mch name,f_begyg yg,f_begwg wg,f_begyg/1000 fzl from MCR_ACFLOW_BRANCH union select f_id id,f_mch name,F_SHDYG yg,F_SHDWG wg,f_fzl fzl from HCR_ACFLOW_BRANCH");
                Dictionary<string, PowerBasicObject> objs;
                if (root.earth.objManager.zLayers.ContainsKey(EObjectCategory.导线类.ToString()))
                {
                    //objs = root.earth.objManager.zLayers[EObjectCategory.导线类.ToString()].pModels;
                    objs = root.distnet.getAllObjDictByCategory(EObjectCategory.导线类);
                    foreach (DataRow dr in dt.Rows)
                    {
                        id = dr["id"].ToString();
                        if (objs.TryGetValue(id, out obj))
                        {
                            if (obj.busiRunData == null) obj.createRunData();
                            RunDataACLineBase rdata = obj.busiRunData as RunDataACLineBase;
                            rdata.rateOfLoad = dr.getDouble("fzl");
                            rdata.activePower = dr.getDouble("yg");
                            rdata.reactivePower = dr.getDouble("wg");
                            (obj as pPowerLine).isInverse = rdata.activePower < 0; //校验方向

                            obj.tooltipMoveTemplate = "PlanningLineTemplate";
                            obj.tooltipMoveContent = obj.busiRunData;
                        }

                    }
                }

                //载入变电站
                dt = DataLayer.DataProvider.getDataTableFromSQL("select f_id id, f_mch name,f_ygfh yg,f_wgfh wg, f_fzl fzl, f_cos fcos, null ryd from MCR_ACFLOW_SS union select f_id id,F_MCH name, F_YGFH yg, F_WGFH wg,f_fzl fzl,f_cos fcos,f_ryd ryd from HCR_ACFLOW_SSLR");
                objs = root.earth.objManager.zLayers[EObjectCategory.变电设施类.ToString()].pModels; // root.earth.objManager.getAllObjDictBelongtoCategory("变电站");
                foreach (DataRow dr in dt.Rows)
                {
                    id = dr["id"].ToString();
                    if (objs.TryGetValue(id, out obj))
                    {
                        if (obj.busiRunData == null) obj.createRunData();
                        RunDataSubstation rdata = obj.busiRunData as RunDataSubstation;
                        rdata.activePower = dr.getDouble("yg");
                        rdata.reactivePower = dr.getDouble("wg");
                        rdata.rateOfLoad = dr.getDouble("fzl") / 100;
                        rdata.powerFactor = dr.getDouble("fcos");
                        rdata.redundancy = dr.getDouble("ryd");
                        rdata.HVL = 90 + rd.Next(40); //zh 注模拟
                        rdata.HVoltPUV = rdata.HVL / 110;

                        obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                        obj.tooltipMoveContent = obj.busiRunData;
                    }
                }
                //zh注，强制模拟配电室
                foreach (var item in root.distnet.getAllObjListByObjType(EObjectType.配电室))
                {
                    if (item.busiRunData == null) item.createRunData();
                    RunDataTransformFacilityBase rdata = item.busiRunData as RunDataTransformFacilityBase;
                    rdata.rateOfLoad = (30.0 + rd.Next(60)) / 100;
                    rdata.HVL = 8 + rd.Next(4);
                    rdata.HVoltPUV = rdata.HVL / 10;
                }

                //载入变压器
                string sql = @"
                select f_id id, f_mch name, f_fh yg,f_wgfh wg,f_fzl fzl,f_glysh fcos,f_gycyxdy hvl,f_zsh allloss,f_shzgl ap,f_tgs closs,f_ts iloss,f_bs tloss,f_bsl tlr from HCR_ACFLOW_MTTV
                union 
                select f_id id, f_mch name, f_fh yg,f_wgfh wg,f_fzl fzl,f_glysh fcos,f_gycyxdy hvl,f_zsh allloss,f_shzgl ap,f_tgs closs,f_ts iloss,f_bs tloss,f_bsl tlr from HCR_ACFLOW_MTTHV
                union 
                select f_id id, f_mch name, f_ygfh yg,f_wgfh wg,f_fzl fzl,f_cos fcos,0 hvl,f_zws allloss,sqrt(f_ygfh*f_ygfh+f_wgfh*f_wgfh) ap,f_pbcus closs,f_pbfes iloss,f_pbsh tloss,f_pbsl tlr from MCR_ACFLOW_MT
                ";
                dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
                if (root.earth.objManager.zLayers.ContainsKey(EObjectCategory.变压器类.ToString()))
                {
                    objs = root.earth.objManager.zLayers[EObjectCategory.变压器类.ToString()].pModels;// root.earth.objManager.getAllObjDictBelongtoCategory("变压器");
                    foreach (DataRow dr in dt.Rows)
                    {
                        id = dr["id"].ToString();
                        if (objs.TryGetValue(id, out obj))
                        {
                            RunDataTransformerBase rundata = new RunDataTransformerBase(obj)
                            {
                                activePower = double.Parse(dr["yg"].ToString()),
                                reactivePower = double.Parse(dr["wg"].ToString()),
                                rateOfLoad = double.Parse(dr["fzl"].ToString()) / 100,
                                powerFactor = double.Parse(dr["fcos"].ToString()),
                                HVL = double.Parse(dr["hvl"].ToString()),
                                allLoss = double.Parse(dr["allloss"].ToString()),
                                apparentPower = double.Parse(dr["ap"].ToString()),
                                copperLoss = double.Parse(dr["closs"].ToString()),
                                ironLoss = double.Parse(dr["iloss"].ToString()),
                                transformLoss = double.Parse(dr["tloss"].ToString()),
                                transformLossRate = double.Parse(dr["tlr"].ToString()),
                            };

                            obj.busiRunData = rundata;
                            obj.tooltipMoveTemplate = "PlanningTransformerTemplate";
                            obj.tooltipMoveContent = obj.busiRunData;
                        }
                    }
                }


                //======= 载入节点数据
                dt = DataLayer.DataProvider.getDataTableFromSQL("select * from MCR_ACFLOW_node");
                //objs = root.earth.objManager.getAllObjDictBelongtoCategory("节点");
                objs = root.distnet.getAllObjDictByObjType(EObjectType.节点);
                foreach (DataRow dr in dt.Rows)
                {
                    id = dr["f_id"].ToString();
                    if (objs.TryGetValue(id, out obj))
                    {
                        RunDataNode rundata = new RunDataNode(obj)
                        {
                            activePower = dr.getDouble("f_yg"),
                            reactivePower = dr.getDouble("f_wg"),
                            volt = dr.getDouble("f_dy"),
                            voltPUV = dr.getDouble("f_dy_by"),
                        };
                        obj.busiRunData = rundata;
                    }

                }



                ////载入3卷变压器
                //dt = DataLayer.DataProvider.getDataTableFromSQL("select * from HCR_ACFLOW_MTTHV");
                //objs = root.earth.objManager.getAllObjDictBelongtoCategory("3卷变压器");
                //foreach (DataRow dr in dt.Rows)
                //{
                //    id = dr["f_id"].ToString();
                //    if (objs.TryGetValue(id, out obj))
                //    {
                //        RunDataTransformer3P rundata = new RunDataTransformer3P()
                //        {
                //            name = dr["F_MCH"].ToString(),
                //            activePower = double.Parse(dr["F_FH"].ToString()),
                //            reactivePower = double.Parse(dr["F_WGFH"].ToString()),
                //            rateOfLoad = double.Parse(dr["F_FZL"].ToString()) / 100,
                //            powerFactor = double.Parse(dr["F_GLYSH"].ToString()),
                //            HVL = double.Parse(dr["F_gycyxdy"].ToString()),
                //            allLoss = double.Parse(dr["F_zsh"].ToString()),
                //            apparentPower = double.Parse(dr["F_shzgl"].ToString()),
                //            copperLoss = double.Parse(dr["F_tgs"].ToString()),
                //            ironLoss = double.Parse(dr["F_ts"].ToString()),
                //            transformLoss = double.Parse(dr["F_bs"].ToString()),
                //            transformLossRate = double.Parse(dr["F_bsl"].ToString()),
                //        };

                //        obj.busiRunData = rundata;
                //        obj.tooltipMoveTemplate = "PlanningTransformer3Template";
                //        obj.tooltipMoveContent = obj.busiRunData;
                //    }
                //}

                #endregion
            }
            cmbMode.SelectedIndex = 0;


            root.earth.VisualRangeChanged += new EventHandler(earth_VisualRangeChanged);
        }

        EViewStatus viewstatus;
        enum EViewStatus { 初始态, 显示变电站, 显示变压器 };
        ///<summary>视图状态变化，改变列表显示内容</summary>
        void earth_VisualRangeChanged(object sender, EventArgs e)
        {
            double curdistance = root.earth.camera.curCameraDistanceToGround;
            EViewStatus newviewstatus = (curdistance < root.visualdistance) ? EViewStatus.显示变压器 : EViewStatus.显示变电站;
            if (viewstatus != newviewstatus)
            {
                viewstatus = newviewstatus;
                if (viewstatus == EViewStatus.显示变电站)
                {
                    var lst2 = (from e0 in root.earth.objManager.zLayers.Values
                                from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.变电设施类 && p.busiRunData != null)
                                orderby (e1.busiRunData as RunDataTransformFacilityBase).rateOfLoad descending
                                select e1).Take(50);
                    lstStation.ItemsSource = lst2;
                }
                else if (viewstatus == EViewStatus.显示变压器)
                {
                    var lst2 = (from e0 in root.earth.objManager.zLayers.Values
                                from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.变压器类 && p.busiRunData != null)
                                orderby (e1.busiRunData as RunDataTransformerBase).rateOfLoad descending
                                select e1).Take(50);
                    lstStation.ItemsSource = lst2;
                }
            }
        }


        public void unload()
        {
            root.distnet.clearFlow();
            root.distnet.clearLoadCol();
            root.distnet.clearVLContour();

            root.earth.config.tooltipMoveEnable = false;


            ////关闭潮流，清理tooltip
            //foreach (pPowerLine item in root.earth.objManager.getObjList("线路"))
            //{
            //    item.isFlow = false;
            //    item.lineColor = Colors.Cyan;
            //    item.tooltipMoveTemplate = null;
            //    item.tooltipMoveContent = null;
            //}

            ////清除附加子对象
            //foreach (pLayer layer in root.earth.objManager.zLayers.Values)
            //{
            //    foreach (PowerBasicObject obj in layer.pModels.Values)
            //    {
            //        obj.submodels.Clear();
            //    }
            //}

            root.earth.UpdateModel();

        }


        Random rd = new Random();
        UCDNV863 root;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            lstLine.MouseDoubleClick += new MouseButtonEventHandler(lst_MouseDoubleClick);
            lstStation.MouseDoubleClick += new MouseButtonEventHandler(lst_MouseDoubleClick);



            cmbMode.Items.Add("典型运行方式");
            cmbMode.Items.Add("最大运行方式");
            cmbMode.Items.Add("最小运行方式");




        }


        private void cmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            datachange(true);
        }

        void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBox).SelectedItem != null)
            {
                PowerBasicObject selobj = (sender as ListBox).SelectedItem as WpfEarthLibrary.PowerBasicObject;
                root.earth.camera.aniLook(selobj.VecLocation, 5);  //动画定位
                //改变外观
                if (selobj is pPowerLine)
                {
                    (selobj as pPowerLine).aniTwinkle.doCount = 20;
                    (selobj as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
                }
                else if (selobj is pSymbolObject)
                {
                    (selobj as pSymbolObject).aniTwinkle.doCount = 20;
                    (selobj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
                }


                propObj.SelectedObject = selobj.busiAccount;
            }



        }


        bool isFlowShow, isLoadShow, isVLShow;

        ///<summary>显示或隐藏潮流</summary>
        internal void ShowFlow(bool isshow)
        {
            isFlowShow = isshow;
            datachange(false);
        }
        ///<summary>显示或隐藏节点负载</summary>
        internal void ShowLoad(bool isshow)
        {
            isLoadShow = isshow;
            datachange(false);
        }
        ///<summary>显示或隐藏电压等值图</summary>
        internal void ShowVL(bool isshow)
        {
            isVLShow = isshow;
            datachange(false);
            //showcontour(isVLShow);
            //showNodeVL(isVLShow);
        }


        ///<summary>路由命令，规划日期改变</summary>
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            datachange(true);
        }

        bool isloadinited; //是否节点数据已初始化
        bool isflowinited; //是否线路负载已初始化

        ///<summary>isChangeDate为false时，不更新数据，仅控制显示或隐藏</summary>
        void datachange(bool isChangeData)
        {


            if (isFlowShow)
                root.distnet.showFlow();
            else
                root.distnet.clearFlow();


            //var lst = (from e0 in root.earth.objManager.zLayers.Values
            //           from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.导线类)
            //           orderby e1.busiData.busiPercentValue descending
            //           select e1).Take(50);
            var lst = (from e0 in root.earth.objManager.zLayers.Values
                       from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.导线类 && p.busiRunData != null)
                       orderby (e1.busiRunData as RunDataACLineBase).rateOfLoad descending
                       select e1).Take(50);
            lstLine.ItemsSource = lst;


            //---- 变电站


            if (isLoadShow)
                root.distnet.showLoadCol();
            else
                root.distnet.clearLoadCol();

            if (isVLShow)
                root.distnet.showVLContour();
            else
                root.distnet.hideVLContour();
            //changeContour();
            //showNodeVL(isVLShow);

            root.earth.UpdateModel();


            earth_VisualRangeChanged(null, null);
        }

        #region ============ 等高线 ================
        pLayer containerLayer;
        List<ContourGraph.ValueDot> dots;
        ContourGraph.Contour con;
        pContour gcon;
        Dictionary<string, pSymbolObject> objs = new Dictionary<string, pSymbolObject>();
        void showcontour(bool isShow)
        {
            if (isShow)
            {
                if (!root.earth.objManager.zLayers.TryGetValue("等高图层", out containerLayer))
                {
                    root.earth.objManager.AddLayer("等高图层", "等高图层", "等高图层");
                    containerLayer = root.earth.objManager.zLayers["等高图层"];
                    containerLayer.deepOrder = -1;


                    //导入和重新计算点位置
                    pSymbolObject ps;
                    dots = new List<ContourGraph.ValueDot>();

                    IEnumerable<PowerBasicObject> tmpobjs = root.earth.objManager.getAllObjListBelongtoCategory("变压器");
                    foreach (PowerBasicObject obj in tmpobjs)//.Where(p => p.busiRunData != null))
                    {
                        ps = obj as pSymbolObject;
                        double tmpvalue = 0.85 + 0.3 * rd.NextDouble();//(ps.busiRunData as RunDataNodeP).voltPUV;
                        dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = Point.Parse(ps.location), value = tmpvalue });
                        objs.Add(ps.id, ps);
                        obj.busiData.busiValue1 = tmpvalue; //存储电压标幺值 , busiValue已被变电站负载占用
                        obj.busiData.busiValue2 = (tmpvalue - 0.85) / 0.3 * 100;  //存储模板用位置信息，避免写转换
                        //更复杂应用应使用busiData自定义类来处理
                        obj.busiData.busiStr2 = tmpvalue.ToString("f2");
                    }
                    double minx, miny, maxx, maxy;
                    miny = dots.Min(p => p.location.X); maxy = dots.Max(p => p.location.X);  //将经度换为X坐标, 纬度换为Y坐标
                    minx = dots.Min(p => p.location.Y); maxx = dots.Max(p => p.location.Y);
                    double w = maxx - minx; double h = maxy - miny;
                    minx = minx - w * 0.2; maxx = maxx + w * 0.2;
                    miny = miny - h * 0.2; maxy = maxy + h * 0.2;
                    w = maxx - minx; h = maxy - miny;
                    //经纬换为屏幕坐标
                    int size = 1024;
                    foreach (ContourGraph.ValueDot dot in dots)
                    {
                        dot.location = new Point((dot.location.Y - minx) / w * size, (maxy - dot.location.X) / h * size);  //重新赋与新的平面点位置, 注，纬度取反，仅适用北半球
                    }

                    //设置计算参数
                    con = new ContourGraph.Contour();
                    con.dots = dots;
                    con.opacityType = ContourGraph.Contour.EOpacityType.倒梯形;
                    con.canvSize = new Size(size, size);
                    con.gridXCount = 300;
                    con.gridYCount = 300;
                    con.Span = 30;
                    con.maxvalue = 1.15;
                    con.minvalue = 0.85;
                    con.dataFillValue = 1;
                    con.dataFillMode = ContourGraph.Contour.EFillMode.单点包络填充;
                    con.dataFillDictance = 100;
                    con.dataFillSpan = 10;
                    con.isDrawGrid = false;
                    con.isDrawLine = false;
                    con.isFillLine = true;
                    //con.isShowData = true;

                    //计算
                    //con.GenContour();
                    //创建图形
                    gcon = new pContour(containerLayer) { id = "等值图" };// { minJD = minx, maxJD = maxx, minWD = miny, maxWD = maxy };
                    gcon.setRange(minx, maxx, miny, maxy);
                    gcon.brush = con.ContourBrush;
                    containerLayer.AddObject("等值线", gcon);

                    //contourtimer.Start();  //timer模拟刷新

                    con.GenCompleted += new EventHandler(con_GenCompleted);
                    con.GenContourAsync(); //异步开始生成
                }

                containerLayer.logicVisibility = true;
            }
            else
            {
                if (root.earth.objManager.zLayers.TryGetValue("等高图层", out containerLayer))
                {
                    containerLayer.logicVisibility = false;
                }
                //contourtimer.Stop();

            }
            root.earth.UpdateModel();
        }



        void con_GenCompleted(object sender, EventArgs e) //异步完成
        {
            gcon.brush = con.ContourBrush;
        }

        void changeContour()
        {
            //if (!(bool)chkContour.IsChecked) return;
            if (!isVLShow) return;

            foreach (var item in dots)
            {
                if (item.id != null)
                {
                    item.value = 0.85 + 0.3 * rd.NextDouble();
                }
            }
            con.ReGenContourAsync();
        }


        #region 节点电压，扩散着色
        void showNodeVL(bool isShow)
        {
            pSymbolObject pso, nobj;
            double topvalue = 0.15;
            double warningvalue = 0.1;
            IEnumerable<PowerBasicObject> tmpobjs = root.earth.objManager.getAllObjListBelongtoCategory("节点");
            if (isShow)
            {
                foreach (PowerBasicObject obj in tmpobjs.Where(p => p.busiRunData != null))
                {
                    pso = obj as pSymbolObject;
                    double v = (0.85 + 0.3 * rd.NextDouble()) - 1;//(pso.busiRunData as RunDataNodeP).voltPUV - 1;
                    Color color;
                    color = v < 0 ? Colors.Blue : Colors.Red;
                    v = Math.Abs(v);
                    if (v > warningvalue)
                    {
                        v = v > topvalue ? topvalue : v;
                        v = v / topvalue;
                        color = MyClassLibrary.Share2D.MediaHelper.getColorSaturation(v, color);

                        if (pso.submodels.Count == 0)
                        {
                            nobj = new pSymbolObject(pso.parent) { id = pso.id + "node", location = pso.location, symbolid = "渐变圆", isH = true, groundHeight = WpfEarthLibrary.Para.SymbolHeight * 0.9 };
                            pso.submodels.Add(nobj.id, nobj);
                        }
                        else
                            nobj = pso.submodels.Values.First() as pSymbolObject;
                        nobj.color = color;
                        nobj.scaleX = 0.0007f;
                        nobj.scaleY = nobj.scaleZ = 0.001f;
                    }
                    else
                    {
                        pso.submodels.Clear();
                    }

                }

            }
            else
            {
                foreach (PowerBasicObject obj in tmpobjs.Where(p => p.busiRunData != null))
                {
                    obj.submodels.Clear();
                }
            }
        }

        #endregion


        //private void chkContour_Checked(object sender, RoutedEventArgs e)
        //{
        //    showcontour(true);
        //}

        //private void chkContour_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    showcontour(false);
        //}
        #endregion


        #region 规划运行之tooltip数据定义
        public class TooltipLineData
        {
            public string name { get; set; }

            public List<TooltipLineItemData> items { get; set; }
        }

        public class TooltipLineItemData
        {
            public string name { get; set; }

            public Color color { get; set; }

            public double len { get; set; }

            public double len2 { get { return 100 - len; } }

            public string strvalue { get; set; }

            public Brush textbrush { get; set; }
        }

        #endregion

    }






}
