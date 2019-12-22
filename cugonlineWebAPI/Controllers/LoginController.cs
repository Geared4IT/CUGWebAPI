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
        /// <returns>list of figures</returns>
        [Route("Figures")]
        [HttpGet]
        public List<FiguresDTO> getFigures()
        {
            var results = cugDB.Mains.Select(m => new FiguresDTO
            {
                Id = m.Id,                
                Title = m.Title.ToUpper(),               
            }).OrderBy(m => m.Title).ToList();

            if (results != null)
            {
                return results;
            }
            else
                return null;
        }


        [Route("GetFigureLinksById")]
        [HttpGet]
        public List<FigureLinksDTO> GetFigureLinksById(string id)
        {
            var idx = "South_Africa";
            var results = cugDB.SeeMains.Where(m => m.Idx.Equals(idx)).Select(m => new FigureLinksDTO
            {
                LinkId = m.Id,
                LinkTitle = m.Title
            }).ToList();

            if (results != null) return results;
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
            var id = int.Parse(idx);
            //var filePath = "";
            //filePath = "https://cugonlinestorage.blob.core.windows.net/images/!cid_00ba01ca6c31%245f9e07f0%240f01a8c0%40desktoptammy_t.jpg";//

            using (testEntities db = new testEntities())
            {
                var images = (from f in db.Files                              
                              where f.fMainId.Equals(id)
                              select new FilesInfo()
                              {
                                  FileName = f.fName,
                                  FilePath = f.filePath,
                                  FileComment = f.fComment
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
    public class FiguresDTO
    {
        public int Id { get; set; }
        public string Idx { get; set; }
        public string Title { get; set; }
        public string Meaning { get; set; }
        public string Body { get; set; }
    }

    public class FigureLinksDTO
    {
        public int LinkId { get; set; }
        public string LinkTitle { get; set; }
    }

    public class FilesInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileComment { get; set; }
    }
    #endregion
}
