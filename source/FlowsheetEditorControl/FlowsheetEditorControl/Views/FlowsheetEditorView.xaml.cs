using FlowsheetEditorControl.Items;
using FlowsheetEditorControl.ViewModels;
using Microsoft.Win32;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace FlowsheetEditorControl.Views
{
    /// <summary>
    /// Interaktionslogik für FlowsheetEditorView.xaml
    /// </summary>
    public partial class FlowsheetEditorView : UserControl, IFlowsheetEntityEditorView
    {

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;
        bool isConnecting = false;
        bool isMoving = false;
        double scale = 1;
        bool isSelecting = false; // Set to 'true' when mouse is held down.

        Point lastMousePosition;
        Point mouseClickPosition;

        private FlowsheetEditorViewModel _vm;
        

        private void resetVm(object sender, DependencyPropertyChangedEventArgs e)
        {
            var newVm = e.NewValue as FlowsheetEditorViewModel;
            if (newVm != null)
                _vm = newVm;
        }

        public FlowsheetEditorView()
        {
            this.DataContextChanged += resetVm;

            InitializeComponent();

            outerScrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            outerScrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            outerScrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;

            outerScrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

            outerScrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            outerScrollViewer.MouseLeftButtonDown += OnMouseLeftButtonDown;

            outerScrollViewer.PreviewMouseRightButtonDown += OnMouseRightButtonDown;
            outerScrollViewer.PreviewMouseRightButtonUp += OnMouseRightButtonUp;
            outerScrollViewer.MouseRightButtonUp += OnMouseRightButtonUp;
            outerScrollViewer.MouseMove += OnMouseMove;
        }

        public void Hide()
        {

        }

        public void Show()
        {

        }



        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(outerScrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                outerScrollViewer.ScrollToHorizontalOffset(outerScrollViewer.HorizontalOffset - dX);
                outerScrollViewer.ScrollToVerticalOffset(outerScrollViewer.VerticalOffset - dY);
            }

            if (isConnecting)
            {
                _vm.CurrentConnector.X = e.GetPosition(connectionCanvas).X;
                _vm.CurrentConnector.Y = e.GetPosition(connectionCanvas).Y;
            }

            var currentPoint = e.GetPosition(connectionCanvas);

            if (Mouse.LeftButton != MouseButtonState.Pressed)
                isSelecting = false;

            if (isSelecting && !isConnecting)
            {
                if (mouseClickPosition.X < currentPoint.X)
                {
                    Canvas.SetLeft(selectionBox, mouseClickPosition.X);
                    selectionBox.Width = currentPoint.X - mouseClickPosition.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, currentPoint.X);
                    selectionBox.Width = mouseClickPosition.X - currentPoint.X;
                }

                if (mouseClickPosition.Y < currentPoint.Y)
                {
                    Canvas.SetTop(selectionBox, mouseClickPosition.Y);
                    selectionBox.Height = currentPoint.Y - mouseClickPosition.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, currentPoint.Y);
                    selectionBox.Height = mouseClickPosition.Y - currentPoint.Y;
                }
                lastMousePosition = currentPoint;
            }


        }

        void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(outerScrollViewer);
            if (mousePos.X <= outerScrollViewer.ViewportWidth && mousePos.Y <
                outerScrollViewer.ViewportHeight)
            {
                outerScrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(outerScrollViewer);
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(connectionCanvas);

            if (e.Delta > 0)
            {
                scale *= 1.3;
            }
            if (e.Delta < 0)
            {
                scale *= 0.7;
            }

            if (scale < 0.2)
                scale = 0.2;

            if (scale > 3)
                scale = 3;
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;

            var centerOfViewport = new Point(outerScrollViewer.ViewportWidth / 2,
                                             outerScrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = outerScrollViewer.TranslatePoint(centerOfViewport, theGrid);



            e.Handled = true;
        }




        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /* var vm = (DataContext as ViewModel);

             if (!Keyboard.IsKeyDown(Key.LeftCtrl))
             {
                 foreach (var item in vm.Items)
                 {
                     item.IsSelected = false;
                 }
             }*/

            mouseClickPosition = e.GetPosition(connectionCanvas);

            Canvas.SetLeft(selectionBox, mouseClickPosition.X);
            Canvas.SetTop(selectionBox, mouseClickPosition.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
            isSelecting = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vm = (DataContext as FlowsheetEditorViewModel);

            foreach (var item in vm.Items)
            {
                if (item.IsSelected)
                {
                    if (vm.Flowsheet != null && vm.Flowsheet.SnapToGrid)
                    {
                        item.Y = Math.Round(item.Y / 10.0) * 10;
                        item.X = Math.Round(item.X / 10.0) * 10;
                    }

                }
            }
            if (isConnecting)
                abortConnect();

            isMoving = false;

            if (isSelecting)
            {
                isSelecting = false;
                selectionBox.Visibility = Visibility.Collapsed;
                Point mouseUpPos = e.GetPosition(selectionOverlay);

                foreach (var item in vm.Items)
                {
                    int x = (int)Canvas.GetLeft(selectionBox);
                    int y = (int)Canvas.GetTop(selectionBox);

                    if (item.X >= x && item.X + item.Width <= x + selectionBox.Width)
                    {
                        if (item.Y >= y && item.Y + item.Height <= y + selectionBox.Height)
                        {
                            item.IsSelected = true;

                        }
                    }
                }

                selectionBox.Width = 0;
                selectionBox.Height = 0;
            }


        }

        void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            outerScrollViewer.Cursor = Cursors.Arrow;
            outerScrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
            var vm = (DataContext as FlowsheetEditorViewModel);
            vm.CurrentObject = null;

            foreach (var item in vm.Items)
                item.IsSelected = false;

            foreach (var con in vm.Connections)
                con.IsSelected = false;

            //if (_aggregator != null)
            //  _aggregator.Raise(new SelectedObjectChangedMessage() { TimeStamp = DateTime.Now, Sender = sender, Parameter = _vm });

        }


        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(outerScrollViewer.ViewportWidth / 2,
                                                         outerScrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow =
                              outerScrollViewer.TranslatePoint(centerOfViewport, theGrid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(connectionCanvas);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / connectionCanvas.Width;
                    double multiplicatorY = e.ExtentHeight / connectionCanvas.Height;

                    double newOffsetX = outerScrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = outerScrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    outerScrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    outerScrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        public override string ToString()
        {
            return "Flowsheet";
        }

        private void itemPanel_MouseMove(object sender, MouseEventArgs e)
        {
            var vm = (DataContext as FlowsheetEditorViewModel);

            if (vm != null)
            {
                var currentPoint = e.GetPosition(outerScrollViewer);

                if (isMoving)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {


                        if (currentPoint != null)
                            foreach (var item in vm.Items)
                            {
                                if (item.IsSelected)
                                {
                                    var deltaX = currentPoint.X - lastMousePosition.X;
                                    var deltaY = currentPoint.Y - lastMousePosition.Y;

                                    item.Y += deltaY / scale;
                                    item.X += deltaX / scale;

                                }
                            }



                    }
                }
                lastMousePosition = currentPoint;

            }
        }
        private void streamLine_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var node = (e.OriginalSource as FrameworkElement).DataContext as Connection;
            var vm = (DataContext as FlowsheetEditorViewModel);

            if (e.ClickCount == 2)
            {
                if (node != null)
                {
                    //  var msg = new FlowsheetConnectionDoubleClickedMessage { Sender = vm, Parameter = node };
                    //   vm.RaiseStreamDetailsRequested(msg);
                    var inspector = new ModelInspectorView();
                    inspector.DataContext = new ModelInspectorViewModel(node.Model);
                    var window = new Window();
                    window.Content = inspector;
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    window.Width = 550;
                    window.Height = 400;
                    window.ShowActivated = true;
                    window.Title = node.Name;
                    window.WindowStyle = WindowStyle.ToolWindow;
                    window.Owner = Application.Current.MainWindow;
                    window.Show();


                }
                e.Handled = false;
            }
        }

        private void itemPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var node = (e.OriginalSource as FrameworkElement).DataContext as DrawableItem;
            var vm = (DataContext as FlowsheetEditorViewModel);

            if (e.ClickCount == 1)
            {
                if (node != null)
                {
                    if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        foreach (var item in vm.Items)
                            item.IsSelected = false;
                    }

                    node.IsSelected = true;
                    vm.CurrentObject = node;
                    // if (_aggregator != null)
                    //    _aggregator.Raise(new SelectedObjectChangedMessage() { TimeStamp = DateTime.Now, Sender = sender, Parameter = node });

                }
                e.Handled = false;
                _vm.CurrentObject = node;
                isSelecting = false;
                isMoving = true;
                //propertyGrid.SelectedObject = node;
            }

            if (e.ClickCount == 2)
            {
                if (node != null)
                {
                    var unit = node as VisualUnit;

                    if (unit != null)
                    {

                        var model = unit.Model as FlowsheetObject;

                        if (model != null)
                        {
                            // var msg = new FlowsheetElementDoubleClickedMessage { Sender = vm, Parameter = unit };
                            // vm.RaiseElementDetailsRequested(msg);
                            var inspector = new ModelInspectorView();
                            inspector.DataContext = new ModelInspectorViewModel(model);
                            var window = new Window();
                            window.Content = inspector;
                            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            window.Width = 550;
                            window.Height = 400;
                            window.ShowActivated = true;
                            window.Title = unit.Name + "["+ model.Class+"]" ;
                            window.WindowStyle = WindowStyle.ToolWindow;
                            window.Owner = Application.Current.MainWindow;
                            window.Show();
                        }
                    }

                }
                e.Handled = false;

                //propertyGrid.SelectedObject = node;
            }
        }

        #region Connecting
        private void connectorIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = (DataContext as FlowsheetEditorViewModel);

            if (vm != null)
            {
                var con = (e.OriginalSource as FrameworkElement).DataContext as Connector;

                if (!con.IsConnected)
                {
                    vm.NewSource = con;

                    vm.CurrentConnector = new Connector();
                    vm.CurrentConnector.X = e.GetPosition(connectionCanvas).X;
                    vm.CurrentConnector.Y = e.GetPosition(connectionCanvas).Y;

                    vm.CurrentConnection = new Connection { Source = vm.NewSource, Sink = vm.CurrentConnector };

                    vm.Connections.Add(vm.CurrentConnection);

                    if (vm.CurrentObject != null)
                    {
                        vm.CurrentObject.IsSelected = false;
                        vm.CurrentObject = null;
                    }
                    isConnecting = true;
                    e.Handled = true;
                }

            }
        }


        private void connectorIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = (DataContext as FlowsheetEditorViewModel);

            if (vm != null)
            {
                var con = (e.OriginalSource as FrameworkElement).DataContext as Connector;

                vm.NewSink = con;

                if (vm.NewSink != null && vm.NewSource != null && vm.NewSink != vm.NewSource)
                {

                    if (!vm.NewSource.Type.Contains(vm.NewSink.Type))
                    {
                        MessageBox.Show("Datatypes of input and outputs have to match!");
                        vm.NewSource = null;
                        vm.NewSink = null;
                        abortConnect();
                        return;
                    }

                    //if ((vm.NewSource.Owner.DisplayIcon == IconTypes.Stream && vm.NewSink.Owner.DisplayIcon != IconTypes.Stream) || (vm.NewSource.Owner.DisplayIcon != IconTypes.Stream && vm.NewSink.Owner.DisplayIcon == IconTypes.Stream))
                    {
                        vm.NewSink.IsConnected = true;
                        vm.NewSource.IsConnected = true;
                        vm.AddConnection(new Connection { Source = vm.NewSource, Sink = vm.NewSink });
                        vm.NewSource = null;
                        vm.NewSink = null;
                        abortConnect();
                    }
                    /*else
                    {
                        double x, y;
                        x = (vm.NewSource.Owner.X + vm.NewSink.Owner.X) / 2.0;
                        y = (vm.NewSource.Owner.Y + vm.NewSink.Owner.Y) / 2.0;
                        var streamCounter = vm.Items.Count(i => i.DisplayIcon == IconTypes.Stream);
                        var newStream = ItemFactory.Create(ModelTypes.MaterialStream, x, y, null);
                        newStream.Name = "S" + (streamCounter + 1).ToString("000");

                        vm.AddUnit(newStream);
                        vm.AddConnection(new Connection { Source = vm.NewSource, Sink = newStream.Connectors[0] });
                        vm.AddConnection(new Connection { Source = newStream.Connectors[0], Sink = vm.NewSink });

                        abortConnect();
                    }*/

                }

            }
        }

        void abortConnect()
        {
            var vm = (DataContext as FlowsheetEditorViewModel);

            if (vm != null && vm.CurrentConnection != null)
            {

                vm.CurrentConnection.Source.IsConnected = false;
                vm.CurrentConnection.Sink.IsConnected = false;
                vm.CurrentConnection.Source = null;
                vm.CurrentConnection.Sink = null;
                vm.Connections.Remove(vm.CurrentConnection);
                vm.CurrentConnection = null;

                vm.CurrentConnector = null;
                isConnecting = false;

            }
        }
        #endregion

        #region DragDrop Support
        private void theGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myDragDropFormat") ||
                   sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void theGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myDragDropFormat"))
            {
                var toolboxItem = e.Data.GetData("myDragDropFormat") as ToolboxItem;
                if (toolboxItem != null)
                {
                    Point position = e.GetPosition(connectionCanvas);
                    /*var item = ItemFactory.Create(toolboxItem.ModelName, position.X, position.Y, null);
                    if (item != null)
                    {
                        int i = 1;
                        var existingUnitNames = _vm.Items.Select(c => c.Name);
                        var baseName = item.Name;

                        do
                        {
                            item.Name = baseName.Replace("01", i.ToString("00"));
                            i++;

                        } while (existingUnitNames.Contains(item.Name));
                        
                        _vm.AddUnit(item);                     
                    }*/

                }
                
            }
        }
        #endregion

        private void theGrid_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = this.DataContext as FlowsheetEditorViewModel;
            if (vm != null)
            {
                if (e.Key == Key.Delete)
                {

                    foreach (var unit in vm.SelectedItems)
                    {
                        vm.RemoveItem(unit);
                    }

                    foreach (var con in vm.SelectedConnections)
                    {
                        vm.RemoveConnection(con);
                    }
                    // var deleteAction = new Actions.DeleteNodesAction(vm, selectedUnits);
                    //  MessageLib.MessageSystem.GetDispatcher().Broadcast(new PerformUndoableActionMessage(this, deleteAction));


                }

            }
        }

        static bool ShowSaveFileDialog(string extension, string filter, out string path)
        {
            path = "";
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = extension;
            dlg.Filter = filter;
            dlg.FileName = "flowsheet." + extension;
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                path = dlg.FileName;
                return true;
            }
            return false;
        }


        private void ExportFlowsheetImage()
        {
            string filename;
            try
            {
                if (ShowSaveFileDialog("png", "PNG files|*.png", out filename))
                {
                    CreateSaveBitmap(theGrid, filename);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error");
            }
        }

        private void CreateSaveBitmap(FrameworkElement view, string filename)
        {
            Size size = new Size(view.ActualWidth, view.ActualHeight);
            if (size.IsEmpty)
                return;

            RenderTargetBitmap result = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingvisual = new DrawingVisual();
            using (DrawingContext context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(view), null, new Rect(new Point(), size));
                context.Close();
            }

            result.Render(drawingvisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(result));

            using (var stream = File.Create(filename))
            {
                encoder.Save(stream);
            }
                      


        }

        private void ExportFlowsheetImage_Click(object sender, RoutedEventArgs e)
        {
            ExportFlowsheetImage();
        }
    }
}
