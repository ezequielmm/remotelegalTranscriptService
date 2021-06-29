namespace PrecisionReporters.MediaService.Data.Models
{
    public class User : BaseEntity<User>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public bool IsAdmin { get; set; }
    }
}
