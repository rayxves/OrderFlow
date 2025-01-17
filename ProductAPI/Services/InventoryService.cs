using Microsoft.EntityFrameworkCore;
using ProductAPI.Data;
using ProductAPI.Interfaces;
using ProductAPI.Models;

namespace ProductAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDBContext _context;
        public InventoryService(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<Inventory> UpdateInventoryAsync(int productId, int quantity)
        {
            var existsInventory = await _context.Inventories.FirstOrDefaultAsync(x => x.ProductId == productId);
            if (existsInventory == null)
            {
                throw new InvalidOperationException("Inventory not found!");
            }

            if (existsInventory.Quantity < quantity)
            {
                throw new InvalidOperationException("Not enough inventory to fulfill the order.");
            }

            existsInventory.Quantity -= quantity;
            await _context.SaveChangesAsync();
            return existsInventory;
        }
    }
}