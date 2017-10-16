using Mall.Bot.Common.DBHelpers.Models.MFCModels;
using Mall.Bot.Common.DBHelpers.Models.MFCModels.ScheduleModels;
using Mall.Bot.Common.Helpers;
using MFC.Bot.WebService.Contracts;
using Moloko.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mall.Bot.Common.MFCHelpers
{
    public enum GetTicketInformationErrors
    {
        UnknownError, //Internal Error from AIS
        NoExist, //талон с таким номером не был обслужан
        Ended, // завершено обслуживание
        Waiting, // находиттся в оперативной очереди
        Default, // иначе
        InProces, // обслуживается
        Absence, // неявка
    }

    public class Analiser
    {
        /// <summary>
        /// Анализирует полезные данные для генерации талона. Генерирует талон - строку, которая будет отправлена пользователю
        /// </summary>
        /// <param name="cacheHelper"></param>
        /// <param name="talon"></param>
        /// <param name="message"></param>
        /// <param name="serviceName"></param>
        /// <param name="office"></param>
        /// <returns></returns>
        public static string AnaliseTalon(EnquequeResponse talon, string message, string serviceName = null, DBHelpers.Models.MFCModels.Office office = null)
        {
            if (office != null) message = message.Replace("%office%", office.DisplayName);

            message = message.Replace("%number%", BotTextHelper.GetVKSmileNumber(talon.ID));
            if (serviceName != null) message = message.Replace("%servicename%", serviceName);
            //Дата в талоне
            if (talon.TimeOfReceipt == null || talon.TimeOfReceipt.Year == 0001) message = message.Replace("%time%", DateTime.Now.ToString("HH:mm  dd.MM.yyyy"));
            else message = message.Replace("%time%", talon.TimeOfReceipt.ToString("HH:mm  dd.MM.yyyy"));
            //перед вами в очереди
            message = message.Replace("%queuecount%", talon.QueueSize?.toWindows.ToString());
            //офис в котором юзер
            if (office != null) message = message.Replace("%adress%", office.DisplayAddress);
            return message;
        }
        /// <summary>
        /// Возвращает true, если офис открыт, false - если закрыт, null - если данных недостоточно
        /// </summary>
        /// <param name="Schedule"></param>
        /// <returns></returns>
        public static bool? OfficeIsOpen(Schedule Schedule)
        {
            if (Schedule == null || Schedule.WeekSchedule.Count == 0 || Schedule.WeekSchedule.First().WeekDaySchedule.Count == 0)
                return null;

            var date = DateTime.Now;
            var ws = Schedule.WeekSchedule.First().WeekDaySchedule.FirstOrDefault(e => e.DayOfWeek == (byte)date.DayOfWeek);
            if (ws == null)
                return null;
            return ws.ScheduleItem.IsDayOff ? false : ws.ScheduleItem.ScheduleWorktime.Count(e => date.TimeOfDay >= e.StartTime && date.TimeOfDay <= e.EndTime) > 0;
        }
        /// <summary>
        /// Возвращает енум == IdState of Ticket
        /// </summary>
        /// <param name="Talon"></param>
        /// <param name="officeID"></param>
        /// <param name="talonID"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static GetTicketInformationErrors CheckUsersTalonNumber(out EnquequeResponse Talon, int officeID, int talonID)
        {
            Talon = MainAnswerHelper.mfcservice.GetTicketInformation(officeID, talonID);
            //Talon = new EnquequeResponse { ID = talonID, stateTicket = new StateTicket { idState = 5, description = "обслуживаетмся в окне 281" },services = new MFC.Bot.WebService.Contracts.Service[] { new MFC.Bot.WebService.Contracts.Service { name = "Услуга"} } };
            if (Talon == null) return GetTicketInformationErrors.UnknownError;
            if (string.IsNullOrWhiteSpace(Talon?.stateTicket?.description)) return GetTicketInformationErrors.NoExist;

            if (Talon?.stateTicket?.idState != null && Talon?.stateTicket.idState == 4) return GetTicketInformationErrors.Ended;
            if (Talon?.stateTicket?.idState != null && Talon?.stateTicket?.idState == 5) return GetTicketInformationErrors.Waiting;
            if (Talon?.stateTicket?.idState != null && Talon?.stateTicket?.idState == 2) return GetTicketInformationErrors.InProces;
            if (Talon?.stateTicket?.idState != null && Talon?.stateTicket?.idState == 1) return GetTicketInformationErrors.Absence;
            return GetTicketInformationErrors.Default;
        }
        /// <summary>
        /// Возвращает текст, соответствующий статусу талона с запрошенным номером
        /// </summary>
        /// <param name="botUser"></param>
        /// <param name="office"></param>
        /// <param name="TalonID"></param>
        /// <param name="service"></param>
        /// <param name="textHelper"></param>
        /// <returns></returns>
        public static string GetAnalysedAnswer(DBHelpers.Models.MFCModels.BotUser botUser, DBHelpers.Models.MFCModels.Office office, int TalonID, BotTextHelper textHelper, List<WindowsOffice> windows)
        {
            var talon = new EnquequeResponse();
            switch (CheckUsersTalonNumber(out talon, botUser.OfficeID, TalonID))
            {
                case GetTicketInformationErrors.UnknownError:
                    return textHelper.GetMessage("%ginfuncknow%",
                                    new string[] { "%officename%", "%adress%" },
                                    new string[] { office.DisplayName, office.DisplayAddress });
                case GetTicketInformationErrors.NoExist:
                    return textHelper.GetMessage("%ginfnoexist%",
                                    new string[] { "%officename%", "%adress%" },
                                    new string[] { office.DisplayName, office.DisplayAddress });
                case GetTicketInformationErrors.Ended:
                    return textHelper.GetMessage("%ginfended%",
                                    new string[] { "%officename%", "%adress%" },
                                    new string[] { office.DisplayName, office.DisplayAddress });
                case GetTicketInformationErrors.Absence:
                    return textHelper.GetMessage("%ginfabsebce%",
                                    new string[] { "%officename%", "%adress%" },
                                    new string[] { office.DisplayName, office.DisplayAddress });
                case GetTicketInformationErrors.Waiting:
                    var s = AnaliseTalon(talon, textHelper.GetMessage("%getinfo%")+ "\\r\\n\\r\\n"+ textHelper.GetMessage("%queue%"), BotTextHelper.DecodeToUtf8(talon.services[0].name), office);
                    //сохраняем информацию по талону => делаем его выбранным пользователем
                    botUser.TalonID = TalonID;
                    botUser.ServiceID = talon.services[0].id;
                    botUser.NowIs = MFCBotWhatIsHappeningNow.QueueWaiting;
                    s += "\\r\\n\\r\\n" + textHelper.GetMessage("%statustext%");
                    Logging.Logger.Debug(s);
                    return s;
                case GetTicketInformationErrors.InProces:
                    string usefulInf = BotTextHelper.DecodeToUtf8(talon.stateTicket.description);
                    //получаем последнее число из строки
                    string windowStringID = Regex.Match(usefulInf, @"\d+(?!\D*\d)").Value;
                    int windowID = int.Parse(windowStringID);
                    usefulInf = usefulInf.Replace(windowStringID, windows.FirstOrDefault(x => x.WindowID == windowID).Number.ToString());

                    s = $"\U00002705 {usefulInf}\\r\\n\\r\\n";
                    s += AnaliseTalon(talon, textHelper.GetMessage("%getinfo%"), BotTextHelper.DecodeToUtf8(talon.services[0].name), office)+ "\\r\\n\\r\\n";
                    s += textHelper.GetMessage("%helptext%");
                    //Обслуживается => забываем о пользователе. Сессия окончена
                    botUser.OfficeID = 0;
                    botUser.TalonID = 0;
                    botUser.ServiceID = 0;
                    botUser.NowIs = MFCBotWhatIsHappeningNow.SettingOffice;
                    Logging.Logger.Debug(s);
                    return s;
                default:
                    //начальное сообщение
                    return textHelper.GetMessage("%gtinfstart%",
                                    new string[] { "%officename%", "%adress%", "%business%" },
                                    new string[] { office.DisplayName, office.DisplayAddress, MainAnswerHelper.GetBusynessOffice(office.OfficeID) });
            }
        }
    }
}