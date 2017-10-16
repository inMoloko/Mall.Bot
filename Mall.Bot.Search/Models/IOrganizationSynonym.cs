namespace Mall.Bot.Search.Models
{
    public interface IOrganizationSynonym
    {
        int OrganizationSynonymID { get; set; }
        string OrganizationName { get; set; }
        string Synonyms { get; set; }
    }
}
