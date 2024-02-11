using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

using CompSol.API.Models;
using System.Linq;

namespace CompSol.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        //testEntities testDB = new testEntities();

        //[Route("SystemUsers")]
        [HttpGet]
        public List<SystemUsersDTO> GetSystemUsers()
        {
            try
            {
                //var result = testDB.UserMasters.Select(u => new SystemUsersDTO
                //{
                //    Id = u.ID,
                //    Name = u.Name.ToUpper(),
                //    Surname = u.Surname.ToUpper(),
                //    CategoryName = u.categoryN.Trim(),
                //    DateCreated = u.Date_added,
                //    DateLast = u.Date_last,
                //    UserName = u.UserName,
                //    Password = u.Password,
                //    Email = u.Email,
                //    IsEditor = (u.nKey == "1") ? true : false,
                //    IsSuperAdmin = u.isSuperAdmin ?? false,
                //    IsDeleted = u.IsDeleted ?? false
                //}).ToList();

                //if (result != null)
                //{
                //    return result;
                //}
                //else
                    return null;

            }
            catch (Exception ex)
            {
                return new List<SystemUsersDTO>();
            }
        }
    }

    
}
