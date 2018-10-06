using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Resources;
using System.Reflection;
using Microsoft.Win32;
using Drawing;

namespace WPFPsi
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        enum SelectionMode
        {
            None,
            Move,
            Resize
        }

        #region Data Fields        
        
        SelectionMode selectMode = SelectionMode.None;
        FaceElement resizedObject;
        int resizedObjectHandle;
        Point lastPoint = new Point(0, 0);

        #endregion

        #region Constructor
        public Window1()
        {            
            InitializeComponent();
        }
        #endregion

        #region Events
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpenImage = new OpenFileDialog();
            dlgOpenImage.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
            if (dlgOpenImage.ShowDialog() == true)
            {                
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(dlgOpenImage.FileName, UriKind.Relative));
                brush.Stretch = Stretch.None;                
                CropWindow cropWindow = new CropWindow();
                cropWindow.Canvas.Background = brush;
                cropWindow.Owner = this;
                cropWindow.ShowDialog();
            }
        }

        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            NavigationWindow navWnd = new NavigationWindow();
            navWnd.Content = new Autoprofile.Help_Window();
            navWnd.Width = 500;
            navWnd.Height = 350;
            navWnd.Owner = this;
            navWnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            navWnd.ShowDialog();
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point ptClicked = e.GetPosition(drawingSurface);
            
            selectMode = SelectionMode.None;
            FaceElement o;
            FaceElement movedObject = null;
            int handleNumber;

            // Test for resizing (only if control is selected, cursor is on the handle)
            for (int i = drawingSurface.ElementsList.Count - 1; i >= 0; i--)
            {
                o = drawingSurface[i];

                if (o.IsSelected)
                {
                    handleNumber = o.MakeHitTest(ptClicked);

                    if (handleNumber > 0)
                    {
                        selectMode = SelectionMode.Resize;

                        // keep resized object in class member
                        resizedObject = o;
                        resizedObjectHandle = handleNumber;

                        // Since we want to resize only one object, unselect all other objects
                        HelperFunctions.UnselectAll(drawingSurface);
                        o.IsSelected = true;

                        break;
                    }
                }
            }

            // Test for move (cursor is on the object)
            if (selectMode == SelectionMode.None)
            {
                for (int i = drawingSurface.ElementsList.Count - 1; i >= 0; i--)
                {
                    o = drawingSurface[i];

                    if (o.MakeHitTest(ptClicked) == 0)
                    {
                        movedObject = o;
                        break;
                    }
                }

                if (movedObject != null)
                {
                    selectMode = SelectionMode.Move;

                    // Unselect all if Ctrl is not pressed and clicked object is not selected yet
                    if (!movedObject.IsSelected)
                        HelperFunctions.UnselectAll(drawingSurface);

                    // Select clicked object
                    movedObject.IsSelected = true;

                    // Set move cursor
                    drawingSurface.Cursor = Cursors.SizeAll;                    
                }
            }

            lastPoint = ptClicked;

            // Capture mouse until MouseUp event is received
            drawingSurface.CaptureMouse();
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Exclude all cases except left button on/off.
            if (e.MiddleButton == MouseButtonState.Pressed ||
                 e.RightButton == MouseButtonState.Pressed)
            {
                drawingSurface.Cursor = HelperFunctions.DefaultCursor;
                return;
            }

            Point point = e.GetPosition(drawingSurface);

            // Set cursor when left button is not pressed
            if (e.LeftButton == MouseButtonState.Released)
            {
                Cursor cursor = null;

                for (int i = 0; i < drawingSurface.Count; i++)
                {
                    int n = drawingSurface[i].MakeHitTest(point);

                    if (n > 0)
                    {
                        cursor = drawingSurface[i].GetHandleCursor(n);
                        break;
                    }
                }

                if (cursor == null)
                    cursor = HelperFunctions.DefaultCursor;

                drawingSurface.Cursor = cursor;

                return;

            }

            if (!drawingSurface.IsMouseCaptured)
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
                foreach (FaceElement f in drawingSurface.Selection)
                {
                    f.Move(dx, dy);
                }
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!drawingSurface.IsMouseCaptured)
            {
                drawingSurface.Cursor = HelperFunctions.DefaultCursor;
                selectMode = SelectionMode.None;
                return;
            }

            if (resizedObject != null)
            {               
                resizedObject = null;
            }            

            drawingSurface.ReleaseMouseCapture();
            drawingSurface.Cursor = HelperFunctions.DefaultCursor;
            selectMode = SelectionMode.None;
        }
       
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem selecteditem =(ListViewItem)lstViewEyes.SelectedItem;
            FaceElement f = null;
            for (int i = 0; i < drawingSurface.Count; i++)
            {
                if (drawingSurface[i].Info == (string)lstViewEyes.Tag)
                {
                    f = drawingSurface[i];
                    f.Meaning = Autoprofile.Properties.Resources.ResourceManager.GetString("Eyes" + lstViewEyes.SelectedIndex);
                }
            }

            if (f != null)
            {
                Image selectedImage = (Image)selecteditem.Content;
                f.Fill = new ImageBrush(selectedImage.Source);
                HelperFunctions.UnselectAll(drawingSurface);
                f.IsSelected = true;
            }
            else
                AddNewObject(drawingSurface, new FaceElement(120, 120, 230, 210, 2, Colors.Blue, new ImageBrush(((Image)selecteditem.Content).Source), (string)lstViewEyes.Tag, Autoprofile.Properties.Resources.ResourceManager.GetString("Eyes" + lstViewEyes.SelectedIndex)));
            if (drawingSurface.Count == 5)
                OKBtn.IsEnabled = true;
        }

        private void lstViewEyebrows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem selecteditem = (ListViewItem)lstViewEyebrows.SelectedItem;
            FaceElement f = null;
            for (int i = 0; i < drawingSurface.Count; i++)
            {
                if (drawingSurface[i].Info == (string)lstViewEyebrows.Tag)
                {
                    f = drawingSurface[i];
                    f.Meaning = Autoprofile.Properties.Resources.ResourceManager.GetString("Eyebrows" + lstViewEyebrows.SelectedIndex);
                }
            }

            if (f != null)
            {
                Image selectedImage = (Image)selecteditem.Content;
                f.Fill = new ImageBrush(selectedImage.Source);
                HelperFunctions.UnselectAll(drawingSurface);
                f.IsSelected = true;
            }
            else
                AddNewObject(drawingSurface, new FaceElement(120, 120, 230, 210, 2, Colors.Blue, new ImageBrush(((Image)selecteditem.Content).Source), (string)lstViewEyebrows.Tag, Autoprofile.Properties.Resources.ResourceManager.GetString("Eyebrows" + lstViewEyebrows.SelectedIndex)));

            if (drawingSurface.Count == 5)
                OKBtn.IsEnabled = true;
        }

        #endregion

        #region Help Methods

        static void AddNewObject(DrawingCanvas drawingCanvas, FaceElement element)
        {
            HelperFunctions.UnselectAll(drawingCanvas);

            element.IsSelected = true;
            drawingCanvas.ElementsList.Add(element);
        }
        #endregion        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FaceElement f = null;
            string result = null;
            for (int i = 0; i < drawingSurface.Count; i++)
            {
                f = drawingSurface[i];
                result = result + f.Meaning + "\n" + "\n";
            }
            Autoprofile.Analysis_Window window = new Autoprofile.Analysis_Window();
            Run txtrun = new Run();
            txtrun.Text = result;
            window.report.Inlines.Add(txtrun);
            window.Owner = this;
            window.ShowDialog();
        }

        private void ellipse1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelperFunctions.CollapseAllExpanders(infoPanel);
            expEyes.Visibility = Visibility.Visible;
            expEyes.IsExpanded = true;
            drawingSurface.Opacity = 0.910;
        }

        private void ellipse5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelperFunctions.CollapseAllExpanders(infoPanel);
            expEyebrows.Visibility = Visibility.Visible;
            expEyebrows.IsExpanded = true;
            drawingSurface.Opacity = 0.910;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void lstViewNose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem selecteditem = (ListViewItem)lstViewNose.SelectedItem;
            FaceElement f = null;
            for (int i = 0; i < drawingSurface.Count; i++)
            {
                if (drawingSurface[i].Info == (string)lstViewNose.Tag)
                {
                    f = drawingSurface[i];
                    f.Meaning = Autoprofile.Properties.Resources.ResourceManager.GetString("Nose" + lstViewNose.SelectedIndex);
                }
            }

            if (f != null)
            {
                Image selectedImage = (Image)selecteditem.Content;
                f.Fill = new ImageBrush(selectedImage.Source);
                HelperFunctions.UnselectAll(drawingSurface);
                f.IsSelected = true;
            }
            else
                AddNewObject(drawingSurface, new FaceElement(120, 120, 230, 210, 2, Colors.Blue, new ImageBrush(((Image)selecteditem.Content).Source), (string)lstViewNose.Tag, Autoprofile.Properties.Resources.ResourceManager.GetString("Nose" + lstViewNose.SelectedIndex)));

            if (drawingSurface.Count == 5)
                OKBtn.IsEnabled = true;
        }

        private void lstViewLips_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem selecteditem = (ListViewItem)lstViewLips.SelectedItem;
            FaceElement f = null;
            for (int i = 0; i < drawingSurface.Count; i++)
            {
                if (drawingSurface[i].Info == (string)lstViewLips.Tag)
                {
                    f = drawingSurface[i];
                    f.Meaning = Autoprofile.Properties.Resources.ResourceManager.GetString("Lips" + lstViewLips.SelectedIndex);
                }
            }

            if (f != null)
            {
                Image selectedImage = (Image)selecteditem.Content;
                f.Fill = new ImageBrush(selectedImage.Source);
                HelperFunctions.UnselectAll(drawingSurface);
                f.IsSelected = true;
            }
            else
                AddNewObject(drawingSurface, new FaceElement(120, 120, 230, 210, 2, Colors.Blue, new ImageBrush(((Image)selecteditem.Content).Source), (string)lstViewLips.Tag, Autoprofile.Properties.Resources.ResourceManager.GetString("Lips" + lstViewLips.SelectedIndex)));

            if (drawingSurface.Count == 5)
                OKBtn.IsEnabled = true;
        }

        private void lstViewVdl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem selecteditem = (ListViewItem)lstViewVdl.SelectedItem;
            FaceElement f = null;
            for (int i = 0; i < drawingSurface.Count; i++)
            {
                if (drawingSurface[i].Info == (string)lstViewVdl.Tag)
                {
                    f = drawingSurface[i];
                    f.Meaning = Autoprofile.Properties.Resources.ResourceManager.GetString("Vdl" + lstViewVdl.SelectedIndex);
                }
            }

            if (f != null)
            {
                Image selectedImage = (Image)selecteditem.Content;
                f.Fill = new ImageBrush(selectedImage.Source);
                HelperFunctions.UnselectAll(drawingSurface);
                f.IsSelected = true;
            }
            else
                AddNewObject(drawingSurface, new FaceElement(120, 120, 230, 210, 2, Colors.Blue, new ImageBrush(((Image)selecteditem.Content).Source), (string)lstViewVdl.Tag, Autoprofile.Properties.Resources.ResourceManager.GetString("Vdl" + lstViewVdl.SelectedIndex)));

            if (drawingSurface.Count == 5)
                OKBtn.IsEnabled = true;
        }

        private void ellipse3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelperFunctions.CollapseAllExpanders(infoPanel);
            expLips.Visibility = Visibility.Visible;
            expLips.IsExpanded = true;
            drawingSurface.Opacity = 0.910;
        }

        private void ellipse6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelperFunctions.CollapseAllExpanders(infoPanel);
            expNose.Visibility = Visibility.Visible;
            expNose.IsExpanded = true;
            drawingSurface.Opacity = 0.910;
        }

        private void ellipse7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HelperFunctions.CollapseAllExpanders(infoPanel);
            expVdl.Visibility = Visibility.Visible;
            expVdl.IsExpanded = true;
            drawingSurface.Opacity = 0.910;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}