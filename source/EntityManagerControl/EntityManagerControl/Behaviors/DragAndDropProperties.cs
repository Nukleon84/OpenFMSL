using System;
using System.Windows;
using System.Windows.Input;
using OpenFMSL.Contracts.Entities;
using EntityManagerControl.ViewModels;

namespace EntityManagerControl.Behaviors
{
    public static class DragAndDropProperties
    {
        #region EnabledForDrag

        public static readonly DependencyProperty EnabledForDragProperty =
            DependencyProperty.RegisterAttached("EnabledForDrag", typeof(bool), typeof(DragAndDropProperties),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnEnabledForDragChanged)));

        public static bool GetEnabledForDrag(DependencyObject d)
        {
            return (bool)d.GetValue(EnabledForDragProperty);
        }

        public static void SetEnabledForDrag(DependencyObject d, bool value)
        {
            d.SetValue(EnabledForDragProperty, value);
        }

        private static void OnEnabledForDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)d;


            if ((bool)e.NewValue)
            {
                fe.PreviewMouseDown += Fe_PreviewMouseDown;
                fe.MouseMove += Fe_MouseMove;
            }
            else
            {
                fe.PreviewMouseDown -= Fe_PreviewMouseDown;
                fe.MouseMove -= Fe_MouseMove;
            }
        }
        #endregion

        #region DragStartPoint

        public static readonly DependencyProperty DragStartPointProperty =
            DependencyProperty.RegisterAttached("DragStartPoint", typeof(Point?), typeof(DragAndDropProperties));

        public static Point? GetDragStartPoint(DependencyObject d)
        {
            return (Point?)d.GetValue(DragStartPointProperty);
        }


        public static void SetDragStartPoint(DependencyObject d, Point? value)
        {
            d.SetValue(DragStartPointProperty, value);
        }

        #endregion

        static void Fe_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point? dragStartPoint = GetDragStartPoint((DependencyObject)sender);

            if (e.LeftButton != MouseButtonState.Pressed)
                dragStartPoint = null;

            if (dragStartPoint.HasValue)
            {
                // DragObject dataObject = new DragObject();
                //dataObject.ContentType = (((FrameworkElement)sender).DataContext as ToolBoxData).Type;
                // dataObject.DesiredSize = new Size(65, 65);
                var rawObject = ((FrameworkElement)sender).DataContext as EntityViewModel;
                if (rawObject != null && rawObject.IsRenaming)
                {
                    e.Handled = true;
                    return;
                }

                var objectToDrag = new DataObject("myDragDropFormat", ((FrameworkElement)sender).DataContext);

                try
                {
                    DragDrop.DoDragDrop((DependencyObject)sender, objectToDrag, DragDropEffects.Copy);
                }
                catch (Exception ex)
                {
                    e.Handled = false;

                    System.Windows.MessageBox.Show("A drag/drop error occured. Error =" + ex.Message);
                }
                e.Handled = true;
            }
        }

        static void Fe_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetDragStartPoint((DependencyObject)sender, e.GetPosition((IInputElement)sender));
        }
    }
}
