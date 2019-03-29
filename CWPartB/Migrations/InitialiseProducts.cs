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

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());

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
                DateTime date1 = new DateTime(2008, 5, 1, 8, 30, 52);
                sample1.Title = "Anthem";
                sample1.Artist = "Drew";
                sample1.CreatedDate = date1;
                sample1.Mp3Blob = "Anthem.mp3";
                sample1.SampleMp3Blob = "name.mp3";
                // Create another product entity and add it to the table.
                //   ProductEntity sample2 = new ProductEntity(partitionName, "2");


                // Create another product entity and add it to the table.
                //  ProductEntity sample3 = new ProductEntity(partitionName, "3");


                // Create another product entity and add it to the table.
                //   ProductEntity sample4 = new ProductEntity(partitionName, "4");


                // Add product entities to the batch insert operation.
                batchOperation.Insert(sample1);
               // batchOperation.Insert(sample2);
               // batchOperation.Insert(sample3);
               // batchOperation.Insert(sample4);

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);
            }

        }
    }



}
