using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eBookLibraryService.ViewModels
{
    public class BookDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public float? BorrowPrice { get; set; }
        public float BuyingPrice { get; set; }
        public int YearOfPublishing { get; set; }
        public string Genre { get; set; }
        public string ImageUrl { get; set; } 
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

        public string PdfLink { get; set; }
        public string EpubLink { get; set; }
        public string F2bLink { get; set; }
        public string MobiLink { get; set; }
    }

}