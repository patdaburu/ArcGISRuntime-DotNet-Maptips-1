# ArcGISRuntime-DotNet-Maptips-1
A simple demonstration of the use of "maptips" with the ArcGISRuntime for .NET (WPF) Quartz Beta.

To place a map tip, do the following:

1. Double-click to place a marker.  (It appears as a red dot.)
2. Click the red dot to see the map tip.
3. You can drag the map tip control around the map.  It should remain connected to the red dot by an orange line.

It's not pretty, but it should serve to get the point across.

## February 23, 2017 - MapOverlay Bug
In its current state, this project demonstrates a potential workaround for an odd behavior of the MapView in which a map overlay does realign with map reliably after the map view control is resized.

The "fix" rotates the map just a bit, then rotates it back to its original position in the hope that the map overlay will be reset in the process.

To reproduce the error, remove the code in the `Mapview_SizeChanged` event handler in [MainWindow.xaml.cs](blob/master/MainWindow.xaml.cs>) file.
