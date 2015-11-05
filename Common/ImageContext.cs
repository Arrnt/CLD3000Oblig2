using System.Data.Entity;

namespace Common
{
    public class ImageContext : DbContext
    {

        public ImageContext() : base("name=ImageContext") { }
        public DbSet<ImageModel> Images { get; set; }
    }
}