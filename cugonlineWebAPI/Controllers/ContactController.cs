using cugonlineWebAPI.Models;
using cugonlineWebAPI.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace cugonlineWebAPI.Controllers
{
     
    public class ContactController : ApiController
    {
        testEntities cugDB = new testEntities();
        //constructor
        public ContactController()
        {

        }

        public ActionResult Index() 
        {
            return null;
        }

        /// <summary>
        /// import contacts into repo
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
         
        public async Task<object> ImportContacts(List<ImportContacts> list)
        {
            try
            {
                if (list is null)
                {
                    throw new ArgumentNullException(nameof(list));
                }

                ////get import list of contacts
                //var alreadyImportedContacts = cugDB.contactImportedDatas.ToList();

                ////loop through contacts list
                //foreach (var item in list)
                //{
                //    contactImportedData contactDetails = new contactImportedData
                //    {
                //        CompanyName = item.CompanyName,
                //        CompanyAddress = item.CompanyAddress,
                //        Industry = item.Industry,
                //        ContactNumbers = item.ContactNumbers,
                //        OtherNumbers = item.OtherNumbers,
                //        CellNumbers = item.CellNumbers,
                //        EmailAddress = item.EmailAddress,
                //        WebAddress = item.WebAddress
                //    };

                //    //check to see if contact already exists
                //    bool contactExists = alreadyImportedContacts.Any(i => i.CompanyName == item.CompanyName && i.CompanyAddress == item.CompanyAddress
                //                                                        && i.Industry == item.Industry && i.ContactNumbers == item.ContactNumbers
                //                                                        && i.OtherNumbers == item.OtherNumbers && i.CellNumbers == item.CellNumbers
                //                                                        && i.EmailAddress == item.EmailAddress && i.WebAddress == item.WebAddress);

                //    if (!contactExists)
                //    {
                //        //load contact details to table
                //        cugDB.contactImportedDatas.Add(contactDetails);
                //    }
                //}
                //await cugDB.SaveChangesAsync();

                return new Response
                { Status = "Success", Message = list.Count() + " Records SuccessFully imported." };
            }
            catch (Exception ex)
            {
                return new Response { Status = "Error " + ex.Message, Message = "Invalid Data." };
            }
        }

        /// <summary>
        /// contact list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        
        public List<ImportContacts> GetSystemContacts()
        {
            try
            {
                //var results = cugDB.contactImportedDa.Where(ci => ci.CompanyName != null).Select(u => new ImportContacts
                //{
                //    ImportContactId = u.ImportContactId,
                //    CompanyName = u.CompanyName.ToUpper(),
                //    CompanyAddress = u.CompanyAddress.ToUpper(),
                //    Industry = u.Industry.Trim(),
                //    ContactNumbers = u.ContactNumbers,
                //    OtherNumbers = u.OtherNumbers,
                //    CellNumbers = u.CellNumbers,
                //    EmailAddress = u.EmailAddress,
                //    WebAddress = u.WebAddress,
                //    IsProcessed = u.IsProcessed

                //}).OrderBy(u => u.CompanyName).ToList();


                //if (results != null)
                //{
                //    return results;
                //}
                //else
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}