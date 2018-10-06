using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls;
using WPFPsi;

namespace Drawing
{
    /// <summary>
    /// Helper class
    /// </summary>
    public class DrawingCanvas : Canvas
    {
        #region Data Fields
        private VisualCollection visuals;
        #endregion

        #region Constructor
        public DrawingCanvas()
            : base()
        {
            visuals = new VisualCollection(this);
        }
        #endregion

        #region Overrides
        protected override int VisualChildrenCount
        {
            get
            {
                return visuals.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
        #endregion

        #region Methods
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);
        }

        public void DeleteVisual(Visual visual)
        {
            visuals.Remove(visual);
        }

        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            return hitResult.VisualHit as DrawingVisual;
        }
        #endregion

        #region Internal Properties

        internal VisualCollection ElementsList
        {
            get
            {
                return visuals;
            }
        }

        internal WPFPsi.FaceElement this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                    return (WPFPsi.FaceElement)visuals[index];

                return null;
            }
        }

        internal int Count
        {
            get
            {
                return visuals.Count;
            }
        }

        internal int SelectionCount
        {
            get
            {
                int n = 0;

                foreach (WPFPsi.FaceElement f in this.visuals)
                {
                    if (f.IsSelected)
                    {
                        n++;
                    }
                }

                return n;
            }
        }

        internal IEnumerable<FaceElement> Selection
        {
            get
            {
                foreach (FaceElement f in visuals)
                {
                    if (f.IsSelected)
                    {
                        yield return f;
                    }
                }
            }

        }
        #endregion
    }
}