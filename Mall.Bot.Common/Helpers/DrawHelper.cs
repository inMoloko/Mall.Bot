using Moloko.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.MallHelpers.Models;
using System.Configuration;
using Mall.Bot.Search.Models;

namespace Mall.Bot.Common.Helpers
{
    public class DrawHelper
    {
        public const float OrganizationKegel = 9F;

        private CachedDataModel dataOfBot;
        private FindedInformation answer;
        private string title;

        public DrawHelper(CachedDataModel _dataOfBot, FindedInformation _answer, string _title)
        {
            dataOfBot = _dataOfBot;
            answer = _answer;
            title = _title;
        }

        /// <summary>
        /// Рисует все найденные организации на карте этажа
        /// Возвращает структуру с добавленными в нее картинками
        /// </summary>
        /// <returns></returns>
        public FindedInformation DrawFindedShops()
        {
            var groupedOrgs = (List<GroupedOrganization>)answer.GroopedResult; // получает группы огранизаций
            groupedOrgs.OrderByDescending(x => x.AverageRating).ToList();

            foreach (Floor f in dataOfBot.Floors)
            {
                var groupsFromFloor = groupedOrgs.Where(x => x.FloorID == f.FloorID).ToList();
                if (groupsFromFloor.Count != 0)
                {
                    //var bitmap = new BitmapSettings(new Bitmap(Image.FromStream(new MemoryStream(f.File))), f.FloorID);
                    var bitmap = new BitmapSettings(new Bitmap(Image.FromFile(ConfigurationManager.AppSettings["ContentPath"] + $"Floors\\{f.FloorID}.{f.FileExtension}")), f.FloorID);

                    

                    foreach (var group in groupsFromFloor)
                    {
                        foreach (var org in group.Orgs)
                        {
                            Image img;
                            var mObjofThisOrg = group.MapObjects.Where(x => org.OrganizationMapObject.Select(y => y.MapObjectID).Contains(x.MapObjectID));
                            if (group.MapObjects.Count == 1) img = Properties.Resources.Shop;
                            else img = Properties.Resources.ShopG;// для групп рисуем оранжевую точку
                            foreach (var mObj in mObjofThisOrg)
                            {
                                var temp = org.CategoryOrganization.Select(x => x.Category).Select(x => x.ServiceCategoryType);
                                if (temp.Contains(ServiceCategoryType.Service) || temp.Contains(ServiceCategoryType.Link))
                                {
                                    img = ImagingHelper.ResizeImage(img, 110, 110);
                                    bitmap.DrawLocation(mObj.LongitudeFixed, mObj.LatitudeFixed, "default", img);
                                    bitmap.DrawLandMarksExtra(dataOfBot, f.Number);
                                }
                                else
                                {
                                    img = ImagingHelper.ResizeImage(img, (int)(img.Width * 4 / 2.5F), (int)(img.Height * 4 / 2.5F));
                                    bitmap.DrawLocation(mObj.LongitudeFixed, mObj.LatitudeFixed, "default", img);
                                }
                            }
                        }
                    }

                    bitmap.DrawLandMarksExtra(dataOfBot, f.FloorID);
                    bitmap.DrawLandMarksOrganizations(dataOfBot, f.FloorID);
                    bitmap.DrawSignPoint(dataOfBot, f.FloorID, dataOfBot.Customers[0].Name);

                    // расставляем балуны для каждой группы
                    char index = 'A';
                    for (int i = 0 ; i < 5 && i < groupsFromFloor.Count; i++)
                    {
                        var obj = Properties.Resources.ResourceManager.GetObject(index.ToString());
                        Image img = (Bitmap)obj;
                        bitmap.DrawLocation(BotMapHelper.CenterOfDiagonal(groupsFromFloor[i].MapObjects), "MultyDraw", img);
                        index++;
                    }
                    // подпись картинки
                    var tmp = title.Replace("%floornumber%", f.Number.ToString());
                    bitmap.DrawText(tmp, BotTextHelper.LengthOfString(tmp, bitmap), 5F, 23, Color.DarkSlateGray, true);
                    answer.FloorsPictures.Add(bitmap);
                }
            }
            return answer;
        }

