using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.ViewModels
{
    public class BookDetailsViewModel
    {
        public string Cover { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int? PublishYear { get; set; }
        public string Publisher { get; set; }
        public bool IsBorrowed { get; set; }
    }
}