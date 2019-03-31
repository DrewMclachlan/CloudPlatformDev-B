using System;
using Microsoft.Azure.WebJobs;

namespace CWPartB_Webjob
{
    class Program
    {
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }


            config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(5);

            var host = new JobHost(config);

            host.RunAndBlock();
        }
    }
}
