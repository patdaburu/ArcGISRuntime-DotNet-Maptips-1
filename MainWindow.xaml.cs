using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISRuntime_DotNet_Maptips
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The graphics overlay we'll add to the map.  (Graphics will go here.)
        /// </summary>
        GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

        /// <summary>
        /// The circle symbol for the graphics we'll create.
        /// </summary>
        SimpleMarkerSymbol circleSymbol = new SimpleMarkerSymbol(
                SimpleMarkerSymbolStyle.Circle,
                Colors.Red, 10);

        public MainWindow()
        {
            InitializeComponent();
            // Add the graphics overlay to the map view.
            this.mapView.GraphicsOverlays.Add(this.graphicsOverlay);
        }

        /// <summary>
        /// When the user taps the map...
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="GeoViewInputEventArgs"/> instance containing the event data.
        /// </param>
        private async void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear the selection.
            this.graphicsOverlay.ClearSelection();

            // Let's use the tools we have to tell that a graphic has been
            // identified.
            var tolerance = 10d;
            var maximumResults = 1; // Just one, please.
            var identifyResults = await this.mapView.IdentifyGraphicsOverlayAsync(
                this.graphicsOverlay,
                e.Position,
                tolerance,
                maximumResults);

            // If the user didn't click on anything...
            if (identifyResults.Count == 0)
            {
                // Hide the map tip.
                this.maptip.Visibility = Visibility.Hidden;
                return;
            }
            // We got a hit? Great!
            var hitGraphic = identifyResults[0];
            // Select it.
            hitGraphic.IsSelected = true;
            // Now, in an actual production app, you'd probably want to use
            // binding in your control, but to keep things simple for the
            // purposes of this example, we'll just set the text in the map
            // tip manually to match the name we gave the graphic when we
            // created it.
            this.maptipTextBlock.Text = 
                string.Format("My name is {0}.", hitGraphic.Attributes["Name"]);
            // Show the map tip.
            this.maptip.Visibility = Visibility.Visible;
            // Hook the map tip to the graphic's coordinate.
            GeoView.SetViewOverlayAnchor(
                this.maptip, (MapPoint)hitGraphic.Geometry);

        }

        /// <summary>
        /// When the user double-taps the map...
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GeoViewInputEventArgs"/> 
        /// instance containing the event data.
        /// </param>
        private void MapView_GeoViewDoubleTapped(
            object sender, GeoViewInputEventArgs e)
        {
            // Create the graphic.
            var dotGraphic = new Graphic(e.Location, circleSymbol);
            // For demonstration purposes, and just to make sure we get some
            // feedback when we select the graphic, let's randomly pick a
            // name for it.
            var names = new string[]
            {
                "John", "Paul", "George", "Ringo",
                "Kukla", "Fran", "Ollie",
                "Daphne", "Velma", "Fred", "Shaggy", "Scooby"
            };
            dotGraphic.Attributes["Name"] = names[(new Random()).Next(names.Length)];
            // Add the graphic to the graphics overlay.
            graphicsOverlay.Graphics.Add(dotGraphic);
        }
    }
}
