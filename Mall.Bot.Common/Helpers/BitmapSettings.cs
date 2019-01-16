using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using Moloko.Utils;
using System.IO;
using System.Drawing.Drawing2D;
using Mall.Bot.Common.MallHelpers.Models;
using Mall.Bot.Common.DBHelpers.Models;
using Newtonsoft.Json;
using System.Configuration;
using Mall.Bot.Search.Models;

namespace Mall.Bot.Common.Helpers
{
    public class BitmapSettings
    {
        public const float OrganizationKegel = 9F;
        private const float s = 3.5F;
        public Bitmap Bmp;
        /// <summary>
        /// Вспомогательная координата Longitude
        /// </summary>
        public int I;
        /// <summary>
        /// Вспомогательная координата Latitude
        /// </summary>
        public int J;
        public Pen MyPen;
        public float ZoomOfPicture;
        public int FloorID;

        public BitmapSettings()
        {
        }

        public BitmapSettings(Bitmap bmp, int _floorID = 0)
        {
            double koeff = (double)bmp.Width / bmp.Height;
            double temp = 1150 / koeff;
            Image img = ImagingHelper.ResizeImage(bmp, 1150, (int)temp);
       

            //избавляемя от прозрачных областей (типа convert to jpg)
            var b = new Bitmap(img.Width, img.Height);
            b.SetResolution(img.HorizontalResolution, img.VerticalResolution);
            using (var g = Graphics.FromImage(b))
            {
                g.Clear(Color.White);
                g.DrawImageUnscaled(img, 0, 0);
            }
            Bmp = b;
            I = b.Width / 2; 
            J = b.Height / 2;
            ZoomOfPicture = (float)bmp.Width / b.Width;

            MyPen = new Pen(Color.Red, 2.5F);
            FloorID = _floorID;
        }

        /// <summary>
        /// Сохранение изображения
        /// </summary>
        /// <param name="name"></param>
        /// <param name="desktop"></param>
        /// <returns></returns>
        public string Save(string name, bool desktop = false)
        {
            try
            {
                string path = $"C:\\Users\\Terminal\\Desktop\\{name}.jpg";
                if (desktop) path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), name); // сохранение картинки с путем на рабочий стол

                using (MemoryStream memory = new MemoryStream())
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                    {
                        Bmp.Save(memory, ImageFormat.Bmp);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
                return name + ".jpg";
            }
            catch (Exception exc)
            {
                Logging.Logger.Error(exc);
                return name + ".jpg";
            }
        }
        /// <summary>
        /// Добавляет кусочек к картинке. В кусочек можно будет написать номер этажа
        /// </summary>
        public void AddSegmentToBitmap()
        {
            var kostyl = new Bitmap(1, 1);
            kostyl.SetPixel(0, 0, Color.White);

            var b = new Bitmap(kostyl, Bmp.Width, Bmp.Height + 40);

            using (var gr = Graphics.FromImage(b))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                gr.DrawImage(Bmp, new PointF(0, 40));
            }

