using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Toolkit.Primitives;
using ESRI.ArcGIS.Client.Tasks;

namespace ArcGisSilverlightApplication
{
    public partial class MainPage : UserControl
    {
        private bool _isDown = false;
        private Point _lastPoint;

        public MainPage()
        {
            InitializeComponent();
        }

        //Magnifier
        private void MyMagnifyImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyMagnifier.Enabled = !MyMagnifier.Enabled;
        
        }

       
     
        //private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{


        //    _isDown = true;
        //    _lastPoint = e.GetPosition(ResultsDisplayCanvas);

            
        //}
        //private void Canvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (_isDown)
        //    {
        //        Point point = e.GetPosition(ResultsDisplayCanvas);
        //        double deltaX = point.X - _lastPoint.X;
        //        double deltaY = point.Y - _lastPoint.Y;
        //        CompositeTransform transform = null;

        //        if (!(ResultsDisplayCanvas.Children[0].RenderTransform is CompositeTransform))
        //        {
        //            transform = new CompositeTransform();
        //            ResultsDisplayCanvas.Children[0].RenderTransform = transform;
        //        }
        //        else
        //        {
        //            transform = ResultsDisplayCanvas.Children[0].RenderTransform as CompositeTransform;
        //        }

        //        transform.TranslateX += deltaX;
        //        transform.TranslateY += deltaY;

        //        _lastPoint = e.GetPosition(ResultsDisplayCanvas);
        //    }

        //}

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDown = false;

        }
        //
        private void FeatureLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs args)
        {
            args.Graphic.Selected = !args.Graphic.Selected;
            if (args.Graphic.Selected)
                MyDataGrid.ScrollIntoView(args.Graphic, null);
        }
        //void QueryButton_Click(object sender, RoutedEventArgs e)
        //{
        //    QueryTask queryTask =
        //        new QueryTask("http://maverick.arcgis.com/ArcGIS/rest/services/World_WGS84/MapServer/0");
        //    queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
        //    queryTask.Failed += QueryTask_Failed;

        //    ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
        //    query.Text = StateNameTextBox.Text;

        //    query.OutFields.Add("*");
        //    queryTask.ExecuteAsync(query);
        //}

        //void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        //{
        //    FeatureSet featureSet = args.FeatureSet;

        //    if (featureSet != null && featureSet.Features.Count > 0)
        //        QueryDetailsDataGrid.ItemsSource = featureSet.Features;
        //    else
        //        MessageBox.Show("No features returned from query");
        //}

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query execute error: " + args.Error);
        }


    }
}
