using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using Microsoft.WindowsAzure.Storage.Table;
using CWPartB.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace CWPartB.Controllers
{
    //Drew Mclachlan 
    //S1511481  
    public class SampleController : ApiController
    {   
        
        //Hard coded table partition name
        private const String partitionName = "Sample_Partition_1";
        //New instance of the blob storage class which allows the code to store blobs in the connected storage acount
        private BlobStorageService _blobStorageService = new BlobStorageService();
        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable table;


        public SampleController()
        {
            //Connects the cloud store account to the appliaction
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureStorage"].ToString());
            //Connects the table data to the storage account defined above
            tableClient = storageAccount.CreateCloudTableClient();
            //retrives a table 'Samples' from the table data and appends it to the var table
            table = tableClient.GetTableReference("Samples");
        }

        //Retrives the blobs in the MP3 gallery
        private CloudBlobContainer getMP3galleryContainer()
        {

            return _blobStorageService.getCloudBlobContainer();
        }

        /// <summary>
        /// Get all samples
        /// </summary>
        /// <returns></returns>
        // GET: api/Sample

        //Return all table entries
        public IEnumerable<Sample> Get()
        {
          
            //Finds all entries in the Table 'Samples'
            TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));
            List<SampleEntity> entityList = new List<SampleEntity>(table.ExecuteQuery(query));

            //Create a list of Products from the list of ProductEntity with a 1:1 object relationship, filtering data as needed
            IEnumerable<Sample> productList = from e in entityList
                                               select new Sample()
                                               {
                                                   PartitionKey = e.PartitionKey,
                                                   SampleID = e.RowKey,
                                                   Title = e.Title,
                                                   Artist = e.Artist,
                                                   CreatedDate = e.CreatedDate,
                                                   Mp3Blob = e.Mp3Blob,
                                                   SampleMp3Blob = e.SampleMp3Blob,
                                                   SampleMp3URL = e.SampleMp3URL,
                                                   SampleDate = e.SampleDate                                          
                                               };
            return productList;
        }

        // GET: api/Sample/5
        /// <summary>
        /// Get a sample
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        
         //Gets specifc table entity defined by id 
        [ResponseType(typeof(Sample))]
        public IHttpActionResult GetProduct(string id)
        {
            // Create a retrieve operation that takes a product entity.
            TableOperation getOperation = TableOperation.Retrieve<SampleEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult getOperationResult = table.Execute(getOperation);

            // Construct response including a new DTO as apprporiatte
            if (getOperationResult.Result == null) return NotFound();
            else
            {
                SampleEntity sampleEntity = (SampleEntity)getOperationResult.Result;
                Sample p = new Sample()
                {
                    PartitionKey = sampleEntity.PartitionKey,
                    SampleID = sampleEntity.RowKey,
                    Title = sampleEntity.Title,
                    Artist = sampleEntity.Artist,
                    CreatedDate = sampleEntity.CreatedDate,
                    Mp3Blob = sampleEntity.Mp3Blob,
                    SampleMp3Blob = sampleEntity.SampleMp3Blob,
                    SampleMp3URL = sampleEntity.SampleMp3URL,
                    SampleDate = sampleEntity.SampleDate

                };
         
                return Ok(p);
            }
        }

        // POST: api/Sample
        /// <summary>
        /// Create a new sample
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        
            //Creats new sample in table with data passed through POST request body
        [ResponseType(typeof(Sample))]
        public IHttpActionResult PostProduct(Sample product)
        {
           
            //Creats a new sample
            DateTime date = DateTime.Now;
            SampleEntity sampleEntity = new SampleEntity()
            {   
            RowKey = getNewMaxRowKeyValue(),
                PartitionKey = partitionName,
                Title = product.Title,
                Artist = product.Artist,
                CreatedDate = date,
                Mp3Blob = product.Mp3Blob,
                SampleMp3Blob = null,
               SampleMp3URL = null,
                SampleDate = null
             
            };

            // Create the TableOperation that inserts the product entity.
            var insertOperation = TableOperation.Insert(sampleEntity);

            // Execute the insert operation.
            table.Execute(insertOperation);

            return CreatedAtRoute("DefaultApi", new { id = sampleEntity.RowKey }, sampleEntity);
        }

        // PUT: api/Sample/5
        /// <summary>
        /// Update a sample
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
    
        //Updates table entry with new information defined in put reauest body
        //Item to be updated is defined via id passed in url
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSample(string id, Sample sample)
        {
            if (id != sample.SampleID)
            {
                return BadRequest();
            }

            // Create a retrieve operation that takes a product entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<SampleEntity>(partitionName, id);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a ProductEntity object.
            SampleEntity updateEntity = (SampleEntity)retrievedResult.Result;
            
            
            //When updating an entry in the table if a blob is attached to that entry, it becomes no longer
            //relevent as their is new data in the entry, thus must be deleted
            

            //Finds sample blob location from exiting data in the table before update operation
            var path = updateEntity.SampleMp3Blob;
            //creates a new  sample blob object within the code by finding the blob using the path defined above
           var blob = getMP3galleryContainer().GetBlockBlobReference(path);
            //deletes the blob from storage if it exists
            blob.DeleteIfExists();

            //Same operation as above but for the inputblob
            var path2 = updateEntity.Mp3Blob;
            var blob2 = getMP3galleryContainer().GetBlockBlobReference(path2);
            blob2.DeleteIfExists();



            //sets the new values to update the table from the values sent in the PUT request body
            updateEntity.Title = sample.Title;
            updateEntity.Artist = sample.Artist;
            updateEntity.Mp3Blob = null;
            updateEntity.SampleMp3Blob = null;
            updateEntity.SampleMp3URL = null;
            updateEntity.SampleDate = null;


            // Create the TableOperation that inserts the product entity.
            var updateOperation = TableOperation.InsertOrReplace(updateEntity);

            // Execute the update operation.
            table.Execute(updateOperation);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Sample/5
        /// <summary>
        /// Delete a sample
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       
            //If table entry is found from the id past in the url of the Delete request, deletes the entry
        [ResponseType(typeof(Sample))]
        public IHttpActionResult DeleteSample(string id)
        {
            // Create a retrieve operation that takes a product entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<SampleEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            SampleEntity updateEntity = (SampleEntity)retrievedResult.Result;

            //Finds blob location from exiting data in the table before delete operation
            var path = updateEntity.SampleMp3Blob;
           
            //creates a new blob object within the code by finding the blob using the path defined above
            var blob = getMP3galleryContainer().GetBlockBlobReference(path);
            //if the blob exists in storage, deletes it
            blob.DeleteIfExists();

            //Same operation as above but for the inputblob
            var path2 = updateEntity.Mp3Blob;
            var blob2 = getMP3galleryContainer().GetBlockBlobReference(path2);
            blob2.DeleteIfExists();


            if (retrievedResult.Result == null) return NotFound();
            else
            {
                SampleEntity deleteEntity = (SampleEntity)retrievedResult.Result;
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);

                return Ok(retrievedResult.Result);
            }
        }


        //Method that keeps track of the current Rowkey values in the table
        //This method is called when a new entry is added to the Sample table 
        //Finds the most recent rowkey value and provided a rowkey value to the new entry
       //one greater than the last one. 
       //e.g if samples had 6 entries, this method would provide a new sample entity the rowkey value 7
        private String getNewMaxRowKeyValue()
        {
            TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));

            int maxRowKeyValue = 0;
            foreach (SampleEntity entity in table.ExecuteQuery(query))
            {
                int entityRowKeyValue = Int32.Parse(entity.RowKey);
                if (entityRowKeyValue > maxRowKeyValue) maxRowKeyValue = entityRowKeyValue;
            }
            maxRowKeyValue++;
            return maxRowKeyValue.ToString();
        }


    }
}
