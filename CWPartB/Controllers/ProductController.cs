using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using CWPartB.Models;
//using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace CWPartB.Controllers
{
    public class ProductsController : ApiController
    {
        private const String partitionName = "Sample_Partition_1";

        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable table;

        public ProductsController()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Samples");
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns></returns>
        // GET: api/Products
        public IEnumerable<Product> Get()
        {
            TableQuery<ProductEntity> query = new TableQuery<ProductEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));
            List<ProductEntity> entityList = new List<ProductEntity>(table.ExecuteQuery(query));

            // Basically create a list of Product from the list of ProductEntity with a 1:1 object relationship, filtering data as needed
            IEnumerable<Product> productList = from e in entityList
                                               select new Product()
                                               {
                                                   SampleID = e.RowKey,
                                                   Title = e.Title,
                                                   Artist = e.Artist,
                                                   CreatedDate = e.CreatedDate,
                                                   Mp3Blob = e.Mp3Blob,
                                                  
                                               };
            return productList;
        }

        // GET: api/Products/5
        /// <summary>
        /// Get a product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(string id)
        {
            // Create a retrieve operation that takes a product entity.
            TableOperation getOperation = TableOperation.Retrieve<ProductEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult getOperationResult = table.Execute(getOperation);

            // Construct response including a new DTO as apprporiatte
            if (getOperationResult.Result == null) return NotFound();
            else
            {
                ProductEntity productEntity = (ProductEntity)getOperationResult.Result;
                Product p = new Product()
                {
                    SampleID = productEntity.RowKey,
                    Title = productEntity.Title,
                    Artist = productEntity.Artist,
                    CreatedDate = productEntity.CreatedDate,
                   // Mp3Blob = productEntity.Mp3Blob,
           

                };
                return Ok(p);
            }
        }

        // POST: api/Products
        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.Created)]
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
           
        
            DateTime date = DateTime.Now;
            ProductEntity productEntity = new ProductEntity()
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
            var insertOperation = TableOperation.Insert(productEntity);

            // Execute the insert operation.
            table.Execute(insertOperation);

            return CreatedAtRoute("DefaultApi", new { id = productEntity.RowKey }, productEntity);
        }

        // PUT: api/Products/5
        /// <summary>
        /// Update a product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.NoContent)]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(string id, Product product)
        {
            if (id != product.SampleID)
            {
                return BadRequest();
            }

            // Create a retrieve operation that takes a product entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<ProductEntity>(partitionName, id);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a ProductEntity object.
            ProductEntity updateEntity = (ProductEntity)retrievedResult.Result;

            updateEntity.Title = product.Title;
            updateEntity.Artist = product.Artist;
            updateEntity.Mp3Blob = product.Mp3Blob;
          

            // Create the TableOperation that inserts the product entity.
            // Note semantics of InsertOrReplace() which are consistent with PUT
            // See: https://stackoverflow.com/questions/14685907/difference-between-insert-or-merge-entity-and-insert-or-replace-entity
            var updateOperation = TableOperation.InsertOrReplace(updateEntity);

            // Execute the insert operation.
            table.Execute(updateOperation);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Products/5
        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(string id)
        {
            // Create a retrieve operation that takes a product entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<ProductEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result == null) return NotFound();
            else
            {
                ProductEntity deleteEntity = (ProductEntity)retrievedResult.Result;
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);

                return Ok(retrievedResult.Result);
            }
        }

        private String getNewMaxRowKeyValue()
        {
            TableQuery<ProductEntity> query = new TableQuery<ProductEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));

            int maxRowKeyValue = 0;
            foreach (ProductEntity entity in table.ExecuteQuery(query))
            {
                int entityRowKeyValue = Int32.Parse(entity.RowKey);
                if (entityRowKeyValue > maxRowKeyValue) maxRowKeyValue = entityRowKeyValue;
            }
            maxRowKeyValue++;
            return maxRowKeyValue.ToString();
        }


    }
}
