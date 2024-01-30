namespace EntityFrameworkCore.Domain
{
    public class Team : BaseDomainModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Coach Coach { get; set; }
    }
}
