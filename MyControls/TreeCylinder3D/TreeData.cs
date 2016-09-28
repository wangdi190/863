using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Text;
using DataLayer;
using MyClassLibrary.Share3D;
using MyClassLibrary;

namespace MyControlLibrary.Controls3D.TreeCylinder3D
{
    /// <summary>
    /// 数据的总集
    /// </summary>
    internal class SettleDatas
    {
        ///<summary>场景参数</summary>
        public ScenePara3D para = new ScenePara3D() { Center = new Point3D(0, 0, 0), Limit = new Vector3D(0.5*2, 4, 1*2) };  //注，通过limit可控制粗细和高度 
        //=== 3D 模组
        ///<summary>根模组</summary>
        public Model3DGroup mgAll;  // viewport3D中预先定义的
        ///<summary>主数据模组</summary>
        public Model3DGroup mgModel = new Model3DGroup();
        ///<summary>标签模组</summary>
        public Model3DGroup mgLabel = new Model3DGroup();
        ///<summary>标志模组</summary>
        public Model3DGroup mgFlag = new Model3DGroup(); //标志模型组

        public SettleNode curSettle;
        //===
        public SettleNode root;

        public void init()
        {
            mgAll.Children.Add(mgModel);
            mgAll.Children.Add(mgLabel);
            mgAll.Children.Add(mgFlag);

            root.initModel();
            curSettle = root;

        }

    
        bool isAni;
        ///<summary>扩展指定层</summary>
        public void expandToLayer(int layeridx) 
        {
            if (!isAni)
            {
                isAni = true;
                curSettle.setExpandLayer(layeridx);
                curSettle.anibefore();
                moveCamera();
                curSettle.showModel();
            }
        }
        ///<summary>扩展一级</summary>
        public void expand() //扩一级
        {
            if (!isAni)
            {
                isAni = true;
                curSettle.setExpandToNext();
                curSettle.anibefore();
                moveCamera();
                curSettle.showModel();
            }
        }
        ///<summary>折叠一级</summary>
        public void fold() //折叠
        {
            if (!isAni)
            {
                isAni = true;
                curSettle.setFoldToPrev();
                curSettle.anibefore();
                moveCamera();
                curSettle.showModel();
            }
        }


        public PerspectiveCamera camera;
        Matrix3D matrix;
        Point3D savePos;
        Vector3D saveLookDir, saveUpDir;
        ///<summary>计算当前状态下合适观察的相机位置</summary>
        public void moveCamera()  
        {
            Point3D camPos, pcenter, orgPos;
            Vector3D camLookDir, camUpDir;
            Duration duration = TimeSpan.FromSeconds(1);
            savePos = camera.Position;
            saveLookDir = camera.LookDirection;
            saveUpDir = camera.UpDirection;
            //取得占用空间,计算中心点
            double x, y, z;
            x = curSettle.RectVec.X;
            y = curSettle.RectVec.Y;
            z = curSettle.RectVec.Z;
            pcenter = new Point3D(x / 2, 0, z / 4);
            //if (x <= para.Limit.X) pcenter = new Point3D(0, pcenter.Y, pcenter.Z);
            //if (z <= para.Limit.Z) pcenter = new Point3D(pcenter.X, pcenter.Y, 0);

            if (z < y * 2) z = y * 2;
            if (z < x / 2) z = x / 2;
            matrix = Matrix3D.Identity;
            orgPos = new Point3D(pcenter.X, 0, z + y * 6.5);
            matrix.Translate(orgPos - savePos);
            matrix.RotateAt(new Quaternion(new Vector3D(1, 0, 0), -30), pcenter);
            matrix.RotateAt(new Quaternion(new Vector3D(0, 1, 0), 30), pcenter);


            camPos = matrix.Transform(orgPos);
            camLookDir = matrix.Transform(new Vector3D(0, 0, -1));
            camUpDir = matrix.Transform(new Vector3D(0, 1, 0));
            camPos = matrix.Transform(savePos);
            camLookDir = matrix.Transform(saveLookDir);
            camUpDir = matrix.Transform(saveUpDir);



            camera.Position = (camera.Transform as MatrixTransform3D).Transform(camera.Position);
            camera.LookDirection = (camera.Transform as MatrixTransform3D).Transform(camera.LookDirection);
            camera.UpDirection = (camera.Transform as MatrixTransform3D).Transform(camera.UpDirection);
            (camera.Transform as MatrixTransform3D).Matrix = Matrix3D.Identity;

            Point3DAnimation anipos = new Point3DAnimation(camera.Position, camPos, duration, FillBehavior.Stop);
            anipos.Completed += new EventHandler(anipos_Completed);
            Vector3DAnimation anilookdir = new Vector3DAnimation(camera.LookDirection, camLookDir, duration, FillBehavior.Stop);
            Vector3DAnimation aniupdir = new Vector3DAnimation(camera.UpDirection, camUpDir, duration, FillBehavior.Stop);
            camera.Position = camPos;
            camera.LookDirection = camLookDir;
            camera.UpDirection = camUpDir;

            camera.BeginAnimation(PerspectiveCamera.PositionProperty, anipos);
            camera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, anilookdir);
            camera.BeginAnimation(PerspectiveCamera.UpDirectionProperty, aniupdir);
        }

