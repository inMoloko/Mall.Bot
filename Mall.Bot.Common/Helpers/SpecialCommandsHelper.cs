using Mall.Bot.Common.DBHelpers;
using Mall.Bot.Common.DBHelpers.Models;
using Mall.Bot.Common.MallHelpers.Models;
using Mall.Bot.Common.VKApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mall.Bot.Common.Helpers
{
    public class SpecialCommandsHelper
    {
        public BotUser botUser;
        private BotTextHelper textHelper;

        public SpecialCommandsHelper(BotUser _botUser, BotTextHelper _textHelper)
        {
            botUser = _botUser;
            textHelper = _textHelper;
        }

        /// <summary>
        /// Выполение команд, предназначенных разработчикам
        /// </summary>
        /// <param name="thisQuery"></param>
        /// <param name="type"></param>
        /// <param name="Bot"></param>
        /// <param name="botUser"></param>
        /// <param name="trimmedLoweredQuery"></param>
        /// <param name="DataOfBot"></param>
        /// <param name="Requests"></param>
        /// <returns></returns>
        public async Task<int> doDevelopersCommands(/*logging*/ BotUserQuery thisQuery/*where*/, SocialNetworkType type, object Bot, /*data*/ string trimmedLoweredQuery, CachedDataModel DataOfBot, /*other*/ List<VKApiRequestModel> Requests = null)
        {
            var sendHelper = new ApiRouter(type,Bot, botUser, Requests);
            try
            {
                var cmndHelper = new CommandAnswerHelper();
                bool ThisCommandIsNotExist = true;

                if (trimmedLoweredQuery.Contains("route"))
                {
                    ThisCommandIsNotExist = false;
                    int floornumder = 0;
                    switch (cmndHelper.RouteAnalise(trimmedLoweredQuery, DataOfBot, out floornumder))
                    {
                        case 1:
                            var dh = new DrawHelper(new MallHelpers.Models.CachedDataModel(),null,null);
                            //var result = dh.DrawAllWaysAndAllShops(DataOfBot, floornumder);
                            //await sendHelper.BotSendPhoto(result.Bmp);
                            break;
                        case 2:
                            await sendHelper.SendText("Этажа с таким номером нет в выбранном вами торговом центре. /place");
                            break;
                        case 3:
                            await sendHelper.SendText("Синтаксическая ошибка. Используйте /testfunc_route_=номер этажа=");
                            break;
                    }
                }

                if (ThisCommandIsNotExist)
                {
                    await sendHelper.SendText("Такой команды нет. Доступные команды: \\r\\n\\r\\n/testfunc_route_1 - где 1 - любой номер этажа");
                }
            }
            catch
            {
                await sendHelper.SendText("testfunc_функция_параметр  !!!");
            }
            return 1;
        }

        /// <summary>
        /// Выполение команд, предназначенных для детальной работы с Тогрговыми Центрами
        /// </summary>
        /// <param name="thisQuery"></param>
        /// <param name="type"></param>
        /// <param name="Bot"></param>
        /// <param name="botUser"></param>
        /// <param name="trimmedLoweredQuery"></param>
        /// <param name="DataOfBot"></param>
        /// <param name="Requests"></param>
        /// <returns></returns>
        public async Task<Customer> doSetSettingsAboutMallsCommands(/*logging*/ BotUserQuery thisQuery/*where*/, SocialNetworkType type, object Bot, /*data*/ string trimmedLoweredQuery, MallBotContext dbMainContext, /*other*/ List<VKApiRequestModel> Requests = null)
        {
            var sendHelper = new ApiRouter(type, Bot, botUser, Requests);
            try
            {
                if (trimmedLoweredQuery.Contains("starttestmode"))
                {
                    botUser.IsTestMode = 1;
                    botUser.CustomerCompositeID = "empty";
                    botUser.ModifiedDate = DateTime.Now;
                    dbMainContext.SaveChanges();

                    await sendHelper.SendText(textHelper.GetMessage("%starttestmode%"));
                    return null;
                }

                if (trimmedLoweredQuery.Contains("endtestmode"))
                {
                    botUser.IsTestMode = 0;
                    botUser.CustomerCompositeID = "empty";
                    botUser.ModifiedDate = DateTime.Now;
                    dbMainContext.SaveChanges();

                    await sendHelper.SendText(textHelper.GetMessage("%endtestmode%"));
                    return null;
                }

                if (trimmedLoweredQuery.Contains("place"))
                {
                    var cmndHelper = new CommandAnswerHelper();
                    Customer thisCustomer = null;
                    switch (cmndHelper.PlaceAnalise(trimmedLoweredQuery, out thisCustomer))
                    {
                        case 1:
                            botUser.CustomerCompositeID = trimmedLoweredQuery.Split('_')[2].ToUpper();
                            botUser.ModifiedDate = DateTime.Now;
                            dbMainContext.SaveChanges();
                            await sendHelper.SendText( $"Ура! Вы выбрали «{thisCustomer.Name}»");
                            return thisCustomer;
                        case 2:
                            await sendHelper.SendText("Торгового центра с таким ключом не существует!");
                            return null;
                        case 3:
                            await sendHelper.SendText("Синтаксическая ошибка. Используйте /mallset_place_=id= \\r\\nгде =id= - составной ключ. Например, /mallset_place_A1");
                            return null;
                        case 4:
                            await sendHelper.SendText("Ошибка приложения. Попробуйте выполнить команду еще раз");
                            return null;
                    }
                    return null;
                }

                await sendHelper.SendText("Такой команды нет. Доступные команды: \\r\\n\\r\\n/mallset_starttestmode \\r\\n/mallset_endtestmode");
                return null;
            }
            catch
            {
                await sendHelper.SendText("mallset_функция  !!!");
                return null; // ошибка
            }
            
        }
    }
}
