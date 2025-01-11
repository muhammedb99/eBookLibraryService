using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using eBookLibraryService.Models;

namespace eBookLibraryService.Controllers
{
    public class ContactController : Controller
    {
        private readonly IConfiguration _configuration;

        public ContactController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ContactForm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string senderEmail = "ebookstorenoty@gmail.com";
            string senderPassword = "fznt vkrs xivq ftay"; 

            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = $"[Contact Us] {model.Subject}",
                    Body = $"From: {model.FullName} ({model.Email})\n\n{model.Message}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(senderEmail);

                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                smtpClient.Send(mailMessage);

                TempData["SuccessMessage"] = "Your message has been sent successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "There was an error sending your message. Please try again later.";
                Console.WriteLine(ex.Message); 
            }

            return View(model);
        }
    }
}
