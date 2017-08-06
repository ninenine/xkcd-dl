using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace xkcd
{
    public class Comic
    {
        public string title { get; set; }
        public string alt { get; set; }
        public string img { get; set; }
        public int num { get; set; }
    }

    public class Program
    {
        public const string DownloadDir = "XKCD";
        public static Comic LatestComic { get; set; }
        public static void Main(string[] args)
        {
            if (!Directory.Exists(DownloadDir))
            {
                Console.WriteLine("Creating Download directory....");
                Directory.CreateDirectory(DownloadDir);
            }
            Console.WriteLine("Attempting to download latest comic....");
            downloadComic("http://xkcd.com/info.0.json", true).Wait();

            for (int num = 1; num <= LatestComic.num; num++)
            {
                Console.WriteLine("Attempting to download comic #{0}....", num);
                var url = "http://xkcd.com/" + num + "/info.0.json";
                downloadComic(url).Wait();
            }
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static async Task downloadComic(string URL, Boolean latest = false)
        {
            var request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.Accept = "application/json";
            request.ContentType = "application/json; charset=utf-8";
            try
            {
                var response = (HttpWebResponse)await request.GetResponseAsync();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = JsonConvert.DeserializeObject<Comic>(reader.ReadToEnd());
                    // Set latest Comic
                    if (latest)
                        LatestComic = result;
                    string img_url = result.img;
                    string file = img_url.Split('/').Last();
                    string fileUploadPath = DownloadDir + "/" + file;
                    FileInfo fInfo = new FileInfo(fileUploadPath);
                    if (fInfo.Exists)
                    {
                        Console.WriteLine("Comic already downloaded, moving to next...");
                    }
                    else
                    {
                        Console.WriteLine("Downloading comic {0}-{1}:{2}", result.num, result.title, result.img);
                        using (var client = new HttpClient())
                        using (var contentStream = await client.GetStreamAsync(img_url))
                        using (var fileStream = new FileStream(fileUploadPath, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, true))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }
                        Console.WriteLine("Download Complete");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Download Image failed : {0}--{1}", ex.StackTrace, ex.Message);
                throw;
            }
        }
    }
}
