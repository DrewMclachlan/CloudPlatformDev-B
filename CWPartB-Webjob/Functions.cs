using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;
using Microsoft.WindowsAzure.Storage.Table;
using CWPartB.Models;
using System.Linq;

namespace CWPartB_Webjob
{
  
    public class Functions
    {
    
        //rename
        public static void ReadTable(

       
        [QueueTrigger("mp3shortner")] SampleEntity blobInfo,
        [Blob("mp3gallery/mp3/{Mp3Blob}")] CloudBlockBlob inputBlob,
        [Blob("mp3gallery/shortenedmp3/{Mp3Blob}")] CloudBlockBlob outputBlob,
        [Table("Samples", "{PartitionKey}", "{RowKey}")] SampleEntity prod,
        [Table("Samples")] CloudTable tableBinding,
        TextWriter logger)
        {

            if (prod != null)
            {
                DateTime date1;
                using (Stream input = inputBlob.OpenRead())
                using (Stream output = outputBlob.OpenWrite())
                {
                    String copy = null;
                    outputBlob.Properties.ContentType = "audio/mpeg";
                    CreateSample(input, output, 20);
                    date1 = DateTime.Now;
                    copy = inputBlob.Metadata["Title"];
                    outputBlob.Metadata["Title"] = copy;
                }
                logger.WriteLine("Generate20sMP3() completed...");
                logger.WriteLine("Found: PK:{0}, RK:{1}", blobInfo.PartitionKey, blobInfo.RowKey);
                logger.WriteLine("PK:{0}, RK:{1}, Name:{2} BlobName:{3}", prod.PartitionKey, prod.RowKey, prod.Title, prod.Mp3Blob);
                logger.WriteLine("1:{0}, 2:{1}", inputBlob.Name, outputBlob.Name);
              
                var person = new SampleEntity()
                {
                    PartitionKey = blobInfo.PartitionKey,
                    RowKey = blobInfo.RowKey,
                    Mp3Blob = inputBlob.Name,
                    SampleMp3Blob = outputBlob.Name,
                    SampleMp3URL = outputBlob.Uri.ToString(),
                    SampleDate = date1
                };
                person.ETag = "*";
                TableOperation o = TableOperation.Merge(person);
                tableBinding.Execute(o);
            }
            else
            {
                logger.WriteLine("*******");
                logger.WriteLine("Rowkey value {0}, inputed by user did not match a table entity", blobInfo.RowKey);
                logger.WriteLine("*******");
            }

        }
        private static void CreateSample(Stream input, Stream output, int duration)
        {
            using (var reader = new Mp3FileReader(input, wave => new NLayer.NAudioSupport.Mp3FrameDecompressor(wave)))
            {
                Mp3Frame frame;
                frame = reader.ReadNextFrame();
                int frameTimeLength = (int)(frame.SampleCount / (double)frame.SampleRate * 1000.0);
                int framesRequired = (int)(duration / (double)frameTimeLength * 1000.0);

                int frameNumber = 0;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    frameNumber++;

                    if (frameNumber <= framesRequired)
                    {
                        output.Write(frame.RawData, 0, frame.RawData.Length);
                    }
                    else break;
                }
            }
        }
    }
}
