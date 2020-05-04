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

namespace cugonlineWebAPI.Controllers
{

    [RoutePrefix("Api/login")]
    public class LoginController : ApiController
    {
        //string rootPath = "https://cugonlinestorage.blob.core.windows.net/images/";
        readonly string rootPath = "https://cugonlinestorage.blob.core.windows.net/img/";
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
                UserMaster u = new UserMaster
                {
                    Email = Reg.Email,
                    Name = Reg.Name,
                    Password = Reg.Password,
                    Surname = Reg.Surname,
                    UserName = Reg.UserName
                };
                cugDB.UserMasters.Add(u);
                cugDB.SaveChanges();
                return new Response
                { Status = "Success", Message = "Record SuccessFully Saved." };

            }
            catch (Exception ex)
            {
                return new Response { Status = "Error" + ex.Message, Message = "Invalid Data." };
            }
        }

        [Route("Login")]
        [HttpPost]
        public SystemUsersDTO employeeLogin(Login login)
        {
            var u = cugDB.UserMasters.Where(x => x.UserName.Equals(login.UserName) && x.Password.Equals(login.Password)).FirstOrDefault();

            if (u == null)
            {
                return new SystemUsersDTO { Name = "Invalid" };
            }
            else
            {
                LogUserActivity(u);

                return new SystemUsersDTO
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
                    IsAdmin = u.nKey
                };
            }


        }
        /// <summary>
        /// save user login activity
        /// </summary>
        /// <param name="u"></param>
        private void LogUserActivity(UserMaster u)
        {
            UserActivity ua = new UserActivity
            {
                Editor = u.Name,
                DateIn = DateTime.Now,
                TimeIn = DateTime.Now.ToString()
            };

            cugDB.UserActivities.Add(ua);
            cugDB.SaveChanges();
        }

        /// <summary>
        /// get list of Figures
        /// </summary>
        /// <returns>list of figures</returns>
        [Route("Figures")]
        [HttpGet]
        public List<FiguresDTO> getFigures(string filter)
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
        /// get list of users
        /// </summary>
        /// <returns>list of figures</returns>
        [Route("SystemUsers")]
        [HttpGet]
        public List<SystemUsersDTO> getSystemUsers()
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
                IsAdmin = u.nKey,
                IsDeleted = (u.IsDeleted.HasValue) ? u.IsDeleted.Value : false
            }).OrderBy(u => u.UserName).ToList();

            if (results != null)
            {
                return results;
            }
            else
                return null;
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

        [Route("GetFigureLinksByIdx")]
        [HttpGet]
        public List<FigureLinksDTO> GetFigureLinksById(string idx)
        {
            var results = (from sm in cugDB.SeeMains
                           join m in cugDB.Mains on sm.Link equals m.Idx
                           where m.Idx == idx
                           select new FigureLinksDTO
                           {
                               LinkId = sm.Id,
                               LinkIdx = sm.Idx,// sm.Idx,
                               LinkTitle = sm.Title
                           }).ToList();

            if (results != null) return results;
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
                CategoryName = m.CategoryN.Trim()

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
            updateImage.IsDeleted = false;

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

            if (updateReferece == null)
            {
                //add to seemain
                SeeMain m = new SeeMain();

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

        [Route("EditFigure")]
        [HttpPost]
        public object EditFigure(Figure fig)
        {
            try
            {
                var refIdx = "";
                if (fig.Id != 0)//edit
                {
                    var Update = cugDB.Mains.Find(fig.Id);
                    Update.Title = fig.Title;
                    Update.Meaning = fig.Meaning;
                    Update.Body = fig.Body;
                    Update.Idx = fig.Idx;
                    Update.LastUpdated = DateTime.Now;
                    Update.LastUpdatedBy = fig.LastUpdatedBy;
                    Update.CategoryN = fig.CategoryN.Trim();
                    Update.currentStatus = (fig.LastUpdatedBy == 6) ? "live" : "review";

                    cugDB.Entry(Update).State = System.Data.Entity.EntityState.Modified;

                    refIdx = Update.Idx;
                    LogUserActivityEditReference(Update, fig.LastUpdatedBy.Value);
                }
                else//add
                {
                    var nextId = cugDB.Mains.OrderByDescending(main => main.Id).FirstOrDefault().Id + 1;
                    Main m = new Main();

                    m.Id = nextId;
                    m.Idx = nextId.ToString();
                    m.Title = fig.Title;
                    m.Body = fig.Body;
                    m.Meaning = fig.Meaning;
                    m.DateCreated = DateTime.Now;
                    m.CreatedBy = fig.LastUpdatedBy;
                    m.LastUpdatedBy = fig.LastUpdatedBy.Value;
                    m.CategoryN = (fig.CategoryN == null) ? "All" : fig.CategoryN.Trim();
                    m.currentStatus = (fig.LastUpdatedBy == 6) ? "live" : "new";
                    cugDB.Mains.Add(m);
                    refIdx = m.Idx;

                    //log to User Activities
                    LogUserActivityEditReference(m, fig.LastUpdatedBy.Value);

                }

                //email hotlink to David for Review
                //EmailHotLink(refIdx);



                cugDB.SaveChanges();
                return new Response
                { Status = refIdx, Message = "Record SuccessFully Saved." };
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
            var u = cugDB.UserMasters.Where(x => x.ID.Equals(userId)).FirstOrDefault();

            UserActivity ua = new UserActivity
            {
                Editor = u.UserName,
                Reference = m.Idx,
                DateIn = DateTime.Now,
                Edited = DateTime.Now.ToString()
            };

            cugDB.UserActivities.Add(ua);
            cugDB.SaveChanges();
        }

        /// <summary>
        /// Edit Upload Reference
        /// </summary>
        /// <param name="idx"></param>
        private void LogUserActivityEditReferenceUpload(MainFilesLink m, int userId)
        {
            var u = cugDB.UserMasters.Where(x => x.ID.Equals(userId)).FirstOrDefault();

            UserActivity ua = new UserActivity
            {
                Editor = u.Name,
                Reference = m.Idx,
                DateIn = DateTime.Now,
                Edited = DateTime.Now.ToString(),
                Attachment = m.idFiles.Value.ToString(),
                AttachmentId = m.idFiles
            };

            cugDB.UserActivities.Add(ua);
            cugDB.SaveChanges();
        }


        private void EmailHotLink(string idx)
        {

            try
            {
                var fromAddress = new MailAddress("admin@cugonline.co.za", "CUG Admin");
                var toAddress = new MailAddress("david@allenassociates.co.za", "David Allen");
                const string fromPassword = "1Ti4puraeg3@";
                const string subject = "Reference submitted for Review";
                string htmlBody;

                string hotLink = "https://cugonline.co.za/FigureDetails/" + idx;
                htmlBody = "Good Day David, <br/>" +
                    "This is to inform you that Reference <a href='" + hotLink + "' >" + idx + "</a> has been submitted for review ";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,

                    Credentials = new NetworkCredential("gearup4it@gmail.com", fromPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
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

        [Route("GetSystemUserById")]
        [HttpGet]
        public SystemUsersDTO GetSystemUserById(int id)
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
                IsAdmin = u.nKey

            }).FirstOrDefault();

            if (result != null)
            {
                return result;
            }
            else
                return new SystemUsersDTO();
        }


        [Route("GetUserActivityLoginHistory")]
        [HttpGet]
        public List<UserActivitiesDTO> GetUserActivityLoginHistory(DateTime loginFrom, DateTime loginTo)
        {
            var result = cugDB.UserActivities.Where(ua => (ua.DateIn > loginFrom && ua.DateIn < loginTo)
                                                         && ua.Reference.Equals(null)
                                                         && ua.Uploaded.Equals(null)).ToList().Select(ua => new UserActivitiesDTO
                                                         {
                                                             Editor = ua.Editor,
                                                             DateInFormatted = (ua.DateIn.HasValue) ? ua.DateIn.Value.ToString("dd MMM yyyy HH:mm") : ""
                                                         }).ToList();

            if (result != null)
            {
                return result.OrderByDescending(u => u.DateInFormatted).ToList();
            }
            else
                return new List<UserActivitiesDTO>();
        }

        [Route("GetUserActivityEditHistory")]
        [HttpGet]
        public List<UserActivitiesDTO> GetUserActivityEditHistory(DateTime loginFrom, DateTime loginTo)
        {

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
                                                       AttachmentId = null

                                                   }).ToList();



            var uploadResult = (from ua in cugDB.UserActivities
                                join ml in cugDB.MainFiles on ua.AttachmentId.Value equals ml.id
                                where ua.DateIn.Value > loginFrom && ua.DateIn.Value < loginTo
                                && ua.AttachmentId != null
                                select new UserActivitiesDTO
                                {
                                    Editor = ua.Editor,
                                    DateInFormatted = (ua.DateIn != null) ? ua.DateIn.Value.ToString() : "",
                                    Reference = ua.Reference,
                                    FileUploadComment = ml.fComment,
                                    FileUploaded = ml.fName,
                                    UploadUrl = ml.fNamePath,
                                    AttachmentId = ua.AttachmentId.Value,
                                    DateIn = ua.DateIn


                                }).ToList();

            var formattedUpload = new List<UserActivitiesDTO>();

            foreach (var item in uploadResult)
            {
                item.DateInFormatted = item.DateIn.Value.ToString("dd MMM yyyy HH:mm");
                formattedUpload.Add(item);
            }


            var result = referenceResult.Union(formattedUpload).ToList();

            if (result != null)
            {
                return result.OrderByDescending(u => u.DateInFormatted).ToList();
            }
            else
                return new List<UserActivitiesDTO>();
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

            var filePath = "https://cugonlinestorage.blob.core.windows.net/img/";//!cid_00ba01ca6c31%245f9e07f0%240f01a8c0%40desktoptammy_t.jpg";//
            //var filePath ="http://cugonline.co.za/images/";
            using (testEntities db = new testEntities())
            {
                var images = (from mfl in cugDB.MainFilesLinks
                              join mf in cugDB.MainFiles on mfl.idFiles equals mf.id
                              where mfl.Idx == idx
                              select new FilesInfo()
                              {
                                  FileId = mf.id,
                                  FileName = mf.fName,
                                  FilePath = filePath + mf.fName,
                                  FileComment = mf.fComment
                              }).ToList();

                return images;
            }
        }

        [Route("Upload")]
        [HttpPost]
        // public async Task<IHttpActionResult> Upload(string id)
        public object Upload(string id, string comment, int userId)
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
        public string IsAdmin { get; set; }
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
        public string Link { get; set; }
    }

    public class FilesInfo
    {
        public int FileId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileComment { get; set; }
    }
    #endregion
}
