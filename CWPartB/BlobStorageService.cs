﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Diagnostics;


namespace CWPartB
{
    public class BlobStorageService
    {
        public CloudBlobContainer getCloudBlobContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse
                (ConfigurationManager.ConnectionStrings["AzureStorage"].ToString());

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer blobContainer = blobClient.GetContainerReference("mp3gallery");
            if (blobContainer.CreateIfNotExists())
            {
                blobContainer.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });
            }
            return blobContainer;
        }
    }
}