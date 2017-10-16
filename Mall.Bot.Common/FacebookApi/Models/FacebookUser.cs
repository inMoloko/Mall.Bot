namespace Mall.Bot.Common.FacebookApi.Models
{
    public class FacebookUser
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string locale { get; set; }
        public string gender { get; set; }

        public override string ToString()
        {
            return first_name+" "+last_name;
        }
    }
}