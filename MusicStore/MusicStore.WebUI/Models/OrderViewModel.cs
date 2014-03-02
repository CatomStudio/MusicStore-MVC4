using MusicStore.Domain.Entities;
namespace MusicStore.WebUI.Models
{
    public class OrderViewModel
    {
        public Cart Cart { get; set; }
        public Order Order { get; set; }
    }
}