using Contacts.Models;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Contacts.Controllers
{
    public class AccountController : Controller
    {
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr; 
        // GET: Account
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        void connectionString() 
        {
            con.ConnectionString = "data source= DESKTOP-JD4UDGG\\GEARED4IT; database=test; integrated security = SSPI";
        }

        [HttpPost]
        public ActionResult Verify(Account acc)
        {
            connectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = "select * from UserMaster where username = '" + acc.Name+"' and password='" +acc.Password+"' ";
            dr = com.ExecuteReader();
          
            if (dr.Read())
            {
                con.Close();
                return View("Create");
            }
            else
            {
                con.Close();
                return View("Error");
            }
        }
    }
}