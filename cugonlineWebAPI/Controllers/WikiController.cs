using cugonlineWebAPI.Caching;
using cugonlineWebAPI.Models;
using cugonlineWebAPI.VM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
        public List<FiguresDTO> GetSearchResultsBy(string filter, string searchRole, string isFullSearch, string searchCount)
        {

            var results = new List<FiguresDTO>();
            if (searchRole == "null") searchRole = "All";
            switch (searchRole)
            {
                case "All":

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
                                                       CategoryName = m.CategoryN.Trim(),
                                                       CurrentStatus = m.currentStatus.Trim(),
                                                   }).OrderBy(m => m.Title).ToList();

                    else if ((isFullSearch == "true") && (searchCount == "2")) //get body search
                        results = cugDB.Mains.Where(m => m.Body.Contains(filter)
                                                && !m.Title.Contains(filter)
                                                && !m.Meaning.Contains(filter)
                                               ).Select(m => new FiguresDTO
                                               {
                                                   Id = m.Id,
                                                   Idx = m.Idx,
                                                   Title = m.Title,
                                                   Meaning = m.Meaning,
                                                   CategoryName = m.CategoryN.Trim(),
                                                   CurrentStatus = m.currentStatus.Trim(),
                                               }).OrderBy(m => m.Title).ToList();

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
                                                    CategoryName = m.CategoryN.Trim(),
                                                    CurrentStatus = m.currentStatus.Trim(),
                                                }).OrderBy(m => m.Title).ToList();

                    else if ((isFullSearch == "true") && (searchCount == "2")) //get body search
                        results = cugDB.Mains.Where(m => m.Body.Contains(filter)
                                               && !m.Title.Contains(filter)
                                               && !m.Meaning.Contains(filter)
                                               && m.CategoryN.Equals(searchRole)
                                              ).Select(m => new FiguresDTO
                                              {
                                                  Id = m.Id,
                                                  Idx = m.Idx,
                                                  Title = m.Title,
                                                  Meaning = m.Meaning,
                                                  CategoryName = m.CategoryN.Trim(),
                                                  CurrentStatus = m.currentStatus.Trim()
                                              }).OrderBy(m => m.Title).ToList();
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
                                                    CategoryName = m.CategoryN.Trim(),
                                                    CurrentStatus = m.currentStatus.Trim(),
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
                                                    CategoryName = m.CategoryN.Trim(),
                                                    CurrentStatus = m.currentStatus.Trim(),
                                                }).OrderBy(m => m.Title).ToList();
                    break;
                default: //return All case show no authentication...
                    results = cugDB.Mains.Where(m => m.Title.Contains(filter) ||
                                                  m.Meaning.Contains(filter)  //||
                                                                              //  m.Body.Contains(filter)
                                                  ).Select(m => new FiguresDTO
                                                  {
                                                      Id = m.Id,
                                                      Idx = m.Idx,
                                                      Title = m.Title,
                                                      Meaning = m.Meaning,
                                                      CategoryName = m.CategoryN.Trim(),
                                                      CurrentStatus = m.currentStatus.Trim()
                                                  }).OrderBy(m => m.Title).ToList();
                    break;
            }

            return results.Where(r => r.CurrentStatus == "live").ToList();

        }

       
        [Route("GetBibles")]
        [HttpGet]
        [CacheFilter(TimeDuration = 999999)] //milliseconds
        public async Task<Root> GetBibles()
        {
            string Baseurl = "https://api.scripture.api.bible/v1/bibles";

            using (var client = new HttpClient())
            {
                //Passing service base url
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
              
                //HttpRequestMessage attempt
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Baseurl);
                request.Method = HttpMethod.Get;
                request.Headers.Add("api-key", "c70be9f1ee79503784675bfad2e88799");

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                HttpResponseMessage Res = await client.GetAsync(Baseurl);
                Res = await client.SendAsync(request);
                //Checking the response is successful or not which is sent using HttpClient
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api
                    var EmpResponse = Res.Content.ReadAsStringAsync().Result;

                    try
                    {
                        //Deserializing the response recieved from web api and storing into the Employee list
                        Root bibleInfo = JsonConvert.DeserializeObject<Root>(EmpResponse);
                         return bibleInfo;
                    }
                    catch (Exception ex)
                    {
                        return new Root { }; 
                    }

                }

            }
            return new Root {data = new List<Datum>() };
            //return new Response
            //{ Status = "Success", Message = "Record SuccessFully Saved." };

        }
        
        [Route("GetBibleSelected")]
        [HttpGet]
        public async Task<Root> GetBibleSelected(string bibleId, string abbreviation)
        {
            string Baseurl = "https://api.scripture.api.bible/v1/bibles/" + bibleId + "/books";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();

                //HttpRequestMessage attempt
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Baseurl);
                request.Method = HttpMethod.Get;
                request.Headers.Add("api-key", "c70be9f1ee79503784675bfad2e88799");

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                HttpResponseMessage Res = await client.GetAsync(Baseurl);
                Res = await client.SendAsync(request);

                //Checking the response is successful or not which is sent using HttpClient
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api
                    var EmpResponse = Res.Content.ReadAsStringAsync().Result;

                     try
                    {
                        //Deserializing the response recieved from web api and storing into the Employee list
                        Root bibleInfo = JsonConvert.DeserializeObject<Root>(EmpResponse);
                         return bibleInfo;
                    }
                    catch (Exception ex)
                    {
                        return new Root { }; 
                    }
                }

            }
                return new Root { };
        }

        [Route("GetBibleBooksSelected")]
        [HttpGet]
        public async Task<ChapterRoot> GetBibleBooksSelected(string bibleId, string abbreviation)
        {

            //https://api.scripture.api.bible/v1/bibles/65bfdebd704a8324-01/books/GEN?include-chapters=true
            string Baseurl = "https://api.scripture.api.bible/v1/bibles/" + bibleId + "/books/" + abbreviation + "?include-chapters=true";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();

                //HttpRequestMessage attempt
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Baseurl);
                request.Method = HttpMethod.Get;
                request.Headers.Add("api-key", "c70be9f1ee79503784675bfad2e88799");

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                HttpResponseMessage Res = await client.GetAsync(Baseurl);
                Res = await client.SendAsync(request);

                //Checking the response is successful or not which is sent using HttpClient
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api
                    var EmpResponse = Res.Content.ReadAsStringAsync().Result;

                    try
                    {
                        //Deserializing the response recieved from web api and storing into the Employee list
                        //Root bibleInfo = JsonConvert.DeserializeObject<Root>(EmpResponse);
                        ChapterRoot bibleInfo = JsonConvert.DeserializeObject<ChapterRoot>(EmpResponse);
                        return bibleInfo;
                    }
                    catch (Exception ex)
                    {
                        return new ChapterRoot { };
                    }
                }

            }
            return new ChapterRoot { };
        }

        [Route("GetBibleChapterVersesSelected")]
        [HttpGet]
        public async Task<ContentRoot> GetBibleChapterSelected(string bibleId, string chapter)
        {
            //https://api.scripture.api.bible/v1/bibles/65bfdebd704a8324-01/chapters/GEN.2?content-type=html&include-notes=false&include-titles=true&include-chapter-numbers=true&include-verse-numbers=true&include-verse-spans=false
            string Baseurl = "https://api.scripture.api.bible/v1/bibles/" + bibleId + "/chapters/" + chapter + "?content-type=html&include-notes=false&include-titles=true&include-chapter-numbers=true&include-verse-numbers=true&include-verse-spans=false";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();

                //HttpRequestMessage attempt
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri(Baseurl);
                request.Method = HttpMethod.Get;
                request.Headers.Add("api-key", "c70be9f1ee79503784675bfad2e88799");

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                HttpResponseMessage Res = await client.GetAsync(Baseurl);
                Res = await client.SendAsync(request);

                //Checking the response is successful or not which is sent using HttpClient
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api
                    var EmpResponse = Res.Content.ReadAsStringAsync().Result;

                    try
                    {
                        //Deserializing the response recieved from web api and storing into the Employee list                        
                        //ChapterRoot bibleInfo = JsonConvert.DeserializeObject<ChapterRoot>(EmpResponse);
                        ContentRoot bibleInfo = JsonConvert.DeserializeObject<ContentRoot>(EmpResponse);
                        return bibleInfo;
                    }
                    catch (Exception ex)
                    {
                        return new ContentRoot { };
                    }
                }

            }

            return new ContentRoot { };
        }
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class AudioBible
        {
            public string id { get; set; }
            public string name { get; set; }
            public string nameLocal { get; set; }
            public string dblId { get; set; }
        }

        public class Country
        {
            public string id { get; set; }
            public string name { get; set; }
            public string nameLocal { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string dblId { get; set; }
            public object relatedDbl { get; set; }
            public string name { get; set; }
            public string nameLocal { get; set; }
            public string abbreviation { get; set; }
            public string abbreviationLocal { get; set; }
            public string description { get; set; }
            public string descriptionLocal { get; set; }
            public Language language { get; set; }
            public List<Country> countries { get; set; }
            public string type { get; set; }
            public DateTime updatedAt { get; set; }
            public List<AudioBible> audioBibles { get; set; }
        }

        public class Language
        {
            public string id { get; set; }
            public string name { get; set; }
            public string nameLocal { get; set; }
            public string script { get; set; }
            public string scriptDirection { get; set; }
        }

        public class Root
        {
            public List<Datum> data { get; set; }
        }


        public class Chapter
        {
            public string id { get; set; }
            public string bibleId { get; set; }
            public string bookId { get; set; }
            public string number { get; set; }
            public int position { get; set; }
        }

        public class ChapterData
        {
            public string id { get; set; }
            public string bibleId { get; set; }
            public string abbreviation { get; set; }
            public string name { get; set; }
            public string nameLong { get; set; }
            public List<Chapter> chapters { get; set; }
        }

        public class ChapterRoot
        {
            public ChapterData data { get; set; }
        }


        //** Content Root/
        public class Data
        {
            public string id { get; set; }
            public string bibleId { get; set; }
            public string number { get; set; }
            public string bookId { get; set; }
            public string reference { get; set; }
            public string copyright { get; set; }
            public int verseCount { get; set; }
            public string content { get; set; }
            public Next next { get; set; }
            public Previous previous { get; set; }
        }

        public class Meta
        {
            public string fums { get; set; }
            public string fumsId { get; set; }
            public string fumsJsInclude { get; set; }
            public string fumsJs { get; set; }
            public string fumsNoScript { get; set; }
        }

        public class Next
        {
            public string id { get; set; }
            public string number { get; set; }
            public string bookId { get; set; }
        }

        public class Previous
        {
            public string id { get; set; }
            public string number { get; set; }
            public string bookId { get; set; }
        }

        public class ContentRoot
        {
            public Data data { get; set; }
            public Meta meta { get; set; }
        }

    }
}
