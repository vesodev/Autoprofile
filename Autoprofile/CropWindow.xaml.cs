using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Drawing;

namespace WPFPsi
{
    /// <summary>
    /// Interaction logic for CropWindow.xaml
    /// </summary>
    public partial class CropWindow : Window
    {
        enum SelectionMode
        {
            None,
            Move,
            Resize
        }

        SelectionMode selectMode = SelectionMode.None;
        FaceElement resizedObject;
        int resizedObjectHandle;
        Point lastPoint = new Point(0, 0);

        public CropWindow()
        {
            InitializeComponent();
            cropBtn.IsChecked = true;
            btnOK.IsEnabled = false;
            btnCancel.IsEnabled = false;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point ptClicked = e.GetPosition(Canvas);

            selectMode = SelectionMode.None;
            FaceElement o;
            FaceElement movedObject = null;
            int handleNumber;

            if (cropBtn.IsChecked == true)
            {
                AddNewObject(Canvas, new FaceElement(ptClicked.X, ptClicked.Y, ptClicked.X + 1, ptClicked.Y + 1, 2, Colors.Blue, null));
                cropBtn.IsChecked = false;
            }

            // Test for resizing (only if control is selected, cursor is on the handle)
            for (int i = Canvas.ElementsList.Count - 1; i >= 0; i--)
            {
                o = Canvas[i];

                if (o.IsSelected)
                {
                    handleNumber = o.MakeHitTest(ptClicked);

                    if (handleNumber > 0)
                    {
                        selectMode = SelectionMode.Resize;

                        // keep resized object in class member
                        resizedObject = o;
                        resizedObjectHandle = handleNumber;

                        o.IsSelected = true;

                        break;
                    }
                }
            }

            // Test for move (cursor is on the object)
            if (selectMode == SelectionMode.None)
            {
                for (int i = Canvas.ElementsList.Count - 1; i >= 0; i--)
                {
                    o = Canvas[i];

                    if (o.MakeHitTest(ptClicked) == 0)
                    {
                        movedObject = o;
                        break;
                    }
                }

                if (movedObject != null)
                {
                    selectMode = SelectionMode.Move;

                    // Select clicked object
                    movedObject.IsSelected = true;

                    // Set move cursor
                    Canvas.Cursor = Cursors.SizeAll;
                }
            }

            lastPoint = ptClicked;

            // Capture mouse until MouseUp event is received
            Canvas.CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Exclude all cases except left button on/off.
            if (e.MiddleButton == MouseButtonState.Pressed ||
                 e.RightButton == MouseButtonState.Pressed)
            {
                Canvas.Cursor = HelperFunctions.DefaultCursor;
                return;
            }

            Point point = e.GetPosition(Canvas);

            // Set cursor when left button is not pressed
            if (e.LeftButton == MouseButtonState.Released)
            {
                Cursor cursor = null;

                for (int i = 0; i < Canvas.Count; i++)
                {
                    int n = Canvas[i].MakeHitTest(point);

                    if (n > 0)
                    {
                        cursor = Canvas[i].GetHandleCursor(n);
                        break;
                    }
                }

                if (cursor == null)
                    cursor = HelperFunctions.DefaultCursor;

                Canvas.Cursor = cursor;

                return;

            }

            if (!Canvas.IsMouseCaptured)
            {
                return;
            }

            // Find difference between previous and current position
            double dx = point.X - lastPoint.X;
            double dy = point.Y - lastPoint.Y;

            lastPoint = point;

            // Resize
            if (selectMode == SelectionMode.Resize)
            {
                if (resizedObject != null)
                {
                    resizedObject.MoveHandleTo(point, resizedObjectHandle);
                }
            }

            // Move
            if (selectMode == SelectionMode.Move)
            {
                foreach (FaceElement f in Canvas.Selection)
                {
                    f.Move(dx, dy);
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!Canvas.IsMouseCaptured)
            {
                Canvas.Cursor = HelperFunctions.DefaultCursor;
                selectMode = SelectionMode.None;
                return;
            }

            if (resizedObject != null)
            {
                resizedObject = null;
            }

            Canvas.ReleaseMouseCapture();
            Canvas.Cursor = HelperFunctions.DefaultCursor;
            selectMode = SelectionMode.None;
            btnOK.IsEnabled = true;
            btnCancel.IsEnabled = true;
            cropBtn.IsEnabled = false;
        }

        static void AddNewObject(DrawingCanvas drawingCanvas, FaceElement element)
        {
            element.IsSelected = true;
            drawingCanvas.ElementsList.Add(element);
            drawingCanvas.CaptureMouse();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Rect cropRect = Canvas[Canvas.Count - 1].Rectangle;
            double rcScaleX = 1.0;
            double rcScaleY = 1.0;
            ImageBrush theImage = (ImageBrush)Canvas.Background;
            BitmapSource bmSrc = (BitmapSource)theImage.ImageSource;
            rcScaleX = Canvas.ActualWidth / bmSrc.PixelWidth;
            rcScaleY = Canvas.ActualHeight / bmSrc.PixelHeight;
            cropRect.X /= rcScaleX;
            cropRect.Y /= rcScaleY;
            cropRect.Width /= rcScaleX;
            cropRect.Height /= rcScaleY;
            Int32Rect intCropRect = new Int32Rect((int)cropRect.Left, (int)cropRect.Top, (int)cropRect.Width, (int)cropRect.Height);
            CroppedBitmap croppedImage = new CroppedBitmap(bmSrc, intCropRect);
            theImage.ImageSource = croppedImage;
            Canvas.Width = croppedImage.Width;
            Canvas.Height = croppedImage.Height;
            Canvas.Background = theImage;
            Canvas.ElementsList.Clear();
            btnOK.IsEnabled = false;
            btnCancel.IsEnabled = false;
            cropBtn.IsEnabled = true;
            cropBtn.IsChecked = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Canvas.ElementsList.Clear();
            btnOK.IsEnabled = false;
            btnCancel.IsEnabled = false;
            cropBtn.IsEnabled = true;
            cropBtn.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 mainWnd = (Window1)Application.Current.MainWindow;
            ((ImageBrush)Canvas.Background).Stretch = Stretch.Uniform;
            mainWnd.drawingSurface.Background = Canvas.Background;
            mainWnd.drawingSurface.Opacity = 0.910;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ImageBrush theImage = (ImageBrush)Canvas.Background;
            BitmapSource bSource = (BitmapSource)theImage.ImageSource;
            double dScaleFactor = Canvas.ActualHeight / bSource.Height;
            TransformedBitmap source = new TransformedBitmap(bSource, new ScaleTransform(dScaleFactor, dScaleFactor));
            theImage.ImageSource = source;
            Canvas.Width = source.Width;
        }
    }
}