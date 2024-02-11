using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompSol.API.Models
{
    public class SystemUsersDTO
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DateLast { get; set; }
        public string DateCreated { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEditor { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}
