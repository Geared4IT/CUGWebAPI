using System.Collections.Generic;

namespace cugonlineWebAPI.Controllers
{
    public class BibloInfoDTO
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string PageRight { get; set; }
        public string PageLeft { get; set; }
        public string Comments { get; set; }
        public string PageBibloTitle { get; set; }
        public string CopyRight { get; set; }
        public string PageBibloBody { get; set; }
        public string PageRefTitle { get; set; }

        public string IDItems {get;set;}
       
    }
}