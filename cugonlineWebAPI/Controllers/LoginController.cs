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
                
                    u.Email = Reg.Email;                 
                    u.Name = Reg.Name;
                    u.Password = Reg.Password;
                    u.Surname = Reg.Surname;
                    u.UserName = Reg.UserName;
                    cugDB.UserMasters.Add(u);
                    cugDB.SaveChanges();
                    return new Response
                    { Status = "Success", Message = "Record SuccessFully Saved." };
                
            }
            catch (Exception ex)
            {
                
                return new Response   { Status = "Error" + ex.Message, Message = "Invalid Data." };
            }
           
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
            var results = cugDB.Mains.Select(m => new FiguresDTO
            {
                Id = m.Id,
                //Idx = m.Idx.ToUpper(),
                Title = m.Title.ToUpper(),
                //Meaning = m.Meaning.ToUpper()                
            }).OrderBy(m => m.Title).ToList();

            if (results != null)
            {
                return results;
            }
            else
                return null;
        }

        [Route("GetFigureById")]
        [HttpGet]
        public FiguresDTO GetFigureById(int id)
        {
            var result = cugDB.Mains.Where(m => m.Id.Equals(id)).Select(m => new FiguresDTO
            {
                Id = m.Id,
                Idx = m.Idx,
                Title = m.Title,
                Meaning = m.Meaning,
                Body = m.Body

            }).FirstOrDefault();

            if (result != null)
            {
                return result;
            }
            else
                return new FiguresDTO();
        }

        [Route("EditFigure")]
        [HttpPost]
        public object EditFigure(Figure fig)
        {
            try
            {
               
                if (fig.Id != 0)
                {

                    var Update = cugDB.Mains.Find(fig.Id);

                    Update.Title = fig.Title;
                    Update.Meaning = fig.Meaning;
                    Update.Body = fig.Body;
                    Update.Idx = fig.Idx;
                    Update.LastUpdated = DateTime.Now.Date;
                    cugDB.Entry(Update).State = System.Data.Entity.EntityState.Modified;
                    cugDB.SaveChanges();
                    return new Response
                    { Status = "Success", Message = "Record SuccessFully Saved." };
                }
                else
                {
                    Main m = new Main();

                    m.Title = fig.Title;
                    m.Body = fig.Body;
                    m.Meaning = fig.Meaning;
                    
                    cugDB.Mains.Add(m);
                    cugDB.SaveChanges();
                    return new Response
                    { Status = "Success", Message = "Record SuccessFully Saved." };
                }
            }
            catch (Exception ex)
            {               
                return new Response
            { Status = "Error" + ex.Message, Message = "Invalid Data." };
            }
            
        }
    }

    public class FiguresDTO
    {
        public int Id { get; set; }        
        public string Idx { get; set; }
        public string Title { get; set; }
        public string Meaning { get; set; }
        public string Body { get; set; }
    }
}
