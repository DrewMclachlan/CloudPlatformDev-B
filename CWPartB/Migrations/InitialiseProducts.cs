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
            const String partitionName = "Products_Partition_1";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("Products");

            // If table doesn't already exist in storage then create and populate it with some initial values, otherwise do nothing
            if (!table.Exists())
            {
                // Create table if it doesn't exist already
                table.CreateIfNotExists();

                // Create the batch operation.
                TableBatchOperation batchOperation = new TableBatchOperation();

                // Create a product entity and add it to the table.
                ProductEntity product1 = new ProductEntity(partitionName, "1");
                product1.Name = "Alien";
                product1.Category = "Widget";
                product1.Price = 22.31;

                // Create another product entity and add it to the table.
                ProductEntity product2 = new ProductEntity(partitionName, "2");
                product2.Name = "Steel";
                product2.Category = "Material";
                product2.Price = 9.91;

                // Create another product entity and add it to the table.
                ProductEntity product3 = new ProductEntity(partitionName, "3");
                product3.Name = "Thingy";
                product3.Category = "Widget";
                product3.Price = 4.99;

                // Create another product entity and add it to the table.
                ProductEntity product4 = new ProductEntity(partitionName, "4");
                product4.Name = "Plastic";
                product4.Category = "Material";
                product4.Price = 4.99;

                // Add product entities to the batch insert operation.
                batchOperation.Insert(product1);
                batchOperation.Insert(product2);
                batchOperation.Insert(product3);
                batchOperation.Insert(product4);

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);
            }

        }
    }



}
