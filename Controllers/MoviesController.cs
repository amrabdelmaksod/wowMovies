using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
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
        private readonly IToastNotification _toastNotification;
        private List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private long _maxPosterSize = 1048576;
    public MoviesController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(m=>m.Rate).Include(g=>g.Genre).ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };

            return View("MovieForm",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                return View("MovieForm", model);
            }



            var files = Request.Form.Files;
            if (!files.Any())
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please select movie poster");
                return View("MovieForm", model);
            }



            var poster = files.FirstOrDefault();
           

            if (!_allowedExtensions.Contains(Path.GetExtension(poster.FileName.ToLower())))
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only Jpg or PNG are allowed");
                return View("MovieForm", model);
            }

            if (poster.Length > _maxPosterSize)
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Poster cannot be more than one mega byte!");
                return View("MovieForm", model);
            }

            using var dataStream = new MemoryStream();
            await poster.CopyToAsync(dataStream);

            var movies = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Year = model.Year,
                Rate = model.Rate,
                StoryLine = model.StoryLine,
                Poster = dataStream.ToArray()
            };

            _context.Movies.Add(movies);
            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie Added Successfully");
          
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var movieInDb =await _context.Movies.FindAsync(id);
            if (movieInDb == null)
                return NotFound();
            var viewModel = new MovieFormViewModel
            {
                Id = movieInDb.Id,
                Title = movieInDb.Title,
                GenreId = movieInDb.GenreId,
                Year = movieInDb.Year,
                Rate = movieInDb.Rate,
                StoryLine = movieInDb.StoryLine,
                Poster = movieInDb.Poster,

                Genres = await _context.Genres.ToListAsync()

            };
            return View("MovieForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(MovieFormViewModel model, int id)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.ToListAsync();
                return View("Edit", model);
            }
            var movieInDb = await _context.Movies.FindAsync(id);
            if (movieInDb == null)
                return NotFound();

            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var dataStream = new MemoryStream();

                await poster.CopyToAsync(dataStream);

                model.Poster = dataStream.ToArray();

                if (!_allowedExtensions.Contains(Path.GetExtension(poster.FileName.ToLower())))
                {
                    model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only Jpg or PNG are allowed");
                    return View("MovieForm", model);
                }

                if (poster.Length > _maxPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than one mega byte!");
                    return View("MovieForm", model);
                }
                movieInDb.Poster = model.Poster;
            }

            movieInDb.Title = model.Title;
            movieInDb.GenreId = model.GenreId;
            movieInDb.Year = model.Year;
            movieInDb.Rate = model.Rate;
            movieInDb.StoryLine = model.StoryLine;

            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie Updated Successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var movieInDb = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);
            if (movieInDb == null)
                return NotFound();
            return View(movieInDb);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();
            return Ok();
        }


    }
}
