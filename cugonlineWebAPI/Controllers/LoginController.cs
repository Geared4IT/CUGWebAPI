using System.Web.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using cugonlineWebAPI.Models;
using cugonlineWebAPI.VM;
using System.Web.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Net.Mail;
using cugonlineWebAPI.DTO;
using System.Text;
using HtmlAgilityPack;
using cugonlineWebAPI.Caching;

namespace cugonlineWebAPI.Controllers
{
    [RoutePrefix("Api/login")]
    public class LoginController : ApiController
    {
        private string stopWords = "a,able,about,across,after,all,almost,also,am,among,an,and,any,are,as,at,be,because,been,but,by,can,cannot,could,dear,did,do,does,either" +
            ",else,ever,every,for,from,get,got,had,has,have,he,her,hers,him,his,how,however,i,if,in,into,is,it,its,just,least,let,like" +
            ",likely,may,me,might,most,must,my,neither,no,nor,not,of,off,often,on,only,or,other,our,own,rather,said,say,says,she,should,since,so,some,than,that,the,their,them,then,there,these,they,this,tis" +
            ",to,too,twas,us,wants,was,we,were,what,when,where,which,while,who,whom,why,will,with,would,yet,you,your";



        //string rootPath = "https://cugonlinestorage.blob.core.windows.net/images/";
        //readonly string rootPath = "https://cugonlinestorage.blob.core.windows.net/img/";
        testEntities cugDB = new testEntities();

        public LoginController()//CloudBlobContainer blobContainer)
        {
            //constructor
        }

        [Route("InsertUser")]
        [HttpPost]
        public object InsertUser(Register Reg)
        {
            try
            {
                var userId = cugDB.UserMasters.Max(maxID => maxID.ID);

                UserMaster u = new UserMaster
                {
                    Email = Reg.Email,
                    Name = Reg.Name,
                    Password = Reg.Password.Replace(" ", ""),
                    Surname = Reg.Surname,
                    UserName = Reg.UserName.Replace(" ", ""),
                    ID = userId + 1,
                    Date_added = DateTime.Now.ToString(),
                    IsDeleted = true,
                    isSuperAdmin = false,
                    isNew = true
                };
                cugDB.UserMasters.Add(u);
                cugDB.SaveChanges();

                EmailHotLink(Reg);
                return new Response
                { Status = "Success", Message = "Record SuccessFully Saved." };

            }
            catch (Exception ex)
            {
                return new Response { Status = "Error " + ex.Message, Message = "Invalid Data." };
            }
        }

