using System.Collections.Generic;

namespace eBookLibraryService.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddToCart(CartItem item)
        {
            Items.Add(item);
        }

        public void RemoveFromCart(int itemId)
        {
            var item = Items.Find(i => i.Id == itemId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public float GetTotalPrice()
        {
            float total = 0;
            foreach (var item in Items)
            {
                total += item.Price; // Price is already set for each cart item
            }
            return total;
        }
    }
}
