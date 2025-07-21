using System.ComponentModel.DataAnnotations.Schema;

namespace APIGateWay.Entities
{
    [Table("Photos")]
    public class Photo
    {       
        public int Id { get; set; }
        public string Url { get; set; }
        public Boolean IsMain { get; set; }
        public bool IsApproved { get; set; }
        public string PublicId { get; set; }
        public int AppUserId { get; set; }
        public App_User AppUser { get; set; }
       // public int AppUserId { get; set; }
    }
}