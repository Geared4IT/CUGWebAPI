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

namespace cugonlineWebAPI.Controllers
{

    [RoutePrefix("Api/login")]
    public class LoginController : ApiController
    {
        string rootPath = "https://cugonlinestorage.blob.core.windows.net/images/";
        testEntities cugDB = new testEntities();
        // private readonly CloudBlobContainer _blobContainer;
        private const string Container = "images_t";

        public LoginController()//CloudBlobContainer blobContainer)
        {
            //The Path of the Image store on the server side
            var _test_rootPath = HostingEnvironment.MapPath("~/images/");
            /// _blobContainer = blobContainer;
        }

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
                return new SystemUsersDTO { Name = "Invalid"};
            }
            else
                return new SystemUsersDTO {
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
                CategoryName =  u.categoryN.Trim(),
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
                              LinkTitle = sm.Idx
                          }).ToList();

            if (results != null) return results;
                                 return null;
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
                LastUpdatedBy = (m.LastUpdatedBy.HasValue == true) ? cugDB.UserMasters.Where(u => u.ID.Equals(m.LastUpdatedBy.Value)).FirstOrDefault().Name : ""

            }).FirstOrDefault();

            if (result != null)
            {
                return result;
            }
            else
                return new FiguresDTO();
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


        [Route("DeleteReference")]
        [HttpPost]
        public object DeleteReference(FigureLinksDTO reference)
        {
            var updateReferece = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkIdx) && sm.Link.Equals(reference.LinkTitle)).FirstOrDefault();
            //updateReferece.re
            cugDB.SeeMains.Remove(updateReferece);
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
            var updateReferece = cugDB.SeeMains.Where(sm => sm.Idx.Equals(reference.LinkIdx) && sm.Link.Equals(reference.LinkTitle)).FirstOrDefault();
            
            if(updateReferece == null)
            {
                //add to seemain
                SeeMain m = new SeeMain();

                m.Idx = reference.LinkIdx;
                m.Link = reference.LinkTitle;
                m.Title = reference.LinkTitle.Replace("_"," ");
                m.categoryN = "ALL";
                m.catFlag = "O";
                cugDB.SeeMains.Add(m);
                cugDB.SaveChanges();
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
                if (fig.Id != 0)
                {

                    var Update = cugDB.Mains.Find(fig.Id);

                    Update.Title = fig.Title;
                    Update.Meaning = fig.Meaning;
                    Update.Body = fig.Body;
                    Update.Idx = fig.Idx;
                    Update.LastUpdated = DateTime.Now.Date;
                    Update.LastUpdatedBy = fig.LastUpdatedBy;
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
            var filePath ="http://cugonline.co.za/images/";
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
        public async Task<IHttpActionResult> Upload(string id)
        {

            int figureId = int.Parse(id);
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

                    var uniquefileName = figureId + "_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    if (HttpContext.Current.Request.Files[fileNum] != null && HttpContext.Current.Request.Files[fileNum].ContentLength > 0)
                    {
                        CloudBlockBlob azureBlockBlob = storageContainer.GetBlockBlobReference(uniquefileName);
                        azureBlockBlob.UploadFromStream(HttpContext.Current.Request.Files[fileNum].InputStream);

                        
                        try//saving to database...
                        {
                            if (figureId == 0)//edit
                            {
                                var Update = cugDB.Files.Find(figureId);
                                Update.fComment = "GetComment";
                                cugDB.Entry(Update).State = System.Data.Entity.EntityState.Modified;
                                cugDB.SaveChanges();
                            }
                            else
                            {
                                Models.File f = new Models.File();

                                f.fName = file.FileName;
                                f.filePath = rootPath + uniquefileName;
                                f.fComment = file.FileName + "GetComment";
                                f.fType = Path.GetExtension(file.FileName);
                                f.fExported = "N";
                                f.fMainId = figureId;

                                cugDB.Files.Add(f);
                                cugDB.SaveChanges();

                            }
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
        public int CategoryId { get; set;  }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
        public string IsAdmin { get; set; }
    }

    public class FigureLinksDTO
    {
        public int LinkId { get; set; }
        public string LinkTitle { get; set; }
        public string LinkIdx { get;  set; }
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
