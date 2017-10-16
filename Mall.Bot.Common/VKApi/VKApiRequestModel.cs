namespace Mall.Bot.Common.VKApi
{
    public enum RequestType
    {
        SendMessage = 1,
        SendMessageWithPhoto
    }
    public class VKApiRequestModel
    {
        public RequestType Type { get; set; }
        public ulong User_ID { get; set; }
        public string Message { get; set; }
        public byte [] Photo { get; set; }

        public VKApiRequestModel(ulong userID, string message, RequestType type = RequestType.SendMessage, byte [] photo = null)
        {
            User_ID = userID;
            Message = message;
            Type = type;
            Photo = photo;
        }
    }
}
