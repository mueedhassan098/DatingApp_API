namespace APIGateWay.Dtos
{
    public class AddForPhotoApprovalDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Url { get; set; }
        public bool IsApproved { get; set; }
        //public bool ApprovedBy { get; set; }
    }
}
