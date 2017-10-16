namespace Mall.Bot.Common.Helpers
{
 
    public enum InputDataType
    {
        GeoLocation = 1,
        Image,
        Text,
    }
    public enum CardBotWhatIsHappeningNow
    {
        Start = 1,
        SearchCard,
        AddCard,
        SetCardName,
        SetIsShared
    }


    public enum MallBotWhatIsHappeningNow
    {
        SettingCustomer = 1,
        SearchingOrganization,
        SearchingWay,
        GettingAllOrganizations
    }

    public enum MFCBotWhatIsHappeningNow
    {
        SettingOffice = 1,
        SettingOpportunity,
        SettingService,
        GetingTicketInformation,
        QueueWaiting,
    }

    public enum VodBotWhatIsHappeningNow
    {
        DoNothing = 1,
        AddPhoto,
        SetIndication,
        SetProducerName,
    }

    public enum AttachmentType
    {
        image = 1,
        audio,
        video,
        file,
        template,
        location
    }
    public enum ContentType { text, location}
    public enum ObjectTypes
    {
        user = 1,
        page,
        permissions,
        payments
    }
    public enum SenderActionType
    {
        typing_on = 1,
        typing_off = 2,
        mark_seen = 3,
    }
    public enum SocialNetworkType
    {
        VK = 1,
        Telegram,
        Facebook
    }
    public class TypesHelper
    {
    }
}