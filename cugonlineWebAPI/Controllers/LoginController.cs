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

namespace cugonlineWebAPI.Controllers
{

    [RoutePrefix("Api/login")]
    public class LoginController : ApiController
    {
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


        //[Route("BibleFootNoteContent")]
        //[HttpGet]
        //public BibleFootNoteContentDTO BibleFootNoteContent(string compoKey)
        //{
        //    try
        //    {
        //        var key = cugDB.BibleFootNotes.Where(b => b.CompoKey.Equals(compoKey)).FirstOrDefault();

        //        if (key != null)
        //        {
        //            var results = cugDB.BibleFootNoteContents.Where(
        //              m => m.Idx.Equals(key.Idx)
        //              ).Select(m => new BibleFootNoteContentDTO
        //              {
        //                  Idx = m.Idx,
        //                  Content = m.Content
        //              }).FirstOrDefault();

        //            return results;

        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        [Route("BibleFootNoteContent")]
        [HttpGet]
        public BibleFootNoteContentDTO BibleFootNoteContent(string Idx)
        {
            try
            {
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
                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(results.Content);
                    string strPreviousOuterHtml = string.Empty;
                    HtmlNodeCollection nc = document.DocumentNode.SelectNodes("//a");
                    if (nc != null)
                    {
                        foreach (HtmlNode node in nc)
                        {
                            strPreviousOuterHtml = node.OuterHtml;
                            if (node.Attributes["href"] != null)
                            {
                                node.Attributes.Add("data-idx", node.Attributes["href"].Value.Substring(node.Attributes["href"].Value.LastIndexOf('=') + 1).Replace(" ", "_"));
                                node.Attributes["href"].Value = "/bibleReferences?" + node.Attributes["href"].Value.Substring(node.Attributes["href"].Value.LastIndexOf('=') + 1).Replace(" ", "_");
                            }

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
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Route("BibleReferences")]
        [HttpGet]
        public List<BibleReferencesDTO> GetBibleReferences(string searchFilter)
        {
            
            var results = cugDB.BibleBooks.Where(//m => m.BookId.Contains(filter) && 
                  m => m.ChapterId != 0
                 // && m.BookId.Contains("Genesis") //&& m.ChapterId == 1 && m.VerseId == 16
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
                results = results.Where(b => b.BodyText.ToLower().Contains(searchFilter)).ToList();
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
        [Route("Figures")]
        [HttpGet]
        public List<FiguresDTO> GetFigures(string filter)
        {
            var results = cugDB.Mains.Where(m => m.Title.Contains(filter)).Select(m => new FiguresDTO
            {
                Id = m.Id,
                Idx = m.Idx,
                Title = m.Title.ToUpper(),
                LastUpdated = m.LastUpdated.ToString(),
                LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : ""

            }).OrderBy(m => m.Title).ToList();

            if (results != null)
            {
                return results;
            }
            else
                return null;
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
                //result = cugDB.sp_EmptyReferencesDetails(filter, type).Select(m => new FiguresDTO
                //{
                //    Id = m.id,
                //    Idx = m.Idx,
                //    Title = m.Title.ToUpper(),
                //    LastUpdated = m.LastUpdated.ToString(),
                //    CategoryName = m.CategoryN.Trim(),
                //    CurrentStatus = m.currentStatus.Trim(),
                //    LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : "",


                //}).OrderBy(m => m.Title).ToList();
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
            //var results = cugDB.Mains.Select(m => new FiguresLinkDTO
            var results = cugDB.Mains.Where(m => m.Title.Contains(filter)).Select(m => new FiguresLinkDTO
            {
                value = m.Idx,
                label = m.Title.ToUpper(),
            }).OrderBy(m => m.value).ToList();

            if (results != null)
            {
                return results;
            }
            else
                return null;
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
                               join m in cugDB.Mains on sm.Link equals m.Idx
                               where m.Idx == idx
                               select new FigureLinksDTO
                               {
                                   LinkId = sm.Id,
                                   LinkIdx = sm.Idx,// sm.Idx,
                                   LinkTitle = sm.Title,
                                   LinkIdxFriendlyName = sm.Idx.Replace("_", " ")
                               }).OrderBy(x => x.LinkIdx).ToList();

                if (results != null) return results;
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
            var updateReferece = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkIdx) && sm.Link.Equals(reference.LinkTitle)).FirstOrDefault();

            cugDB.SeeMains.Remove(updateReferece);
            cugDB.SaveChanges();

            //remove  reciprocal
            var updateReciprocal = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkTitle)
                                                     && sm.Link.Equals(reference.LinkIdx)).FirstOrDefault();

            cugDB.SeeMains.Remove(updateReciprocal);
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
                { Status = "Success", Message = "Image Deleted Saved." };
            }
            updateImage.IsDeleted = true;

