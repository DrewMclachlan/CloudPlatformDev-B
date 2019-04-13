using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Linq;
using CWPartB.Models;
using Newtonsoft.Json;

namespace CWPartB
{
    //Drew Mclachlan
    //S1511481

    public partial class _Default : System.Web.UI.Page
    {
        // accessor variables and methods for blob containers and queues
        private BlobStorageService _blobStorageService = new BlobStorageService();
        private CloudQueueService _queueStorageService = new CloudQueueService();

        private CloudBlobContainer getMP3galleryContainer()
        {  
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



        //Method called when the refresh button is clicked on the user fourm
        protected void Rbutton_Click(object sender, EventArgs e)
        {
            //Redirects the current page to the current page in turn causing a refresh
            Response.Redirect(Request.RawUrl);
        }

        // Method called when user clickes the "Submit" button
        protected void submitButton_Click(object sender, EventArgs e)
        {
            //Sets a new String value to the text submitted in the Rowkey fourm on the user page
            String s = this.rk.Text;
            
            //Sets an empty int varrible
            int n;

            //Sets a boolean to the result of attempting to parse the value rowkey value recived from the user
            //The value isNumher will now be set as true or false depending on if the rowkey value was a number or not.
            bool isNumber = int.TryParse(s, out n);


            //This if statment is the first step of validation for the appliactions.
            //Ensures that the user has uploaded a mp3file for processing, The rowkey value entered is not null,
            //and that the rowkey value must be a number before continuing. As these 3 attributes are essential for the app running smoothly.
            if (upload.HasFile && s != null && isNumber == true)
            {
                // Gets the file name specified by the user without the .mp3 extension. 
                var filename = Path.GetFileNameWithoutExtension(upload.FileName);
                
                //Generates a randomised name for the uploaded mp3file
                var name = string.Format(string.Format("{0}", Guid.NewGuid()));
                String path = "mp3/" + name;
            

               
                var blob = getMP3galleryContainer().GetBlockBlobReference(path);
                blob.Properties.ContentType = GetMimeType(upload.FileName);
                //Sets the Blob metadata as the filename specified by the user
                blob.Metadata["Title"] = filename;

                // Actually upload the data to the
                // newly instantiated blob
                blob.UploadFromStream(upload.FileContent);
                blob.SetMetadata();      

                //Creates a new SampleEntity object, that containes a PartitionKey, which again can be hard coded as there is only one in use
                //A rowkey as defined by the user & the name of the newly instantiated blob in storage 
                SampleEntity QueueData = new SampleEntity() { PartitionKey = "Sample_Partition_1", RowKey = s, Mp3Blob = name };

                // Place a the new QueueData object in the queue via JSON serliasation to tell the worker role that a new mp3 blob exists
                //As well as to communicate the RowKey of the of table entry the user wishes to update
                var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(QueueData));
                getMP3shortnerQueue().AddMessage(queueMessage);
                System.Diagnostics.Trace.WriteLine(String.Format("*** WebRole: Enqueued '{0}'", path));
                }
        }

        //Called on each mp3shortned blob when being displayed
        public string getTitle(Uri blobURI)
        {
            //sets the blob variable
            CloudBlockBlob blob = new CloudBlockBlob(blobURI);
            //retrieves the attrivutes
            blob.FetchAttributes();
            //Returns the meta data attached to the blob, under the meta data heading Title
            //the data returned is the filename of the file set by the user
            return blob.Metadata["Title"];

        }




        //Called before the page is rendered
        protected void Page_PreRender(object sender, EventArgs e)
        {
            try
            {
                //Finds all blobs stored in the shortned mp3 container within  blob storage, These are the blobs processed by the webjob
                //Then lists the blobs on the deafult.apx which the 20 second clip can be played
                //This also sets the title of the blob, using the blob meta data, this is done by the getTitle method above
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