﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetAng.API.Data;
using NetAng.API.Models.Domain;
using NetAng.API.Repositories.Interface;

namespace NetAng.API.Repositories.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category> CreateAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category?> DeleteAsync(Guid id)
        {
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
            if (existingCategory is not null)
            {
                _context.Categories.Remove(existingCategory);
                await _context.SaveChangesAsync();
                return existingCategory;
            }

            return null;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(
            string? query = null,
            string? sortBy = null,
            string? sortDirection = null,
            int? pageNumber = 1,
            int? pageSize = 100)
        {
            // Query
            var categories = _context.Categories.AsQueryable();

            // Filtering
            if (string.IsNullOrWhiteSpace(query) == false)
            {
                // Due to stringComparison.OrdinalIgnoreCase, this query apply normal alphabetical search (except for Turkish)
                // Ignore checking case and culture => upper case and lower case letters are treated as the same
                categories = categories.Where(x => x.Name.Contains(query));
            }

            // Sorting
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    var isAsc = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) 
                        ? true : false;

                    categories = isAsc ? categories.OrderBy(x => x.Name) : categories.OrderByDescending(x => x.Name);
                }
                if (string.Equals(sortBy, "URL", StringComparison.OrdinalIgnoreCase))
                {
                    var isAsc = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
                        ? true : false;

                    categories = isAsc ? categories.OrderBy(x => x.UrlHandle) : categories.OrderByDescending(x => x.Name);
                }
            }

            // Pagination
            // PageNumber 1 page size 5 - skip 0, take 5
            // PageNumber 2 page size 5 - skip 5, take 5
            // PageNumber 3 page size 5 - skip 10, take 5
            var skipResults = (pageNumber - 1) * pageSize;
            categories = categories.Skip(skipResults ?? 0).Take(pageSize ?? 100);

            return await categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories.FirstOrDefaultAsync(category => category.Id == id);
        }

        public async Task<int> GetCount()
        {
            return await _context.Categories.CountAsync();
        }

        public async Task<Category?> UpdateAsync(Category category)
        {
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

            if (existingCategory is not null)
            {
                _context.Entry(existingCategory).CurrentValues.SetValues(category); 
                await _context.SaveChangesAsync();
                return category;
            }

            return null;
        }
    }
}