            Bmp = b;
            I = b.Width / 2; // вспомогательные координаты
            J = b.Height / 2;
        }

        /// <summary>
        /// Отображение точки на Bmp
        /// </summary>
        /// <param name="point"></param>
        /// <param name="p">Параметр. А - балун с А, B - балун с B, MultyDraw - </param>
        /// <param name="img"></param>
        public void DrawLocation(System.Windows.Point point, string p, Image img = null)
        {
            DrawLocation(point.X, point.Y, p, img);
        }
        public void DrawLocation(double x, double y, string p, Image img = null)
        {
            x /= ZoomOfPicture; y /= ZoomOfPicture;
            using (var gr = Graphics.FromImage(Bmp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                switch (p)
                {
                    case "A":
                        img = Properties.Resources.A;
                        img = ImagingHelper.ResizeImage(img, (int)(img.Width / s), (int)(img.Height / s));
                        gr.DrawImage(
                            img,
                            (float)x + I - img.Width / 2,
                            (float)y + J - img.Height);
                        break;
                    case "B":
                        img = Properties.Resources.B;
                        img = ImagingHelper.ResizeImage(img, (int)(img.Width / s), (int)(img.Height / s));
                        gr.DrawImage(
                            img,
                            (float)x + I - img.Width / 2,
                            (float)y + J - img.Height);
                        break;
                    case "MultyDraw":
                        img = ImagingHelper.ResizeImage(img, (int)(img.Width / s), (int)(img.Height / s));
                        gr.DrawImage(
                            img,
                            (float)x + I - img.Width / 2,
                            (float)y + J - img.Height);
                        break;
                    default:
                        img = ImagingHelper.ResizeImage(img, (int)(img.Width / s), (int)(img.Height / s));
                        gr.DrawImage(
                            img,
                            (float)x + I - img.Width / 2,
                            (float)y + J - img.Height / 2);
                        break;
                }
            }
        }
        /// <summary>
        /// Отображение служебных организаций на Bmp
        /// </summary>
        /// <param name="DataOfBot"></param>
        /// <param name="FloorID"></param>
        public void DrawLandMarksExtra(CachedDataModel DataOfBot, int FloorID)
        {
            var AllOrgsOnThisFloor = DataOfBot.GetOrganizations(DataOfBot.MapObjects.Where(x => x.FloorID == FloorID).ToList());
            var ServiceOrgsOnThisFloor = AllOrgsOnThisFloor.Where(x => DataOfBot.IsServiceOrganizaion(x)).ToList();
            var MapObjectsOnThisFloor = DataOfBot.GetMapObjects(ServiceOrgsOnThisFloor);
            foreach (var mapObj in MapObjectsOnThisFloor.Where(x => x.FloorID == FloorID))
            {
                var cat = ServiceOrgsOnThisFloor.FirstOrDefault(x => x.OrganizationMapObject.Select(y => y.MapObject).Contains(mapObj))?.
                    CategoryOrganization?.FirstOrDefault(x => x.Category?.ServiceCategoryType == ServiceCategoryType.Service || x.Category?.ServiceCategoryType == ServiceCategoryType.Link)?.Category;

                if(cat != null && cat?.CategoryID != null && cat?.LogoExtension != null)
                {
                    Image img = Image.FromFile(ConfigurationManager.AppSettings["ContentPath"] + $"Categories\\{cat.CategoryID}.{cat.LogoExtension}");
                    using (Bitmap newBitmap = new Bitmap(img))
                    {
                        newBitmap.SetResolution(96, 96);
                        img.Dispose();
                        img = ImagingHelper.ResizeImage(newBitmap, 90, 90);
                        DrawLocation(mapObj.LongitudeFixed, mapObj.LatitudeFixed, "default" , img);
                    }
                    img.Dispose();
                }
            }
        }
        /// <summary>
        /// Отображение якорных либо имющих высокий MapSize организаций
        /// </summary>
        /// <param name="organisations"></param>
        /// <param name="FloorID"></param>
        public void DrawLandMarksOrganizations(CachedDataModel dataOfBot, int FloorID)
        {
            var vyborka = dataOfBot.Organizations.Where(x => x.IsAnchor == true && x?.OrganizationMapObject.Where(y => y?.MapObject?.Params != null && y.MapObject.Params.Contains("SignPointRadius")).Count() == 0).ToList();
            var mObjs = dataOfBot.GetMapObjects(vyborka).Where(x => x.FloorID == FloorID && x.Params == null || (x.Params != null && !x.Params.Contains("SignPointRadius"))).ToList();
            DrawSpecialOrgs(mObjs, vyborka);
        }
        /// <summary>
        /// Отображает огранизации с подписью их названий и с черной точков
        /// </summary>
        /// <param name="mapObjects"></param>
        public void DrawSpecialOrgs(List<MapObject> mapObjects, List<Organization> parrentOrganizations)
        {
            using (var gr = Graphics.FromImage(Bmp))
            {
                foreach (var mObj in mapObjects)
                {
                    if (mObj?.Params != "Перейдите на следующий этаж") // для текстового описания. так обозначается переход между этажами
                    {
                        string OrgName = parrentOrganizations.FirstOrDefault(x => x.OrganizationMapObject.Select(y => y?.MapObject).Contains(mObj))?.Name;
                        Image img = Properties.Resources.ShopLocation;
                        img = ImagingHelper.ResizeImage(img, (int)(img.Width * 2 / 2.5F), (int)(img.Height * 2 / 2.5F));
                        DrawLocation(mObj.LongitudeFixed, mObj.LatitudeFixed, "default", img);

                        DrawText(OrgName,
                            mObj.LongitudeFixed / ZoomOfPicture + I,
                            mObj.LatitudeFixed / ZoomOfPicture + J,

                            OrganizationKegel, Color.Black);
                        img.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// Отображает текст, заданного цвета, места, размера и т.д. (если эта подпись картинки, то добавлят кусок к подложке)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="kegel"></param>
        /// <param name="color"></param>
        /// <param name="IsFloor"></param>
        public void DrawText(string text, double longitude, double latitude, float kegel, Color color, bool IsFloor = false)
        {
            text = text.Replace("(", "");
            text = text.Replace(")", "");

            if (IsFloor)
            {
                AddSegmentToBitmap();
            }

            using (var gr = Graphics.FromImage(Bmp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Create font and brush.
                Font drawFont = new Font("open sans", kegel);
                //Font drawFont = new Font("Segoe UI", kegel, FontStyle.Bold);
                SolidBrush drawBrush = new SolidBrush(color);

                // Create point for upper-left corner of drawing.
                PointF drawPoint = new PointF((int)(longitude), (int)(latitude));

                // Set format of string.
                StringFormat drawFormat = new StringFormat();
                drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                // Draw string to Bmp.
                gr.DrawString(text, drawFont, drawBrush, drawPoint, drawFormat);
            }
        }
        /// <summary>
        /// подпись названия для крупных магазинова (большие белые)
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="FloorID"></param>
        public void DrawSignPoint(CachedDataModel dataOfBot, int FloorID, string CustomerName = null) // специально для любимой радуги :3
        {
            var mapObjectsWithSignPoints = dataOfBot.MapObjects.Where(x => x.FloorID == FloorID && x.Params != null && x.Params.Contains("SignPointRadius") && x.Params.Contains("SignText")).ToList();
            var f = dataOfBot.Floors.FirstOrDefault(x => x.FloorID == FloorID);
            foreach (var item in mapObjectsWithSignPoints)
            {
                var sp = JsonConvert.DeserializeObject<SignPoint>(item.Params);
                sp.FixSignPointRadius(item, f);
                GetKegelResult res = ImagingHelper.GetKegel(Bmp, sp.SignText, item.LongitudeFixed, item.LatitudeFixed, sp.SignPointRadiusFixed * 2 / ZoomOfPicture);
                if (!string.IsNullOrWhiteSpace(CustomerName) && CustomerName == "ТРК Радуга") DrawText(sp.SignText, (item.LongitudeFixed / ZoomOfPicture + I) + res.LongitudeDist, (item.LatitudeFixed / ZoomOfPicture + J) - res.LatitudeDist, res.Kegel, Color.Orange);
                DrawText(sp.SignText, (item.LongitudeFixed / ZoomOfPicture + I) + res.LongitudeDist, (item.LatitudeFixed / ZoomOfPicture + J) - res.LatitudeDist, res.Kegel, Color.White);
            }
        }

        public void DrawCurve(List<Point> points)
        {
            using (var gr = Graphics.FromImage(Bmp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                gr.SmoothingMode = SmoothingMode.HighQuality;
                MyPen.DashPattern = new float[] { 5.0F, 2.5F};
                gr.DrawCurve(MyPen, points.ToArray(), 0.04F);
            }
        }

        //штуки для разрабов
        /// <summary>
        /// Рисует все магазины на карте этажа
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="DataOfBot"></param>
        /// <param name="bitmap"></param>
        public void DrawAllShops(MapHelper.Graph graph, CachedDataModel DataOfBot)
        {
            //TODO Что это! Почему vyborka = null
            //var vyborka = DataOfBot.Organizations.Where(x => x.Floor != null && x.Floor.Number == graph.Layers[0].LayerID).ToList();
            
            //Закоментил потому что Null
//            List<Organization> vyborka = null;
//            using (var gr = Graphics.FromImage(Bmp))
//            {
//                gr.SmoothingMode = SmoothingMode.HighQuality;
//                gr.CompositingQuality = CompositingQuality.HighQuality;
//                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
//                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
//                int i = 0;
//                foreach (var org in vyborka)
//                {
//                    if (org.Longitude != null && org.Latitude != null)
//                    {
//                        var temp = new System.Windows.Point((double)org.Longitude, (double)org.Latitude);
//
//                        var vertex = graph.Layers[0].GetVertex(temp);
//                        if (vertex == null)
//                        {
//                            int numofsegments = graph.Layers[0].Segments.Count;
//                            graph.Layers[0].AddVertexWithShortestSegment(temp);
//                            if (numofsegments + 1 == graph.Layers[0].Segments.Count)
//                            {
//                                var Addedsegment = graph.Layers[0].Segments[graph.Layers[0].Segments.Count - 1];
//                                gr.DrawLine(new Pen(Color.Green, 10 / ZoomOfPicture),
//                                    (float)(Addedsegment.Vertex0.Point.X) / ZoomOfPicture + I,
//                                    (float)(Addedsegment.Vertex0.Point.Y) / ZoomOfPicture + J,
//                                    (float)(Addedsegment.Vertex1.Point.X) / ZoomOfPicture + I,
//                                    (float)(Addedsegment.Vertex1.Point.Y) / ZoomOfPicture + J);
//
//                                graph.Layers[0].Segments.RemoveAt(graph.Layers[0].Segments.Count - 1);
//                            }
//                            else
//                            {
//                                var Addedsegment = graph.Layers[0].Segments[graph.Layers[0].Segments.Count - 3];
//
//                                gr.DrawLine(new Pen(Color.Green, 10 / ZoomOfPicture),
//                                    (float)(Addedsegment.Vertex0.Point.X) / ZoomOfPicture + I,
//                                    (float)(Addedsegment.Vertex0.Point.Y) / ZoomOfPicture + J,
//                                    (float)(Addedsegment.Vertex1.Point.X) / ZoomOfPicture + I,
//                                    (float)(Addedsegment.Vertex1.Point.Y) / ZoomOfPicture + J);
//
//                                graph.Layers[0].Segments.RemoveAt(graph.Layers[0].Segments.Count - 3);
//                            }
//                            i++;
//                        }
//                        DrawLocation((float)org.Longitude, (float)org.Latitude, "", Properties.Resources.Shop);
//                    }
//                }
//            }
        }
        /// <summary>
        /// Рисует все пути на карте этажа
        /// </summary>
        /// <param name="dataOfBot"></param>
        /// <param name="FloorNumber"></param>
        /// <returns></returns>
        public BitmapSettings DrawAllWaysAndAllShops(CachedDataModel dataOfBot, int FloorNumber)
        {
            var bitmap = new BitmapSettings(new Bitmap(Image.FromFile(ConfigurationManager.AppSettings["ContentPath"] + $"Floors\\{dataOfBot.Floors.FirstOrDefault(x => x.Number == FloorNumber).FloorID}.{dataOfBot.Floors.FirstOrDefault(x => x.Number == FloorNumber).FileExtension}")));

            var mbMapHelper = new BotMapHelper();
            var graph = mbMapHelper.GetOneLayer(dataOfBot, dataOfBot.Floors.FirstOrDefault(x => x.Number == FloorNumber));
            DrawAllShops(graph, dataOfBot);
            var layer = graph.Layers[0];

            using (var gr = Graphics.FromImage(bitmap.Bmp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                foreach (var item in layer.Segments)
                {
                    gr.DrawLine(bitmap.MyPen,
                        (float)(item.Vertex0.Point.X) / ZoomOfPicture + bitmap.I,
                        (float)(item.Vertex0.Point.Y) / ZoomOfPicture + bitmap.J,
                        (float)(item.Vertex1.Point.X) / ZoomOfPicture + bitmap.I,
                        (float)(item.Vertex1.Point.Y) / ZoomOfPicture + bitmap.J);
                }
            }
            DrawLandMarksExtra(dataOfBot, FloorNumber);

            var tmp = $"Этаж {FloorNumber.ToString()}   {dataOfBot.Customers[0].Name} {dataOfBot.Customers[0].LocaleCity}";
            DrawText(tmp, BotTextHelper.LengthOfString(tmp, bitmap), 5F, 23, Color.DarkSlateGray, true);

            return bitmap;
        }
    }
}
