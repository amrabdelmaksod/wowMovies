using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wowMovies.Models;
using wowMovies.ViewModels;

namespace wowMovies.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.Include(g=>g.Genre).ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                return View(model);
            }



            var files = Request.Form.Files;
            if (!files.Any())
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please select movie poster");
                return View(model);
            }



            var poster = files.FirstOrDefault();
            var allowedExtensions = new List<string> { ".jpg", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(poster.FileName.ToLower())))
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only Jpg or PNG are allowed");
                return View(model);
            }

            model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            return View(model);

        }
    }
}
