using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Domain.Entities
{
    public class Account
    {
        [HiddenInput(DisplayValue = false)]
        public int AccountId { get; set; }
        [Required(ErrorMessage = "Please enter your username")]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please enter your fullname")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter your address")]
        public string Address { get; set; }
        public byte[] ImageData { get; set; }
        [HiddenInput(DisplayValue = false)]
        public string ImageMimeType { get;set;}
    }
}
