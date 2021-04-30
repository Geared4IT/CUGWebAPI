using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cugonlineWebAPI.DTO
{
    public class UserActivitiesDTO
    {
        public string DateInFormatted { get; set; }

        public int ID { get; set; }
        public string Editor { get; set; }
        public DateTime? DateIn { get; set; }
        public string TimeIn { get; set; }
        public string Reference { get; set; }
        public string Edited { get; set; }
        public string Uploaded { get; set; }
        public string Attachment { get; set; }
        public string Deleted { get; set; }
        public int UserActivitiesID { get; set; }

        public string FileUploadComment { get; set; }
        public int? AttachmentId { get; set; }
        public string FileUploaded { get; set; }
        public string UploadUrl { get; set; }
        public  string Idx { get; set; }
    }
}