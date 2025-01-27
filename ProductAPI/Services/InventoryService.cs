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

        public async Task<Inventory> AddToInventoryAsync(int productId, int quantity)
        {
            var existsInventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (existsInventory == null)
            {
                throw new InvalidOperationException("Inventory does not exist");
            }

            existsInventory.Quantity += quantity;
            _context.Inventories.Update(existsInventory);
            await _context.SaveChangesAsync();
            return existsInventory;
        }

        public async Task<Inventory> UpdateInventoryAsync(int productId, int quantity)
        {
            Console.WriteLine($"Trying to update inventory for ProductId {productId} with quantity {quantity}");


            var existsInventory = await _context.Inventories.FirstOrDefaultAsync(x => x.ProductId == productId);
            if (existsInventory == null)
            {
                throw new InvalidOperationException("Inventory not found!");
            }
            Console.WriteLine($"Current Inventory for ProductId {productId}: {existsInventory.Quantity}");


            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (existsInventory.Quantity < quantity)
                {
                    throw new InvalidOperationException("Not enough inventory to fulfill the order.");
                }

                existsInventory.Quantity -= quantity;
                if (existsInventory.Quantity < 0)
                {
                    throw new InvalidOperationException("Inventory cannot go negative.");
                }

                _context.Inventories.Update(existsInventory);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException("The inventory was updated by another process. Please try again.");
                }

                Console.WriteLine($"Updated Inventory for ProductId {productId}: {existsInventory.Quantity}");


                await transaction.CommitAsync();
                return existsInventory;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


        }
    }
}