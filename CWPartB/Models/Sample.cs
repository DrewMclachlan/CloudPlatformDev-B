using System;
using System.ComponentModel.DataAnnotations;
//Drew Mclachlan
//S1511481

// This is a Data Transfer Object (DTO) class. This is sent/received in REST requests/responses.
namespace CWPartB.Models
{
    public class Sample
    {
        /// <summary>
        /// Sample PartionKey
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Sample ID
        /// </summary>
        [Key]
        public string SampleID { get; set; }

        /// <summary>
        /// Title of the Sample
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Name of Artist
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Creation date/time of entity
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Name of uploaded blob in blob storage
        /// </summary>
        public string Mp3Blob { get; set; }


        /// <summary>
        /// Name of sample blobb in blob storage 
        /// </summary>
        public string SampleMp3Blob { get; set; }

        /// <summary>
        /// Web Service resource URL of mp3 sample 
        /// </summary>
        public string SampleMp3URL { get; set; }

        /// <summary>
        /// creation date/time of sample blob
        /// </summary>
        public DateTime? SampleDate { get; set; }

    }
}