            cugDB.Entry(updateImage).State = System.Data.Entity.EntityState.Modified;
            cugDB.SaveChanges();

            return new Response
            { Status = "Success", Message = "Reference Deleted Saved." };
        }

        [Route("AddReference")]
        [HttpPost]
        public object AddReference(FigureLinksDTO reference)
        {

            var linkInfo = cugDB.Mains.Where(m => m.Idx.Equals(reference.LinkIdx)).FirstOrDefault();//get link information

            var updateReferece = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkIdx)  //check if link exists in Reference...
                                                         && sm.Link.Equals(reference.Link)).FirstOrDefault();

            try
            {
                if (updateReferece == null)
                {
                    //add to seemain
                    SeeMain m = new SeeMain();
                    //var maxId = cugDB.SeeMains.Max(sm => sm.Id) + 1;
                    //m.Id = maxId;
                    m.Idx = reference.LinkIdx;
                    m.Link = reference.Link;
                    m.Title = linkInfo.Title;//.LinkTitle.Replace("_", " ");
                    m.categoryN = linkInfo.CategoryN.Trim();//.Link;
                    m.catFlag = "O";
                    cugDB.SeeMains.Add(m);
                    cugDB.SaveChanges();

                    //do reciprocal
                    var recipricolInfo = cugDB.Mains.Where(m_recip => m_recip.Idx.Equals(reference.Link)).FirstOrDefault();//get inverse link information

                    var reciprocalReferece = cugDB.SeeMains.Where(sm_recip => sm_recip.Idx.Equals(reference.Link)  //check inverse...
                                                                 && sm_recip.Link.Equals(reference.LinkIdx)).FirstOrDefault();

                    if (reciprocalReferece == null)
                    {
                        //add to reciprocal seemain
                        SeeMain m_recip = new SeeMain();
                        m_recip.Idx = reference.Link;
                        m_recip.Link = reference.LinkIdx;
                        m_recip.Title = recipricolInfo.Title;
                        m_recip.categoryN = !string.IsNullOrEmpty(recipricolInfo.CategoryN) ? recipricolInfo.CategoryN.Trim() : "All";//.Link;
                        m_recip.catFlag = "O";
                        cugDB.SeeMains.Add(m_recip);
                        cugDB.SaveChanges();

                    }

                    return new Response
                    { Status = "Success", Message = "Record SuccessFully Saved." };

                }

                return new Response
                { Status = "Success", Message = "Reference Already Exists." };
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message;
            }


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
                    referenceItem.currentStatus = "review";// (fig.IsSuperAdmin) ? "live" : "review";

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
                    m.LastUpdatedBy = fig.LastUpdatedBy.Value;
                    m.CategoryN = (fig.CategoryN == null) ? "All" : fig.CategoryN.Trim();
                    m.currentStatus = (fig.IsSuperAdmin) ? "review" : "new";
                    m.LastUpdated = DateTime.Now;
                    refIdx = m.Idx;
                    cugDB.Mains.Add(m);
                    cugDB.SaveChanges();
                    //log to User Activities
                    LogUserActivityEditReference(m, fig.LastUpdatedBy.Value);
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
                    LogUserActivityEditReference(referenceItem, fig.LastUpdatedBy.Value);
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
                    LogUserActivityEditReference(m, fig.LastUpdatedBy.Value);
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
                var u = cugDB.UserMasters.Where(x => x.ID.Equals(userId)).FirstOrDefault();

                var dt = DateTime.Now.AddHours(9);
                UserActivity ua = new UserActivity
                {
                    Editor = u.UserName,
                    Reference = m.Idx,
                    DateIn = Convert.ToDateTime(dt),
                    Edited = DateTime.Now.ToString(),
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

        /// <summary>
        /// Edit Upload Reference
        /// </summary>
        /// <param name="idx"></param>
        public void LogUserActivityEditReferenceUpload(MainFilesLink m, int userId)
        {


            try
            {
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
                if (user.ID != -1)
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
                                                      && ua.Reference.Equals(null)
                                                      && ua.Uploaded.Equals(null)).ToList().Select(ua => new UserActivitiesDTO
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
            var referenceResult = cugDB.UserActivities.Where(ua => (ua.DateIn >= loginFrom && ua.DateIn <= loginTo)
                                                   && !ua.Reference.Equals(null)
                                                   && !ua.AttachmentId.HasValue
                                                   ).ToList().Select(ua => new UserActivitiesDTO
                                                   {
                                                       Editor = ua.Editor,
                                                       DateInFormatted = (ua.DateIn.HasValue) ? ua.DateIn.Value.ToString("dd MMM yyyy HH:mm") : "",
                                                       Reference = ua.Reference,
                                                       FileUploadComment = "",
                                                       FileUploaded = "",
                                                       UploadUrl = "",
                                                       AttachmentId = null,
                                                       DateIn = ua.DateIn.Value,

                                                   }).OrderByDescending(ua => ua.DateIn).ToList();


                var uploadResult = (from ua in cugDB.UserActivities
                                    join ml in cugDB.MainFiles on ua.AttachmentId.Value equals ml.id
                                    where ua.DateIn.Value >= loginFrom && ua.DateIn.Value <= loginTo
                                    && ua.AttachmentId != null
                                    && ml.IsDeleted.Value != true
                                    select new UserActivitiesDTO
                                    {
                                        Editor = ua.Editor,
                                        DateInFormatted = (ua.DateIn != null) ? ua.DateIn.Value.ToString() : "",
                                        Reference = ua.Reference,
                                        FileUploadComment = ml.fComment,
                                        FileUploaded = ml.fName,
                                        UploadUrl = ml.fNamePath,
                                        AttachmentId = ua.AttachmentId.Value,
                                        DateIn = ua.DateIn.Value


                                    }).OrderByDescending(u => u.DateIn).ToList();

                var formattedUpload = new List<UserActivitiesDTO>();

                foreach (var item in uploadResult)
                {
                    item.DateInFormatted = item.DateIn.Value.ToString("dd MMM yyyy HH:mm");
                    formattedUpload.Add(item);
                }


                var result = referenceResult.Union(formattedUpload).ToList();

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
            var filePath = "https://cugonline.co.za/images/";
            using (testEntities db = new testEntities())
            {
                var images = (from mfl in cugDB.MainFilesLinks
                              join mf in cugDB.MainFiles on mfl.idFiles equals mf.id
                              where mfl.Idx == idx
                              && mf.IsDeleted != true
                              select new FilesInfo()
                              {
                                  FileId = mf.id,
                                  FileName = mf.fName,
                                  FilePath = mf.fNamePath,//  filePath + mf.fName,
                                  FileComment = mf.fComment
                              }).ToList();

                return images;
            }
        }

        [Route("Upload")]
        [HttpPost]
        public object Upload(string id, string comment, int userId)
        {
            var file = HttpContext.Current.Request.Files[0];//we have the file...

            // var uniquefileName = id + "_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            //string ftpAddress = @"197.242.150.135"; // ConfigurationManager.AppSettings.Get("CloudStorageContainerReference")
            //string username = "cugftp"; // ConfigurationManager.AppSettings.Get("CloudStorageContainerReference")
            //string password = "Kjgv5FtNX!2$7qBtP3"; // ConfigurationManager.AppSettings.Get("CloudStorageContainerReference")

            var postedFile = HttpContext.Current.Request.Files[0];
            var filePath = HttpContext.Current.Server.MapPath("~/images/" + postedFile.FileName);
            // var dbtesting = filePath;

            // var destinationDirectory = new DirectoryInfo(Path.GetDirectoryName(filePath));

            
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

        [Route("UploadOld")]
        [HttpPost]
        // public async Task<IHttpActionResult> Upload(string id)
        public object UploadOld(string id, string comment, int userId)
        {


            var file = HttpContext.Current.Request.Files[0];//we have the file...

            if (HttpContext.Current.Request.Files.Count > 0)
            {

                var accountName = "cugonlinestorage";// ConfigurationManager.AppSettings["cugonlinestorage"];
                var accountKey = "V9xb1fQUAt/90BtzG5+1o1rlcKMP1cY83PGONzzNu5bxXW4DZ09c+/yLW4ixbdnLaNcRpkrJX7OqFALKet0FcQ==";// ConfigurationManager.AppSettings["V9xb1fQUAt/90BtzG5+1o1rlcKMP1cY83PGONzzNu5bxXW4DZ09c+/yLW4ixbdnLaNcRpkrJX7OqFALKet0FcQ=="];
                CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);

                var storageClient = storageAccount.CreateCloudBlobClient();
                var storageContainer = storageClient.GetContainerReference(ConfigurationManager.AppSettings.Get("CloudStorageContainerReference"));
                storageContainer.CreateIfNotExists();
                for (int fileNum = 0; fileNum < HttpContext.Current.Request.Files.Count; fileNum++)
                {

                    var uniquefileName = id + "_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    if (HttpContext.Current.Request.Files[fileNum] != null && HttpContext.Current.Request.Files[fileNum].ContentLength > 0)
                    {


                        CloudBlockBlob azureBlockBlob = storageContainer.GetBlockBlobReference(uniquefileName);
                        azureBlockBlob.UploadFromStream(HttpContext.Current.Request.Files[fileNum].InputStream);


                        try//saving to database...
                        {

                            var rootPath = "http://gearup4it.net/images/";

                            var nextFileId = cugDB.MainFiles.OrderByDescending(mf => mf.id).FirstOrDefault().id + 1;
                            MainFile f = new MainFile();
                            f.id = nextFileId;
                            f.fName = uniquefileName;// file.FileName;
                            f.fNamePath = rootPath + uniquefileName;
                            f.fComment = comment;
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

                        }

                        catch (Exception ex)
                        {
                            throw ex;

                        }
                    }

                }

            }

            return Ok();
        }

        #endregion
    }


    #region Cloud storage
    public class CloudFile
    {
        public string FileName { get; set; }
        public string URL { get; set; }
        public long Size { get; set; }
        public static CloudFile CreateFromIListBlobItem(IListBlobItem item)
        {
            if (item is CloudBlockBlob)
            {
                var blob = (CloudBlockBlob)item;
                return new CloudFile
                {
                    FileName = blob.Name,
                    URL = blob.Uri.ToString(),
                    Size = blob.Properties.Length
                };
            }
            return null;
        }
    }

    public class CloudFilesModel
    {
        public CloudFilesModel()
                : this(null)
        {
            Files = new List<CloudFile>();
        }
        public CloudFilesModel(IEnumerable<IListBlobItem> list)
        {
            Files = new List<CloudFile>();
            if (list != null && list.Count<IListBlobItem>() > 0)
            {
                foreach (var item in list)
                {
                    CloudFile info = CloudFile.CreateFromIListBlobItem(item);
                    if (info != null)
                    {
                        Files.Add(info);
                    }
                }
            }
        }
        public List<CloudFile> Files { get; set; }
    }
    #endregion
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
        public string label { get; set; }
        public string value { get; set; }
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
        public string FileName { get; set; }
        public string FileComment { get; set; }
        public string LinkTitle { get; set; }
    }
    #endregion
}
