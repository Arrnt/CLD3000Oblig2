using Common;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace WebJob
{
    public class Functions
    {
        public static void GenerateScaledImage(
         [QueueTrigger("imagerequest")] BlobInformation blobInfo,
         [Blob("images/{BlobName}", FileAccess.Read)] Stream input,
         [Blob("images/{BlobNameWithoutExtension}_scaled.jpg")] CloudBlockBlob outputBlob)
        {
            using (Stream output = outputBlob.OpenWrite())
            {
                ResizeImage(input, output);
                outputBlob.Properties.ContentType = "image/jpeg";
            }

            using (var db = new ImageContext())
            {
                var id = blobInfo.ImageId;
                var im = db.Images.Find(id);
                if (im == null)
                {
                    throw new Exception($"ImageId {id} not found, can't scale");
                }
                im.ScaledImageURL = outputBlob.Uri.ToString().Replace("https://cld3000blob.blob.core.windows.net", "http://az827916.vo.msecnd.net");
                db.SaveChanges();
            }
        }

        public static void ResizeImage(Stream input, Stream output)
        {
            var originalImage = new Bitmap(input);
            const int width = 300;
            var height = ((double)originalImage.Height / originalImage.Width) * width;
            Bitmap scaledImage = null;
            try
            {
                scaledImage = new Bitmap(width, (int)height);
                using (var graphics = Graphics.FromImage(scaledImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(originalImage, 0, 0, width, (int)height);
                }
                scaledImage.Save(output, ImageFormat.Jpeg);
            }
            finally
            {
                scaledImage?.Dispose();
            }
        }
    }
}
