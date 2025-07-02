namespace APIGateWay.Entities
{
    public class UserLike
    {
        public App_User SourceUser { get; set; }
        public int SourceUserId { get; set; }
        public App_User TargetUser { get; set; }
        public int TargetUserId { get; set; }
    }
}