        [Route("Login")]
        [HttpPost]
        public SystemUsersDTO EmployeeLogin(Login login)
        {

            try
            {
                var u = cugDB.UserMasters.Where(x => x.UserName.Equals(login.UserName) && x.Password.Equals(login.Password)).FirstOrDefault();
                if (u == null)
                {
                    return new SystemUsersDTO { Name = "Invalid" };
                }
                else
                {
                    LogUserActivity(u);//todo fix...

                    var userDto = new SystemUsersDTO
                    {
                        Id = u.ID,
                        Name = u.Name.ToUpper(),
                        Surname = u.Surname.ToUpper(),
                        CategoryName = u.categoryN.Trim(),
                        DateCreated = u.Date_added,
                        DateLast = u.Date_last,
                        UserName = u.UserName,
                        Password = u.Password,
                        Email = u.Email,
                        IsEditor = (u.nKey == "1") ? true : false,
                        IsSuperAdmin = u.isSuperAdmin.Value
                    };

                    return userDto;
                }
            }
            catch (Exception ex)
            {

                return new SystemUsersDTO { Name = "Invalid" };
            }



        }
        /// <summary>
        /// save user login activity
        /// </summary>
        /// <param name="u"></param>
        private void LogUserActivity(UserMaster u)
        {
            try
            {

                var myNum = cugDB.UserActivities.Max(maxID => maxID.UserActivitiesID);
                var dt = DateTime.Now.AddHours(9);
                UserActivity ua = new UserActivity
                {
                    Reference = "login",
                    Editor = u.Name,
                    DateIn = dt,
                    TimeIn = dt.ToString(),
                    UserActivitiesID = cugDB.UserActivities.Max(maxID => maxID.UserActivitiesID) + 1,
                    ID = cugDB.UserActivities.Max(maxID => maxID.UserActivitiesID) + 1
                };

                cugDB.UserActivities.Add(ua);
                cugDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [Route("BibleFootNoteContent")]
        [HttpGet]
        public BibleFootNoteContentDTO BibleFootNoteContent(string Idx)
        {
            try
            {
                Idx = Idx.Replace(" ", "_");//Simple replace for now todo: replace function
                var results = cugDB.BibleFootNoteContents.Where(
                      m => m.Idx.Equals(Idx)
                      ).Select(m => new BibleFootNoteContentDTO
                      {
                          Idx = m.Idx,
                          Content = m.Content
                      }).FirstOrDefault();
                #region test anchor tag manipulation 
                if (results != null)
                {
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(results.Content);
                    string strPreviousOuterHtml = string.Empty;
                    HtmlNodeCollection nc = document.DocumentNode.SelectNodes("//a");
                    if (nc != null)
                    {
                        foreach (HtmlNode node in nc)
                        {
                            strPreviousOuterHtml = node.OuterHtml;
                            //if (node.Attributes["href"] != null)
                            //{
                            //    node.Attributes.Add("data-idx", node.Attributes["href"].Value.Substring(node.Attributes["href"].Value.LastIndexOf('=') + 1).Replace(" ", "_"));
                            //    node.Attributes["href"].Value = "/bibleReferences?" + node.Attributes["href"].Value.Substring(node.Attributes["href"].Value.LastIndexOf('=') + 1).Replace(" ", "_");
                            //}

                            document.DocumentNode.InnerHtml = document.DocumentNode.InnerHtml.Replace(strPreviousOuterHtml, node.OuterHtml);
                        }
                    }
                    var newContent = document.DocumentNode.OuterHtml;
                    results.Content = newContent;
                }

                #endregion
                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Route("BibleFootNote")]
        [HttpGet]
        public BibleFootNoteContentDTO BibleFootNote(string compoKey)
        {
            try
            {
                var key = cugDB.BibleFootNotes.Where(b => b.CompoKey.Equals(compoKey)).FirstOrDefault();
                if (key != null)
                {
                    var results = cugDB.BibleFootNoteContents.Where(
                      m => m.Idx.Equals(key.Idx)
                      ).Select(m => new BibleFootNoteContentDTO
                      {
                          Idx = m.Idx,
                          Content = m.Content
                      }).FirstOrDefault();
                    #region test anchor tag manipulation 

                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(results.Content);
                    string strPreviousOuterHtml = string.Empty;
                    HtmlNodeCollection nc = document.DocumentNode.SelectNodes("//a");
                    if (nc != null)
                    {
                        foreach (HtmlNode node in nc)
                        {
                            strPreviousOuterHtml = node.OuterHtml;
                            //JJ. updated db footnote.asp > bibleReferences?
                            //if (node.Attributes["href"] != null)
                            //{
                            //    //node.Attributes.Add("data-idx", node.Attributes["href"].Value.Substring(node.Attributes["href"].Value.LastIndexOf('=') + 1).Replace(" ", "_"));
                            //    //node.Attributes["href"].Value = "/bibleReferences?" + node.Attributes["href"].Value.Substring(node.Attributes["href"].Value.LastIndexOf('=') + 1).Replace(" ", "_");
                            //}
                            document.DocumentNode.InnerHtml = document.DocumentNode.InnerHtml.Replace(strPreviousOuterHtml, node.OuterHtml);
                        }
                    }
                    var newContent = document.DocumentNode.OuterHtml;
                    results.Content = newContent;
                    #endregion

                    return results;
                }
                else
                {
                    return null;// new BibleFootNoteContentDTO();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Route("BibleReferences")]
        [CacheFilter(TimeDuration = 60000)] // 1 minutes
        [HttpGet]
        public List<BibleReferencesDTO> GetBibleReferences(string searchFilter)
        {
            //implement caching
            Dictionary<object, object> obj = new Dictionary<object, object>();

            var results = cugDB.BibleBooks.Where(//m => m.BookId.Contains(filter) && 
                  m => m.ChapterId != 0
                  ).Select(m => new BibleReferencesDTO
                  {
                      Id = m.ID,
                      BookOf = m.BookId,
                      ChapterId = m.ChapterId.Value,
                      VerseId = m.VerseId.Value,
                      BodyText = m.BodyText.ToString(),
                      CompoKey = m.CompoKey
                  }).OrderBy(m => m.BookOf).ThenBy(m => m.ChapterId).ThenBy(m => m.VerseId).ToList();

            if (!searchFilter.Equals("undefined"))
            {
                //store results
                //var resultRepo = results;//clone
                List<BibleReferencesDTO> resultRepo = new List<BibleReferencesDTO>();//clone
                                                                                     //split keywords
                string[] keywords = searchFilter.Split(' ');

                //remove stop words from key words
                string[] array_stopWords = stopWords.Split(',');

                keywords = keywords.Except(array_stopWords).ToArray();

                //iterate through keywords
                foreach (var word in keywords)
                {
                    var keyword = word.ToLower();
                    resultRepo.AddRange(results.Where(b => b.BodyText.ToLower().Contains(keyword)
                                                    && !resultRepo.Select(r => r.BodyText.ToLower()).Contains(keyword)));
                    //var temptest = results.Where(b => b.BodyText.ToLower().Contains(keyword)).ToList();

                }
                results = resultRepo.Distinct().ToList();
            }

            if (results != null)
            {
                return results;
            }
            else
                return null;
        }

        /// <summary>
        /// get list of Figures
        /// </summary>
        /// <returns>list of figures</returns>
        [Route("XReferences")]
        // [CacheFilter(TimeDuration = 60000)] // 1 minutes
        [HttpGet]
        public List<FiguresDTO> GetXReferences(string searchFilter = null)
        {
            //implement caching
            // Dictionary<object, object> obj = new Dictionary<object, object>();

            List<FiguresDTO> results = new List<FiguresDTO>();

            results = cugDB.Mains.Where(m => m.currentStatus.ToLower().Equals("live")
            && !m.Title.Equals("")
            && !m.Title.Equals("")
            && m.Title != null).Select(m => new FiguresDTO
            {
                Id = m.Id,
                Idx = m.Idx,
                Title = m.Title.ToUpper(),
                LastUpdated = m.LastUpdated.ToString(),
                LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",
                CurrentStatus = m.currentStatus

            }).OrderBy(m => m.Title).ToList();

            if (results != null)
            {
                return results;
            }
            else
                return null;
        }


        [Route("Figures")]
        [HttpGet]
        [CacheFilter(TimeDuration = 999999999)] // 11 days
        public List<FiguresDTO> GetFigures(string search, string isAdmin, bool? isSearchAttachments)
        {
            Dictionary<object, object> obj = new Dictionary<object, object>();

            //bool isSearchAttachments = true;
            List<FiguresDTO> results = new List<FiguresDTO>();
            if (search == "undefined" || search == null) search = "";
            if (String.IsNullOrEmpty(search))
            {
                results = cugDB.sp_SearchFigures(search).Select(m => new FiguresDTO
                {
                        Id = m.Id,
                        Idx = m.Idx,
                        Title = m.Title.ToUpper(),
                        Body = m.Body,
                        Meaning = m.Meaning,
                        LastUpdated = m.LastUpdated.ToString(),
                        LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",
                        CurrentStatus = m.CurrentStatus
                    }).ToList();
                 
            }
            else
            {            

                results = cugDB.sp_SearchFigures(search).Select(m => new FiguresDTO
                {
                    Id = m.Id,
                    Idx = m.Idx,
                    Title = m.Title.ToUpper(),
                    Body = m.Body,
                    Meaning = m.Meaning,
                    LastUpdated = m.LastUpdated.ToString(),
                    LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",
                    CurrentStatus = m.CurrentStatus
                }).ToList();

                //get list of attachments linked to References
                var attachmentFilesLink = cugDB.MainFilesLinks.ToList();

                //update results with attachment status
                foreach (var item in results)
                {
                    item.HasAttachments = attachmentFilesLink.Where(r => r.Idx == item.Idx).Count() > 0 ? "Yes" : "No";
                }

            }

            if (isSearchAttachments.Value)//include attachments
            {           

                //get list of attachments filtered by search criteria
                var attachmentResults = (from mfl in cugDB.MainFilesLinks
                                         join mf in cugDB.MainFiles on mfl.idFiles equals mf.id
                                         join m in cugDB.Mains on mfl.Idx equals m.Idx
                                        where mf.fName.Contains(search)
                                               || mf.fComment.Contains(search)
                                              
                                           select new FiguresDTO
                                           {
                                               Id = m.Id,
                                               Idx = m.Idx,
                                               Title = m.Title.ToUpper(),
                                               Body = m.Body,
                                               Meaning = m.Meaning,
                                               LastUpdated = m.LastUpdated.ToString(),
                                               LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",
                                               CurrentStatus = m.currentStatus,
                                               HasAttachments = "Yes"
                                           }).OrderBy(x => x.Title).ToList();
                //add attachment results to results.
                foreach (var item in attachmentResults)
                { 
                    //check if idx exists in results
                    var refereceExists = results.Where(r => r.Idx == item.Idx).Count() > 0 ? true : false;
                    if (!refereceExists)
                    {
                        results.Add(item);
                    }                    
                }                
            }

          

            if (results != null)
            {
                if (!isAdmin.Equals("true") && search != null)//only show live References
                {
                    results = results.Where(r => r.CurrentStatus.ToLower().Equals("live")).ToList();
                }
                var distinctResults = results.Distinct().ToList();
                return distinctResults;
            }
            else
                return new List<FiguresDTO>();//no results
        }

        /// <summary>
        /// get Report Details
        /// </summary>
        /// <returns>list of figures</returns>
        [Route("GetReportDetails")]
        [HttpGet]
        public List<FiguresDTO> GetReportDetails(string filter, string field)
        {

            switch (filter)
            {
                case "null":
                    filter = null;
                    break;
                case null:
                    filter = "";
                    break;
                default:
                    break;
            }

            List<FiguresDTO> result = new List<FiguresDTO>();


            if (field == "CurrentStatus") // live / review / new
            {
                result = cugDB.Mains.Where(m => m.currentStatus.Equals(filter)).Select(m => new FiguresDTO
                {
                    Id = m.Id,
                    Idx = m.Idx,
                    Title = m.Title.ToUpper(),
                    LastUpdated = m.LastUpdated.ToString(),
                    CategoryName = m.CategoryN.Trim(),
                    CurrentStatus = m.currentStatus.Trim(),
                    LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",


                }).OrderBy(m => m.Title).ToList();
            }
            else if (field == "Category")
            {
                result = cugDB.Mains.Where(m => m.CategoryN.Equals(filter)).Select(m => new FiguresDTO
                {
                    Id = m.Id,
                    Idx = m.Idx,
                    Title = m.Title.ToUpper(),
                    LastUpdated = m.LastUpdated.ToString(),
                    CategoryName = m.CategoryN.Trim(),
                    CurrentStatus = m.currentStatus.Trim(),
                    LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : ""

                }).OrderBy(m => m.Title).ToList();
            }

            if (result != null)
            {
                return result;
            }
            else
                return null;
        }

        /// <summary>
        /// get Empty Reference Report Details
        /// </summary>
        /// <returns>list of figures</returns>
        [Route("GetEmptyReferenceReportDetails")]
        [HttpGet]
        public List<FiguresDTO> GetEmptyReferenceReportDetails(int filter, string type)
        {
            type = type.Contains("Empty References") ? "Body" : "Meaning";

            List<FiguresDTO> result = new List<FiguresDTO>();

            try
            {
                result = cugDB.sp_EmptyReferencesDetails(filter, type).Select(m => new FiguresDTO
                {
                    Id = m.id,
                    Idx = m.Idx,
                    Title = m.Title.ToUpper(),
                    LastUpdated = m.LastUpdated.ToString(),
                    CategoryName = m.CategoryN.Trim(),
                    CurrentStatus = m.currentStatus.Trim(),
                    LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",


                }).OrderBy(m => m.Title).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }



            if (result != null)
            {
                return result;
            }
            else
                return null;
        }

        /// <summary>
        /// get list of users
        /// </summary>
        /// <returns>list of figures</returns>
        [Route("SystemUsers")]
        [HttpGet]
        public List<SystemUsersDTO> GetSystemUsers()
        {
            try
            {
                var results = cugDB.UserMasters.Select(u => new SystemUsersDTO
                {
                    Id = u.ID,
                    Name = u.Name.ToUpper(),
                    Surname = u.Surname.ToUpper(),
                    CategoryName = u.categoryN.Trim(),
                    DateCreated = u.Date_added,
                    DateLast = u.Date_last,
                    UserName = u.UserName,
                    Password = u.Password,
                    Email = u.Email,
                    IsEditor = (u.nKey == "1") ? true : false,
                    IsSuperAdmin = u.isSuperAdmin ?? false,
                    IsDeleted = u.IsDeleted ?? false
                }).OrderByDescending(u => u.Id).ToList();


                if (results != null)
                {
                    return results;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [Route("FigureLinksList")]
        [HttpGet]
        public List<FiguresLinkDTO> FigureLinksList(string filter)
        {
            try
            {
                List<FiguresLinkDTO> results = new List<FiguresLinkDTO>();
                if (!string.IsNullOrEmpty(filter))
                {
                    results = cugDB.Mains.Where(m => !m.currentStatus.Equals("Deleted")
                    && (m.Title.Contains(filter) || m.Body.Contains(filter))
                     && !m.Title.Equals("")
                     && m.Title != null
                       ).OrderBy(m => m.Title).Select(m => new FiguresLinkDTO
                       {
                           Id = m.Id,
                           value = m.Idx,
                           label = m.Title.ToUpper(),
                           body = "",// m.Body,
                           meaning = m.Meaning

                       }).ToList();
                }

                else
                {
                    results = cugDB.Mains.Where(m => m.currentStatus.ToLower().Equals("live")
                        && !m.Title.Equals("")
                        && m.Title != null
                        ).OrderBy(m => m.Title).Take(10).Select(m => new FiguresLinkDTO
                        {
                            Id = m.Id,
                            value = m.Idx,
                            label = m.Title.ToUpper(),
                            body = m.Body,
                            meaning = m.Meaning

                        }).ToList();
                }


                if (results != null)
                {
                    return results;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //
        [Route("GetFigureCurrentStatus")]
        [HttpGet]
        public List<FigureStasticsDTO> GetFigureCurrentStatus()
        {
            var results = (from m in cugDB.Mains
                           group m by m.currentStatus into newGroup
                           orderby newGroup.Key
                           select new FigureStasticsDTO
                           {
                               CurrentStatus = newGroup.Key.Trim(),
                               TotalCount = newGroup.Count()
                           }).ToList();


            if (results != null) return results;
            return null;
        }
        [Route("GetFigureStatistics")]
        [HttpGet]
        public List<FigureStasticsDTO> GetFigureStatistics()
        {
            try
            {
                var results = (from m in cugDB.Mains
                               group m by m.CategoryN into newGroup
                               orderby newGroup.Key
                               select new FigureStasticsDTO
                               {
                                   Category = newGroup.Key.Trim(),
                                   TotalCount = newGroup.Count()
                               }).ToList();


                if (results != null) return results;
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [Route("GetDatabaseStatistics")]
        [HttpGet]
        public List<DataBaseInfoDTO> GetDataBaseStatistics()
        {
            try
            {
                int strLength = 1;
                var emptyReference = cugDB.sp_EmptyReferences(strLength);

                var dbInfoList = new List<DataBaseInfoDTO>();


                foreach (var item in emptyReference)
                {
                    var emptyReferenceData = new DataBaseInfoDTO
                    {
                        Title = item.Title,
                        TotalRecords = item.numRecords.Value
                    };

                    dbInfoList.Add(emptyReferenceData);
                }

                if (dbInfoList != null)
                    return dbInfoList;
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [Route("GetBibloStatistics")]
        [HttpGet]
        public List<DataBaseInfoDTO> GetBibloStatistics()
        {
            var results = (from m in cugDB.BibloMains
                           select new DataBaseInfoDTO
                           {
                               Title = "Title",
                               TotalRecords = cugDB.BibloMains.Count()
                           }).ToList();


            if (results != null) return results;
            return null;
        }

        [Route("GetFigureLinksByIdx")]
        [HttpGet]
        public List<FigureLinksDTO> GetFigureLinksById(string idx)
        {
            try
            {
                var results = (from sm in cugDB.SeeMains
                               join m in cugDB.Mains on sm.Idx equals m.Idx
                               where (sm.Idx == idx.Replace("_", "/")
                                   || sm.Idx == idx
                                   )
                               select new FigureLinksDTO
                               {
                                   LinkId = sm.Id,
                                   LinkIdx = sm.Link.Replace("/", "_"),// sm.Idx,
                                   LinkTitle = sm.Title ?? "",
                                   LinkIdxFriendlyName = sm.Title.Replace("_", " ")// sm.Idx.Replace("_", " ")
                               }).Distinct().OrderBy(x => x.LinkIdx).ToList();

                if (results != null) return results.Where(r => !r.LinkTitle.Equals(idx)).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }

            return new List<FigureLinksDTO>();
        }

        [Route("GetFigureByIdx")]
        [HttpGet]
        public FiguresDTO GetFigureById(string idx)
        {
            var result = cugDB.Mains.Where(m => m.Idx.Equals(idx)).Select(m => new FiguresDTO
            {
                Id = m.Id,
                Idx = m.Idx,
                Title = m.Title,
                Meaning = m.Meaning,
                Body = m.Body,
                LastUpdated = m.LastUpdated.ToString(),
                LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",
                CategoryName = m.CategoryN.Trim(),
                CurrentStatus = m.currentStatus.Trim()

            }).FirstOrDefault();

            if (result != null)
            {
                return result;
            }
            else
                return new FiguresDTO();
        }



        [Route("DeleteReference")]
        [HttpPost]
        public object DeleteReference(FigureLinksDTO reference)
        {
            //var updateReferece = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkIdx) && sm.Link.Equals(reference.LinkTitle)).FirstOrDefault();
            var updateReferece = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkIdx) && sm.Link.Equals(reference.LinkTitle)).FirstOrDefault();

            if (updateReferece != null) cugDB.SeeMains.Remove(updateReferece);
            //cugDB.SaveChanges();

            //remove  reciprocal
            var updateReciprocal = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkTitle)
                                                     && sm.Link.Equals(reference.LinkIdx)).FirstOrDefault();

            if (updateReciprocal != null) cugDB.SeeMains.Remove(updateReciprocal);
            cugDB.SaveChanges();

            return new Response
            { Status = "Success", Message = "Reference Deleted Saved." };
        }

        [Route("DeleteImage")]
        [HttpPost]
        public object DeleteImage(FilesInfo item)
        {
            var updateImage = cugDB.MainFiles.Where(mf => mf.id.Equals(item.FileId)).FirstOrDefault();

            if (updateImage == null)
            {
                return new Response
                { Status = "Success", Message = "Image Deleted." };
            }
            updateImage.IsDeleted = true;

            cugDB.Entry(updateImage).State = System.Data.Entity.EntityState.Modified;
            cugDB.SaveChanges();

            return new Response
            { Status = "Success", Message = "Reference Deleted." };
        }

        /// <summary>
        /// Add Cross Reference to current Figure
        /// Reciprocal Definition: Add current Figure to Cross Reference Figure... (Cross Polunate)
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        [Route("AddReference")]
        [HttpPost]
        public object AddReference(FigureLinksDTO reference)
        {

            if (reference.Link == reference.LinkIdx)
                return new Response
                { Status = "Success", Message = "Cannot self XReference." };

            var linkInfo = cugDB.Mains.Where(m => m.Id.Equals(reference.LinkId)).FirstOrDefault();//get link information

            var updateReference = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.Link)  //check if link exists in Reference...
                                                         && sm.Link.Equals(reference.LinkIdx)).FirstOrDefault();

            try
            {
                if (updateReference == null)
                {
                    var titleFormatted = (reference.LinkIdx.Contains("_") ? reference.LinkIdx.Remove(reference.LinkIdx.LastIndexOf("_")) : reference.LinkIdx);
                    //add to seemain ( //selected XRef does not exist in current Figure so add it..._
                    SeeMain m = new SeeMain();
                    m.Idx = reference.Link;//reference.LinkIdx.Replace("/","_");
                    m.Link = reference.LinkIdx;// reference.Link;
                    m.Title = titleFormatted;// remove ID number
                    m.categoryN = !string.IsNullOrEmpty(linkInfo.CategoryN) ? linkInfo.CategoryN.Trim() : "All";//.Link; linkInfo.CategoryN.Trim();//.Link;
                    m.catFlag = "O";
                    cugDB.SeeMains.Add(m);
                    cugDB.SaveChanges();

                    //do reciprocal
                    var recipricolInfo = cugDB.Mains.Where(m_recip => m_recip.Idx.Equals(reference.Link)).FirstOrDefault();//1. get inverse link information (Cross Reference Details)

                    var reciprocalReferece = cugDB.SeeMains.Where(sm_recip => sm_recip.Idx.Equals(reference.LinkIdx)  //2. check inverse in Main Details...if current Figure details exists
                                                           && sm_recip.Link.Equals(reference.Link)).FirstOrDefault();

                    //select XRef does not exist in current Figure so add it...
                    if (reciprocalReferece == null)
                    {
                        //add to reciprocal seemain
                        SeeMain m_recip = new SeeMain();
                        m_recip.Idx = reference.LinkIdx;
                        m_recip.Link = reference.Link;
                        m_recip.Title = recipricolInfo.Title;
                        m_recip.categoryN = !string.IsNullOrEmpty(recipricolInfo.CategoryN) ? recipricolInfo.CategoryN.Trim() : "All";//.Link;
                        m_recip.catFlag = "O";
                        cugDB.SeeMains.Add(m_recip);
                        cugDB.SaveChanges();
                    }

                    return new Response
                    { Status = "Success", Message = "XReference Saved!" };

                }

                return new Response
                { Status = "Success", Message = "XReference Already Exists." };
            }
            catch (Exception ex)
            {
                return new Response
                { Status = "Success", Message = ex.Message };
            }

        }
        [Route("EditAttachmentTitle")]
        [HttpPost]
        public object EditAttachmentTitle(FilesInfo info)
        {
            var item = cugDB.MainFiles.Where(m => m.id.Equals(info.FileId)).FirstOrDefault();
            item.fComment = info.FileComment;
            cugDB.SaveChanges();

            var message = "Title SuccessFully updated.";
            return new Response
            { Status = item.id.ToString(), Message = message };
        }
        [Route("EditFigure")]
        [HttpPost]
        public object EditFigure(Figure fig)
        {
            try
            {
                var refIdx = "";
                var referenceItem = cugDB.Mains.Where(m => m.Idx.Equals(fig.Idx)).FirstOrDefault();
                if (referenceItem != null)//edit
                {

                    referenceItem.Title = fig.Title;
                    referenceItem.Meaning = fig.Meaning;
                    referenceItem.Body = fig.Body;
                    referenceItem.Idx = fig.Idx;
                    referenceItem.LastUpdated = DateTime.Now;
                    referenceItem.LastUpdatedBy = fig.LastUpdatedBy;
                    referenceItem.CategoryN = (fig.CategoryN == null) ? "All" : fig.CategoryN.Trim();// fig.CategoryN.Trim();
                    // referenceItem.currentStatus = (fig.LastUpdatedBy == 6) ? "live" : "review";
                    referenceItem.currentStatus = (fig.CurrentStatus == null) ? "review" : fig.CurrentStatus;// (fig.IsSuperAdmin) ? "live" : "review";

                    cugDB.Entry(referenceItem).State = System.Data.Entity.EntityState.Modified;

                    refIdx = referenceItem.Idx;
                    cugDB.SaveChanges();
                    LogUserActivityEditReference(referenceItem, fig.LastUpdatedBy.Value);
                }
                else//add
                {
                    var maxId = cugDB.Mains.Max(main => main.Id) + 1;
                    Main m = new Main();

                    m.Id = maxId;
                    m.Idx = fig.Title.Replace(" ", "_") + "_" + maxId;
                    m.Title = fig.Title;
                    m.Body = fig.Body;
                    m.Meaning = fig.Meaning;
                    m.DateCreated = DateTime.Now;
                    m.CreatedBy = fig.LastUpdatedBy;
                    m.LastUpdatedBy = (fig.LastUpdatedBy.HasValue) ? fig.LastUpdatedBy.Value : 0;
                    m.CategoryN = (fig.CategoryN == null) ? "All" : fig.CategoryN.Trim();
                    m.currentStatus = (fig.IsSuperAdmin) ? "review" : "new";
                    m.LastUpdated = DateTime.Now;
                    refIdx = m.Idx;
                    cugDB.Mains.Add(m);
                    cugDB.SaveChanges();
                    //log to User Activities
                    LogUserActivityEditReference(m, fig.UserId);
                }

                //email hotlink to David for Review
                //EmailHotLink(refIdx);
                var message = (fig.CurrentStatus != "Deleted") ? "Record SuccessFully Saved." : "Record Deleted.";
                return new Response
                { Status = refIdx.ToString(), Message = message };
            }
            catch (Exception ex)
            {
                return new Response
                { Status = "Error" + ex.Message, Message = "Invalid Data." };
            }

        }

        [Route("EditFigureLive")]
        [HttpPost]
        public object EditFigureLive(Figure fig)
        {
            try
            {
                var refIdx = "";
                var referenceItem = cugDB.Mains.Where(m => m.Idx.Equals(fig.Idx)).FirstOrDefault();
                if (referenceItem != null)//edit
                {

                    referenceItem.Title = fig.Title;
                    referenceItem.Meaning = fig.Meaning;
                    referenceItem.Body = fig.Body;
                    referenceItem.Idx = fig.Idx;
                    referenceItem.LastUpdated = DateTime.Now;
                    referenceItem.LastUpdatedBy = fig.LastUpdatedBy;
                    referenceItem.CategoryN = (fig.CategoryN == null) ? "All" : fig.CategoryN.Trim();
                    referenceItem.currentStatus = "live";// (fig.IsSuperAdmin) ? "live" : "review";

                    cugDB.Entry(referenceItem).State = System.Data.Entity.EntityState.Modified;

                    refIdx = referenceItem.Idx;
                    cugDB.SaveChanges();
                    LogUserActivityEditReference(referenceItem, fig.UserId);
                }
                else//add
                {
                    var maxId = cugDB.Mains.Max(main => main.Id) + 1;
                    Main m = new Main
                    {
                        Id = maxId,
                        Idx = fig.Title.Replace(" ", "_") + "_" + maxId,
                        Title = fig.Title,
                        Body = fig.Body,
                        Meaning = fig.Meaning,
                        DateCreated = DateTime.Now,
                        CreatedBy = fig.LastUpdatedBy,
                        LastUpdatedBy = fig.LastUpdatedBy.Value,
                        CategoryN = (fig.CategoryN == null) ? "All" : fig.CategoryN.Trim(),
                        currentStatus = "live",// (fig.IsSuperAdmin) ? "live" : "new";
                        LastUpdated = DateTime.Now
                    };
                    refIdx = m.Idx;
                    cugDB.Mains.Add(m);
                    cugDB.SaveChanges();
                    //log to User Activities
                    LogUserActivityEditReference(m, fig.UserId);
                }

                //email hotlink to David for Review
                //EmailHotLink(refIdx);

                return new Response
                { Status = refIdx.ToString(), Message = "Record SuccessFully Saved." };
            }
            catch (Exception ex)
            {
                return new Response
                { Status = "Error" + ex.Message, Message = "Invalid Data." };
            }

        }

        /// <summary>
        /// Edit Reference
        /// </summary>
        /// <param name="m"></param>
        /// <param name="userId"></param>
        private void LogUserActivityEditReference(Main m, int userId)
        {

            try
            {

                // return;
                var u = cugDB.UserMasters.Where(x => x.ID.Equals(userId)).FirstOrDefault();
                if (u == null) return;
                //check if reference already logged
                var ar = cugDB.UserActivities.Where(ua => ua.AttachmentId.Value.Equals(m.Id)).FirstOrDefault();

                var dt = DateTime.Now.AddHours(9);
                if (ar == null)//log
                {
                    UserActivity ua = new UserActivity
                    {
                        Editor = u.UserName,
                        Reference = m.Title,
                        DateIn = Convert.ToDateTime(dt),
                        Edited = Convert.ToDateTime(dt).ToString(),
                        UserActivitiesID = cugDB.UserActivities.Max(maxID => maxID.UserActivitiesID) + 1,
                        ID = cugDB.UserActivities.Max(maxID => maxID.UserActivitiesID) + 1,
                        AttachmentId = m.Id
                    };

                    cugDB.UserActivities.Add(ua);
                }
                else
                {//update datein time stamp
                    ar.Editor = u.UserName;
                    ar.Reference = m.Title;
                    ar.DateIn = Convert.ToDateTime(dt);
                    ar.Edited = Convert.ToDateTime(dt).ToString();
                    cugDB.SaveChanges();
                }


                cugDB.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Edit Upload Reference
        /// </summary>
        /// <param name="idx"></param>
        public void LogUserActivityEditReferenceUpload(MainFilesLink m, int userId)
        {


            try
            {
                return;
                var u = cugDB.UserMasters.Where(x => x.ID.Equals(userId)).FirstOrDefault();

                var dt = DateTime.Now.AddHours(9);
                UserActivity ua = new UserActivity
                {
                    Editor = u.Name,
                    Reference = m.Idx,
                    DateIn = dt,
                    Edited = dt.ToString(),
                    Attachment = m.idFiles.Value.ToString(),
                    AttachmentId = m.idFiles,
                    UserActivitiesID = cugDB.UserActivities.Max(maxID => maxID.UserActivitiesID) + 1,
                    ID = cugDB.UserActivities.Max(maxID => maxID.UserActivitiesID) + 1
                };

                cugDB.UserActivities.Add(ua);
                cugDB.SaveChanges();

            }
            catch (Exception ex)
            {

                //throw;todo tomorrow...
            }

        }


        private void EmailHotLink(Register reg)
        {
            try
            {
                var fromAddress = new MailAddress(reg.Email, reg.Name);
                string subject = "CUG : Online Registration request";
                string htmlBody;

                htmlBody = "Good Day CUG Admin, <br/>" +
                    "This is to inform you that " + reg.Name + " " + reg.Surname + " email : " + reg.Email + "  has requested registration for cugonline. <br/> " +
                    " username requested : " + reg.UserName + " <br/>" +
                    " password requested : " + reg.Password + " <br/>" +
                    " Please review <a href='https://cugonline.co.za/Users'> User Management to Activate account. </a>";

                var smtp = new SmtpClient
                {
                    Host = "relay-hosting.secureserver.net",
                    Port = 25,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };


                //send to David admin
                var toAddress = new MailAddress("davidrallen1942@gmail.com", "CUG Admin");
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }

                //send to David admin
                toAddress = new MailAddress("cugonlinesa@gmail.com", "CUG Admin");
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }


                //send to David admin
                toAddress = new MailAddress("gearup4it@gmail.com", "CUG Admin");
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }

                //email applicant
                subject = "CUG : Online Registration received";
                htmlBody = "Good Day " + reg.Name + " " + reg.Surname +
                                  "Thank you for your CUG online Registration request. <br/> " +
                                  " username requested : " + reg.UserName + " <br/>" +
                                  " password requested : " + reg.Password + " <br/>" +
                                  " Our consultants will contact you shortly. <a href='https://cugonline.co.za/login'> </a>";



                toAddress = new MailAddress(reg.Email, reg.Name);
                fromAddress = new MailAddress("cugonlinesa@gmail.com", "CUG Admin");
                using (var message = new MailMessage(toAddress, fromAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private void emailConfirmation(int id)
        {
            var u = cugDB.UserMasters.Where(x => x.ID.Equals(id)).FirstOrDefault();

            //send email to applicant
            var fromAddress = new MailAddress(u.Email, u.Name);
            string subject = "CUG : Online Registration request";
            string htmlBody;


            var smtp = new SmtpClient
            {
                Host = "relay-hosting.secureserver.net",
                Port = 25,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            //email applicant
            subject = "CUG : Online Registration completed.";
            htmlBody = "Good Day " + u.Name + " " + u.Surname +
                              " Your CUG online Registration is completed . login details: <br/> " +
                              " username requested : " + u.UserName + " <br/>" +
                              " password requested : " + u.Password + " <br/>" +
                              "Welcome to the CUG Family. <a href='https://cugonline.co.za/login'> </a>";



            var toAddress = new MailAddress(u.Email, u.Name);
            fromAddress = new MailAddress("cugonlinesa@gmail.com", "CUG Admin");
            using (var message = new MailMessage(toAddress, fromAddress)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }

        //edit User
        [Route("EditSystemUser")]
        [HttpPost]
        public object EditSystemUser(UserMaster user)
        {
            try
            {
                if (user.ID > 0)
                {

                    var Update = cugDB.UserMasters.Find(user.ID);

                    Update.Name = user.Name;
                    Update.Surname = user.Surname;
                    Update.UserName = user.UserName;
                    Update.Password = user.Password;
                    Update.Email = user.Email;
                    Update.categoryN = user.categoryN;
                    Update.isSuperAdmin = user.isSuperAdmin;
                    Update.nKey = user.nKey == "true" ? "1" : "0";
                    Update.Date_last = DateTime.Now.ToString();
                    cugDB.Entry(Update).State = System.Data.Entity.EntityState.Modified;
                    cugDB.SaveChanges();
                    return new Response
                    { Status = "Success", Message = "Record SuccessFully Saved." };
                }
                else
                {
                    var nextUserId = cugDB.UserMasters.OrderByDescending(u => u.ID).FirstOrDefault().ID + 1;
                    UserMaster um = new UserMaster();
                    um.ID = nextUserId;
                    um.Name = user.Name;
                    um.Surname = user.Surname;
                    um.UserName = user.UserName;
                    um.Password = user.Password;
                    um.Email = user.Email;
                    um.categoryN = user.categoryN;
                    um.isSuperAdmin = user.isSuperAdmin;
                    um.IsDeleted = false;
                    um.Date_added = DateTime.Now.Date.ToLongDateString();

                    cugDB.UserMasters.Add(um);
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


        [Route("DeleteUserById")]
        [HttpGet]
        public SystemUsersDTO DeleteUserById(int id)
        {

            try
            {
                var userDetails = cugDB.UserMasters.Where(m => m.ID.Equals(id)).FirstOrDefault();

                if (userDetails.isNew.Value == true)//registration complete
                {
                    emailConfirmation(id);
                }

                userDetails.IsDeleted = !userDetails.IsDeleted;
                userDetails.isNew = false;
                cugDB.Entry(userDetails).State = System.Data.Entity.EntityState.Modified;
                cugDB.SaveChanges();
                 
                return new SystemUsersDTO { IsDeleted = true };
            }
            catch (Exception ex)
            {
                throw;
            }

        }



        [Route("GetSystemUserById")]
        [HttpGet]
        public SystemUsersDTO GetSystemUserById(int id)
        {

            try
            {
                var result = cugDB.UserMasters.Where(u => u.ID.Equals(id)).Select(u => new SystemUsersDTO
                {
                    Id = u.ID,
                    Name = u.Name,
                    Surname = u.Surname,
                    CategoryName = u.categoryN.Trim(),
                    UserName = u.UserName,
                    Email = u.Email,
                    Password = u.Password,
                    IsEditor = (u.nKey == "1") ? true : false,
                    IsSuperAdmin = u.isSuperAdmin.HasValue ? u.isSuperAdmin.Value : false

                }).FirstOrDefault();

                if (result != null)
                {
                    return result;
                }
                else
                    return new SystemUsersDTO();
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        [Route("GetUserActivityLoginHistory")]
        [HttpGet]
        public List<UserActivitiesDTO> GetUserActivityLoginHistory(DateTime loginFrom, DateTime loginTo)
        {

            try
            {
                loginTo = loginTo.AddDays(1);
                var result = cugDB.UserActivities.Where(ua => (ua.DateIn >= loginFrom && ua.DateIn <= loginTo)
                                                      && ua.Reference.Equals("login")
                                                      //   && ua.Uploaded.Equals(null)
                                                      ).ToList().Select(ua => new UserActivitiesDTO
                                                      {
                                                          Editor = ua.Editor,
                                                          DateInFormatted = (ua.DateIn.HasValue) ? ua.DateIn.Value.ToString("dd MMM yyyy HH:mm") : "",
                                                          DateIn = ua.DateIn.Value
                                                      }).OrderByDescending(ua => ua.DateIn).ToList();

                if (result != null)
                {
                    return result;
                }
                else
                    return new List<UserActivitiesDTO>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Route("GetUserActivityEditHistory")]
        [HttpGet]
        public List<UserActivitiesDTO> GetUserActivityEditHistory(DateTime loginFrom, DateTime loginTo)
        {

            try
            {

                loginTo = loginTo.AddDays(1);
                var lastHistoryDate = DateTime.Now.AddMonths(-1);

                var referenceResult = (from ua in cugDB.UserActivities
                                       join m in cugDB.Mains on ua.AttachmentId equals m.Id //userActity ID = ReferenceId
                                       where ua.DateIn.Value >= loginFrom && ua.DateIn.Value <= loginTo
                                        && ua.AttachmentId != null
                                        && m.currentStatus.ToLower() != "deleted"
                                       && ua.DateIn > lastHistoryDate
                                       orderby ua.DateIn descending
                                       select new { ua.Editor, ua.DateIn, m.Title, ua.AttachmentId, m.Id, m.Idx }).ToList().Select(x => new UserActivitiesDTO
                                       {
                                           Editor = x.Editor,
                                           //DateInFormatted = Convert.ToString(x.DateIn.Value),
                                           DateInFormatted = (x.DateIn.HasValue) ? x.DateIn.Value.ToString("dd MMM yyyy HH:mm") : "",
                                           Reference = x.Title,
                                           FileUploadComment = "",
                                           FileUploaded = "",
                                           UploadUrl = "",
                                           AttachmentId = x.Id,
                                           DateIn = x.DateIn.Value,
                                           ID = x.AttachmentId.Value,
                                           Idx = x.Idx

                                       }).ToList();


                var result = referenceResult.ToList();

                if (result != null)
                {
                    //return result.OrderByDescending(u => u.DateInFormatted).ToList();
                    return result.OrderByDescending(u => u.DateIn).ToList();
                }
                else
                    return new List<UserActivitiesDTO>();

            }
            catch (Exception ex)
            {

                throw;
            }


        }

        [Route("GetNewRegistration")]
        [HttpGet]
        public bool GetNewRegistration()
        {
            var u = cugDB.UserMasters.Where(x => x.isNew.Value == true).FirstOrDefault();

            if (u == null)
            {
                return false;
            }
            return true;//we have a user!!
        }

        #region ... Images
        /// <summary>
        /// Return all files to the client
        /// </summary>
        /// <returns></returns>

        [Route("GetFiles")]
        [HttpGet]
        public List<FilesInfo> GetFilesById(string idx)
        {
            List<FilesInfo> files = new List<FilesInfo>();

            //var filePath = "https://cugonlinestorage.blob.core.windows.net/img/";//!cid_00ba01ca6c31%245f9e07f0%240f01a8c0%40desktoptammy_t.jpg";//
            var filePath = "https://cugonline.co.za/images/pdf_icon.png";
            using (testEntities db = new testEntities())
            {
                var images = (from mfl in cugDB.MainFilesLinks
                              join mf in cugDB.MainFiles on mfl.idFiles equals mf.id
                              where mfl.Idx == idx
                              && mfl.Idx != "0"
                              && mf.IsDeleted != true
                              select new FilesInfo()
                              {
                                  FileId = mf.id,
                                  FileName = mf.fName,
                                  FilePath = (mf.fNamePath != null) ?
                                                (mf.fNamePath.Contains(".pdf") || mf.fNamePath.Contains(".doc")) ? filePath : mf.fNamePath
                                                : filePath,//  filePath + mf.fName,
                                  FileComment = mf.fComment,
                                  ThumbNail = (mf.fNamePath != null) ?
                                                (mf.fNamePath.Contains(".pdf") || mf.fNamePath.Contains(".doc")) ? mf.fNamePath : mf.fNamePath
                                                : filePath
                              }).ToList();

                return images;
            }
        }

        [Route("Upload")]
        [HttpPost]
        public object Upload(string id, string comment, int userId)
        {
            var file = HttpContext.Current.Request.Files[0];//we have the file...

            var postedFile = HttpContext.Current.Request.Files[0];
            var filePath = HttpContext.Current.Server.MapPath("~/images/" + postedFile.FileName);

            try
            {
                //log to database
                var rootPath = "https://geared4it.net/images/";
                var nextFileId = cugDB.MainFiles.OrderByDescending(mf => mf.id).FirstOrDefault().id + 1;
                MainFile f = new MainFile();
                f.id = nextFileId;
                f.fName = postedFile.FileName;// uniquefileName;// file.FileName;
                f.fNamePath = rootPath + postedFile.FileName;// uniquefileName;
                f.fComment = (comment == "undefined") ? Path.GetFileNameWithoutExtension(postedFile.FileName) : comment;
                f.fType = Path.GetExtension(file.FileName);
                f.fExported = "N";

                cugDB.MainFiles.Add(f);
                cugDB.SaveChanges();

                //link to MainFilesLink
                var nextFileLinkId = cugDB.MainFilesLinks.OrderByDescending(mfl => mfl.Id).FirstOrDefault().Id + 1;
                MainFilesLink fl = new MainFilesLink
                {
                    Id = nextFileLinkId,
                    Idx = id,
                    fSorted = null,
                    idFiles = nextFileId
                };

                cugDB.MainFilesLinks.Add(fl);
                cugDB.SaveChanges();

                //save to User Activies
                LogUserActivityEditReferenceUpload(fl, userId);

                postedFile.SaveAs(filePath);
                return postedFile.FileName + " filePath: " + filePath;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion
    }

    #region ... Models DTOs

    public class FilesDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileComment { get; set; }
        public string FileType { get; set; }
    }

    public class FiguresLinkDTO
    {
        public int Id { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        public string body { get; set; }
        public string meaning { get; set; }
    }

    public class BibleFootNoteContentDTO
    {
        public string Idx { get; set; }
        public string Content { get; set; }
    }
    public class BibleReferencesDTO
    {
        public int Id { get; set; }
        public string BookOf { get; set; }
        public int ChapterId { get; set; }
        public int VerseId { get; set; }
        public string BodyText { get; set; }
        public string CompoKey { get; set; }
        public int ResultsFound { get; set; }
    }

    public class FiguresDTO
    {
        public int Id { get; set; }
        public string Idx { get; set; }
        public string Title { get; set; }
        public string Meaning { get; set; }
        public string Body { get; set; }
        public string LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CategoryName { get; set; }
        public string CurrentStatus { get; set; }
        public string HasAttachments { get; set; }
    }

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

    public class FigureStasticsDTO
    {
        public string Category { get; set; }
        public string CurrentStatus { get; set; }
        public int TotalCount { get; set; }
    }
    public class FigureLinksDTO
    {
        public int LinkId { get; set; }
        public string LinkTitle { get; set; }
        public string LinkIdx { get; set; }
        public string LinkIdxFriendlyName { get; set; }
        public string Link { get; set; }
    }

    public class FilesInfo
    {
        public int FileId { get; set; }
        public string FilePath { get; set; }
        public string FileTitle { get; set; }
        public string FileName { get; set; }
        public string FileComment { get; set; }
        public string ThumbNail { get; set; }
        public int? SortOrder { get; set; }
    }
    #endregion
}
