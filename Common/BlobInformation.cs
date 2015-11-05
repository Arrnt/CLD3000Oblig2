using System;
using System.IO;

namespace Common
{
    public class BlobInformation
    {
        public Uri BlobUri { get; set; }
        public string BlobName => BlobUri.Segments[BlobUri.Segments.Length - 1];
        public string BlobNameWithoutExtension => Path.GetFileNameWithoutExtension(BlobName);
        public int ImageId { get; set; }
    }
}