        void anipos_Completed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            (camera.Transform as MatrixTransform3D).Matrix = matrix;
            camera.Position = savePos;
            camera.LookDirection = saveLookDir;
            camera.UpDirection = saveUpDir;
            isAni = false;

        }

    }


    /// <summary>
    /// 数据可视化表现的节点性质类，树结构组织，可包容子节点
    /// </summary>
    internal class SettleNode
    {
        public enum ESplitDir { x方向, y方向, z方向 }
        public enum EBehavior { 扩展子项, 收缩子项, 被扩展, 被收缩, 无 }
        public enum EModelType { }

        public SettleNode(SettleDatas zowner)
        {
            owner = zowner;
        }
        public SettleNode(SettleDatas zowner, SettleNode zparent)
        {
            owner = zowner;
            parent = zparent;
        }
        //节点属性
        ///<summary>所属容器类</summary>
        public SettleDatas owner;
        ///<summary>父节点</summary>
        public SettleNode parent;
        ///<summary>根节点</summary>
        SettleNode root  
        {
            get
            {
                SettleNode p = this;
                while (!p.isRoot)
                {
                    p = p.parent;
                }
                return p;
            }
        }
        ///<summary>节点索引号</summary>
        int idx 
        {
            get
            {
                if (!isRoot)
                    return parent.nodes.IndexOf(this);
                else
                    return -1;
            }
        }
        ///<summary>前一节点</summary>
        SettleNode PrevNode 
        {
            get
            {
                if (idx == 0)
                    return null;
                else
                    return parent.nodes[idx - 1];
            }
        }
        IEnumerable<SettleNode> ContainerNodes //所包含的所有节点，包括自身
        {
            get
            {
                List<SettleNode> tmp = new List<SettleNode>();
                tmp.Add(this);
                IEnumerable<SettleNode> lst = tmp.AsEnumerable();
                if (haveChild)
                    lst = lst.Union(from e1 in nodes
                                    from e2 in e1.ContainerNodes
                                    select e2);
                return lst;
            }
        }
        ///<summary>层级</summary>
        int layer
        {
            get
            {
                int layernum = 0;
                SettleNode p = this;
                while (!p.isRoot)
                {
                    layernum++;
                    p = p.parent;
                }
                return layernum;
            }
        }  //层级
        ///<summary>层级控制的不同缩放比例</summary>
        public double layerScale  
        {
            get
            {
                return 1.0 / Math.Pow(2, layer); ;
            }
        }
        ///<summary>子节点集</summary>
        public List<SettleNode> nodes = new List<SettleNode>(); 

        public string nodename;
        ///<summary>子节点的分裂方向</summary>
        public ESplitDir splitDir = ESplitDir.x方向;
        ///<summary>下一次调用showmodel时，节点的行为方式</summary>
        public EBehavior behavior = EBehavior.无;
        ///<summary>是否有子节点</summary>
        bool haveChild 
        {
            get { return nodes.Count > 0; }
        }
        ///<summary>是否是根节点</summary>
        bool isRoot
        {
            get
            {
                return parent == null;
            }
        }
        bool _isExpand;
        ///<summary>是否处于扩展状态</summary>
        public bool isExpand
        {
            get
            {
                return _isExpand;
            }
            set
            {
                if (_isExpand == value)
                    behavior = EBehavior.无;
                else if (value)
                {
                    behavior = EBehavior.扩展子项;
                    if (haveChild)
                        foreach (SettleNode node in nodes)
                            node.behavior = EBehavior.被扩展;
                }
                else
                {
                    behavior = EBehavior.收缩子项;
                    if (haveChild)
                        foreach (SettleNode node in nodes)
                            node.behavior = EBehavior.被收缩;
                }

                _isExpand = value;
                //if (!value)
                //    if (haveChild)
                //        foreach (SettleNode node in nodes)
                //            node.isExpand = value;
            }
        }
        ///<summary>是否可见</summary>
        bool isVisual
        {
            get
            {
                if (isRoot)
                    return !isExpand;
                else
                    return parent.isExpand && !isExpand;
            }
        }


        //结算数据属性
        public string settlename;
        double _energy;
        public double energy
        {
            get
            {
                if (haveChild)
                    return nodes.Sum(p => p.energy);
                else
                    return _energy;
            }
            set
            {
                _energy = value;
            }
        }
        double _price;
        public double price
        {
            get
            {
                double result = 0;
                if (energy > 0)
                    result = fee / energy;
                else
                    result = _price;
                return result;
            }
            set
            {
                _price = value;
            }
        }
        double _fee;
        public double fee
        {
            get
            {
                if (haveChild)
                    return nodes.Sum(p => p.fee);
                else
                    return _fee;
            }
            set
            {
                _fee = value;
            }
        }
        
        ///<summary>缩放矢量</summary>
        public Vector3D scaleVec
        {
            get
            {
                double sx = 0.5, sy = 1, sz = 0.25;
                if (isRoot)
                {
                    sx = owner.para.Limit.X * sx;
                    sy = owner.para.Limit.Y * sy;
                    sz = sx; 
                }
                else
                {
                    double maxprice = root.ContainerNodes.Where(p => p.layer == layer).Max(p => p.price);
                    double maxenergy = root.ContainerNodes.Where(p => p.layer == layer).Max(p => p.energy);
                    sx = price / maxprice * owner.para.Limit.X * sx;
                    sy = energy / maxenergy * owner.para.Limit.Y * sy;
                    //sx = price / parent.nodes.Max(p => p.price) * owner.para.Limit.X * sx;
                    //sy = energy / parent.nodes.Max(p => p.energy) * owner.para.Limit.Y * sy;
                    sz = owner.para.Limit.Z * sz;
                    sz = sx;// owner.para.Limit.Z* sz;
                }
                return new Vector3D(sx, sy, sz) * layerScale ;
            }
        }
        ///<summary>当前状态下，占用的空间</summary>
        public Vector3D RectVec  // 
        {
            get
            {
                double sx = 0, sy = 0, sz = 0;
                if (isExpand && haveChild)
                { //注：分裂方向，由子节点合计，非分裂方向，取子节点（子节点可能进行过分裂）与本节点之大值
                    sx = splitDir == ESplitDir.x方向 ? nodes.Sum(p => p.RectVec.X) : nodes.Max(p => p.RectVec.X);
                    sy = splitDir == ESplitDir.y方向 ? nodes.Sum(p => p.RectVec.Y) : nodes.Max(p => p.RectVec.Y);
                    sz = splitDir == ESplitDir.z方向 ? nodes.Sum(p => p.RectVec.Z) : nodes.Max(p => p.RectVec.Z);

                    return new Vector3D(sx, sy, sz);
                }
                else
                    return owner.para.Limit * layerScale;

            }
        }
        ///<summary>与父节点的相对位移</summary>
        public Vector3D relativeTranslateVec  
        {
            get
            {
                double sx = 0, sy = 0, sz = 0;
                if (!isRoot)
                {

                    if (parent.nodes[0].Equals(this))
                    {
                        return new Vector3D(0, 0, 0);// RectVec / 2 - parent.RectVec / 2;
                    }
                    else
                    {

                        sx = (parent.splitDir == ESplitDir.x方向) ? PrevNode.RectVec.X : 0;
                        sy = (parent.splitDir == ESplitDir.y方向) ? PrevNode.RectVec.Y : 0;
                        sz = (parent.splitDir == ESplitDir.z方向) ? PrevNode.RectVec.Z : 0;

                        return PrevNode.relativeTranslateVec + new Vector3D(sx, sy, sz);
                    }

                }
                return new Vector3D(sx, sy, sz);
            }
        }
        ///<summary>当前状态下的绝对位移</summary>
        public Vector3D absoluteTranslateVec  
        {
            get
            {
                Vector3D result = new Vector3D(0, 0, 0);
                if (!isRoot)
                {
                    if (parent.isExpand)
                        result = parent.absoluteTranslateVec + relativeTranslateVec;
                    else
                        result = parent.absoluteTranslateVec;
                }
                return result;
            }
        }

        public SolidColorBrush mbrush = new SolidColorBrush(Color.FromArgb(0xff, 0xFf, 0x80, 0x80));
        public Model3D zmodel; //主模型
        public Model3D zlabel; //标签模型
        // 配置参数
        public Vector3D oldTranslate;
        Duration duration = TimeSpan.FromSeconds(1);
        //========= 方法
        ///<summary>添加节点</summary>
        public SettleNode addNode(string name)
        {
            SettleNode node = new SettleNode(owner, this);
            node.nodename = name;
            nodes.Add(node);
            return node;
        }
        ///<summary>初始化</summary>
        public void initModel()
        {
            //构建文字圆柱
            //MeshGeometry3D meshtop, meshmain;
            //meshtop = Share.Model3DHelper.genCylinder3DTopMesh();
            //meshmain = Share.Model3DHelper.genCylinder3DMainMesh();
            //GeometryModel3D modeltop, modelmain;
            //modeltop = Share.Model3DHelper.genTextPanel2(meshtop, mbrush, Brushes.Black, 36, price.ToString("f4"), 1, false, new Thickness(0.5, 0, 0.5, 0));
            //double w, h;
            //h = 200;
            //w = ((Math.PI * scaleVec.X / scaleVec.Y) * h);
            //bool isrotation = w / Math.PI < h;
            //modelmain = Share.Model3DHelper.genTextPanel2(meshmain, mbrush, Brushes.Black, 36, energy.ToString("f0"), w / h, isrotation, isrotation ? new Thickness(1.5, 0.5, 1.5, 0.5) : new Thickness(3, 0, 3, 0));
            //Model3DGroup mmg = new Model3DGroup();
            //mmg.Children.Add(modeltop);
            //mmg.Children.Add(modelmain);
            //zmodel = mmg;

            //构建圆边方柱
            //MeshGeometry3D meshtop, meshmain;
            //meshtop = Share.Model3DHelper.genBoxTopMesh();
            //meshmain = Share.Model3DHelper.genBoxMidMesh();
            //GeometryModel3D modeltop, modelmain;
            //modeltop = Share.Model3DHelper.genTextPanel2(meshtop, mbrush, Brushes.Black, 36, price.ToString("f4"), 1, false, new Thickness(0.5, 0, 0.5, 0));
            //double w, h;
            //h = 200;
            //w = ((Math.PI * scaleVec.X / scaleVec.Y) * h);
            //bool isrotation = w / Math.PI < h;
            //modelmain = Share.Model3DHelper.genTextPanel2(meshmain, mbrush, Brushes.Black, 36, energy.ToString("f0"), w / h, isrotation, isrotation ? new Thickness(1.5, 0.5, 1.5, 0.5) : new Thickness(3, 0, 3, 0));
            //Model3DGroup mmg = new Model3DGroup();
            //mmg.Children.Add(modeltop);
            //mmg.Children.Add(modelmain);
            //zmodel = mmg;


            //构建园边柱
            //MeshGeometry3D meshtop = (MeshGeometry3D)Application.Current.FindResource("meshSCylTop");
            //MeshGeometry3D meshmid = (MeshGeometry3D)Application.Current.FindResource("meshSCylMid");
            //MaterialGroup mat = new MaterialGroup();
            //mat.Children.Add(new DiffuseMaterial(mbrush));
            //mat.Children.Add(new SpecularMaterial(Brushes.White, 80));
            ////Vector3D basescale = new Vector3D(1.0 / meshmid.Bounds.SizeX, 1.0 / (meshmid.Bounds.SizeY+meshtop.Bounds.SizeY), 1.0 / meshmid.Bounds.SizeZ); //缩放为与常用的一致，2*2*2
            //Point3D basePoint = new Point3D(meshmid.Bounds.X + meshmid.Bounds.SizeX / 2, meshmid.Bounds.Y, meshmid.Bounds.Z + meshmid.Bounds.SizeZ / 2);//底面中心
            ////Vector3D zscale = new Vector3D(scaleVec.X * basescale.X, scaleVec.Y * basescale.Y, scaleVec.Z * basescale.Z);
            //Vector3D zscale = new Vector3D(scaleVec.X, scaleVec.Y, scaleVec.Z);
            //Vector3D ztranslate = absoluteTranslateVec + new Vector3D(0, 1, 0);
            //Transform3DGroup tg = new Transform3DGroup();
            //tg.Children.Add(new ScaleTransform3D(zscale, basePoint));
            //tg.Children.Add(new TranslateTransform3D(ztranslate));
            //zmodel = Model3DHelper.genBoxCylModelGroup(tg, mat, meshtop, meshmid);
            //foreach (GeometryModel3D one in (zmodel as Model3DGroup).Children)
            //{
            //    one.SetValue(ToolTipService.ToolTipProperty, this);
            //}
            ////zmodel.SetValue(ToolTipService.ToolTipProperty, this);



            //构建无文字圆柱
            MeshGeometry3D mesh = (MeshGeometry3D)Application.Current.FindResource("meshCyl");
            MaterialGroup mat = new MaterialGroup();
            mat.Children.Add(new DiffuseMaterial(mbrush));
            mat.Children.Add(new SpecularMaterial(Brushes.White, 80));
            zmodel = new GeometryModel3D(mesh, mat);
            zmodel.SetValue(ToolTipService.ToolTipProperty, this);
            Vector3D basescale = new Vector3D(1.0 / zmodel.Bounds.SizeX, 1.0 / zmodel.Bounds.SizeY, 1.0 / zmodel.Bounds.SizeZ); //缩放为与常用的一致，2*2*2
            Point3D basePoint = new Point3D(zmodel.Bounds.X + zmodel.Bounds.SizeX / 2, zmodel.Bounds.Y, zmodel.Bounds.Z + zmodel.Bounds.SizeZ / 2);//底面中心
            Vector3D zscale = new Vector3D(scaleVec.X * basescale.X, scaleVec.Y * basescale.Y, scaleVec.Z * basescale.Z);
            Vector3D ztranslate = absoluteTranslateVec + new Vector3D(0, -zmodel.Bounds.Y, 0);
            Transform3DGroup tg = new Transform3DGroup();
            tg.Children.Add(new ScaleTransform3D(zscale, basePoint));
            tg.Children.Add(new TranslateTransform3D(ztranslate));
            zmodel.Transform = tg;

            MeshGeometry3D meshlabel = (MeshGeometry3D)Application.Current.FindResource("meshText");
            Vector3D vecTranslate = new Vector3D(absoluteTranslateVec.X, absoluteTranslateVec.Y + zmodel.Bounds.SizeY, absoluteTranslateVec.Z);
            string label = nodename + "\r\n" + fee.ToString("f0");
            double scale = layerScale;
            int rownums = MyFunction.StringContainsSubstringNums(label, "\r\n");
            double temp=2;//新疆怪需求
            double panelheight = 0.2 * scale * (rownums + 1) * temp;
            zlabel = Model3DHelper.genTextPanel(meshlabel, Brushes.Transparent, new SolidColorBrush(Colors.Yellow), 36, label, null, panelheight, vecTranslate, 0.5, 0);


            if (haveChild)
                foreach (SettleNode e0 in nodes)
                    e0.initModel();
        }

        ///<summary>显示树结构分解视图</summary>
        public void showModel()
        {
            DoubleAnimation da;
            ImageBrush lbrush;
            SolidColorBrush mmainbrush = null;// mtopbrush;
            lbrush = (((zlabel as GeometryModel3D).Material as MaterialGroup).Children[0] as DiffuseMaterial).Brush as ImageBrush;
            //mtopbrush = ((((zmodel as Model3DGroup).Children[0] as GeometryModel3D).Material as MaterialGroup).Children[0] as DiffuseMaterial).Brush as ImageBrush;
            //mmainbrush = ((((zmodel as Model3DGroup).Children[1] as GeometryModel3D).Material as MaterialGroup).Children[0] as DiffuseMaterial).Brush as ImageBrush;
            if (zmodel is GeometryModel3D)
                mmainbrush = ((((zmodel as GeometryModel3D)).Material as MaterialGroup).Children[0] as DiffuseMaterial).Brush as SolidColorBrush;
            else
                mmainbrush = ((((zmodel as Model3DGroup).Children[0] as GeometryModel3D).Material as MaterialGroup).Children[0] as DiffuseMaterial).Brush as SolidColorBrush;

            TranslateTransform3D translate;
            switch (behavior)
            {
                case EBehavior.扩展子项:
                    da = new DoubleAnimation(1, 0, duration, FillBehavior.Stop);
                    da.Completed += new EventHandler(da_Completed);
                    //mtopbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    //mtopbrush.Opacity = 0;
                    mmainbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    mmainbrush.Opacity = 0;
                    translateModel(oldTranslate, absoluteTranslateVec, oldTranslate);

                    lbrush.BeginAnimation(ImageBrush.OpacityProperty, da);
                    lbrush.Opacity = 0;
                    translateLabel(oldTranslate, absoluteTranslateVec, oldTranslate);
                    break;
                case EBehavior.被扩展:
                    da = new DoubleAnimation(0, 1, duration, FillBehavior.Stop);
                    //mtopbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    //mtopbrush.Opacity = 1;
                    mmainbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    mmainbrush.Opacity = 1;
                    translateModel(parent.oldTranslate, absoluteTranslateVec, oldTranslate);

                    lbrush.BeginAnimation(ImageBrush.OpacityProperty, da);
                    lbrush.Opacity = 1;
                    translateLabel(parent.oldTranslate, absoluteTranslateVec, oldTranslate);
                    break;
                case EBehavior.收缩子项:
                    da = new DoubleAnimation(0, 1, duration, FillBehavior.Stop);
                    da.Completed += new EventHandler(da_Completed);
                    //mtopbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    //mtopbrush.Opacity = 1;
                    mmainbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    mmainbrush.Opacity = 1;
                    translateModel(nodes[0].absoluteTranslateVec, absoluteTranslateVec, oldTranslate);

                    lbrush.BeginAnimation(ImageBrush.OpacityProperty, da);
                    lbrush.Opacity = 1;
                    translateLabel(nodes[0].absoluteTranslateVec, absoluteTranslateVec, oldTranslate);
                    break;
                case EBehavior.被收缩:
                    da = new DoubleAnimation(1, 0, duration, FillBehavior.Stop);
                    //mtopbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    //mtopbrush.Opacity = 0;
                    mmainbrush.BeginAnimation(SolidColorBrush.OpacityProperty, da);
                    mmainbrush.Opacity = 0;
                    translateModel(oldTranslate, parent.absoluteTranslateVec, oldTranslate);

                    lbrush.BeginAnimation(ImageBrush.OpacityProperty, da);
                    lbrush.Opacity = 0;
                    translateLabel(oldTranslate, parent.absoluteTranslateVec, oldTranslate);
                    break;
                case EBehavior.无:
                    if (isVisual)
                    {
                        owner.mgModel.Children.Add(zmodel);
                        translate = null;
                        foreach (Transform3D one in (zmodel.Transform as Transform3DGroup).Children)
                        {
                            if (one is TranslateTransform3D)
                            {
                                translate = one as TranslateTransform3D;
                                break;
                            }
                        }
                        if (translate == null)
                            return;

                        //translate = (zmodel.Transform as Transform3DGroup).Children[1] as TranslateTransform3D;
                        translate.OffsetX = absoluteTranslateVec.X;
                        //translate.OffsetY = absoluteTranslateVec.Y;
                        translate.OffsetZ = absoluteTranslateVec.Z;

                        owner.mgLabel.Children.Add(zlabel);
                        translate = (zlabel.Transform as Transform3DGroup).Children[1] as TranslateTransform3D;
                        translate.OffsetX = absoluteTranslateVec.X;
                        translate.OffsetY = absoluteTranslateVec.Y + zmodel.Bounds.SizeY * 1.01 + 1;
                        translate.OffsetZ = absoluteTranslateVec.Z;

                    }

                    break;
            }
            if (haveChild)
                foreach (SettleNode e0 in nodes)
                    e0.showModel();


        }
        void translateModel(Vector3D srcvec, Vector3D objvec, Vector3D oldvec)
        {
            DoubleAnimation dx, dy, dz;
            TranslateTransform3D translate = null;
            foreach (Transform3D one in (zmodel.Transform as Transform3DGroup).Children)
            {
                if (one is TranslateTransform3D)
                {
                    translate = one as TranslateTransform3D;
                    break;
                }
            }
            if (translate == null)
                return;

            if (objvec.X != oldvec.X)
            {
                dx = new DoubleAnimation(srcvec.X, objvec.X, duration, FillBehavior.Stop);
                translate.BeginAnimation(TranslateTransform3D.OffsetXProperty, dx);
            }
            translate.OffsetX = objvec.X;
            if (objvec.Y != oldvec.Y)  //备忘：遗留问题，此暂并未使用到Y方向的分裂，无需对齐
            {
                dy = new DoubleAnimation(srcvec.Y, objvec.Y, duration, FillBehavior.Stop);
                translate.BeginAnimation(TranslateTransform3D.OffsetYProperty, dy);
                translate.OffsetY = objvec.Y;
            }
            if (objvec.Z != oldvec.Z)
            {
                dz = new DoubleAnimation(srcvec.Z, objvec.Z, duration, FillBehavior.Stop);
                translate.BeginAnimation(TranslateTransform3D.OffsetZProperty, dz);
            }
            translate.OffsetZ = objvec.Z;

        }
        void translateLabel(Vector3D srcvec, Vector3D objvec, Vector3D oldvec)
        {
            DoubleAnimation dx, dy, dz;
            TranslateTransform3D translate = (zlabel.Transform as Transform3DGroup).Children[1] as TranslateTransform3D;
            if (objvec.X != oldvec.X)
            {
                dx = new DoubleAnimation(srcvec.X, objvec.X, duration, FillBehavior.Stop);
                translate.BeginAnimation(TranslateTransform3D.OffsetXProperty, dx);
            }
            translate.OffsetX = objvec.X;
            if (objvec.Y != oldvec.Y)
            {
                dy = new DoubleAnimation(srcvec.Y + 1, objvec.Y + zmodel.Bounds.SizeY * 1.01 + 1, duration, FillBehavior.Stop);
                translate.BeginAnimation(TranslateTransform3D.OffsetYProperty, dy);
            }
            translate.OffsetY = objvec.Y + zmodel.Bounds.SizeY * 1.01 + 1;
            if (objvec.Z != oldvec.Z)
            {
                dz = new DoubleAnimation(srcvec.Z, objvec.Z, duration, FillBehavior.Stop);
                translate.BeginAnimation(TranslateTransform3D.OffsetZProperty, dz);
            }
            translate.OffsetZ = objvec.Z;

        }

        void da_Completed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            anicomplete();
        }


        //=======
        ///<summary>动画前的准备, 功能：重新载入模型，以处理透明</summary>
        public void anibefore() 
        {
            owner.mgModel.Children.Clear();
            owner.mgLabel.Children.Clear();
            loadModel(0);
            loadModel(1);
            loadModel(2);
        }
        ///<summary>载入模型</summary>
        void loadModel(int showtype)
        {
            switch (showtype)
            {
                case 0:
                    //1. 载入无动画可见模型
                    if (isVisual && behavior == EBehavior.无)
                    {
                        owner.mgModel.Children.Add(zmodel);
                        owner.mgLabel.Children.Add(zlabel);
                    }
                    break;
                case 1:
                    //2. 载入被遮模型
                    if (behavior == EBehavior.被扩展 || behavior == EBehavior.被收缩)
                    {
                        owner.mgModel.Children.Add(zmodel);
                        owner.mgLabel.Children.Add(zlabel);
                    }
                    break;
                case 2:
                    //3. 载入遮模型
                    if (behavior == EBehavior.扩展子项 || behavior == EBehavior.收缩子项)
                    {
                        owner.mgModel.Children.Add(zmodel);
                        owner.mgLabel.Children.Add(zlabel);
                    }
                    break;
            }

            if (haveChild)
                foreach (SettleNode e0 in nodes)
                    e0.loadModel(showtype);

        }
        ///<summary>动画后的处理，移除不可见对象</summary>
        public void anicomplete()  
        {
            if (behavior == EBehavior.扩展子项 || behavior == EBehavior.被收缩)
            {
                owner.mgModel.Children.Remove(zmodel);
                owner.mgLabel.Children.Remove(zlabel);
            }
            behavior = EBehavior.无;
            oldTranslate = new Vector3D(absoluteTranslateVec.X, absoluteTranslateVec.Y, absoluteTranslateVec.Z);
            if (haveChild)
                foreach (SettleNode e0 in nodes)
                    e0.anicomplete();
        }
        //===设置进行扩展的属性expand
        public void setExpandLayer(int layeridx)
        {
            isExpand = layer <= layeridx;

            if (haveChild)
                foreach (SettleNode e in nodes)
                    e.setExpandLayer(layeridx);
        }
        public void setExpandToNext()
        {
            if (!isExpand && haveChild)
                isExpand = true;
            else
                if (haveChild)
                    foreach (SettleNode e in nodes)
                        e.setExpandToNext();
        }
        public void setFoldToPrev()
        {
            if (haveChild && isExpand)
                foreach (SettleNode e in nodes)
                    e.setFoldToPrev();
            else
                if (!isRoot)
                    if (parent.isExpand)
                        parent.isExpand = false;
        }
        //========tooltip支持方法


        internal void clearNode()
        {
            foreach (SettleNode item in nodes)
            {
                item.clearNode();
            }
            zlabel = null;
            zmodel = null;
            nodes.Clear();
        }
    }

}
