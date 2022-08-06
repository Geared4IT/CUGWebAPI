namespace cugonlineWebAPI.VM
{
    public class ImportContacts
    {
        public long ImportContactId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string Industry { get; set; }
        public string ContactNumbers { get; set; }
        public string OtherNumbers { get; set; }
        public string CellNumbers { get; set; }
        public string EmailAddress { get; set; }
        public string WebAddress { get; set; }
        public bool IsProcessed { get; set; }
    }
}