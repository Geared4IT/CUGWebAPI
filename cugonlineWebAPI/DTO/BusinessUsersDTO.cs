using System;
 

namespace cugonlineWebAPI.Controllers
{
    public class BusinessUsersDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }//generated
        public string CellNumber { get; set; }//primary
        public string UnlockCode { get; set; }//4 digit number
        public string BackupCellNumber { get; set; }//optional
        public string UUID { get; set; }

        public string CompanyCodePrimary { get; set; }//entered on registration
        ///TODO: Create UserCompanies linking table
        public string Email { get; set; }//optional
        public string Password { get; set; }//optional
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastSyncDate { get; set; }//Last login...
        public string ErrorMessage { get; set; }
        //Roles
        // - Admin / Cashier / Stock Control

    }
}