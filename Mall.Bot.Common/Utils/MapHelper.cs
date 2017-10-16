using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Moloko.Utils
{
    public class MapHelper
    {

        //.const.public const 

        public static double dot(Vector u, Vector v)
        {
            return u.X*v.X+ u.Y*v.Y;
        }

        public static double norm(Vector v)
        {
            return Math.Sqrt(dot(v,v));
        }

        public static double Distance(Vector u, Vector v)
        {
            return norm(u-v);
        }

        public static double Distance(Point p0, Point p1)
        {
            return Distance((Vector)p0, (Vector)p1);
        }

        /// <summary>
        /// Расстояние от точки до отрезка
        /// http://algolist.manual.ru/maths/geom/distance/pointline.php
        /// </summary>        
        public static Point ClosestSegmentPoint(Point point, Point segmentPoint0, Point segmentPoint1)
        {
             Vector v = segmentPoint1 - segmentPoint0;
             Vector w = point - segmentPoint0;

             double c1 = dot(w, v);
             if (c1 <= 0)
                 return segmentPoint0;

             double c2 = dot(v, v);
             if (c2 <= c1)
                 return segmentPoint1;

             double b = c1 / c2;
             Point Pb = segmentPoint0 + b * v;
             return Pb;
        }



        public static List<Vertex> FindPathVertex(Point destPoint, int destPointLayerID, Graph graph, bool isShortWaysUsed = false)
        {
            //Ищем точку на графе, ближайшую к destPoint на том же layer, что и она
            Point ближайшаяPointГрафа = new Point(0, 0);
            var minРасстояниеДоГрафа = double.MaxValue;
            Segment ближайшийSegment = null;
            var destPointLayer = graph.GetLayer(destPointLayerID);
            foreach (var segment in destPointLayer.Segments.Where(e=>e.Vertex0.Layer == e.Vertex1.Layer))
            {
                var point = ClosestSegmentPoint(destPoint, segment.Vertex0.Point, segment.Vertex1.Point);
                var dist = Distance(point, destPoint);
                if (minРасстояниеДоГрафа > dist)
                {
                    ближайшаяPointГрафа = point;
                    minРасстояниеДоГрафа = dist;
                    ближайшийSegment = segment;
                }
            }

            if (ближайшийSegment == null)
                return null;

            Vertex destVertex = null;
            Vertex ближайшаяVertexГрафа = null;

            //Надо проверить, что при поиске точки не совпали с существующими в графе
            var needDeleteDestVertex = (destVertex = destPointLayer.GetVertex(destPoint)) == null;
            var needDeleteБлижайшаяVertexГрафа = (ближайшаяVertexГрафа = destPointLayer.GetVertex(ближайшаяPointГрафа)) == null;
            
            try
            {
                if (needDeleteDestVertex)
                {
                    destVertex = destPointLayer.AddVertex(destPoint);
                    ближайшаяVertexГрафа = destPointLayer.AddVertex(ближайшаяPointГрафа);

                    destPointLayer.AddSegment(destPoint, ближайшаяPointГрафа);
                    if (needDeleteБлижайшаяVertexГрафа)
                    {
                        destPointLayer.AddSegment(ближайшаяPointГрафа, ближайшийSegment.Vertex0.Point);
                        destPointLayer.AddSegment(ближайшаяPointГрафа, ближайшийSegment.Vertex1.Point);
                        destPointLayer.RemoveSegment(ближайшийSegment);
                    }
                }

                var vertexsList = FindPath(graph.StartVertex, destVertex, graph);
                return vertexsList;
            }
            catch (Exception exc)
            {
                Moloko.Utils.Logging.Logger.Error(exc.ToString());
                return null;
            }
            finally
            {
                if (needDeleteDestVertex)
                    destPointLayer.RemoveVertex(destVertex);
                if (needDeleteБлижайшаяVertexГрафа)
                {
                    destPointLayer.RemoveVertex(ближайшаяVertexГрафа);
                    destPointLayer.AddSegment(ближайшийSegment.Vertex0, ближайшийSegment.Vertex1);
                }
            }
            
        }


        /// <summary>
        /// Формирование Point3DCollection для Path для конкретного этажа
        /// </summary>
        public static Point3DCollection PathToPoint3DCollection(List<Vertex> vertexsList, int layerID, Graph graph)
        {
            //var vertexsList = FindPathVertex(destPoint, destPointLayerID, graph);
            if (vertexsList == null || vertexsList.Count == 0 || graph.StartVertex == null)
                return null;

            var points = new Point3DCollection();

            //построение пути на этаже с учетом того, что путь может прерываться
            for (int i = 0; i < vertexsList.Count(); i++)
            {
                var vertex = vertexsList[i];
                if (i > 0)
                {
                    var prevVertex = vertexsList[i - 1];
                    if ((prevVertex.Layer.LayerID == layerID) && (vertex.Layer.LayerID == layerID))
                    {
                        points.Add(new Point3D(prevVertex.Point.Y, prevVertex.Point.X, 3));
                        points.Add(new Point3D(vertex.Point.Y, vertex.Point.X, 3));
                    }
                }
            }
            return points;
        }


        /// <summary>
        /// Формирование PathGeometry для Path для конкретного этажа с учетом повотота карты
        /// </summary>
        public static PathGeometry PathToPathGeometry(List<Vertex> vertexsList, int layerID, Graph graph, float angleDegrees, Point center)
        {
            //var vertexsList = FindPathVertex(destPoint, destPointLayerID, graph);
            if (vertexsList == null || vertexsList.Count == 0 || graph.StartVertex == null)
                return null;

            var pathGeometry = new PathGeometry();
            PathFigure figure = null;
            int prevVertexLayer = int.MinValue;
            Vertex startSegmentVertex = null;
            List<LineSegment> segments = new List<LineSegment>();

            foreach (var vertex in vertexsList)
            {
                if (prevVertexLayer != vertex.Layer.LayerID)
                {
                    if (layerID == vertex.Layer.LayerID)
                    {
                        startSegmentVertex = vertex;
                    }
                    else
                    {
                        if (segments.Count > 0)
                        {
                            figure = new PathFigure(MapHelper.RoutePoint(startSegmentVertex.Point, angleDegrees,center), segments, false);
                            pathGeometry.Figures.Add(figure);
                            segments.Clear();
                        }
                    }
                }
                else
                {
                    if (vertex.Layer.LayerID == layerID)
                        segments.Add(new LineSegment(MapHelper.RoutePoint(vertex.Point, angleDegrees, center), true));
                }
                prevVertexLayer = vertex.Layer.LayerID;
            }

            if (segments.Count > 0)
            {
                figure = new PathFigure(MapHelper.RoutePoint(startSegmentVertex.Point, angleDegrees, center), segments, false);
                pathGeometry.Figures.Add(figure);
                segments.Clear();
            }
            return pathGeometry.Figures.Count == 0 ? null : pathGeometry;
        }


        /// <summary>
        /// Поворот точки относительно другой точки
        /// </summary>
        public static Point RoutePoint(Point point, float angleDegrees, Point center)
        {            
            double angleRadians = angleDegrees * Math.PI / 180d;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);
            return new System.Windows.Point((point.X - center.X) * cos - (point.Y - center.Y) * sin + center.X, (point.X - center.X) * sin + (point.Y - center.Y) * cos + center.Y);
        }

        public static List<Vertex> FindPath(Vertex startVertex, Vertex destVertex, Graph graph)
        {
            graph.ClearДейкстраData();
                        
            startVertex.ДейкстраLength = 0;

            var непосещенныеВершины = graph.GetVertexs();
            while (true)
            {
                //Выбираем непосещенную вершину с минимальной меткой
                //TODO можно сортировать непосещенныеВершины для оптимизации
                var текущаяВершина = непосещенныеВершины.OrderBy(e => e.ДейкстраLength).FirstOrDefault();
                if (текущаяВершина == null || текущаяВершина.ДейкстраLength == double.MaxValue)
                    break;
                foreach(var v in текущаяВершина.VertexsToSegment.Where(e=>!e.Key.ДейкстраIsПосещена))
                {
                    var vertex = v.Key;
                    if (vertex.ДейкстраLength > текущаяВершина.ДейкстраLength + v.Value.Length)
                    {
                        vertex.ДейкстраLength = текущаяВершина.ДейкстраLength + v.Value.Length;
                        vertex.ДейкстраПредшествующаяVertex = текущаяВершина;
                    }
                }
                текущаяВершина.ДейкстраIsПосещена = true;
                непосещенныеВершины.Remove(текущаяВершина);
            }

            if (destVertex.ДейкстраIsПосещена)
            {
                var path = new List<Vertex>();
                var vertex = destVertex;
                while (vertex != startVertex)
                {
                    path.Insert(0, vertex);
                    vertex = vertex.ДейкстраПредшествующаяVertex;
                }
                path.Insert(0, vertex);
                return path;
            }
            else
                return null;
        }

        
        public class Vertex : ICloneable
        {
            public double ДейкстраLength { get; set; }
            public bool ДейкстраIsПосещена { get; set; }
            public Vertex ДейкстраПредшествующаяVertex { get; set; }
            public Point Point {get; set;}

            public bool IsLift { get; set; }

            public Layer Layer { get; set; }

            public Dictionary<Vertex, Segment> VertexsToSegment = null;

            public Vertex(Point point, bool _isLift = false)
            {
                Point = point;
                VertexsToSegment = new Dictionary<Vertex, Segment>();
                ДейкстраПредшествующаяVertex = null;
                ДейкстраIsПосещена = false;
                ДейкстраLength = double.MaxValue;
                IsLift = _isLift;
            }

            public void ClearДейкстраData()
            {
                ДейкстраПредшествующаяVertex = null;
                ДейкстраIsПосещена = false;
                ДейкстраLength = double.MaxValue;
            }

            public void AddSegment(Segment segment)
            {


                if (!(VertexsToSegment.ContainsValue(segment) || VertexsToSegment.ContainsKey(segment.Vertex0 == this ? segment.Vertex1 : segment.Vertex0)))
                    VertexsToSegment.Add(segment.Vertex0 == this ? segment.Vertex1 : segment.Vertex0, segment);
            }

            public void RemoveSegment(Vertex vertex)
            {
                if (VertexsToSegment.ContainsKey(vertex))
                    VertexsToSegment.Remove(vertex);
            }

            public object Clone()
            {
                return new Vertex(this.Point);
            }

        }

        /// <summary>        
        /// </summary>
        public class Segment
        {
            public Vertex Vertex0 { get; set; }
            public Vertex Vertex1 { get; set; }

            double _length = double.NaN;
            bool _isShortWay = false;
            public double Length
            {
                get
                {
                    if (!double.IsNaN(_length))
                        return _length;
                    else if (Vertex0.Layer != Vertex1.Layer)
                        //Если организации на разных этажах задаем к-нить расстояние, чтобы путь через другой этаж
                        //внезапно не оказался короче
                        if (Vertex0.IsLift && Vertex1.IsLift)
                            return _length = 500;//Если огранизации - лифты, то задаваемое к-нить расстояние должно быть поменьше, чтобы при однотипных путях между этажами путь на лифте оказался короче
                        else
                            return _length = 1000;
                    else
                        return _length = MapHelper.Distance(Vertex0.Point, Vertex1.Point);
                }
            }

            public bool IsShortWay { get { return _isShortWay; } }


            public Segment(Vertex vertex0, Vertex vertex1, bool isShortWay = false)
            {
                Vertex0 = vertex0;
                Vertex1 = vertex1;
                _isShortWay = isShortWay;
            }
        }

        public class Layer
        {
            Dictionary<Point, Vertex> pointToVertex = null;
            public List<Vertex> Vertexs = null;
            public List<Segment> Segments = null;
            public Graph Graph = null;
            public int LayerID { get; set; }

            public Layer(int layerID)
            {
                Vertexs = new List<Vertex>();
                Segments = new List<Segment>();
                pointToVertex = new Dictionary<Point, Vertex>();
                LayerID = layerID;
            }

            public Layer(List<Path> paths, int layerID, Graph praph)
                : this(layerID)
            {
                 Init(paths, layerID);
                 Graph = praph;
            }

            public void ClearДейкстраData()
            {
                foreach (var vertex in Vertexs)
                    vertex.ClearДейкстраData();
            }
            public Vertex AddVertex(Point point)
            {
                if (pointToVertex.ContainsKey(point))
                    return pointToVertex[point];
                else
                {
                    var vertex = new Vertex(point);
                    vertex.Layer = this;
                    Vertexs.Add(vertex);
                    pointToVertex.Add(point, vertex);
                    return vertex;
                }
            }

            public Vertex AddVertexWithShortestSegment(Point destPoint)
            {
                Point ближайшаяPointГрафа = new Point(0, 0);
                var minРасстояниеДоГрафа = double.MaxValue;
                Segment ближайшийSegment = null;

                foreach (var segment in Segments.Where(e => e.Vertex1.Layer == e.Vertex0.Layer))
                {
                    var point = ClosestSegmentPoint(destPoint, segment.Vertex0.Point, segment.Vertex1.Point);
                    var dist = Distance(point, destPoint);
                    if (minРасстояниеДоГрафа > dist)
                    {
                        ближайшаяPointГрафа = point;
                        minРасстояниеДоГрафа = dist;
                        ближайшийSegment = segment;
                    }
                }

                if (ближайшийSegment == null)
                    return null;

                //Надо проверить, что при поиске точки не совпали с существующими в графе
                var needDeleteDestVertex = GetVertex(destPoint) == null;
                var needDeleteБлижайшаяVertexГрафа = GetVertex(ближайшаяPointГрафа) == null;
                Vertex destVertex = null;
                try
                {
                    if (needDeleteDestVertex)
                    {
                        destVertex = AddVertex(destPoint);
                        var ближайшаяVertexГрафа = AddVertex(ближайшаяPointГрафа);
                        AddSegment(destPoint, ближайшаяPointГрафа);
                        if (needDeleteБлижайшаяVertexГрафа)
                        {
                            AddSegment(ближайшаяPointГрафа, ближайшийSegment.Vertex0.Point);
                            AddSegment(ближайшаяPointГрафа, ближайшийSegment.Vertex1.Point);
                            RemoveSegment(ближайшийSegment);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logging.Logger.Error(exc.Message);
                }
                return destVertex;
            }

            

            public Vertex GetVertex(Point point)
            {
                return pointToVertex.ContainsKey(point) ? pointToVertex[point] : null;
                //return pointToVertex.First().Value;
            }

            public Vertex GetVertex(double x, double y)
            {
                return GetVertex(new Point(x, y));
            }

            public void RemoveVertex(Vertex vertex)
            {
                //Чтобы ни осталось никаких ссылок
                foreach (var ver in Vertexs)
                    ver.ClearДейкстраData();

                foreach (var ver in vertex.VertexsToSegment)
                {
                    //Удаляем ссылки на сегменты с этой вершиной в других вершинах
                    ver.Key.RemoveSegment(vertex);
                    //Удаляем все сегменты с этой вершиной из графа
                    Segments.Remove(ver.Value);
                }
                vertex.VertexsToSegment.Clear();
                Vertexs.Remove(vertex);
                pointToVertex.Remove(vertex.Point);
            }


            public void RemoveSegment(Segment segment)
            {
                //Чтобы ни осталось никаких ссылок
                foreach (var ver in Vertexs)
                    ver.ClearДейкстраData();
                Segments.Remove(segment);
                foreach (var ver in Vertexs.Where(e => e == segment.Vertex0 || e == segment.Vertex1))
                    ver.RemoveSegment(ver == segment.Vertex0 ? segment.Vertex1 : segment.Vertex0);
            }

            public void AddSegment(Point point0, Point point1)
            {
                var vertex0 = AddVertex(point0);
                var vertex1 = AddVertex(point1);
                var segment = new Segment(vertex0, vertex1);
                Segments.Add(segment);
                vertex0.AddSegment(segment);
                vertex1.AddSegment(segment);
            }

            public void AddSegment(Vertex vertex0, Vertex vertex1, bool _isShortWay = false)
            {                
                var segment = new Segment(vertex0, vertex1, _isShortWay);
                Segments.Add(segment);
                vertex0.AddSegment(segment);
                vertex1.AddSegment(segment);
            }


            public void Init(List<Path> paths, int layerID)
            {                
                foreach (var path in paths)
                {
                    var geometry = path.Data.GetFlattenedPathGeometry();
                    if (geometry.Figures.Count != 1 || geometry.Figures[0].Segments.Count != 1 || !(geometry.Figures[0].Segments[0] is LineSegment))
                        throw new Exception($"{0}Некорретно задан путь. Каждый маршрут должен состоять из одного LineSegment");
                    var figure = geometry.Figures[0];
                    var lineSegment = (LineSegment)figure.Segments[0];

                    var segmentPoint0 = figure.StartPoint;
                    var segmentPoint1 = lineSegment.Point;
                    AddSegment(segmentPoint0, segmentPoint1);
                }
            }

        }

        /// <summary>
        /// Корректная последовательность вызовов
        /// var graph = new Graph();
        /// graph.AddLayer(...);
        /// praph.Init();
        /// </summary>
        public class Graph
        {

            public List<Layer> Layers = null;
            public Vertex StartVertex = null;

            public Graph()
            {
                Layers = new List<Layer>();             
            }

            public Layer AddLayer(Layer layer)
            {                
                layer.Graph = this;
                Layers.Add(layer);
                return layer;
            }

            public Layer AddLayer(List<Path> paths, int layerID)
            {
                var layer = new Layer(paths, layerID, this);
                layer.Graph = this;                
                Layers.Add(layer);                
                return layer;
            }

            public void Init(Point startPoint, int? startPointLayerID)
            {
                StartVertex = null;
                var layer = Layers.FirstOrDefault(e=>e.LayerID == startPointLayerID);
                if (layer == null)
                    Logging.Logger.Error("Слой с идентификатором {0} не найден", startPointLayerID);
                else
                {
                    //Если начальной точки нет в графе, то добавляем ее в граф и сегмент к графу
                    StartVertex = layer.GetVertex(startPoint)?? layer.AddVertexWithShortestSegment(startPoint);
                }
            }

            public void ClearДейкстраData()
            {
                foreach (var layer in Layers)
                    layer.ClearДейкстраData();
            }

            public List<Vertex> GetVertexs()
            {
                List<Vertex> vertexs = new List<Vertex>();
                foreach (var layer in Layers)
                    vertexs.AddRange(layer.Vertexs);
                return vertexs;
            }

            public Vertex GetVertex(Point point, int layerID)
            {
                return Layers.FirstOrDefault(e => e.LayerID == layerID).GetVertex(point);
            }

            public Layer GetLayer(int layerID)
            {
                return Layers.FirstOrDefault(e => e.LayerID == layerID);
            }
        }
   
    }
}
