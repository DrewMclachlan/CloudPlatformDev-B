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
                ProductEntity sample1 = new ProductEntity(partitionName, "1");
                DateTime date1 = DateTime.Now;
                sample1.Title = "Anthem1";
                sample1.Artist = "Drew";
                sample1.CreatedDate = date1;
                sample1.Mp3Blob = null;
                sample1.SampleMp3Blob = null;
                sample1.SampleMp3URL = null;
                sample1.SampleDate = null;
                // Create another product entity and add it to the table.
                //   ProductEntity sample2 = new ProductEntity(partitionName, "2");

                ProductEntity sample2 = new ProductEntity(partitionName, "2");
                DateTime date2 = DateTime.Now;
                sample2.Title = "Anthem2";
                sample2.Artist = "Drew";
                sample2.CreatedDate = date2;
                sample2.Mp3Blob = null;
                sample2.SampleMp3Blob = null;
                sample2.SampleMp3URL = null;
                sample2.SampleDate = null;




                // Create another product entity and add it to the table.
                //  ProductEntity sample3 = new ProductEntity(partitionName, "3");


                // Create another product entity and add it to the table.
                //   ProductEntity sample4 = new ProductEntity(partitionName, "4");


                // Add product entities to the batch insert operation.
                batchOperation.Insert(sample1);
                batchOperation.Insert(sample2);
               // batchOperation.Insert(sample3);
               // batchOperation.Insert(sample4);

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);
            }

        }
    }



}
