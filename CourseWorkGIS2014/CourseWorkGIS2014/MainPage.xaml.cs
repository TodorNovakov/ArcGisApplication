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
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Data;

namespace CourseWorkGIS2014
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }
        //Magnifier
        private void MyMagnifyImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyMagnifier.Enabled = !MyMagnifier.Enabled;

        }

        //Feature Data Grid
        private void FeatureLayer_MouseLeftButtonDown(object sender, GraphicMouseButtonEventArgs args)
        {
            args.Graphic.Selected = !args.Graphic.Selected;
            if (args.Graphic.Selected)
                MyDataGrid.ScrollIntoView(args.Graphic, null);
        }


        //Query Task
        void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            QueryTask queryTask =
                new QueryTask("http://maverick.arcgis.com/ArcGIS/rest/services/World_WGS84/MapServer/0");
            queryTask.ExecuteCompleted += QueryTask_ExecuteCompleted;
            queryTask.Failed += QueryTask_Failed;

            ESRI.ArcGIS.Client.Tasks.Query query = new ESRI.ArcGIS.Client.Tasks.Query();
            query.Text = StateNameTextBox.Text;

            query.OutFields.Add("*");
            queryTask.ExecuteAsync(query);
        }

        void QueryTask_ExecuteCompleted(object sender, ESRI.ArcGIS.Client.Tasks.QueryEventArgs args)
        {
            FeatureSet featureSet = args.FeatureSet;

            if (featureSet != null && featureSet.Features.Count > 0)
                QueryDetailsDataGrid.ItemsSource = featureSet.Features;
            else
                MessageBox.Show("No features returned from query");
        }

        private void QueryTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Query execute error: " + args.Error);
        }

        //Identify task
        private List<IdentifyResult> _lastIdentifyResult;

        // Perform Identify when the map is clicked.
        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs args)
        {
            // Show an icon at the Identify location.
            GraphicsLayer graphicsLayer = MyMap.Layers["IdentifyIconGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
            ESRI.ArcGIS.Client.Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = args.MapPoint,
                Symbol = LayoutRoot.Resources["IdentifyLocationSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
            };
            graphicsLayer.Graphics.Add(graphic);

            // Identify task initialization.
            IdentifyTask identifyTask = new IdentifyTask("http://maverick.arcgis.com/ArcGIS/rest/services/World_WGS84/MapServer/");
            identifyTask.ExecuteCompleted += IdentifyTask_ExecuteCompleted;
            identifyTask.Failed += IdentifyTask_Failed;

            // Initialize Identify parameters. Specify search of all layers.
            IdentifyParameters identifyParameters = new IdentifyParameters();
            //identifyParameters.LayerOption = LayerOption.all;
            identifyParameters.LayerIds.AddRange(new int[] { 0, 1 });

            // Pass the current Map properties to Identify parameters.
            identifyParameters.MapExtent = MyMap.Extent;
            identifyParameters.Width = (int)MyMap.ActualWidth;
            identifyParameters.Height = (int)MyMap.ActualHeight;

            // Identify features at the click point.
            identifyParameters.Geometry = args.MapPoint;

            identifyTask.ExecuteAsync(identifyParameters);
        }

        // Populate ComboBox with the results when Identify is complete.
        private void IdentifyTask_ExecuteCompleted(object sender, IdentifyEventArgs args)
        {
            IdentifyComboBox.Items.Clear();

            // Check for new results.
            if (args.IdentifyResults.Count > 0)
            {
                // Show ComboBox and attributes DataGrid.
                IdentifyResultsStackPanel.Visibility = Visibility.Visible;

                // Add results to ComboBox.
                foreach (IdentifyResult result in args.IdentifyResults)
                {
                    string title = string.Format("{0} ({1})", result.Value.ToString(), result.LayerName);
                    IdentifyComboBox.Items.Add(title);
                }

                // Workaround for ComboBox bug.
                IdentifyComboBox.UpdateLayout();

                // Store the list of Identify results.
                _lastIdentifyResult = args.IdentifyResults;

                // Initialize ComboBox and fire SelectionChanged.
                IdentifyComboBox.SelectedIndex = 0;
            }
            else
            {
                // Hide ComboBox and attributes DataGrid and notify the user.
                IdentifyResultsStackPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show("No features found");
            }
        }

        // Show geometry and attributes of the selected feature.
        void IdentifyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear previously selected feature from GraphicsLayer.
            GraphicsLayer graphicsLayer = MyMap.Layers["ResultsGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            // Check that ComboBox has a selected item. This is needed because SelectionChanged fires
            // when ComboBox.Clear is called.
            if (IdentifyComboBox.SelectedIndex > -1)
            {
                // Update DataGrid with the selected feature's attributes.
                Graphic selectedFeature = _lastIdentifyResult[IdentifyComboBox.SelectedIndex].Feature;
                IdentifyDetailsDataGrid.ItemsSource = selectedFeature.Attributes;

                // Apply the symbol and add the selected feature to the map.
                selectedFeature.Symbol = LayoutRoot.Resources["SelectedFeatureSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                graphicsLayer.Graphics.Add(selectedFeature);
            }
        }

        // Notify when Identify fails.
        private void IdentifyTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Identify failed: " + args.Error);
        }


        //FIND TASK
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();

            FindTask findTask = new FindTask("http://maverick.arcgis.com/ArcGIS/rest/services/World_WGS84/MapServer/");
            findTask.Failed += FindTask_Failed;

            FindParameters findParameters = new FindParameters();
            // Layer ids to search
            findParameters.LayerIds.AddRange(new int[] { 0,1 });
            // Fields in layers to search
            findParameters.SearchFields.AddRange(new string[] { "CITY_NAME","CONTINENT" });
            // Return features in map's spatial reference
            findParameters.SpatialReference = MyMap.SpatialReference;

            // Bind data grid to find results.  Bind to the LastResult property which returns a list
            // of FindResult instances.  When LastResult is updated, the ItemsSource property on the 
            // will update.  
            Binding resultFeaturesBinding = new Binding("LastResult");
            resultFeaturesBinding.Source = findTask;
            FindDetailsDataGrid.SetBinding(DataGrid.ItemsSourceProperty, resultFeaturesBinding);

            findParameters.SearchText = FindText.Text;
            findTask.ExecuteAsync(findParameters);

            // Since binding to DataGrid, handling the ExecuteComplete event is not necessary.
        }

        private void FindDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Highlight the graphic feature associated with the selected row
            DataGrid dataGrid = sender as DataGrid;

            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex > -1)
            {
                FindResult findResult = (FindResult)FindDetailsDataGrid.SelectedItem;
                Graphic graphic = findResult.Feature;

                switch (graphic.Attributes["Shape"].ToString())
                {
                    case "Polygon":
                        graphic.Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                        break;
                    case "Polyline":
                        graphic.Symbol = LayoutRoot.Resources["DefaultLineSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                        break;
                    case "Point":
                        graphic.Symbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                        break;
                }

                GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
                graphicsLayer.ClearGraphics();
                graphicsLayer.Graphics.Add(graphic);
            }
        }

        private void FindTask_Failed(object sender, TaskFailedEventArgs args)
        {
            MessageBox.Show("Find failed: " + args.Error);
        }

        //Edit Feauters (Feature Data Form)
        //private void FeatureLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs args)
        //{
        //    FeatureLayer featureLayer = sender as FeatureLayer;

        //    foreach (Graphic g in featureLayer.Graphics)
        //        if (g.Selected)
        //            g.UnSelect();

        //    args.Graphic.Select();
        //    MyFeatureDataForm.GraphicSource = args.Graphic;

        //    FeatureDataFormBorder.Visibility = Visibility.Visible;
        //}
    }
}
