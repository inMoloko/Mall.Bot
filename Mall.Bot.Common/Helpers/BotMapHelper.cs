using Moloko.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.MallHelpers.Models;
using Mall.Bot.Common.MallHelpers;
using Mall.Bot.Search.Models;
using MoreLinq;

namespace Mall.Bot.Common.Helpers
{
    public class WayModel
    {
        public List<MapHelper.Vertex> Way { get; set; }
        public MapObject From { get; set; }
        public MapObject To { get; set; }
    }
    public class GroupedOrganization
    {
        /// <summary>
        /// Оргиназации, которым принадлежат группированные точки
        /// </summary>
        public List<Organization> Orgs { get; set; } 
        /// <summary>
        /// группированные точки
        /// </summary>
        public List<MapObject> MapObjects { get; set; } 
        public float AverageRating
        {
            get
            {
                if (Orgs.Count != 0)
                {
                    float sum = 0;
                    int i = 0;
                    foreach (var item in Orgs)
                    {
                        if (item.Rating != null)
                        {
                            sum += (float)item.Rating;
                            i++;
                        }
                    }
                    if (i == 0) return 0;
                    return sum / (float)i;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int FloorID
        {
            get
            {
                if (MapObjects.Count != 0) return MapObjects[0].FloorID;
                else return -1;
            }
        }
    }


    public class BotMapHelper
    {
        public const double neighbors = 150;
        public const double vicinity = 100;
        public const double deadLine = 20;


        /// <summary>
        /// Возвращает две точки: левую нижнюю и правую верхнюю (точно в таком порядке)
        /// </summary>
        /// <param name="mObjs"></param>
        /// <returns></returns>
        /// вынес их для более гибкого использования
        public static List<System.Windows.Point> PointsDiagonal(List<MapObject> mObjs)
        {
            if (mObjs.Count == 1) throw new Exception("Takes two or more map objects!!");

            var oup = new MapObject(); double cup = double.MinValue;
            var odown = new MapObject(); double cdown = double.MaxValue;
            var oleft = new MapObject(); double cleft = double.MaxValue;
            var oright = new MapObject(); double cright = double.MinValue;

            for (int i = 0; i < mObjs.Count; i++)
            {
                #region Находим верхнюю, нижнюю, левую и правую точки
                //верхняя
                if (mObjs[i].LatitudeFixed > cup)
                {
                    cup = (double)mObjs[i].LatitudeFixed;
                    oup = mObjs[i];
                }

                //правая
                if (mObjs[i].LongitudeFixed > cright)
                {
                    cright = (double)mObjs[i].LongitudeFixed;
                    oright = mObjs[i];
                }

                //нижняя
                if (mObjs[i].LatitudeFixed < cdown)
                {
                    cdown = (double)mObjs[i].LatitudeFixed;
                    odown = mObjs[i];
                }

                //левая
                if (mObjs[i].LongitudeFixed < cleft)
                {
                    cleft = (double)mObjs[i].LongitudeFixed;
                    oleft = mObjs[i];
                }
                #endregion
            }
            //находим "левую нижнюю" и "правую верхнюю" точки прямоугольника, куда эти точки можно вписать.
            var ld = new System.Windows.Point(Math.Min((double)oleft.LongitudeFixed, (double)odown.LongitudeFixed), Math.Min((double)oright.LatitudeFixed, (double)odown.LatitudeFixed));
            var ru = new System.Windows.Point(Math.Max((double)oright.LongitudeFixed, (double)oup.LongitudeFixed), Math.Max((double)oright.LatitudeFixed, (double)oup.LatitudeFixed));
            var res = new List<System.Windows.Point>();
            res.Add(ld);
            res.Add(ru);
            return res;
        }

        /// <summary>
        /// группирует найденные организации. 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="dataOfbot"></param>
        /// <returns></returns>
        public static List<GroupedOrganization> GroupFuzzySearchResult(List<FuzzySearchResult> result, CachedDataModel dataOfbot)
        {
            var orgs = GetDataHelper.GetOrganizationFromFuzzySearchResult(result, dataOfbot); // получаем список найденных организаций
            return GroupFuzzySearchResult(orgs, dataOfbot);
        }
        public static List<GroupedOrganization> GroupFuzzySearchResult(List<Organization> orgs, CachedDataModel dataOfbot)
        {
            var resGroups = new List<HashSet<MapObject>>();
            var mapObject = dataOfbot.GetMapObjects(orgs);
            foreach (Floor f in dataOfbot.Floors)
            {
                var groups = new List<HashSet<MapObject>>(); // мини-группы. органиия + другие организации, находящиея на заданном расстоянии от нее
                var thisFloorMapObj = mapObject.Where(x => x.FloorID == f.FloorID && x.Params == null).ToList(); // типа если парамс нул, то это вход организации

                for (int i = 0; i < thisFloorMapObj.Count; i++)
                {
                    var close = new HashSet<MapObject>(); // мини-группа
                    close.Add(thisFloorMapObj[i]);
                    for (int j = 0; j < thisFloorMapObj.Count; j++)
                    {
                        if (i != j)
                        {
                            var res = Math.Sqrt(
                                (thisFloorMapObj[j].LongitudeFixed - thisFloorMapObj[i].LongitudeFixed) * 
                                (thisFloorMapObj[j].LongitudeFixed - thisFloorMapObj[i].LongitudeFixed) 
                                +
                                (thisFloorMapObj[j].LatitudeFixed - thisFloorMapObj[i].LatitudeFixed) * 
                                (thisFloorMapObj[j].LatitudeFixed - thisFloorMapObj[i].LatitudeFixed));

                            if (res < 1100)
                            {
                                close.Add(thisFloorMapObj[j]);
                            }
                        }
                    }
                    groups.Add(close);
                }

                for (int i = 0; i < groups.Count; i++)
                {
                    var tempres = new HashSet<MapObject>();
                    tempres = groups[i];
                    for (int j = 0; j < groups.Count; j++)
                    {
                        if (i != j)
                        {
                            if (groups[i].Intersect(groups[j]).Count() != 0) // если двух мини-группах есть одинаковые элементы, 
                            {
                                tempres.UnionWith(groups[j]); //то такие мини-группы должны быть в одной большой группе,
                                groups.RemoveAt(j); // а объединенные мини-группы больше не нужны
                                j--;
                            }
                        }
                    }
                    resGroups.Add(tempres); // добавление группы
                }
            }
            
            var groupedResult = new List<GroupedOrganization>();
            foreach (var item in resGroups)
            {
                groupedResult.Add(new GroupedOrganization { MapObjects = item.ToList(), Orgs = dataOfbot.GetOrganizations(item.ToList()) }); // формирование результата
            }
            return groupedResult;
        }
        /// <summary>
        /// Возвращает точку ~ центр группы
        /// </summary>
        /// <param name="mObjs"></param>
        /// <returns></returns>
        public static System.Windows.Point CenterOfDiagonal(List<MapObject> mObjs)
        {
            if (mObjs.Count == 1) return new System.Windows.Point(mObjs[0].LongitudeFixed, mObjs[0].LatitudeFixed);
            var ldANDru = PointsDiagonal(mObjs);
            return new System.Windows.Point((ldANDru[0].X + ldANDru[1].X)/2, (ldANDru[0].Y + ldANDru[1].Y) / 2); // возвращает центр диагонали
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mObjs"></param>
        /// <returns></returns>
        public static double LengthOfDiagonal(List<MapObject> mObjs)
        {
            if (mObjs.Count == 1) return 0;
            var ldANDru = PointsDiagonal(mObjs);
            return Math.Sqrt((ldANDru[0].X - ldANDru[1].X) * (ldANDru[0].X - ldANDru[1].X) + (ldANDru[0].Y - ldANDru[1].Y) * (ldANDru[0].Y - ldANDru[1].Y));
        }
        /// <summary>
        /// возвращает длину пути
        /// </summary>
        /// <param name="way"></param>
        /// <returns></returns>
        public double LengthOfWay(List<MapHelper.Vertex> way)
        {
            double res = 0;
            for (int i = 0; i < way.Count-1; i++)
            {
                res += MapHelper.Distance(way[i].Point, way[i + 1].Point);
            }
            return res;
        }
        /// <summary>
        /// Возвращает кратчайший путь
        /// </summary>
        /// <param name="First"></param>
        /// <param name="Second"></param>
        /// <param name="DataOfBot"></param>
        /// <returns></returns>
        public WayModel GetClosestWay(Organization First, List<Organization> Second, CachedDataModel DataOfBot)
        {
            List<List<MapHelper.Vertex>> ways = new List<List<MapHelper.Vertex>>();
            double mindist = double.MaxValue;
            var resultID = 0;
            var first = First.OrganizationMapObject.Select(x => x.MapObject).FirstOrDefault(x => x.Params == null); //точка - начало пути
            var seconds = DataOfBot.GetMapObjects(Second);// точки - возможные окончания пути
            for (int i = 0; i < seconds.Count; i++)
            {
                ways.Add(GetWay(first, seconds[i], DataOfBot));
                var tmp = LengthOfWay(ways[i]);
                if (mindist > tmp)
                {
                    mindist = tmp;
                    resultID = i;
                }
            }
            return new WayModel { From = first, To = seconds[resultID], Way = ways[resultID] };
        }
        public WayModel GetClosestWay(FuzzySearchResult First, List<FuzzySearchResult> Second, CachedDataModel DataOfBot)
        {
            Organization _First = DataOfBot.Organizations.FirstOrDefault(x => x.OrganizationID == First.ID);
            List<Organization> _Second = new List<Organization>();
            foreach (var item in Second)
            {
                _Second.Add(DataOfBot.Organizations.FirstOrDefault(x => x.OrganizationID == item.ID));
            }
            return GetClosestWay(_First,_Second,DataOfBot);
        }


        /// <summary>
        /// Поиск пути между двумя организациями
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="custumerID"></param>
        /// <returns></returns>
        public List<MapHelper.Vertex> GetWay(MapObject First, MapObject Second, CachedDataModel DataOfBot)
        {
            var graph = GetGraph(DataOfBot);
            graph.Init(new System.Windows.Point(First.LongitudeFixed, First.LatitudeFixed), DataOfBot.Floors.FirstOrDefault(x => x.FloorID == First.FloorID).Number);
            return MapHelper.FindPathVertex(new System.Windows.Point(Second.LongitudeFixed, Second.LatitudeFixed), DataOfBot.Floors.FirstOrDefault(x => x.FloorID == Second.FloorID).Number, graph);
        }

        /// <summary>
        /// Поиск ориентиров для текстового описания. Алгоритм: 
        /// 1. просматриваем все сегменты в пути - результате. 
        /// 2. каждые "const deadline" единиц ишем организацию в радусе "const vicinity". 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="FloorID"></param>
        /// <param name="collection"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public List<MapObject> FindOrientirs(List<Point> points, int FloorID, CachedDataModel dataOfBot, BitmapSettings bitmap, MapObject First, MapObject Second)
        {
            double sum = 0;
            int i;
            var result = new List<MapObject>();
            for (i = 0; i < points.Count - 1; i++)
            {
                var p1 = new System.Windows.Point(points[i].X, points[i].Y);
                var p2 = new System.Windows.Point(points[i + 1].X, points[i + 1].Y);
                double distance = MapHelper.Distance(p1, p2);

                if (distance > (deadLine - sum))
                {
                    Point p = GetPoint(deadLine - sum, distance - (deadLine - sum), points[i], points[i + 1]);
                    points.Insert(i + 1, p);
                    result.Add(FindClosestPopularOrganization(points[i + 1], dataOfBot, FloorID, bitmap));
                    sum = 0;
                }
                else
                {
                    sum += distance;
                    if (sum >= deadLine)
                    {
                        result.Add(FindClosestPopularOrganization(points[i], dataOfBot, FloorID, bitmap));
                        sum = 0;
                    }
                }
            }
            result.RemoveAll(x => x == null);
            result = result.DistinctBy(x => x.MapObjectID).ToList();
            result = RemoveClosestPopularOrganizations(result);
            result.RemoveAll(x => x.MapObjectID == First.MapObjectID || x.MapObjectID == Second.MapObjectID);

            var last = dataOfBot.MapObjects.FirstOrDefault(x =>
            Math.Abs(Math.Round(x.LongitudeFixed) - Math.Round((points.Last().X - bitmap.I) * bitmap.ZoomOfPicture)) <= 10 &&
            Math.Abs(Math.Round(x.LatitudeFixed) - Math.Round((points.Last().Y - bitmap.J) * bitmap.ZoomOfPicture)) <= 10);

            var temp = new MapObject
            {
                FloorID = FloorID,
                Params = "Перейдите на следующий этаж"
                //Name = "Перейдите на следующий этаж",
                //KeyWords = last.Name,
                //Floor = new Floor { Number = layerID }
            };

            result.Add(temp);

            return result;
        }
        /// <summary>
        /// Удаление организаций, найденных для тектосвого описания, если они распиоложены слишком близко друг к другу
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<MapObject> RemoveClosestPopularOrganizations(List<MapObject> result)
        {
            for (int i = 0; i < result.Count - 1; i++)
            {
                var p1 = new System.Windows.Point(result[i].LongitudeFixed,result[i].LatitudeFixed);
                var p2 = new System.Windows.Point(result[i+1].LongitudeFixed, result[i+1].LatitudeFixed);
                double distance = MapHelper.Distance(p1, p2);
                if (distance < neighbors)
                {
                    result.RemoveAt(i);
                }
            }
            return result;
        }
        /// <summary>
        /// Поиск организации с заданными параметрами
        /// </summary>
        /// <param name="p"></param>
        /// <param name="collection"></param>
        /// <param name="FloorID"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public MapObject FindClosestPopularOrganization(Point p, CachedDataModel dataOfBot, int FloorID, BitmapSettings bitmap)
        {
            //var popular = collection.Where(x => (x.IsAnchor == true || x.Rating > 0.5 || x.SignText != null) && layerID == x.Floor.Number && x.Longitude != null && x.Latitude != null).ToList();
            var popularOrgs = dataOfBot.Organizations.Where(x => x.IsAnchor == true || x.Rating > 0.5).ToList();
            var popularMapObjs = dataOfBot.GetMapObjects(popularOrgs).Where(x => x.FloorID == FloorID).ToList();

            var localPoint = new System.Windows.Point((p.X - bitmap.I) * bitmap.ZoomOfPicture, (p.Y - bitmap.J) * bitmap.ZoomOfPicture);
            double minDest = double.MaxValue;
            MapObject Result = null;

            foreach (var mobj in popularMapObjs)
            {
                var shop = new System.Windows.Point(mobj.LongitudeFixed, mobj.LatitudeFixed);

                double temp = MapHelper.Distance(localPoint, shop);
                if (temp < minDest && temp < vicinity)
                {
                    minDest = temp;
                    Result = mobj;
                }
            }
            return Result;
        }
        /// <summary>
        /// Выччисление координат точки находящейся на определенном растоянии от начала отрезка http://testent.ru/publ/studenty/vysshaja_matematika/delenie_otrezka_v_dannom_otnoshenii/35-1-0-1054
        /// </summary>
        /// <param name="DestOfFirstSector"></param>
        /// <param name="DestOfSecondSector"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private Point GetPoint(double DestOfFirstSector, double DestOfSecondSector, Point A, Point B)
        {
            double lambda = DestOfFirstSector / DestOfSecondSector;
            double x = (A.X + lambda * B.X) / (1 + lambda);
            double y = (A.Y + lambda * B.Y) / (1 + lambda);
            return new Point((int)x, (int)y);
        }
        /// <summary>
        /// Возвражает граф со всеми вершинами на карте этажа
        /// </summary>
        /// <param name="DestOfFirstSector"></param>
        /// <param name="DestOfSecondSector"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public MapHelper.Graph GetGraph(CachedDataModel DataOfBot)
        {
            var graph = new MapHelper.Graph();
            #region Строит Layers
            foreach (Floor f in DataOfBot.Floors.OrderBy(x => x.Number))
            {
                PathPoint[][] Lines = null;
                if (f.Paths[0] == '<') // xml быть не должно, поэтому я решил сильно не запариваться по этому поводу
                {
                    if (f.Number != 2)
                    {
                        var pathHelper = new PathParserHelper();
                        Lines = pathHelper.Do(f.Paths);
                    }
                    else
                    {
                        break;
                        // супер костыль. Дело в том, что в базе Mall_new у Уфы 2 этажа с разными xml-ками. (причем 2ой этаж пуст) 
                        //Я понимаю, что мне не надо писать лишний парсер, тем более, что 1 лишний я уже написал)) А удалить этаж из базы я не рискнул, мало ли он кому нужен
                        //поэтому этот код работает только для Уфы. Надо будет обсудить этот момент и возможно убрать 2ой этаж из базы. А пока ((((временно)))) так                    
                    }
                }
                else
                {
                    Lines = f.PathsFixed;
                }

                var layer = new MapHelper.Layer(f.Number);
                for (int i = 0; i < Lines.Length; i++)
                {
                    var v1 = layer.AddVertex(new System.Windows.Point(Lines[i][0].X, Lines[i][0].Y));
                    var v2 = layer.AddVertex(new System.Windows.Point(Lines[i][1].X, Lines[i][1].Y));
                    layer.AddSegment(v1, v2);
                }
                graph.AddLayer(layer);
            }
            #endregion
            // Поиск переходов между этажами и добавление их в граф
            var FloorIDs = DataOfBot.Floors.Select(x => x.FloorID).ToList();
            foreach (var link in DataOfBot.MapObjectLinks)
            {
                if (link.MapObjectFrom != null &&
                    link.MapObjectTo != null &&
                    link.MapObjectFrom.FloorID != null &&
                    link.MapObjectTo.FloorID != null &&
                    FloorIDs.Contains(link.MapObjectFrom.FloorID) &&
                    FloorIDs.Contains(link.MapObjectTo.FloorID))
                {

                    var layerTo = graph.Layers.FirstOrDefault(e => e.LayerID == DataOfBot.Floors.FirstOrDefault(y => y.FloorID == link.MapObjectTo.FloorID).Number);
                    var layerFrom = graph.Layers.FirstOrDefault(e => e.LayerID == DataOfBot.Floors.FirstOrDefault(y => y.FloorID == link.MapObjectFrom.FloorID).Number);

                    var temp = new System.Windows.Point(link.MapObjectFrom.LongitudeFixed, link.MapObjectFrom.LatitudeFixed);
                    var vertexFrom = layerFrom.GetVertex(temp) ?? layerFrom.AddVertexWithShortestSegment(temp);

                    temp = new System.Windows.Point(link.MapObjectTo.LongitudeFixed, link.MapObjectTo.LatitudeFixed);
                    var vertexTo = layerTo.GetVertex(temp) ?? layerTo.AddVertexWithShortestSegment(temp);

                    if (vertexFrom == null || vertexTo == null)
                        Logging.Logger.Warn("Change Floor Vertex has no beed added!");
                    else
                    {
                        
                        if (DataOfBot.GetOrganization(link.MapObjectFrom)?.Name == "Лифт" && DataOfBot.GetOrganization(link.MapObjectTo)?.Name == "Лифт")
                        {
                            vertexTo.IsLift = true;
                            vertexFrom.IsLift = true;
                        }
                        layerFrom.AddSegment(vertexFrom, vertexTo);
                    }
                }
            }
            return graph;
        }
        /// <summary>
        /// Возвращает граф с одним уровнем
        /// </summary>
        /// <param name="DataOfBot"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public MapHelper.Graph GetOneLayer(CachedDataModel DataOfBot, Floor f)
        {
            //var graph = new MapHelper.Graph();

            //PathPoint[][] Lines = null;
            //if (f.Paths[0] == '<') // xml быть не должно, поэтому я решил сильно не запариваться по этому поводу
            //{
            //    var pathHelper = new PathParserHelper();
            //    Lines = pathHelper.Do(f.Paths);
            //}
            //else
            //{
            //    Lines = JsonConvert.DeserializeObject<PathPoint[][]>(f.Paths);
            //}

            //var layer = new MapHelper.Layer(f.Number);
            //for (int i = 0; i < Lines.Length; i++)
            //{
            //    var v1 = layer.AddVertex(new System.Windows.Point(Lines[i][0].X, Lines[i][0].Y));
            //    var v2 = layer.AddVertex(new System.Windows.Point(Lines[i][1].X, Lines[i][1].Y));
            //    layer.AddSegment(v1, v2);
            //}
            //graph.AddLayer(layer);

            //var vyborka = DataOfBot.Organizations;

            //foreach (var link in DataOfBot.OrganizationLinks)
            //{
            //    var org = vyborka.FirstOrDefault(e => e.OrganizationID == link.OrganizationToID);
            //    if (org != null && org.Longitude != null && org.Latitude != null)
            //    {
            //        var temp = new System.Windows.Point((double)org.Longitude, (double)org.Latitude);
            //        var vertexFrom = graph.Layers[0].GetVertex(temp) ?? graph.Layers[0].AddVertexWithShortestSegment(temp);
            //    }

            //    org = vyborka.FirstOrDefault(e => e.OrganizationID == link.OrganizationFromID);
            //    if (org != null && org.Longitude != null && org.Latitude != null)
            //    {
            //        var temp = new System.Windows.Point((double)org.Longitude, (double)org.Latitude);
            //        var vertexFrom = graph.Layers[0].GetVertex(temp) ?? graph.Layers[0].AddVertexWithShortestSegment(temp);
            //    }
            //}
            //return graph;
            return null;
        }
    }
}
