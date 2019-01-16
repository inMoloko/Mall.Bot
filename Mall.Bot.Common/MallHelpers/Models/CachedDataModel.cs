using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Linq;
using Mall.Bot.Search.Models;

namespace Mall.Bot.Common.MallHelpers.Models
{
    public class CachedDataModel
    {
        /// <summary>
        /// Торговые центры
        /// </summary>
        public List<Customer> Customers { get; set; }
        /// Список организаций
        /// </summary>
        public List<Organization> Organizations { get; set; }
        /// <summary>
        /// Список этажей
        /// </summary>
        public List<Floor> Floors { get; set; }
        /// <summary>
        /// Словарь синонимов по организациям
        /// </summary>
        public List<OrganizationSynonym> Synonyms { get; set; }
        /// <summary>
        /// Категории
        /// </summary>
        public List<Category> Categories { get; set; }
        /// <summary>
        /// текстовочки!
        /// </summary>
        public List<BotText> Texts { get; set; }
        /// <summary>
        /// Точки на карте
        /// </summary>
        public List<MapObject> MapObjects { get; set; }
        /// <summary>
        /// справочник
        /// </summary>
        public List<OrganizationMapObject> OrganizationMapObjects { get; set; }
        /// <summary>
        /// Переходы
        /// </summary>
        public List<MapObjectLink> MapObjectLinks { get; set; }
        /// <summary>
        /// терминалы
        /// </summary>
        public List<MTerminal> MTerminals { get; set; }
        /// <summary>
        /// терминалы
        /// </summary>
        public List<TerminalMapObject> TerminalMapObjects { get; set; }

        public CachedDataModel(MallBotContext dbContext)
        {
            OrganizationMapObjects = dbContext.OrganizationMapObject.Include(nameof(MapObject)).ToList();
            TerminalMapObjects = dbContext.TerminalMapObject.Include(nameof(MapObject)).ToList();

            Organizations = dbContext.Organization.Include("CategoryOrganization.Category").Include("OrganizationMapObject.MapObject").Where(x => !x.CategoryOrganization.Select(y => y.Category.ServiceCategoryType).Contains(ServiceCategoryType.Terminal) && 
                x.OrganizationMapObject.Count != 0 &&
                x.Name != null && x.IsUsed != false).OrderByDescending(x => x.Rating).ToList();

            MTerminals = dbContext.MTerminal.Include("TerminalMapObject.MapObject").ToList();
            Synonyms = dbContext.OrganizationSynonym.Where(x => x.Synonyms != null).ToList();
            Categories = dbContext.Category.Where(x => x.IsUsed && x.Name != null && x.CustomerID != null).ToList();
            Floors = dbContext.Floor.Where(x => x.Paths != null && x.CustomerID != null && x.FileExtension != null && x.Type == null).OrderBy(x => x.Number).ToList();
            Customers = dbContext.Customer.ToList();
            MapObjects = dbContext.MapObject.Where(x => x.Longitude >= -180 && x.Longitude <= 180 && x.Latitude >= -90 && x.Latitude <= 90 && x.Floor.Type == null).ToList();
            MapObjectLinks = dbContext.MapObjectLink.ToList();

            var temp = dbContext.BotCustomersSetting.ToList();
            foreach (var item in Customers)
            {
                //var thisSystemSetting = dbContext.SystemSetting.FirstOrDefault(x => x.CustomerID == item.CustomerID && x.SystemSettingTypeID == dbContext.SystemSettingType.FirstOrDefault(y => y.Name == "TUTORIAL_DATA").SystemSettingTypeID);
                //List<SystemSettingHistory> ssHistories = null;
                //if (thisSystemSetting != null) ssHistories = dbContext.SystemSettingHistory.Where(x => x.SystemSettingID == thisSystemSetting.SystemSettingID).OrderByDescending(x => x.ModifiedDate).ToList();

                var tmp = temp.FirstOrDefault(x => x.CustomerID == item.CustomerID);
                if(tmp == null)
                {
                    throw new Exception($"Нет настроект для торгового центра CustomerID: {item.CustomerID}");
                }
                item.IsPublish = tmp.IsPublish;
                item.Location = DbGeography.FromText($"POINT({tmp.GeoLongitude ?? "-161"} {tmp.GeoLatitude ?? "1"})");
            }

            foreach (var item in MapObjects)
                item.FixCoords(Floors.FirstOrDefault(x => x.FloorID == item.FloorID));

            foreach (var item in Floors.Where(x => x.ImportMetadata != null && x.Width != null && x.Height != null))
                item.FixPaths();

            Texts = dbContext.BotText.ToList();
        }

        

        public CachedDataModel()
        {
        }


        public Organization GetOrganization(MapObject item)
        {
            return Organizations.FirstOrDefault(x => x.OrganizationMapObject.Select(z => z.MapObjectID).Contains(item.MapObjectID));
        }

        public List<Organization> GetOrganizations(List<MapObject> input)
        {
            var res = new HashSet<Organization>();
            foreach (var item in input)
            {
                var curOrg = Organizations.FirstOrDefault(x => x.IsUsed != false && x.OrganizationMapObject.Select(z => z.MapObjectID).Contains(item.MapObjectID));
                if(curOrg != null) res.Add(curOrg);
            }
            return res.ToList();
        }
        public List<MapObject> GetMapObjects(List<Organization> list)
        {
            var res = new List<MapObject>();
            foreach (var item in list)
            {
                var orgmobj = item.OrganizationMapObject.Select(x => x.MapObject);
                res.AddRange((orgmobj));
            }
            return res;
        }
        public bool IsServiceOrganizaion(Organization org)
        {
            if (org != null && org?.CategoryOrganization != null)
                foreach (var item in org?.CategoryOrganization)
                {
                    if(item != null)
                        if (item?.Category != null)
                            if (item?.Category?.ServiceCategoryType == ServiceCategoryType.Service || item?.Category?.ServiceCategoryType == ServiceCategoryType.Link) return true;
                    if (item?.Category?.ParentID != null)
                    {
                        var tmp = Categories.FirstOrDefault(x => x.CategoryID == item?.Category?.ParentID)?.ServiceCategoryType;
                        if (tmp == ServiceCategoryType.Service || tmp == ServiceCategoryType.Link) return true;
                    }
                }
            return false;
        }

        public MTerminal GetTerminal(MapObject from)
        {
            return MTerminals.FirstOrDefault(x => x.TerminalMapObject.Select(z => z.MapObjectID).Contains(from.MapObjectID));
        }
    }
}
