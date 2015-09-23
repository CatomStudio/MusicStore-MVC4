using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace MusicStore.Domain.Entities
{
    public class Order
    {
        [PrimaryKey(true)]
        [HiddenInput(DisplayValue = false)]
        public int OrderId { get; set; }
        [Required]
        [HiddenInput(DisplayValue = false)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please enter the recipient name")]
        public string Recipient { get; set; }
        [Required(ErrorMessage = "Please enter the recipient address")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
        [Required(ErrorMessage = "Please select the payment option")]
        public string PaymentOption { get; set; }
        [Required]
        public string Lines { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
        [Required]
        public DateTime Datetime { get; set; }
    }
}
