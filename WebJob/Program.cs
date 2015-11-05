using Microsoft.Azure.WebJobs;

namespace WebJob
{
    class Program
    {
        static void Main()
        {
            var host = new JobHost();
            host.RunAndBlock();
        }
    }
}