        /// <summary>
        /// Рисует путь way от orgFrom до orgTo
        /// </summary>
        /// <param name="orgFrom"></param>
        /// <param name="orgTo"></param>
        /// <param name="way"></param>
        /// <returns></returns>
        public FindedInformation DrawWay(MapObject orgFrom, MapObject orgTo, List<MapHelper.Vertex> way)
        {
            int i = 0;
            bool Continue = true;

            while (Continue)
            {
                var Floor = dataOfBot.Floors.FirstOrDefault(x => x.Number == way[i].Layer.LayerID);
                var bitmap = new BitmapSettings(new Bitmap(Image.FromFile(ConfigurationManager.AppSettings["ContentPath"] + $"Floors\\{dataOfBot.Floors.FirstOrDefault(x => x.FloorID == Floor.FloorID).FloorID}.{dataOfBot.Floors.FirstOrDefault(x => x.FloorID == Floor.FloorID).FileExtension}")));
                var points = new List<Point>();

                bitmap.DrawSignPoint(dataOfBot, Floor.FloorID, dataOfBot.Customers[0].Name);
                if (i == 0) bitmap.DrawLocation(way[i].Point.X, way[i].Point.Y, "A");

                bool flag = true;
                while (flag && i < way.Count)
                {
                    if (Floor.Number == way[i].Layer.LayerID)
                    {
                        points.Add(new Point((int)(way[i].Point.X / bitmap.ZoomOfPicture) + bitmap.I, (int)(way[i].Point.Y / bitmap.ZoomOfPicture + bitmap.J)));
                        i++;
                    }
                    else
                    {
                        flag = false;

                        if (points.Count > 1)
                        {
                            bitmap.DrawCurve(points);
                            bitmap.DrawLandMarksExtra(dataOfBot, Floor.FloorID);
                            bitmap.DrawLandMarksOrganizations(dataOfBot, Floor.FloorID);

                            var mapHelper = new BotMapHelper();

                            answer.TextDescription.AddRange(mapHelper.FindOrientirs(points, Floor.FloorID, dataOfBot, bitmap, orgFrom, orgTo));
                            var lstMapObj = answer.TextDescription.Where(x => x.FloorID == Floor.FloorID).ToList();
                            bitmap.DrawSpecialOrgs(lstMapObj, dataOfBot.GetOrganizations(lstMapObj));

                            var text = title.Replace("%floornumber%", Floor.Number.ToString());
                            bitmap.DrawText(text, BotTextHelper.LengthOfString(text, bitmap), 5F, 23, Color.DarkSlateGray, true);

                            answer.FloorsPictures.Add(bitmap);
                        }
                    }
                }
                if (i == way.Count)
                {
                    Continue = false;

                    if (points.Count > 1)
                    {
                        bitmap.DrawCurve(points);
                        bitmap.DrawLandMarksExtra(dataOfBot, Floor.FloorID);
                        bitmap.DrawLandMarksOrganizations(dataOfBot, Floor.FloorID);
                        bitmap.DrawLocation(way[i - 1].Point.X, way[i - 1].Point.Y, "B");

                        var mapHelper = new BotMapHelper();

                        answer.TextDescription.AddRange(mapHelper.FindOrientirs(points, Floor.FloorID, dataOfBot, bitmap, orgFrom, orgTo));
                        var lstMapObj = answer.TextDescription.Where(x => x.FloorID == Floor.FloorID).ToList();
                        bitmap.DrawSpecialOrgs(lstMapObj, dataOfBot.GetOrganizations(lstMapObj));

                        var text = title.Replace("%floornumber%", Floor.Number.ToString());
                        bitmap.DrawText(text, BotTextHelper.LengthOfString(text, bitmap), 5F, 23, Color.DarkSlateGray, true);

                        answer.FloorsPictures.Add(bitmap);
                    }
                }
            }
            return answer;
        }
    }
}
