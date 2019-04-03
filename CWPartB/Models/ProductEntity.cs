using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace CWPartB.Models
{
    public class ProductEntity : TableEntity
    {
        
        public string Title { get; set; }
        public string Artist { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Mp3Blob { get; set; }
        public string SampleMp3Blob { get; set; }
        public string SampleMp3URL { get; set; }
        public DateTime? SampleDate { get; set; }

        public ProductEntity(string partitionKey, string SampleID)
        {
            PartitionKey = partitionKey;
            RowKey = SampleID;
        }

        public ProductEntity() { }

    }
}