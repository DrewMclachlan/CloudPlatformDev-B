using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Configuration;
using CWPartB.Models;

namespace CWPartB.Migrations
{
    public class InitialiseProducts
    {
        public static void go()
        {
            const String partitionName = "Sample_Partition_1";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureStorage"].ToString());

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("Samples");

            // If table doesn't already exist in storage then create and populate it with some initial values, otherwise do nothing
            if (!table.Exists())
            {
                // Create table if it doesn't exist already
                table.CreateIfNotExists();

                // Create the batch operation.
                TableBatchOperation batchOperation = new TableBatchOperation();

                // Create a product entity and add it to the table.
                SampleEntity sample1 = new SampleEntity(partitionName, "1");
                DateTime date1 = DateTime.Now;
                sample1.Title = "USA's National Anthem";
                sample1.Artist = "USA";
                sample1.CreatedDate = date1;
                sample1.Mp3Blob = null;
                sample1.SampleMp3Blob = null;
                sample1.SampleMp3URL = null;
                sample1.SampleDate = null;
              

                SampleEntity sample2 = new SampleEntity(partitionName, "2");
                DateTime date2 = DateTime.Now;
                sample2.Title = "Russian National Anthem";
                sample2.Artist = "Russia";
                sample2.CreatedDate = date1;
                sample2.Mp3Blob = null;
                sample2.SampleMp3Blob = null;
                sample2.SampleMp3URL = null;
                sample2.SampleDate = null;

                SampleEntity sample3 = new SampleEntity(partitionName, "3");
                sample3.Title = "French National Anthem";
                sample3.Artist = "France";
                sample3.CreatedDate = date1;
                sample3.Mp3Blob = null;
                sample3.SampleMp3Blob = null;
                sample3.SampleMp3URL = null;
                sample3.SampleDate = null;

                SampleEntity sample4 = new SampleEntity(partitionName, "4");
                sample4.Title = "Heartbreaker";
                sample4.Artist = "Crazy P";
                sample4.CreatedDate = date2;
                sample4.Mp3Blob = null;
                sample4.SampleMp3Blob = null;
                sample4.SampleMp3URL = null;
                sample4.SampleDate = null;

                SampleEntity sample5 = new SampleEntity(partitionName, "5");
                sample5.Title = "Rebirth of Cool";
                sample5.Artist = "DJ Cam Quartet";
                sample5.CreatedDate = date2;
                sample5.Mp3Blob = null;
                sample5.SampleMp3Blob = null;
                sample5.SampleMp3URL = null;
                sample5.SampleDate = null;

                SampleEntity sample6 = new SampleEntity(partitionName, "6");
                sample6.Title = "The Big Unkown";
                sample6.Artist = "Elder Island";
                sample6.CreatedDate = date2;
                sample6.Mp3Blob = null;
                sample6.SampleMp3Blob = null;
                sample6.SampleMp3URL = null;
                sample6.SampleDate = null;






                // Create another product entity and add it to the table.
                //  ProductEntity sample3 = new ProductEntity(partitionName, "3");


                // Create another product entity and add it to the table.
                //   ProductEntity sample4 = new ProductEntity(partitionName, "4");


                // Add product entities to the batch insert operation.
                batchOperation.Insert(sample1);
                batchOperation.Insert(sample2);
                batchOperation.Insert(sample3);
                batchOperation.Insert(sample4);
                batchOperation.Insert(sample5);
                batchOperation.Insert(sample6);

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);
            }

        }
    }



}
