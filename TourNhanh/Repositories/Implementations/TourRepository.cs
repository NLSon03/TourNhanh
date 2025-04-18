﻿using Microsoft.EntityFrameworkCore;
using TourNhanh;
using TourNhanh.Models;
using TourNhanh.Repositories.Interfaces;

namespace TourNhanh.Repositories.Implementations
{
    public class TourRepository : ITourRepository
    {
        private readonly AppDbContext _context;

        public TourRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tour>> GetAllAsync()
        {
            return await _context.Tours
                .Include(tour=>tour.Transport)
                .Include(tour=>tour.Category)
                .ToListAsync();
        }
        public async Task<Tour?> GetByIdAsync(int id)
        {
            return await _context.Tours
                .Include(tour => tour.Transport)
                .Include(tour => tour.Category)
                .FirstOrDefaultAsync(tour => tour.Id == id);
        }

        public async Task CreateAsync(Tour tour)
        {
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tour tour)
        {
            _context.Tours.Update(tour);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tour = await GetByIdAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Tour>> GetSortedToursAsync(string sortOrder)
        {
            var tours = _context.Tours
                .Include(t => t.Transport)
                .Include(t => t.Category)
                .Include(t => t.Reviews)
                .AsQueryable();

            switch (sortOrder)
            {
                case "rating":
                    tours = tours.OrderByDescending(t => t.Reviews.Any() ? t.Reviews.Average(r => r.Rating) : double.MinValue)
                                 .ThenByDescending(t => t.Id);
                    break;
                case "popular":
                    tours = tours.OrderByDescending(t => (t.maxParticipants - t.RemainingSlots) > 0 ? (t.maxParticipants - t.RemainingSlots) : int.MinValue)
                                 .ThenByDescending(t => t.Id);
                    break;
                case "newest":
                    tours = tours.OrderByDescending(t => t.Id);
                    break;
                default:
                    tours = tours.OrderBy(t => t.Name);
                    break;
            }

            return await tours.Take(10).ToListAsync();
        }
    }
}
