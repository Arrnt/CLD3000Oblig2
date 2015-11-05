using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public class ImageModel
    {
        [Key]
        public int ImageId { get; set; }

        [StringLength(100)]
        [DisplayName("Tittel")]
        public string Title { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [DisplayName("Beskrivelse")]
        public string Description { get; set; }

        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }

        [StringLength(2083)]
        [DisplayName("Scaled Image")]
        public string ScaledImageURL { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PostedDate { get; set; }
    }
}
