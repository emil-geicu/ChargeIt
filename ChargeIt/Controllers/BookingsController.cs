using ChargeIt.Data;
using ChargeIt.Data.DbModels;
using ChargeIt.Models;
using ChargeIt.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Net;
using System.Net.Mail;

namespace ChargeIt.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly EmailSettings _emailSettings;
        private readonly List<int> _totalAvailableHours;
        private const int TotalAvailableHoursInADay = 24;
        public BookingsController(ApplicationDbContext applicationDbContext, EmailSettings emailSettings)
        {
            _applicationDbContext = applicationDbContext;
            _emailSettings = emailSettings;
            _totalAvailableHours = new List<int>(); 
            for (var hour=0; hour < TotalAvailableHoursInADay; hour++)
            {
                _totalAvailableHours.Add(hour);
            }
        }
        public IActionResult Index()
        {

            var chargeMachines = _applicationDbContext.ChargeMachines.Select(cm => new DropDownViewModel
            {
                Id = cm.Id,
                Value = $"{cm.Code},{cm.City},{cm.Street},{cm.Number}"

            }).ToList();

            var carViewModels = _applicationDbContext.Cars.Select(c => new DropDownViewModel
            {
                Id = c.Id,
                Value = c.PlateNumber
            }).ToList();

            var bookingViewModel = new BookingViewModel()
            {
                ChargeMachines = chargeMachines,
                Cars = carViewModels,
                
            };
            return View(bookingViewModel);
        }

        private void SendEmailToTheUser(int bookingId)
        {
            var booking = _applicationDbContext.Bookings
                .Include(b => b.ChargeMachine)
                .Include(b => b.Car)
                .ThenInclude(c => c.Owner)
                .FirstOrDefault(b => b.Id == bookingId);
            var qrCode = GetBookingQRCode(booking.Code);
            var emailBody = @$"<h3>A new order has been created for your car : {booking.Car.PlateNumber}</h3>
                <p><b>Order number:</b>{booking.Code}</p>
                <p><b>Interval:</b>{booking.StartTime.ToString("yyyy-MM-dd HH:mm")} - {booking.EndTime.ToString("yyyy-MM-dd HH:MM")}</p>
                <p><b>Charge machine code:</b>{booking.ChargeMachine.Code}</p>
                <p><b>City:</b>{booking.ChargeMachine.City}</p>
                <p><b>Street:</b>{booking.ChargeMachine.Street}</p>
                <p><b>Number:</b>{booking.ChargeMachine.Number}</p>
                <img src=""{qrCode}"" alt=""QR code"" width=""200""/>
            ";

            var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.EmailAdress);
            message.To.Add(booking.Car.Owner.Email);
            message.Subject = "New booking was created for you!";
            message.IsBodyHtml = true;
            message.Body = emailBody;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_emailSettings.EmailAdress, "zutztexjiwhnzwlv"),
                EnableSsl = true,
            };

          

            smtpClient.Send(message);
        }
        public IActionResult AddNewBooking(BookingViewModel bookingViewModel)
        {

            if (ModelState.IsValid)
            {
                var startTime = bookingViewModel.Date.Value.AddHours(bookingViewModel.IntervalHour.Value);
                var endTime = startTime.AddMinutes(59).AddSeconds(59);

                if (_applicationDbContext.Bookings.FirstOrDefault(b => b.ChargeMachineId == bookingViewModel.ChargeMachineId && b.StartTime == startTime) != null)
                {
                    ModelState.AddModelError(nameof(BookingViewModel.IntervalHour), "There is already an allocated interval for the selected machine");
                }

                if (_applicationDbContext.Bookings.FirstOrDefault(b => b.CarId == bookingViewModel.CarId && b.StartTime == startTime) != null)
                {
                    ModelState.AddModelError(nameof(BookingViewModel.IntervalHour), "This car ha already been allocated to the selected interval");
                }


                if (ModelState.IsValid)
                {
                    var newBooking = new BookingDbModel
                    {
                        CarId = bookingViewModel.CarId.Value,
                        ChargeMachineId = bookingViewModel.ChargeMachineId.Value,
                        StartTime = startTime,
                        EndTime = endTime,
                        Code = Guid.NewGuid()
                    };

                    _applicationDbContext.Bookings.Add(newBooking);
                    _applicationDbContext.SaveChanges();

                    SendEmailToTheUser(newBooking.Id);

                    return RedirectToAction("Index", "Bookings");
                }
            }
            if (!ModelState.IsValid)
            {
                var chargeMachines = _applicationDbContext.ChargeMachines.Select(cm => new DropDownViewModel
                {
                    Id = cm.Id,
                    Value = $"{cm.Code},{cm.City},{cm.Street},{cm.Number}"
 
                }).ToList();

                var carViewModels = _applicationDbContext.Cars.Select(c => new DropDownViewModel
                {
                    Id = c.Id,
                    Value = c.PlateNumber
                }).ToList();

                bookingViewModel.ChargeMachines = chargeMachines;
                bookingViewModel.Cars = carViewModels;
                bookingViewModel.IntervalHour = null;
                
                return View("Index", bookingViewModel);
            }

            return RedirectToAction("Index", "Bookings");
        }

        
        [HttpGet]
        public ActionResult<List<int>> GetAvailableIntervals(int chargeMachineId, DateTime date)
        {
             var notAvailableHours = _applicationDbContext.Bookings.Where(b => b.ChargeMachineId == chargeMachineId
                          && b.StartTime >= date 
                          &&b.StartTime <= date.AddHours(23).AddMinutes(59).AddSeconds(59)).Select(b => b.StartTime.Hour).ToList();
            var currentDate = DateTime.Now;
            var totalAvailableHours = _totalAvailableHours;
            if (date.Date == currentDate.Date)
            {
                var currentHour = currentDate.Hour;
                totalAvailableHours = totalAvailableHours.Where(tav => tav >= currentHour).ToList();
            }

            var availableHours = _totalAvailableHours.Except(notAvailableHours).ToList();

            return availableHours;
        }

        private string GetBookingQRCode(Guid code)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(code.ToString(), QRCodeGenerator.ECCLevel.Q);
            var bitmapByteQRCode = new BitmapByteQRCode(qrCodeData);
            var encodedQrCode = "data:image/png;base64," + Convert.ToBase64String(bitmapByteQRCode.GetGraphic(20));
            return encodedQrCode;
        }

        /*[HttpGet("Bookings/DeleteBooking/{id}/{idToRemove}/{page}")]
        public IActionResult DeleteBooking(int id,int idToRemove,string page)
        {
            var existingBooking = _applicationDbContext.Bookings.FirstOrDefault(c => c.Id == id);
            if (existingBooking != null)
            {
                _applicationDbContext.Bookings.Remove(existingBooking);
                _applicationDbContext.SaveChanges();
            }

            if (page == "CarDetails")
            {
                return RedirectToAction(page, "Cars", new { id = idToRemove });
            }
            else
                return RedirectToAction(page, "ChargeMachines", new { id = idToRemove });

           
        }*/
        //[HttpGet("Bookings/DeleteBooking/{id}/{idToRemove}/{page}")]
        public IActionResult DeleteBooking(int id, int idToRemove, string page)
        {
            var existingBooking = _applicationDbContext.Bookings.FirstOrDefault(b => b.Id == id);

            if (existingBooking != null && existingBooking.StartTime >= DateTime.Now)
            {
                _applicationDbContext.Bookings.Remove(existingBooking);
                _applicationDbContext.SaveChanges();
            }

            if (page == "CarDetails")
            {
                return RedirectToAction("StationDetails", "ChargeMachines", new { id = idToRemove });
            }
            else
            {
                return RedirectToAction("CarDetails", "Cars", new { id = idToRemove });
               
            }
        }


    }
}
