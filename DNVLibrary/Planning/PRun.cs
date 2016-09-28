using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Planning
{
    class PRun : BaseMain
    {
        public PRun(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {



            // 工具栏初始化
            for (int i = 2015; i < 2026; i++)
                cmbYear.Items.Add(i);
            cmbYear.SelectedValue = 2020;
            toolboxSub.Children.Add(cmbYear);
            cmbYear.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(cmbYear_SelectionChanged);

            zButton btn;
            btn = new zButton() { text = "潮流", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnFlow_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "负载率", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoad_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "电压", group = "1", Margin = new System.Windows.Thickness(0, 0, 10, 0) };
            btn.Click += new System.Windows.RoutedEventHandler(btnVL_Click);
            toolboxSub.Children.Add(btn);

            //btn = new zButton() { text = "断面潮流", group = "5" };
            //btn.Click += new System.Windows.RoutedEventHandler(btnSection_Click);
            //toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "电源追溯", group = "2" };
            btn.Click += new System.Windows.RoutedEventHandler(btnTrace_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "供电范围", group = "3" };
            btn.Click += new System.Windows.RoutedEventHandler(btnRange_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "负荷等值图", group = "6" };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoadContour_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "区块负荷预测", group = "4" };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoadForcast_Click);
            toolboxSub.Children.Add(btn);
        }

        void cmbYear_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (app != null)
                UCDNV863.CmdPlanningDateChanged.Execute(this, app.panel);
        }
        System.Windows.Controls.ComboBox cmbYear = new System.Windows.Controls.ComboBox() { VerticalContentAlignment = System.Windows.VerticalAlignment.Center }; //规划年选择
        internal int PlanningYear { get { return (int)cmbYear.SelectedValue; } }


        AppBase app;
        //潮流
        void btnFlow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    app = new Planning.PRunFlow(root);
                    app.begin();
                }
            }
            (app as PRunFlow).ShowFlow(btn.isChecked);
        }
        //负载
        void btnLoad_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    app = new Planning.PRunFlow(root);
                    app.begin();
                }
            }
            (app as PRunFlow).ShowLoad(btn.isChecked);
        }
        //电压
        void btnVL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    app = new Planning.PRunFlow(root);
                    app.begin();
                }
            }
            (app as PRunFlow).ShowVL(btn.isChecked);
        }
        //负荷等值图
        void btnLoadContour_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunLoadContour)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunLoadContour))  //不为本身，执行创建
                {
                    app = new Planning.PRunLoadContour(root);
                    app.begin();
                }

            }
            (app as Planning.PRunLoadContour).showhide(btn.isChecked);
        }
        //断面
        void btnSection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunSection)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunSection))  //不为本身，执行创建
                {
                    app = new Planning.PRunSection(root);
                    app.begin();
                }
            }

        }

        //区块负荷预测
        void btnLoadForcast_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunLoadForcast)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunLoadForcast))  //不为本身，执行创建
                {
                    app = new Planning.PRunLoadForcast(root);
                    app.begin();
                }
            }

        }
        //供电范围
        void btnRange_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunSupplyRange)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunSupplyRange))  //不为本身，执行创建
                {
                    app = new Planning.PRunSupplyRange(root);
                    app.begin();
                }
            }
        }
        //电源追溯
        void btnTrace_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunRetrospect)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunRetrospect))  //不为本身，执行创建
                {
                    app = new Planning.PRunRetrospect(root);
                    app.begin();
                }
            }

        }





        protected override void load()  //进入时装载数据
        {
            //状态栏提示添加
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.push();
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_模拟运行";

        }
        protected override void unload()  //退出时卸载数据
        {
            if (app != null) app.end();
            //状态栏提示恢复
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.pop();
        }

    }
}
