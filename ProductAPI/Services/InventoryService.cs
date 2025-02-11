using Microsoft.EntityFrameworkCore;
using ProductAPI.Data;
using ProductAPI.Dtos;
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

        public async Task<bool> AddToInventoryAsync(IEnumerable<OrderItemResponse> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in items)
                {
                    var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);


                    inventory.Quantity += item.Quantity;
                    _context.Inventories.Update(inventory);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;

            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }


        }

        public async Task<bool> UpdateInventoryWithTransactionAsync(IEnumerable<OrderItemResponse> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in items)
                {
                    var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                    if (inventory == null || inventory.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return false;

                    }

                    inventory.Quantity -= item.Quantity;
                    _context.Inventories.Update(inventory);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }


        }
    }
}