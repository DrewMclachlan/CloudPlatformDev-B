using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Linq;
using CWPartB.Models;
using Newtonsoft.Json;

namespace CWPartB
{
    public partial class _Default : System.Web.UI.Page
    {
        // accessor variables and methods for blob containers and queues
        private BlobStorageService _blobStorageService = new BlobStorageService();
        private CloudQueueService _queueStorageService = new CloudQueueService();

        private CloudBlobContainer getMP3galleryContainer()
        {
            System.Diagnostics.Debug.WriteLine("called");

            return _blobStorageService.getCloudBlobContainer();
        }

        private CloudQueue getMP3shortnerQueue()
        {
            return _queueStorageService.getCloudQueue();
        }


        private string GetMimeType(string Filename)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("called2");

                string ext = Path.GetExtension(Filename).ToLowerInvariant();
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (key != null)
                {
                    string contentType = key.GetValue("Content Type") as String;
                    if (!String.IsNullOrEmpty(contentType))
                    {
                        return contentType;
                    }
                }
            }
            catch
            {
            }
            return "application/octet-stream";
        }

        protected void Rbutton_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl);
        }

        // User clicked the "Submit" button
        protected void submitButton_Click(object sender, EventArgs e)
        {
            if (upload.HasFile)
            {
                // Get the file name specified by the user without the .mp3 extension. 
                var filename = Path.GetFileNameWithoutExtension(upload.FileName);
                var name = string.Format(string.Format("{0}", Guid.NewGuid()));
                String path = "mp3/" + name;

                var blob = getMP3galleryContainer().GetBlockBlobReference(path);
               
                blob.Properties.ContentType = GetMimeType(upload.FileName);
                blob.Metadata["Title"] = filename;


         

                // Actually upload the data to the
                // newly instantiated blob
                blob.UploadFromStream(upload.FileContent);
                blob.SetMetadata();

                ProductEntity blobInfo = new ProductEntity() { PartitionKey="Sample_Partition_1", RowKey = "1", Mp3Blob = name };
                var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                getMP3shortnerQueue().AddMessage(queueMessage);

                // Place a message in the queue to tell the worker
                // role that a new mp3 blob exists
                //   getMP3shortnerQueue().AddMessage(new CloudQueueMessage(System.Text.Encoding.UTF8.GetBytes(name)));

               System.Diagnostics.Trace.WriteLine(String.Format("*** WebRole: Enqueued '{0}'", path));
            }
        }

        public string getTitle(Uri blobURI)
        {
            CloudBlockBlob blob = new CloudBlockBlob(blobURI);
            blob.FetchAttributes();
            return blob.Metadata["Title"];

        }





        protected void Page_PreRender(object sender, EventArgs e)
        {
            try
            {

                MP3DisplayControl.DataSource = from o in getMP3galleryContainer().GetDirectoryReference("shortenedmp3").ListBlobs()
                                               select new { Url = o.Uri, Title = getTitle(o.Uri) };

                MP3DisplayControl.DataBind();
            }
            catch (Exception)
            {
            }
        }
    }
}