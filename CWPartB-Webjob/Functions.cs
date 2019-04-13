using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;
using Microsoft.WindowsAzure.Storage.Table;
using CWPartB.Models;
using System.Linq;

namespace CWPartB_Webjob
{   //Drew Mclachlan
    //S1511481
  
    public class Functions
    {
    
        //Main Function for webjob
        public static void ReadTable(
        //The first line sets a QueueTrigger to launch once a SampleEntity object is detected within the "mp3shortner" Queue
        //The serlized data sent from the code behind is then appened to a new SampleEntity object, blobinfo.
        [QueueTrigger("mp3shortner")] SampleEntity blobInfo,
        //The Webjob now uses the Mp3BBlob attribute of blobInfo, which was set in the code behind
        //to retreive the blob that containes the mp3file uploaded by the user and append it to the inputblob variable
        [Blob("mp3gallery/mp3/{Mp3Blob}")] CloudBlockBlob inputBlob,
        //This line creates a refrence for where the output blob should be stored when it is created
        [Blob("mp3gallery/shortenedmp3/{Mp3Blob}")] CloudBlockBlob outputBlob,
        //Using the PartitionKey and Rowkey attributes of blobinfo a specific coloumn of the table is found and appended to a SampleEntity sample
        [Table("Samples", "{PartitionKey}", "{RowKey}")] SampleEntity sample,
        //Retreives the Samples table and appends it to a cloudtable variable, tablebinding so operations such as updating the information can be preformed on the table
        [Table("Samples")] CloudTable tableBinding,
        TextWriter logger)
        {
            //Determines if the table coloumn found via the data sent from blobinfo exists
            if (sample != null)
            {
                //Sets a date time object
                DateTime date1;

                //Allows the inputblob to be read from
                using (Stream input = inputBlob.OpenRead())
                //Allows the output blob to be written to            
                using (Stream output = outputBlob.OpenWrite())
                {           
          
                    String copy = null;

                    //Sets conenttype of the output blob
                    outputBlob.Properties.ContentType = "audio/mpeg";
                    //Calls the create sample method, sending in the inputblob, outputblob and the required duration of the shortened mp3
                    CreateSample(input, output, 20);
                    //Sets the datetime object to the current date time
                    date1 = DateTime.Now;
                   
                    //copys the inputblob metadata to the output blob
                    //ensuring that the outputblob will also have the metadata set to to the filename specfied by the user
                    copy = inputBlob.Metadata["Title"];
                    outputBlob.Metadata["Title"] = copy;
                }

                //writes information to the webjob console
                logger.WriteLine("Generate20sMP3() completed...");
                logger.WriteLine("Found: PK:{0}, RK:{1}", blobInfo.PartitionKey, blobInfo.RowKey);
                logger.WriteLine("PK:{0}, RK:{1}, Name:{2} BlobName:{3}", sample.PartitionKey, sample.RowKey, sample.Title, sample.Mp3Blob);
                logger.WriteLine("1:{0}, 2:{1}", inputBlob.Name, outputBlob.Name);
              

                //creates a new sampleEntity object which contains information to be updated into the table coloumn found from the blobinfo object
                //passed from the Queue
                var updateSample = new SampleEntity()
                {
                    //Sets the partion key and row key, so when merging the data into the table the table operation knowns what coloumn to preform the operation on
                    PartitionKey = blobInfo.PartitionKey,
                    RowKey = blobInfo.RowKey,
                    //Sets MP3blob to the name of the inputblob
                    Mp3Blob = inputBlob.Name,
                    //Sets SampleMp3Blob to the outputBlob name
                    SampleMp3Blob = outputBlob.Name,
                    //Sets the sampleblob url to the location of the outputblob
                    SampleMp3URL = outputBlob.Uri.ToString(),
                    //Sets the sampledate to the time set in the code above, which is when the sample outputblob was created
                    SampleDate = date1
                };
                updateSample.ETag = "*";
                //creates a merge table operation to merge new data into an exsiting coloumn in the table
                TableOperation o = TableOperation.Merge(updateSample);
                //excutes the table operation on the "Samples table"
                tableBinding.Execute(o);
            }
            else
            //If the rowkey supplied by the user and passed to the webjob via blobinfo does not match a rowkey in the table this block of code will fire 
            //informing the user
            {

                logger.WriteLine("*******");
                logger.WriteLine("Rowkey value {0}, inputed by user did not match a table entity", blobInfo.RowKey);
                logger.WriteLine("*******");
            }

        }

        //Generates a 20 second mp3sample from the inputblob, which will contain an mp3 file uploaded by the user
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
                        //Writes the shortned mp3 to the output blob for use in the display code
                        output.Write(frame.RawData, 0, frame.RawData.Length);
                    }
                    else break;
                }
            }
        }
    }
}
