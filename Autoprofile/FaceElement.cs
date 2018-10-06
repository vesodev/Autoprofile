using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;

namespace WPFPsi
{
    class FaceElement : DrawingVisual
    {
        #region Data Fields

        bool selected;
        const double HitTestWidth = 8.0;
        const double HandleSize = 12.0;
        double lineWidth;
        Color objColor;
        Brush fillBrush;
        string sElementInfo;
        string sMeaning;

        double rectangleLeft;
        double rectangleTop;
        double rectangleRight;
        double rectangleBottom;

        // Objects used to draw handles
        // external handle brush
        static SolidColorBrush handleBrush1 = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        // middle
        static SolidColorBrush handleBrush2 = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        // internal
        static SolidColorBrush handleBrush3 = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));

        #endregion

        #region Properties

        public bool IsSelected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                RefreshDrawing();
            }
        }

        public int HandleCount
        {
            get { return 8; }
        }

        public Rect Rectangle
        {
            get
            {
                double l, t, w, h;

                if (rectangleLeft <= rectangleRight)
                {
                    l = rectangleLeft;
                    w = rectangleRight - rectangleLeft;
                }
                else
                {
                    l = rectangleRight;
                    w = rectangleLeft - rectangleRight;
                }

                if (rectangleTop <= rectangleBottom)
                {
                    t = rectangleTop;
                    h = rectangleBottom - rectangleTop;
                }
                else
                {
                    t = rectangleBottom;
                    h = rectangleTop - rectangleBottom;
                }

                return new Rect(l, t, w, h);
            }
        }

        public double Left
        {
            get { return rectangleLeft; }
            set { rectangleLeft = value; }
        }

        public double Top
        {
            get { return rectangleTop; }
            set { rectangleTop = value; }
        }

        public double Right
        {
            get { return rectangleRight; }
            set { rectangleRight = value; }
        }

        public double Bottom
        {
            get { return rectangleBottom; }
            set { rectangleBottom = value; }
        }

        public Brush Fill
        {
            get { return fillBrush; }
            set
            {
                fillBrush = value;
                RefreshDrawing();
            }
        }

        public string Info
        {
            get { return sElementInfo; }
        }

        public string Meaning
        {
            get { return sMeaning; }
            set { sMeaning = value; }
        }

        public Color LineColor
        {
            get { return objColor; }
            set { objColor = value; }
        }

        #endregion

        #region Methods

        public Point GetHandle(int handleNumber)
        {
            double x, y, xCenter, yCenter;

            xCenter = (rectangleRight + rectangleLeft) / 2;
            yCenter = (rectangleBottom + rectangleTop) / 2;
            x = rectangleLeft;
            y = rectangleTop;

            switch (handleNumber)
            {
                case 1:
                    x = rectangleLeft;
                    y = rectangleTop;
                    break;
                case 2:
                    x = xCenter;
                    y = rectangleTop;
                    break;
                case 3:
                    x = rectangleRight;
                    y = rectangleTop;
                    break;
                case 4:
                    x = rectangleRight;
                    y = yCenter;
                    break;
                case 5:
                    x = rectangleRight;
                    y = rectangleBottom;
                    break;
                case 6:
                    x = xCenter;
                    y = rectangleBottom;
                    break;
                case 7:
                    x = rectangleLeft;
                    y = rectangleBottom;
                    break;
                case 8:
                    x = rectangleLeft;
                    y = yCenter;
                    break;
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        public int MakeHitTest(Point point)
        {
            if (IsSelected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (Contains(point))
                return 0;

            return -1;
        }

        public Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return HelperFunctions.DefaultCursor;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        public void MoveHandleTo(Point point, int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    rectangleLeft = point.X;
                    rectangleTop = point.Y;
                    break;
                case 2:
                    rectangleTop = point.Y;
                    break;
                case 3:
                    rectangleRight = point.X;
                    rectangleTop = point.Y;
                    break;
                case 4:
                    rectangleRight = point.X;
                    break;
                case 5:
                    rectangleRight = point.X;
                    rectangleBottom = point.Y;
                    break;
                case 6:
                    rectangleBottom = point.Y;
                    break;
                case 7:
                    rectangleLeft = point.X;
                    rectangleBottom = point.Y;
                    break;
                case 8:
                    rectangleLeft = point.X;
                    break;
            }

            RefreshDrawing();
        }

        /// <summary>
        /// Move object
        /// </summary>
        public void Move(double deltaX, double deltaY)
        {
            rectangleLeft += deltaX;
            rectangleRight += deltaX;

            rectangleTop += deltaY;
            rectangleBottom += deltaY;

            RefreshDrawing();
        }

        public bool Contains(Point point)
        {
            return this.Rectangle.Contains(point);
        }

        /// <summary>
        /// Draw tracker rectangle
        /// </summary>
        static void DrawTrackerRectangle(DrawingContext drawingContext, Rect rectangle)
        {
            // External rectangle
            drawingContext.DrawRectangle(handleBrush1, null, rectangle);

            // Middle
            drawingContext.DrawRectangle(handleBrush2, null,
                new Rect(rectangle.Left + rectangle.Width / 8,
                         rectangle.Top + rectangle.Height / 8,
                         rectangle.Width * 6 / 8,
                         rectangle.Height * 6 / 8));

            // Internal
            drawingContext.DrawRectangle(handleBrush3, null,
                new Rect(rectangle.Left + rectangle.Width / 4,
                 rectangle.Top + rectangle.Height / 4,
                 rectangle.Width / 2,
                 rectangle.Height / 2));
        }

        /// <summary>
        /// Refresh drawing.
        /// Called after change occurs
        /// </summary>
        public void RefreshDrawing()
        {
            DrawingContext dc = this.RenderOpen();

            Draw(dc);

            dc.Close();
        }

        /// <summary>
        /// Get handle rectangle by 1-based number
        /// </summary>
        public Rect GetHandleRectangle(int handleNumber)
        {
            Point point = GetHandle(handleNumber);

            return new Rect(point.X - HandleSize / 2, point.Y - HandleSize / 2,
                HandleSize, HandleSize);
        }

        public void Draw(DrawingContext drawingContext)
        {
            if (IsSelected)
            {
                DrawTracker(drawingContext);
            }
            drawingContext.DrawRectangle(
                fillBrush,
                new Pen(new SolidColorBrush(objColor), lineWidth),
                Rectangle);
        }

        public void DrawTracker(DrawingContext drawingContext)
        {
            for (int i = 1; i <= HandleCount; i++)
            {
                DrawTrackerRectangle(drawingContext, GetHandleRectangle(i));
            }
        }

        #endregion

        #region Constructors

        public FaceElement(double left, double top, double right, double bottom,
            double lineWidth, Color objectColor, Brush fillBrush)
        {
            this.rectangleLeft = left;
            this.rectangleTop = top;
            this.rectangleRight = right;
            this.rectangleBottom = bottom;
            this.lineWidth = lineWidth;
            this.objColor = objectColor;
            this.fillBrush = fillBrush;
        }

        public FaceElement(double left, double top, double right, double bottom,
            double lineWidth, Color objectColor, Brush fillBrush, string info, string meaning)
        {
            this.rectangleLeft = left;
            this.rectangleTop = top;
            this.rectangleRight = right;
            this.rectangleBottom = bottom;
            this.lineWidth = lineWidth;
            this.objColor = objectColor;
            this.fillBrush = fillBrush;
            this.sElementInfo = info;
            this.sMeaning = meaning;
        }

        #endregion
    }
}