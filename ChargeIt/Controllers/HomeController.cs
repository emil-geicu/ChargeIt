using ChargeIt.Data;
using ChargeIt.Data.DbModels;
using ChargeIt.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChargeIt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;

        }
      /*  public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
*/
        public IActionResult Index()
        {

            //*** add to database

            /* var booking = new BookingDbModel()
             {
                 Code = Guid.NewGuid(),
                 StartTime = DateTime.Now,
                 EndTime = DateTime.Now.AddHours(1),
                 Car = new CarDbModel
                 {
                     PlateNumber = "DJ98AVC"
                 },
                 ChargeMachine = new ChargeMachineDbModel
                 {
                     Code = Guid.NewGuid(),
                     City = "Craiova",
                     Street = "Caracal",
                     Number = 178,
                     Latitude = 44.2984616,
                     Longitude = 23.8304351

                 }
             };

             _dbContext.Bookings.Add(booking);*/

            _dbContext.Bookings.Add(new BookingDbModel()
            {
                StartTime=DateTime.Now,
                EndTime=DateTime.Now.AddHours(1),
                Code=Guid.NewGuid(),
                CarId=5,
                ChargeMachineId=4
            });
            _dbContext.Bookings.Add(new BookingDbModel()
            {
                StartTime = DateTime.Now.AddHours(3),
                EndTime = DateTime.Now.AddHours(4),
                Code = Guid.NewGuid(),
                CarId = 4,
                ChargeMachineId = 2
            });
            //_dbContext.SaveChanges();
            // *view information inside var

            /*var chargeMachines = _dbContext.ChargeMachines.Where(chargeMachine => chargeMachine.Id==2).ToList();
            _dbContext.SaveChanges();*/

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}