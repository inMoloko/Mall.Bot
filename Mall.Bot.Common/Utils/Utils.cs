using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using NLog;

namespace Moloko.Utils
{
    public static class Logging
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
    }

    public static class Tools
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
    }
    public static class Utils
    {
        static Random _random = null;
        public static Random Random
        {
            get
            {
                return _random == null? _random = new Random((int)DateTime.Now.Ticks): _random;
            }
        }

        public static Point GatPathCenterPoint(System.Windows.Shapes.Path path)
        {
            PathGeometry g = (path as System.Windows.Shapes.Path).Data.GetFlattenedPathGeometry();
            double minx, miny, maxx, maxy;
            double dx, dy;
            double x = 0, y = 0;
            dx = (double)path.GetValue(Canvas.LeftProperty);
            dy = (double)path.GetValue(Canvas.TopProperty);
            dx = double.IsNaN(dx) ? 0 : dx;
            dy = double.IsNaN(dy) ? 0 : dy;
            minx = miny = double.MaxValue;
            maxx = maxy = double.MinValue;

            foreach (var f in g.Figures)
                foreach (var s in f.Segments)
                    if (s is PolyLineSegment)
                        foreach (var pt in ((PolyLineSegment)s).Points)
                        {
                            x = pt.X + dx;
                            y = pt.Y + dy;
                            minx = x < minx ? x : minx;
                            miny = y < miny ? y : miny;
                            maxx = x > maxx ? x : maxx;
                            maxy = y > maxy ? y : maxy;
                        }

            return new Point(minx + (maxx - minx) / 2, miny + (maxy - miny) / 2);
        }



        public static void Foreach<T>(object parent, Action<object> action)
        {
            if (!(parent is DependencyObject) || action == null)
                return;
            var d = parent as DependencyObject;
            int childrenCount = VisualTreeHelper.GetChildrenCount(d);
            if (childrenCount > 0)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
                {
                    var child = VisualTreeHelper.GetChild(d, i);
                    Foreach<T>(child, action);
                    if (child is T)
                        action(child);

                }
            }
            else if (parent is ItemsControl)
            {
                ItemsControl ic = (ItemsControl)parent;
                foreach (var item in ic.Items)
                {
                    Foreach<T>(item, action);
                    if (item is T)
                        action(item);
                }

            }
            else if (parent is ContentControl)
            {
                ContentControl cc = (ContentControl)parent;
                Foreach<T>(cc.Content, action);
                if (cc.Content is T)
                    action(cc.Content);

            } if (parent is ContentPresenter)
            {
                ContentPresenter cp = (ContentPresenter)parent;
                Foreach<T>(cp.Content, action);
                if (cp.Content is T)
                    action(cp.Content);

            }
        }

        public static bool First<T>(object parent, Action<object> action)
        {
            if (!(parent is DependencyObject) || action == null)
                return false;
            var d = parent as DependencyObject;
            int childrenCount = VisualTreeHelper.GetChildrenCount(d);
            if (childrenCount > 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(d, i);
                    if (First<T>(child, action))
                        return true;
                    if (child is T)
                    {
                        action(child);
                        return true;
                    }

                }
            }
            else if (parent is ItemsControl)
            {
                ItemsControl ic = (ItemsControl)parent;
                foreach (var item in ic.Items)
                {
                    if (First<T>(item, action))
                        return true;
                    if (item is T)
                    {
                        action(item);
                        return true;
                    }
                }

            }
            else if (parent is ContentControl)
            {
                ContentControl cc = (ContentControl)parent;
                if (First<T>(cc.Content, action))
                    return true;
                if (cc.Content is T)
                {
                    action(cc.Content);
                    return true;
                }

            } if (parent is ContentPresenter)
            {
                ContentPresenter cp = (ContentPresenter)parent;
                if (First<T>(cp.Content, action))
                    return true;
                if (cp.Content is T)
                {
                    action(cp.Content);
                    return true;
                }

            }
            return false;
        }
        
    }
    

}
