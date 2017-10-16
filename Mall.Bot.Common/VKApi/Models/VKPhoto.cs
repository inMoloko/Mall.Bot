namespace Mall.Bot.Common.VKApi.Models
{
    public class VKPhoto
    {
        public long id { get; set; }
        public long album_id { get; set; }
        public long owner_id { get; set; }
        public string photo_75 { get; set; }
        public string photo_130 { get; set; }
        public string photo_604 { get; set; }
        public string photo_807 { get; set; }
        public string photo_1280 { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string text { get; set; }
        public string date { get; set; }

        public string PhotoUrl
        {
            get
            {
                if (photo_1280 != null) return photo_1280;
                if (photo_807 != null) return photo_807;
                if (photo_604 != null) return photo_604;
                if (photo_130 != null) return photo_130;
                if (photo_75 != null) return photo_75;
                return null;
            }
        }
    }
}
