using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;
using Microsoft.WindowsAzure.Storage.Table;
using CWPartB.Models;

namespace CWPartB_Webjob
{
    public class Functions
    {
       
        public static void GenerateSample(
        [QueueTrigger("audiosamplemaker")] Product sampleInQueue,
       
        [Table("Samples", "{PartitionKey}", "{RowKey}")] Product sampleInTable,
        [Table("Samples")] CloudTable tableBinding, TextWriter logger)
        {
            if (sampleInTable == null)
            {
                logger.WriteLine("Person not found: PK:{0}, RK:{1}",
                      sampleInTable.SampleID);
            }
            else
            {
                logger.WriteLine("Person found:, RK:{0}, Name:{1}",
                        sampleInTable.SampleID, sampleInTable.Title);
            }

        }

        public static void Generate20sMP3(
        [QueueTrigger("mp3shortner")] String blobInfo,
        [Blob("mp3gallery/mp3/{queueTrigger}")] CloudBlockBlob inputBlob,
        [Blob("mp3gallery/shortenedmp3/{queueTrigger}")] CloudBlockBlob outputBlob, TextWriter logger)
        {
            logger.WriteLine("ShortenMP3() started...");
            logger.WriteLine("Input blob is: " + blobInfo);

            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                outputBlob.Properties.ContentType = "audio/mpeg";
                CreateSample(input, output, 20);
                outputBlob.Metadata["Title"] = blobInfo;
            }
            outputBlob.SetMetadata();
            logger.WriteLine("Generate20sMP3() completed...");
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
