using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{
    class IEdit : BaseMain
    {
        public IEdit(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {
            
        }
        

        protected override void load()  //进入时装载数据
        {
            if (IEditController.uctop == null) IEditController.uctop = new Top();
            toolboxSub.Children.Add(IEditController.uctop);

            root.distnet.scene.camera.adjustCameraAngle(0);
            root.distnet.isEditMode = true;
            root.distnet.editpanel.Margin = new System.Windows.Thickness(0, 80, 0, 0);

            if (IEditController.ucpanel == null)
                IEditController.ucpanel = new IEditPanel(root);
            root.grdContent.Children.Add(IEditController.ucpanel);

            DNVLibrary.Run.DataGenerator.initRunData(root.distnet);
            DNVLibrary.Run.DataGenerator.StartGenData(root.distnet);
        }
        protected override void unload()  //退出时卸载数据
        {
            root.distnet.isEditMode = false;

            if (root.grdContent.Children.Contains(IEditController.ucpanel))
                root.grdContent.Children.Remove(IEditController.ucpanel);

            DNVLibrary.Run.DataGenerator.StopGenData();
        }
    }

    //class IEdit : AppBase
    //{
    //    public IEdit(UCDNV863 Root)
    //        : base(Root)
    //    {
    //    }

    //    internal override void load()
    //    {
    //        root.distnet.scene.camera.adjustCameraAngle(0);
    //        root.distnet.isEditMode = true;
    //        root.distnet.editpanel.Margin = new System.Windows.Thickness(0, 80, 0, 0);
    //    }

    //    internal override void unload()
    //    {
    //        root.distnet.isEditMode = false;
    //    }

    //    internal void isshowedit(bool isedit)
    //    {
    //        root.distnet.isEditMode = isedit;
    //    }
    //}

}
