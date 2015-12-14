using Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ImageContext _db = new ImageContext();
        private CloudQueue _requestQueue;
        private static CloudBlobContainer _imagesBlobContainer;

        public HomeController()
        {
            InitializeStorage();
        }

        private void InitializeStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            var blobClient = storageAccount.CreateCloudBlobClient();
            _imagesBlobContainer = blobClient.GetContainerReference("images");
            var queueClient = storageAccount.CreateCloudQueueClient();
            _requestQueue = queueClient.GetQueueReference("imagerequest");
        }

        public async Task<ActionResult> Index()
        {
            var adsList = _db.Images.AsQueryable();
            return View(await adsList.ToListAsync());
        }

        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Title,Description")] ImageModel im, HttpPostedFileBase imageFile)
        {
            CloudBlockBlob imageBlob = null;
            if (!ModelState.IsValid) return View(im);
            if (imageFile != null && imageFile.ContentLength != 0)
            {
                imageBlob = await UploadAndSaveBlobAsync(imageFile);
                im.ImageURL = imageBlob.Uri.ToString();
            }
            if (string.IsNullOrEmpty(im.Title) || string.IsNullOrEmpty(im.ImageURL)) return Create();
            im.PostedDate = DateTime.Now;
            _db.Images.Add(im);
            await _db.SaveChangesAsync();
            Trace.TraceInformation($"Created ImageId {im.ImageId} in database");

            if (imageBlob == null) return RedirectToAction("Index");
            var blobInfo = new BlobInformation { ImageId = im.ImageId, BlobUri = new Uri(im.ImageURL) };
            var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
            await _requestQueue.AddMessageAsync(queueMessage);
            Trace.TraceInformation($"Created queue message for ImageId {im.ImageId}");
            return RedirectToAction("Index");
        }


        private static async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase imageFile)
        {
            Trace.TraceInformation($"Uploading image file {imageFile.FileName}");
            var blobName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var imageBlob = _imagesBlobContainer.GetBlockBlobReference(blobName);
            using (var fileStream = imageFile.InputStream)
            {
                await imageBlob.UploadFromStreamAsync(fileStream);
            }
            Trace.TraceInformation($"Uploaded image file to {imageBlob.Uri}");
            return imageBlob;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}