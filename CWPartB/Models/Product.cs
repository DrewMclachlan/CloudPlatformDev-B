using System;
using System.ComponentModel.DataAnnotations;

// This is a Data Transfer Object (DTO) class. This is sent/received in REST requests/responses.
namespace CWPartB.Models
{
    public class Product
    {
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
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Name of uploaded blob in blob storage
        /// </summary>
        public string Mp3Blob { get; set; }


        /// <summary>
        /// Name of sample blobb in blob storage 
        /// </summary>
        public string SampleMp3Blob { get; set; }

    }
}