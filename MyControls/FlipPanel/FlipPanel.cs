using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;


namespace MyControlLibrary.FlipPanel
{
    public class FlipPanel : Panel
    {
        public static readonly DependencyProperty FrontVisibleProperty = DependencyProperty.Register("FrontVisible", typeof(bool), typeof(FlipPanel), new PropertyMetadata(true, OnFrontVisibleChanged));
        public static readonly DependencyProperty SpinTimeProperty = DependencyProperty.Register("SpinTime", typeof(double), typeof(FlipPanel), new PropertyMetadata(1.0));
        public static readonly DependencyProperty SpinAxisProperty = DependencyProperty.Register("SpinAxis", typeof(Orientation), typeof(FlipPanel), new PropertyMetadata(Orientation.Horizontal, OnSpinAxisChanged));

        private static readonly Vector3D AxisX = new Vector3D(1, 0, 0);
        private static readonly Vector3D AxisY = new Vector3D(0, 1, 0);
        private static readonly Vector3D AxisZ = new Vector3D(0, 0, 1);

        private static readonly Material visualHostMaterial = new DiffuseMaterial(Brushes.White);
        private static readonly MeshGeometry3D mesh = Plane.CreateXY(AxisZ, 1, 1);

        private ModelVisual3D model = new ModelVisual3D();

        private Viewport2DVisual3D frontVisual3D;
        private Viewport2DVisual3D backVisual3D;

        private UIElement frontElement;
        private UIElement backElement;

        private Viewport3D viewPort;
        private ModelVisual3D contentContainer;
        private AxisAngleRotation3D rotation = new AxisAngleRotation3D(AxisY, 0);
        private TranslateTransform3D translation = new TranslateTransform3D();
        private ScaleTransform3D scale = new ScaleTransform3D(1, 1, 1);

        public event EventHandler AniCompleted;
        protected virtual void RaiseAniCompletedEvent()
        {
            if (AniCompleted != null)
                AniCompleted(this,null);
        }

        static FlipPanel()
        {
            visualHostMaterial.SetValue(Viewport2DVisual3D.IsVisualHostMaterialProperty, true);
        }

        public FlipPanel()
        {
            InitializeComponent();
            SizeChanged += HandleSizeChanged;
            Loaded += (s, e) => InvalidateMeasure();
            contentContainer.Children.Add(model);
            SetupModel();
        }
        
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded != viewPort)
                throw new InvalidOperationException("Add children using the Front and Back properties");
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private void InitializeComponent()
        {
            viewPort = new Viewport3D
            {
                Camera = new PerspectiveCamera 
                { 
                    FieldOfView = 14, //90
                    Position = new Point3D(0, 0, 4), //0.5
                    LookDirection = new Vector3D(0, 0, -1)
                }
            };

            viewPort.Children.Add(new ModelVisual3D { Content = new AmbientLight(Colors.DarkGray) });
            viewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(0, 0, -1)) });

            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new RotateTransform3D(rotation));
            transform.Children.Add(translation);
            transform.Children.Add(scale);
            contentContainer = new ModelVisual3D { Transform = transform };

            viewPort.Children.Add(contentContainer);
            Children.Add(viewPort);
        }

        private static void OnFrontVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlipPanel)d).Spin();
        }

        private static void OnSpinAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlipPanel)d).SetupModel();
        }

        public bool FrontVisible
        {
            get { return (bool)GetValue(FlipPanel.FrontVisibleProperty); }
            set { SetValue(FlipPanel.FrontVisibleProperty, value); }
        }

        public double SpinTime
        {
            get { return (double)GetValue(FlipPanel.SpinTimeProperty); }
            set { SetValue(FlipPanel.SpinTimeProperty, value); }
        }

        public Orientation SpinAxis
        {
            get { return (Orientation)GetValue(FlipPanel.SpinAxisProperty); }
            set { SetValue(FlipPanel.SpinAxisProperty, value); }
        }

        private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0)
                scale.ScaleY = e.NewSize.Height / e.NewSize.Width;
        }

        private void SetupModel()
        {
            Visual front = null;
            Visual back = null;

            if (frontVisual3D != null)
            {
                front = frontVisual3D.Visual;
                frontVisual3D.Visual = null;
            }

            if (backVisual3D != null)
            {
                back = backVisual3D.Visual;
                backVisual3D.Visual = null;
            }
            frontVisual3D = new Viewport2DVisual3D
            {
                Geometry = mesh,
                Material = visualHostMaterial,
            };

            backVisual3D = new Viewport2DVisual3D
            {
                Geometry = mesh,
                Material = visualHostMaterial,
                Transform = new RotateTransform3D(new AxisAngleRotation3D(SpinAxis == Orientation.Vertical ? AxisY : AxisX, 180))
            };

            rotation.Axis = SpinAxis == Orientation.Vertical ? AxisY : AxisX;

            if (front != null)
                frontVisual3D.Visual = front;
            if (back != null)
                backVisual3D.Visual = back;

            model.Children.Clear();
            model.Children.Add(frontVisual3D);
            model.Children.Add(backVisual3D);

            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            viewPort.Measure(constraint);

            if (frontElement != null)
                frontElement.Measure(constraint);

            if (backElement != null)
                backElement.Measure(constraint);

            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            viewPort.Arrange(new Rect(arrangeBounds));

            if (frontElement != null)
                frontElement.Arrange(new Rect(new Point(), arrangeBounds));

            if (backElement != null)
                backElement.Arrange(new Rect(new Point(), arrangeBounds));

            return arrangeBounds;
        }

        public void Spin()
        {
            Front.InvalidateVisual();
            Back.InvalidateVisual();
            //DoubleAnimation rotationAnimation = new DoubleAnimation(FrontVisible ? 0 : 180, new Duration(TimeSpan.FromSeconds(SpinTime)));
            DoubleAnimation rotationAnimation = new DoubleAnimation(rotation.Angle-180, new Duration(TimeSpan.FromSeconds(SpinTime)));
            rotationAnimation.Completed += new EventHandler(rotationAnimation_Completed);
            rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, rotationAnimation);

            //DoubleAnimationUsingKeyFrames translationAnimation = new DoubleAnimationUsingKeyFrames();
            //translationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-0.5, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(SpinTime / 2.0)), new SineEase()));
            //translationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(SpinTime)), new SineEase { EasingMode = EasingMode.EaseIn }));
            //translation.BeginAnimation(TranslateTransform3D.OffsetZProperty, translationAnimation);

        }

        public void rotationAnimation_Completed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            RaiseAniCompletedEvent();
        }



        public UIElement Front
        {
            get { return frontElement; }
            set
            {
                frontVisual3D.Visual = null;
                frontVisual3D.Visual = value;
                frontElement = value;
            }
        }

        public UIElement Back
        {
            get { return backElement; }
            set
            {
                backVisual3D.Visual = null;
                backVisual3D.Visual = value;
                backElement = value;
            }
        }
    }
}
