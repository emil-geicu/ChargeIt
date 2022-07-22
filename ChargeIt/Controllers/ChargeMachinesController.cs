using ChargeIt.Data;
using ChargeIt.Data.DbModels;
using ChargeIt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargeIt.Controllers
{
    [Authorize]
    public class ChargeMachinesController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ChargeMachinesController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        
        public IActionResult Index()
        {
            
            var chargeMachinesViewModels = _applicationDbContext.ChargeMachines.Select(cm => new ChargeMachineViewModel
            {
                Id = cm.Id,
                City = cm.City,
                Code = cm.Code,
                Latitude = cm.Latitude,
                Longitude = cm.Longitude,
                Number = cm.Number,
                Street = cm.Street
            }).ToList();

            /*chargeMachines.ForEach(cm =>
            {
                chargeMachinesViewModels.Add(new ChargeMachineViewModel
                {
                    Id = cm.Id,
                    City = cm.City,
                    Code = cm.Code,
                    Latitude = cm.Latitude,
                    Longitude = cm.Longitude,
                    Number = cm.Number,
                    Street = cm.Street

                });
            });*/


            return View(chargeMachinesViewModels);
        }

        public IActionResult AddStation()
        {
            var chargeMachineViewModel = new ChargeMachineViewModel();

            return View();
        }

        [HttpPost]
        public IActionResult AddNewStation(ChargeMachineViewModel chargeMachineViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddStation", chargeMachineViewModel);
            }
            _applicationDbContext.ChargeMachines.Add(new ChargeMachineDbModel
            {
                City = chargeMachineViewModel.City,
                Code = Guid.NewGuid(),
                Latitude = (double)chargeMachineViewModel.Latitude,
                Longitude = (double)chargeMachineViewModel.Longitude,
                Number = (int)chargeMachineViewModel.Number,
                Street = chargeMachineViewModel.Street
            });

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index","ChargeMachines");
        }


        public IActionResult DeleteStation(int id)
        {
            var existingChargeMachine = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == id);
            if (existingChargeMachine != null)
            {
                _applicationDbContext.ChargeMachines.Remove(existingChargeMachine);
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult EditStation(int id)
        {
            var existingChargeMachine = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == id);
            if (existingChargeMachine == null)
            {
                return RedirectToAction("Index");
            }
            var chargeMachineViewModel = new ChargeMachineViewModel()
            {
                Id = existingChargeMachine.Id,
                City = existingChargeMachine.City,
                Code = Guid.NewGuid(),
                Latitude = (double)existingChargeMachine.Latitude,
                Longitude = (double)existingChargeMachine.Longitude,
                Number = (int)existingChargeMachine.Number,
                Street = existingChargeMachine.Street
            };
            return View(chargeMachineViewModel);
        }
        [HttpPost]
        public IActionResult EditExistingStation(ChargeMachineViewModel model)
        {
            if (!ModelState.IsValid)
            { 
                return View("EditStation", model);
            }

            var existingStation = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == model.Id);

            existingStation.City = model.City;
            existingStation.Street = model.Street;
            existingStation.Number = (int)model.Number;
            existingStation.Latitude = (double)model.Latitude;
            existingStation.Longitude = (double)model.Longitude;

            _applicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult  StationDetails(int id)
        {
            var existingChargeMachine = _applicationDbContext.ChargeMachines.FirstOrDefault(c => c.Id == id);
            if (existingChargeMachine == null)
            {
                return RedirectToAction("Index");
            }
            var chargeMachineViewModel = new ChargeMachineViewModel()
            {

                City = existingChargeMachine.City,
                Code = existingChargeMachine.Code,
                Id = existingChargeMachine.Id,
                Latitude = existingChargeMachine.Latitude,
                Longitude = existingChargeMachine.Longitude,
                Number = existingChargeMachine.Number,
                Street= existingChargeMachine.Street


            };

            var bookings = _applicationDbContext.Bookings.Select(b => new BookingViewModel
            {
                Id=b.Id,
                CarId = b.CarId,
                ChargeMachineId = b.ChargeMachineId,
                Date = b.StartTime,
                IntervalHour = b.StartTime.Hour,
                Code = b.Code,
                canCancel = b.StartTime > DateTime.Now ? true : false
            }).Where(b => b.ChargeMachineId == chargeMachineViewModel.Id).ToList();

            var chargeMachineBookings = new ChargeMachineBookingsViewModel()
            {
                ChargeMachine=chargeMachineViewModel,
                Bookings = bookings
            };
            return View(chargeMachineBookings);
        }
        public double ReturnLatitude(int id)
        {
            var existingStation = _applicationDbContext.ChargeMachines.FirstOrDefault(c => c.Id == id);
            return existingStation.Latitude;
        }
        public double ReturnLongitude(int id)
        {
            var existingStation = _applicationDbContext.ChargeMachines.FirstOrDefault(c => c.Id == id);
            return existingStation.Longitude;
        }
    }

}
