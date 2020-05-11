using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace WebApplication1.Models
{
    public class FileUpload
    {
        public int id { get; set; }

        [DisplayName("File Name")]
        public string fileName { get; set; }

        [DisplayName("File URL")]
        public string fileUrl { get; set; }

        [DisplayName("Receiver Email")]
        public string receiverEmail { get; set; }

        public string userEmail { get; set; }
    }
}