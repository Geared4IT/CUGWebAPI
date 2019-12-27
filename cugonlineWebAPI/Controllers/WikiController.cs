using cugonlineWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;

namespace cugonlineWebAPI.Controllers
{
    [RoutePrefix("Api/wiki")]
    public class WikiController : ApiController
    {
        string rootPath = "https://cugonlinestorage.blob.core.windows.net/images/";
        testEntities cugDB = new testEntities();
        private const string Container = "images_t";

        public WikiController()
        {
            var _test_rootPath = HostingEnvironment.MapPath("~/images/");            
        }

        /// <summary>
        /// search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("GetSearchResultsBy")]
        [HttpGet]
        public List<FiguresDTO> GetSearchResultsBy(string filter)
        {
            
            var results = cugDB.Mains.Where(m => m.Title.Contains(filter)  || 
                                                m.Meaning.Contains(filter) //||
                                                //m.Body.Contains(filter)
                                                ).Select(m => new FiguresDTO
            {
                Id = m.Id,
                Title = m.Title,
                Meaning = m.Meaning//,
               // Body = (m.Body.Length >100) ? m.Body.Substring(0,100) : m.Body
            }).ToList();

            if (results != null) return results;
            return null;
        }

    }
}
