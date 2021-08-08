using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace wowMovies.Models
{
    public class Genre
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        
        public byte Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }

        // Navigation properties
        public ICollection<Movie> Movies { get; set; }
    }
}
