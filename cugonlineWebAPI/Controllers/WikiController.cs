﻿using cugonlineWebAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace cugonlineWebAPI.Controllers
{
    [RoutePrefix("Api/wiki")]
    public class WikiController : ApiController
    {
        //string rootPath = "https://cugonlinestorage.blob.core.windows.net/images/";
        testEntities cugDB = new testEntities();
        //private const string Container = "images_t";

        public WikiController()
        {
           // var _test_rootPath = HostingEnvironment.MapPath("~/images/");            
        }

        /// <summary>
        /// search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("GetSearchResultsBy")]
        [HttpGet]
        public List<FiguresDTO> GetSearchResultsBy(string filter , string searchRole, string isFullSearch, string searchCount)
        {

            var results = new List<FiguresDTO>();
            if (searchRole == "null") searchRole = "ALL";
            switch (searchRole)
            {
                case "ALL":

                    if (isFullSearch == "false" && (searchCount == "1"))
                        results = cugDB.Mains.Where(m => m.Title.Contains(filter) ||
                                                   m.Meaning.Contains(filter)  //||
                                                                               //  m.Body.Contains(filter)
                                                   ).Select(m => new FiguresDTO
                                                   {
                                                       Id = m.Id,
                                                       Idx = m.Idx,
                                                       Title = m.Title,
                                                       Meaning = m.Meaning,
                                                       CategoryName = m.CategoryN.Trim()
                                                   }).OrderBy(m => m.Title).ToList();

                    else if((isFullSearch == "true") && (searchCount == "2")) //get body search
                        results = cugDB.Mains.Where(m =>   m.Body.Contains(filter)
                                                && !m.Title.Contains(filter)
                                                && !m.Meaning.Contains(filter)
                                               ).Select(m => new FiguresDTO
                                               {
                                                   Id = m.Id,
                                                   Idx = m.Idx,
                                                   Title = m.Title,
                                                   Meaning = m.Meaning,
                                                   CategoryName = m.CategoryN.Trim()
                                               }).Take(20).OrderBy(m => m.Title).ToList();                   

                    break;

                case "Radiation":
                    if (isFullSearch == "false" && (searchCount == "1"))
                        results = cugDB.Mains.Where(m => m.Title.Contains(filter) ||
                                                m.Meaning.Contains(filter) &&
                                                m.CategoryN.Equals(searchRole)
                                                ).Select(m => new FiguresDTO
                                                {
                                                    Id = m.Id,
                                                    Idx = m.Idx,
                                                    Title = m.Title,
                                                    Meaning = m.Meaning,
                                                    CategoryName = m.CategoryN.Trim()
                                                }).OrderBy(m => m.Title).ToList();

                    else if ((isFullSearch == "true") && (searchCount == "2")) //get body search
                        results = cugDB.Mains.Where(m => m.Body.Contains(filter)
                                               && !m.Title.Contains(filter)
                                               && !m.Meaning.Contains(filter)
                                               &&  m.CategoryN.Equals(searchRole)
                                              ).Select(m => new FiguresDTO
                                              {
                                                  Id = m.Id,
                                                  Idx = m.Idx,
                                                  Title = m.Title,
                                                  Meaning = m.Meaning,
                                                  CategoryName = m.CategoryN.Trim()
                                              }).Take(20).OrderBy(m => m.Title).ToList();
                    break;
                case "Aqua":
                    results = cugDB.Mains.Where(m => m.Title.Contains(filter) ||
                                                m.Meaning.Contains(filter) &&
                                                m.CategoryN.Equals(searchRole)
                                                ).Select(m => new FiguresDTO
                                                {
                                                    Id = m.Id,
                                                    Idx = m.Idx,
                                                    Title = m.Title,
                                                    Meaning = m.Meaning,
                                                    CategoryName = m.CategoryN.Trim()
                                                }).OrderBy(m => m.Title).ToList();
                    break;
                case "Biblopedia":
                    results = cugDB.Mains.Where(m => m.Title.Contains(filter) ||
                                                m.Meaning.Contains(filter) &&
                                                m.CategoryN.Equals(searchRole)
                                                ).Select(m => new FiguresDTO
                                                {
                                                    Id = m.Id,
                                                    Idx = m.Idx,
                                                    Title = m.Title,
                                                    Meaning = m.Meaning,
                                                    CategoryName = m.CategoryN.Trim()
                                                }).OrderBy(m => m.Title).ToList();
                    break;
                default: //return ALL case show no authentication...
                    results = cugDB.Mains.Where(m => m.Title.Contains(filter) ||
                                                  m.Meaning.Contains(filter)  //||
                                                                              //  m.Body.Contains(filter)
                                                  ).Select(m => new FiguresDTO
                                                  {
                                                      Id = m.Id,
                                                      Idx = m.Idx,
                                                      Title = m.Title,
                                                      Meaning = m.Meaning,
                                                      CategoryName = m.CategoryN.Trim()
                                                  }).OrderBy(m => m.Title).ToList();
                    break;
            }

            if (results != null) return results;
            return null;
        }

    }
}
