﻿using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.Helpers;
using Moloko.Utils;
using System.Drawing;

namespace Mall.Test
{
    class Program
    {

        static void Main(string[] args)
        {
            Draw();
        }
        static void Draw()
        {
            string path = @"C:\MOLOKO.Backup\Floors\23.png";//ConfigurationManager.AppSettings["ContentPath"] + $"Floors\\{f.FloorID}.{f.FileExtension}";
            int floorID = 23;
            var bitmap = new BitmapSettings(new Bitmap(Image.FromFile(path)), floorID);
            var img = Image.FromFile(@"C:\Git\Mall.Bot\Mall.Bot.Tests\Mall.Bot.Test\Resources\Shop.png");
            MapObject mObj = new MapObject();
            Floor floor = new Floor();
            mObj.Longitude = 60.505823849213;
            mObj.Latitude = 56.82443852894;

            //mObj.Longitude = (56.820858302078406 - 56.82621855007867)/2  + 56.820858302078406;
            //mObj.Latitude = (60.498856113983154 - 60.51062564714051) / 2 + 60.51062564714051;

            //56.823765, 60.505341
            //floor.ImportMetadata = "60.498856113983154;56.820858302078406;60.51062564714051;56.82621855007867";
            floor.ImportMetadata = "60.498856113983154;56.82621855007867;60.51062564714051;56.820858302078406";
            floor.Width = 4352;
            floor.Height = 3584;

            mObj.FixCoords(floor);

            img = ImagingHelper.ResizeImage(img, 110, 110);
            System.IO.File.WriteAllBytes(@"C:\Temp\vk_m.png", ImagingHelper.ImageToByteArray(img));

            bitmap.DrawLocation(mObj.LongitudeFixed, mObj.LatitudeFixed, "default", img);

            System.IO.File.WriteAllBytes(@"C:\Temp\vk.png", ImagingHelper.ImageToByteArray(bitmap.Bmp));
        }

        private static void doWork()
        {
            var trans = new OldMallToNewMallTransformation("A", 3);
            trans.MoveLogoToFileSystem(@"C:\MOLOKO.Backup\Organizations\");
        }
    }
}