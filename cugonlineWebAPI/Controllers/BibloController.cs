using cugonlineWebAPI.Models;
using cugonlineWebAPI.VM;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace cugonlineWebAPI.Controllers
{
    [RoutePrefix("Api/biblo")]
    public class BibloController : ApiController
    {
        testEntities cugDB = new testEntities();

        public BibloController()
        {
            //constructor
        }

        [Route("GetBiblioInfo")]
        [HttpGet]
        public List<BibloInfoDTO> GetBiblioInfo()
        {

            var results = cugDB.BibloMains.Where(bm => bm.ID.Equals("Pagex") || bm.ID.Equals("page1Top") || bm.ID.Equals("Page1l")
                                                || bm.ID.Equals("Page1r") || bm.ID.Equals("Comments")

                                                || bm.ID.Equals("PageBibloTitle") || bm.ID.Equals("CopyRight")
                                                || bm.ID.Equals("PageBibloBody")
                                                || bm.ID.Equals("refTop"))
                                                .Select(bm => new BibloInfoDTO
                                                {
                                                    Id = bm.MainId,
                                                    Body = bm.Body,
                                                    IDItems = bm.ID
                                                }).OrderBy(bm => bm.Id).ToList();
            if (results != null) return results;
            return new List<BibloInfoDTO>();
        }

        [Route("EditBibloInfo")]
        [HttpPost]
        public object EditBibloInfo(BibloInfoDTO info)
        {
            try
            {

                var UpdateBibloItems = cugDB.BibloMains.Where(bm => bm.ID.Equals("Pagex") || bm.ID.Equals("page1Top") || bm.ID.Equals("Page1l")
                                                || bm.ID.Equals("Page1r") || bm.ID.Equals("Comments")
                                                || bm.ID.Equals("PageBibloTitle") || bm.ID.Equals("CopyRight")
                                                || bm.ID.Equals("PageBibloBody")
                                                || bm.ID.Equals("refTop")).OrderBy(bm => bm.MainId).ToList();

                UpdateBibloItems[4].Body = info.PageRight;
                UpdateBibloItems[5].Body = info.PageLeft;
                UpdateBibloItems[8].Body = info.Comments;

                UpdateBibloItems[7].Body = info.PageBibloTitle;
                UpdateBibloItems[6].Body = info.PageBibloBody;
                UpdateBibloItems[2].Body = info.CopyRight;
                UpdateBibloItems[1].Body = info.PageRefTitle;


                cugDB.SaveChanges();
                return new Response
                { Status = "saved", Message = "Record SuccessFully Saved." };

            }
            catch (Exception ex)
            {
                return new Response
                { Status = "Error" + ex.Message, Message = "Invalid Data." };
            }

        }

        #region ... Images
        /// <summary>
        /// Return all biblo files to the client
        /// </summary>
        /// <returns></returns>

        [Route("GetBibloFiles")]
        [HttpGet]
        public List<FilesInfo> GetFilesById()
        {
            List<FilesInfo> files = new List<FilesInfo>();

            var filePath = "https://cugonlinestorage.blob.core.windows.net/bibloimg/";

            using (testEntities db = new testEntities())
            {
                var images = (from up in cugDB.BibloUploads

                              select new FilesInfo()
                              {
                                  FileId = up.Id,
                                  FileName = up.fName,
                                  FilePath = filePath + up.fName,
                                  FileComment = up.fDescription
                              }).ToList();

                return images;
            }
        }

        [Route("UploadBiblo")]
        [HttpPost]
        // public async Task<IHttpActionResult> Upload(string id)
        public object UploadBiblo(string comment)
        {
            var file = HttpContext.Current.Request.Files[0];//we have the file...

            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var accountName = "cugonlinestorage";// ConfigurationManager.AppSettings["cugonlinestorage"];
                var accountKey = "V9xb1fQUAt/90BtzG5+1o1rlcKMP1cY83PGONzzNu5bxXW4DZ09c+/yLW4ixbdnLaNcRpkrJX7OqFALKet0FcQ==";
                CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);

                var storageClient = storageAccount.CreateCloudBlobClient();
                var storageContainer = storageClient.GetContainerReference(ConfigurationManager.AppSettings.Get("CloudStorageContainerReference"));
                storageContainer.CreateIfNotExists();
                for (int fileNum = 0; fileNum < HttpContext.Current.Request.Files.Count; fileNum++)
                {

                    var uniquefileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    if (HttpContext.Current.Request.Files[fileNum] != null && HttpContext.Current.Request.Files[fileNum].ContentLength > 0)
                    {
                        CloudBlockBlob azureBlockBlob = storageContainer.GetBlockBlobReference(uniquefileName);
                        azureBlockBlob.UploadFromStream(HttpContext.Current.Request.Files[fileNum].InputStream);

                        try//saving to database...
                        {
                            var nextFileId = cugDB.BibloUploads.OrderByDescending(mf => mf.Id).FirstOrDefault().Id + 1;
                            BibloUpload f = new BibloUpload
                            {
                                Id = nextFileId,
                                fName = uniquefileName,// file.FileName;
                                                       // f.fNamePath = rootPath + uniquefileName;
                                fDescription = comment,
                                fType = Path.GetExtension(file.FileName)
                            };

                            cugDB.BibloUploads.Add(f);
                            cugDB.SaveChanges();

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

        [Route("DeleteBibloImage")]
        [HttpPost]
        public object DeleteBibloImage(FilesInfo item)
        {
            var updateImage = cugDB.BibloUploads.Where(mf => mf.Id.Equals(item.FileId)).FirstOrDefault();

            if (updateImage == null)
            {
                return new Response
                { Status = "Success", Message = "Image Deleted Saved." };
            }
           // updateImage.IsDeleted = false;

            cugDB.Entry(updateImage).State = System.Data.Entity.EntityState.Deleted;
            cugDB.SaveChanges();

            return new Response
            { Status = "Success", Message = "Reference Deleted Saved." };
        }

        #endregion
    }
}
