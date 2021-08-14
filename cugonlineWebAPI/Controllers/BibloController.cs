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

            //var filePath = "https://cugonlinestorage.blob.core.windows.net/bibloimg/";
            var filePath = "https://geared4it.net/images/";

            using (testEntities db = new testEntities())
            {
                var images = (from up in cugDB.BibloUploads

                              select new FilesInfo()
                              {
                                  FileId = up.Id,
                                  FileName = up.fName,
                                  FilePath = filePath + up.fName,
                                  ThumbNail = up.fTitle,
                                  FileComment = up.fDescription,
                                  SortOrder = up.SortOrder
                              }).OrderByDescending(f => f.SortOrder).ThenByDescending(f => f.ThumbNail).ToList();

                return images;
            }
        }

        [Route("UploadBiblo_Old")]
        [HttpPost]
        public object UploadBiblo_Old(string comment)
        {
            var file = HttpContext.Current.Request.Files[0];//we have the file...


            var uniquefileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            //string ftpAddress = @"197.242.150.135"; // ConfigurationManager.AppSettings.Get("CloudStorageContainerReference")
            //string username = "cugftp"; // ConfigurationManager.AppSettings.Get("CloudStorageContainerReference")
            //string password = "Kjgv5FtNX!2$7qBtP3"; // ConfigurationManager.AppSettings.Get("CloudStorageContainerReference")


            var postedFile = HttpContext.Current.Request.Files[0];
            var filePath = HttpContext.Current.Server.MapPath("~/images/" + postedFile.FileName);

            var rootPath = "https://geared4it.net/images/";

            var nextFileId = cugDB.BibloUploads.OrderByDescending(mf => mf.Id).FirstOrDefault().Id + 1;
            BibloUpload f = new BibloUpload
            {
                Id = nextFileId,
                fName = postedFile.FileName,// uniquefileName,// file.FileName;
                                            // f.fNamePath = rootPath + uniquefileName;
                fDescription = comment,
                fTitle = comment,
                fType = Path.GetExtension(file.FileName)
            };

            cugDB.BibloUploads.Add(f);
            cugDB.SaveChanges();

            postedFile.SaveAs(filePath);

            return postedFile.FileName + " filePath: " + filePath;


        }

        [Route("UploadBiblo")]
        [HttpPost]
        public object UploadBiblo(string description, string title)
        {
            var file = HttpContext.Current.Request.Files[0];//we have the file...


            var postedFile = HttpContext.Current.Request.Files[0];
            var filePath = HttpContext.Current.Server.MapPath("~/images/" + postedFile.FileName);


            try
            {
                var rootPath = "https://geared4it.net/images/";
                var nextFileId = cugDB.BibloUploads.OrderByDescending(mf => mf.Id).FirstOrDefault().Id + 1;
                BibloUpload f = new BibloUpload
                {
                    Id = nextFileId,
                    fName = postedFile.FileName,// uniquefileName,// file.FileName;
                                                // f.fNamePath = rootPath + uniquefileName;
                    fDescription = description,
                    fTitle = title,
                    fType = Path.GetExtension(file.FileName)
                };

                cugDB.BibloUploads.Add(f);
                cugDB.SaveChanges();

                postedFile.SaveAs(filePath);

                return postedFile.FileName + " filePath: " + filePath;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [Route("DeleteFile")]
        [HttpPost]
        public object DeleteFile(string id)
        {
            int fileId = int.Parse(id);
            var item = cugDB.BibloUploads.Where(u => u.Id.Equals(fileId)).FirstOrDefault();

            cugDB.BibloUploads.Remove(item);
            cugDB.SaveChanges();

            return new Response
            { Status = "Success", Message = "File Reference Deleted Saved." };
        }
        [Route("EditFileDetails")]
        [HttpPost]
        public object EditFileDetails(string description, string title, string id, string sortOrder)
        {
            try
            {
                int fileId = int.Parse(id);
                var item = cugDB.BibloUploads.Where(u => u.Id.Equals(fileId)).FirstOrDefault();

                item.fTitle = title;
                item.fDescription = description;
                item.SortOrder = int.Parse(sortOrder);

                cugDB.SaveChanges();
                return new Response
                { Status = "saved", Message = "Record SuccessFully Saved." };
            }
            catch (Exception ex)
            {

                throw;
            }


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
