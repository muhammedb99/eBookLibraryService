using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace eBookLibraryService.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddToCart(CartItem item)
        {
            if (Items == null)
            {
                Items = new List<CartItem>();
            }

            var existingItem = Items.FirstOrDefault(i => i.Book.Id == item.Book.Id && i.IsBorrow == item.IsBorrow);

            if (existingItem == null)
            {
                Items.Add(item);
            }
        }

        public void RemoveFromCart(int itemId)
        {
            if (Items == null) return;

            var item = Items.Find(i => i.Id == itemId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public float GetTotalPrice()
        {
            if (Items == null || !Items.Any())
            {
                return 0;
            }

            return Items.Sum(item => item.Price);
        }
    }
}