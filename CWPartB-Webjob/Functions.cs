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
        public static void ReadTable(
        [QueueTrigger("mp3shortner")] ProductEntity blobInfo,
        [Blob("mp3gallery/mp3/{queueTrigger}")] CloudBlockBlob inputBlob,
        [Blob("mp3gallery/shortenedmp3/{queueTrigger}")] CloudBlockBlob outputBlob,
        [Table("Samples", "{PartitionKey}", "{RowKey}")] ProductEntity prod,
        TextWriter logger)
        {
            logger.WriteLine("found: PK:{0}, RK:{1}",
                          blobInfo.PartitionKey, blobInfo.RowKey);
            {
                logger.WriteLine("PK:{0}, RK:{1}, Name:{2} BlobName:{3}",
              prod.PartitionKey, prod.RowKey, prod.Title, prod.Mp3Blob);
            }
        }

        //public static void Generate20sMP3(
        // [QueueTrigger("mp3shortner")] String blobInfo,
        // [Blob("mp3gallery/mp3/{queueTrigger}")] CloudBlockBlob inputBlob,
        // [Blob("mp3gallery/shortenedmp3/{queueTrigger}")] CloudBlockBlob outputBlob, TextWriter logger)
        //{
        //    logger.WriteLine("ShortenMP3() started...");
        //    logger.WriteLine("Input blob is: " + blobInfo);

        //    using (Stream input = inputBlob.OpenRead())
        //    using (Stream output = outputBlob.OpenWrite())
        //    {
        //        String copy = null;
        //        outputBlob.Properties.ContentType = "audio/mpeg";
        //        CreateSample(input, output, 20);
        //        copy = inputBlob.Metadata["Title"];
        //        outputBlob.Metadata["Title"] = copy;
        //    }
        //    outputBlob.SetMetadata();
        //    logger.WriteLine("Generate20sMP3() completed...");
        //}


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
