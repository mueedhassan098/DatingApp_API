namespace APIGateWay.Helpers
{
    public class MessageParams:PaginationParams
    {
        public string Username { get; set; }
        public string Container { get; set; } = "Unread"; // Unread, Read, or All
       //  public int CurrentUserId { get; set; } // Added to filter messages for the current user
    }
}
