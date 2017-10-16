using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.DBHelpers.Models.MFCModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Bot.Common.MFCHelpers.Models
{
    public class MFCBotModel
    {
        public List<Office> Offices { get; set; }
        //public List<Service> Services{ get; set; } // исключил услуги за ненадобностью. Оставил в коде на всякий случай
        public List<Section> Sections = new List<Section>();
        public List<Section> AllSections { get; set; }
        public List<SectionOffice> SectionOffices { get; set; }
        public List<BotText> Texts { get; set; }
        public List<BotJoke> Jokes { get; set; }
        public List<WindowsOffice> WindowsOffices { get; set; }

        public MFCBotModel(MFCBotContext dbContext)
        {
            SectionOffices = dbContext.SectionOffice.ToList();
            
            Offices = dbContext.Office.Where(x => (x.IsActive != null && x.TerritoryID != null && (bool)x.IsActive) || x.Name == "Ярмарка").ToList();

            var supportedOffices = SectionOffices.Select(y => y.OfficeID);

            Offices = Offices.Where(x => supportedOffices.Contains(x.OfficeID)).ToList();

            //Services = dbContext.Service.DistinctBy(x => x.DisplayName).OrderByDescending(x => x.Priority).ToList(); // исключил услуги за ненадобностью. Оставил в коде на всякий случай
            //foreach (var item in Services)
            //{
            //    item.DisplayName = item.DisplayName.Replace("\r", "");
            //    item.DisplayName = item.DisplayName.Replace("\n", "");
            //    item.DisplayName = item.DisplayName.Replace(Environment.NewLine, "");
            //}

            AllSections = dbContext.Section.Where(x => x.IsActive).OrderByDescending(x => x.Rating).ToList();
            foreach (var item in Sections)
            {
                item.Name = item.Name.Replace("\r", "");
                item.Name = item.Name.Replace("\n", "");
                item.Name = item.Name.Replace(Environment.NewLine, "");
            }

            Texts = dbContext.BotText.ToList();
            Jokes = dbContext.BotJoke.ToList();

            WindowsOffices = dbContext.WindowsOffice.ToList();
        }
        /// <summary>
        /// Оставляет только конечные кнопки, у которых нет потомков
        /// </summary>
        public void RetainLeafs()
        {
            var result = new List<Section>();
            if(Sections != null && Sections.Count != 0)
            {
                Sections = Sections.Where(z => !Sections.Select(x => x.ParentID).Contains(z.SectionID)).ToList();
            }
        }
    }
}
