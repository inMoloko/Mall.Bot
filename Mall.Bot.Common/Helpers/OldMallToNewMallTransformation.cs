using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Portal.Tasks;
using Moloko.Utils;
using System;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Mall.Bot.Common.Helpers
{
    public class OldMallToNewMallTransformation
    {
        public MallBotContext _context { get; set; }
        private int _customerID { get; set; }

        public OldMallToNewMallTransformation(string dbName, int customerID)
        {
            _context = new MallBotContext(dbName);
            _customerID = customerID;
        }

        public void ConvertPixelsToGeo()
        {
            try
            {
                var hpr = new PixelToGeoTransformation(_context);
                hpr.FixMapObjects(_customerID);
                Logging.Logger.Debug("MapObjects has been converted");
                hpr.FixFloorPaths(_customerID);
                Logging.Logger.Debug("Floor Paths has been converted");
            }
            catch(Exception exc)
            {
                Logging.Logger.Error(exc);
            }
        }

        public void MoveLogoToFileSystem(string organisationImagesPath)
        {
            try
            {
                var orgs = _context.Organization.Where(x => x.CustomerID == _customerID && x.Logo != null 
                && _context.OrganizationMapObject.Where(y => y.OrganizationID == x.OrganizationID).Count() > 0).ToArray();
                var orginmgs = _context.OrganizationImage.ToList();

                for (int i = 0; i < orgs.Count(); i++)
                {
                    var img = Image.FromStream(new MemoryStream(orgs[i].Logo));
                    if (orginmgs.FirstOrDefault(x => x.OrganizationID == orgs[i].OrganizationID && x.Type == "logo") == null){

                        
                        var oim = new OrganizationImage { OrganizationID = orgs[i].OrganizationID, Type = "logo", Extension = "png", Width = img.Width, Height = img.Height };
                        orginmgs.Add(oim);
                        _context.Entry(oim).State = EntityState.Added;
                        _context.SaveChanges();
                    }

                    img.Save(organisationImagesPath + $"{orgs[i].OrganizationID}_logo.png", ImageFormat.Png);
                }
                Console.WriteLine("ok");
            }
            catch(Exception exc)
            {
                Console.WriteLine("Err");
                Logging.Logger.Error(exc);
            }
            Console.ReadLine();
        }
    }
}
