using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;
using Drawing;

namespace WPFPsi
{
    static class HelperFunctions
    {
        public static Cursor DefaultCursor
        {
            get
            {
                return Cursors.Arrow;
            }
        }

        public static void SelectAll(DrawingCanvas drawingCanvas)
        {
            for (int i = 0; i < drawingCanvas.Count; i++)
            {
                drawingCanvas[i].IsSelected = true;
            }
        }

        public static void UnselectAll(DrawingCanvas drawingCanvas)
        {
            for (int i = 0; i < drawingCanvas.Count; i++)
            {
                drawingCanvas[i].IsSelected = false;
                drawingCanvas[i].LineColor = Colors.Transparent;
            }
        }

        public static void CollapseAllExpanders(Panel containingPanel)
        {
            StackPanel panel = (StackPanel)containingPanel;
            foreach (object o in panel.Children)
            {
                if (o is Expander)
                {
                    ((Expander)o).IsExpanded = false;
                    ((Expander)o).Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}