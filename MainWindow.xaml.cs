﻿using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
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

            #region Moving the Map Tip

            // We're going to be moving the map tip around, so we'll need
            // a translation transform.
            var mapTipXlateTransform = new TranslateTransform();
            this.maptip.RenderTransform = mapTipXlateTransform;

            // In the event handlers below, we need access to the map tip's
            // parent window.
            Window maptipParentWindow = Window.GetWindow(this.maptip);
            // The handlers below will also need to know where the user first
            // clicks to start a "move" (so let's put a variable in this
            // closure).
            Point dragMaptipStart = default(Point);
            // There will also be sharing of the offset
            Vector dragMaptipOffset = default(Vector);

            // Just to keep all of the relevant code within this one region,
            // let's define the function we'll use to convert the mouse
            // location we get from the MouseEventArgs
            Func<MouseEventArgs, FrameworkElement, Point> getRelativeMousePosition =
                (MouseEventArgs args, FrameworkElement element) =>
            {
                // We need to know where the overlay anchor for the control is.
                MapPoint overlayAnchor = GeoView.GetViewOverlayAnchor(element);
                // Now that we know where the anchor is in terms of its 
                // geometry, we need to know where it is on the screen.
                Point overlayAnchorScreenPoint =
                this.mapView.LocationToScreen(overlayAnchor);
                // Now we want to know the mouse's position relative to the
                // control's parent window.
                Point mousePoint = args.GetPosition(Window.GetWindow(this.maptip));
                // What's the different between where the mouse is and the
                // control's anchor point?
                var mouseAnchorDiffVector =
                Point.Subtract(mousePoint, overlayAnchorScreenPoint);
                // From the moust-to-anchor difference vector, we can now 
                // determine the relative mouse point.
                var relativeMousePoint =
                new Point(mouseAnchorDiffVector.X, mouseAnchorDiffVector.Y);
                // That's what we return.
                return relativeMousePoint;
            };

            // When the mouse is pressed on the map tip...
            this.maptip.MouseDown += (sender, args) =>
            {
                // Get the start position based on where the user clicked and
                // the current offset of the map tip so we know where we're
                // starting from.
                dragMaptipStart = getRelativeMousePosition(args, this.maptip);
                // The starting offset is the map tip's translation.
                dragMaptipOffset =
                new Vector(
                    mapTipXlateTransform.X,
                    mapTipXlateTransform.Y);
                this.maptip.CaptureMouse();
            };

            // When the mouse is moved on the map tip...
            this.maptip.MouseMove += (sender, args) =>
            {
                // If we're not interested, do nothing.
                if (!this.maptip.IsMouseCaptured) return;
                // Get the current position of the mouse relative to the
                // map tip's translation.
                Point relativeMousePosition = getRelativeMousePosition(args, this.maptip);
                Vector offset = Point.Subtract(relativeMousePosition, dragMaptipStart);
                // Update the translation.
                mapTipXlateTransform.X = dragMaptipStart.X + offset.X;
                mapTipXlateTransform.Y = dragMaptipStart.Y + offset.Y;

                #region Adjust the map tip connector.
                this.mapTipConnector.X2 = dragMaptipStart.X + offset.X;
                this.mapTipConnector.Y2 = dragMaptipStart.Y + offset.Y;
                #endregion
            };

            // When the mouse goes up...
            this.maptip.MouseUp += (sender, args) =>
            {
                this.maptip.ReleaseMouseCapture();
            };

            #endregion

            #region Displaying Layer Attribution
            // Retrieve the base layer.  (This probably isn't how you're
            // defining and accessing the base layer for your purposes, but
            // for demonstration...
            var baseLayer = this.mapView.Map.Basemap.BaseLayers[0];
            // We should wait until the layer is loaded to grab its
            // attribution values.
            baseLayer.Loaded += (sender, args) =>
            {
                // The map will likely be loaded in another thread, so we
                // need to invoke.
                this.Dispatcher.Invoke(() =>
                {
                    // Set the relevant properties on the control.
                    this.attributionTextBlock.Text = baseLayer.Attribution;
                    this.attributionTextBlock.ToolTip = baseLayer.Attribution;
                });
            };

            #endregion
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
            var identifyResults = await this.mapView.IdentifyGraphicsOverlayAsync(
                this.graphicsOverlay,
                e.Position,
                tolerance,
                false);

            // If the user didn't click on anything...
            if (identifyResults.Graphics.Count == 0)
            {
                // Hide the map tip.
                this.maptip.Visibility = Visibility.Hidden;
                this.mapTipConnector.Visibility = Visibility.Hidden;
                return;
            }
            // We got a hit? Great!
            var hitGraphic = identifyResults.Graphics[0];
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

            // Bring the map tip back to the point.
            var xlateTransform = this.maptip.RenderTransform as TranslateTransform;
            if (xlateTransform != null)
            {
                xlateTransform.X = 0;
                xlateTransform.Y = 0;
            }

            #region Reset the map tip connector.
            // Make it visible.
            this.mapTipConnector.Visibility = Visibility.Visible;
            // Anchor it to the graphic's geometry.
            GeoView.SetViewOverlayAnchor(this.mapTipConnector, (MapPoint)hitGraphic.Geometry);
            // Shrink the map tip connector line so that it's hidden by the
            // map tip.
            this.mapTipConnector.X2 = 1;
            this.mapTipConnector.Y2 = 1;
            #endregion

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


        #region MapOverlay Adjustment

        /// <summary>
        /// This flag tells us whether we're currently performing an adjustment.
        /// </summary>
        private bool adjusting = false;

        /// <summary>
        /// In this handler we demonstrate a rough workaround to try to bring
        /// the map overlay back into alignment with the map when the
        /// map view changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // The logic gets confused when the map view size changes.  
            // We'll need to figure this out, but for now...
            if (this.adjusting)
            {
                return;
            }

            // If we're in the middle of an adjustment...
            if (this.adjusting)
            {
                // ...do nothing.
                return;
            }
            // We're now in the middle of an adjustment.
            this.adjusting = true;
            // Get the current rotation from the map view.
            double originalRotation = this.mapView.MapRotation;
            // If the original rotation is less than half a degree...
            if (originalRotation < 1)
            {
                // ...let's consider it to be at zero.
                originalRotation = 0.0;
            }
            // The new rotation is the old rotation, plus 1-1/2 degrees.
            double newRotation = originalRotation + 1.5;
            // Rotate to the new rotation.
            await this.mapView.SetViewpointRotationAsync(newRotation);
            // We seem to need to wait for a moment.
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(100);
            });
            // Now rotate back.
            await this.mapView.SetViewpointRotationAsync(originalRotation);
            // We're no longer making the adjustment.
            this.adjusting = false;

        }
        #endregion
    }
}
