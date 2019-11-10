using System.Web.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using cugonlineWebAPI.Models;
using cugonlineWebAPI.VM;

namespace cugonlineWebAPI.Controllers
{
    [RoutePrefix("Api/login")]
    public class LoginController : ApiController
    {
        testEntities cugDB = new testEntities();

        [Route("InsertUser")]
        [HttpPost]
        public object InsertUser(Register Reg)
        {
            try
            {
                UserMaster u = new UserMaster();
                if (u.ID == 0)
                {
                    u.Email = Reg.Email;
                    //u.ID = Reg.Id;
                    u.Name = Reg.Name;
                    u.Password = Reg.Password;
                    u.Surname = Reg.Surname;
                    u.UserName = Reg.UserName;
                    cugDB.UserMasters.Add(u);
                    cugDB.SaveChanges();
                    return new Response
                    { Status = "Success", Message = "Record SuccessFully Saved." };
                }
            }
            catch (Exception)
            {
                throw;
            }
            return new Response
            { Status = "Error", Message = "Invalid Data." };
        }

        [Route("Login")]
        [HttpPost]
        public Response employeeLogin(Login login)
        {
            var log = cugDB.UserMasters.Where(x => x.UserName.Equals(login.UserName) && x.Password.Equals(login.Password)).FirstOrDefault();

            if (log == null)
            {
                return new Response { Status = "Invalid", Message = "Invalid User." };
            }
            else
                return new Response { Status = "Success", Message = "Login Successfully" };
        }

        /// <summary>
        /// get list of Figures
        /// </summary>
        /// <returns></returns>
        [Route("Figures")]
        [HttpGet]
        public List<FiguresDTO> getFigures()
        {
            var results = cugDB.Mains.Select(m => new FiguresDTO {
                                             Title = m.Title.ToUpper(),
                                                Idx = m.Idx.ToUpper() }).OrderBy(m => m.Title).ToList();

            if (results != null)
            {
                return results;
            }
            else
                return null;
        }
    }

    public class FiguresDTO
    {
        public string Title { get; set; }   
        public string Idx { get; set; }
    }
}
