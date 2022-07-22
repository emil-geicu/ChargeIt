using ChargeIt.Data;
using ChargeIt.Data.DbModels;
using ChargeIt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargeIt.Controllers
{
    [Authorize]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public CarsController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public IActionResult Index()
        {
            var carViewModels = _applicationDbContext.Cars.Select(c => new CarViewModel
            {
                Id = c.Id,
                PlateNumber= c.PlateNumber,
                OwnerId=c.OwnerId
            }).ToList();

            foreach(var car in carViewModels)
            {
                
                var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == car.OwnerId);
                var carOwnerViewModel = new CarOwnersViewModel()
                {
                    Name = existingCarOwner.Name,
                    Email = existingCarOwner.Email,
                    Cars = existingCarOwner.Cars
                };

                car.Owner = carOwnerViewModel;
            }
            return View(carViewModels);
        }

        public IActionResult AddCar()
        {
            var carViewModel = new CarViewModel();
            var ownerDropdown = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
            {
                Id = co.Id,
                Value = $"{co.Name},{co.Email}"
            }).ToList();

            carViewModel.CarOwners = ownerDropdown;
            return View(carViewModel);
        }

        [HttpPost]
        public IActionResult AddNewCar(CarViewModel carViewModel)
        {
            if (!ModelState.IsValid)
            {
                var ownerDropdown = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
                {
                    Id = co.Id,
                    Value = $"{co.Name},{co.Email}"
                }).ToList();

                carViewModel.CarOwners = ownerDropdown;
                return View("AddCar", carViewModel);
            }

            var existingCar = _applicationDbContext.Cars.FirstOrDefault(cm => cm.PlateNumber == carViewModel.PlateNumber);

            if (existingCar != null)
            {
                ModelState.AddModelError("PlateNumber","There is already a car with this plate number!");

                var ownerDropdown = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
                {
                    Id = co.Id,
                    Value = $"{co.Name},{co.Email}"
                }).ToList();

                carViewModel.CarOwners = ownerDropdown;
                return View("AddCar", carViewModel);
            }



            _applicationDbContext.Cars.Add(new CarDbModel
            {
                PlateNumber = carViewModel.PlateNumber,
                OwnerId=carViewModel.OwnerId
          
            });

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index", "Cars");
        }


        public IActionResult DeleteCar(int id)
        {
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(c => c.Id == id);
            if (existingCar != null)
            {
                _applicationDbContext.Cars.Remove(existingCar);
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public IActionResult EditCar(int id)
        {
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(c => c.Id == id);
            if (existingCar == null)
            {
                return RedirectToAction("Index");
            }
            var carViewModel = new CarViewModel()
            {
                Id = existingCar.Id,
                PlateNumber = existingCar.PlateNumber,
                OwnerId=existingCar.OwnerId.Value
            };
            var ownerDropdown = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
            {
                Id = co.Id,
                Value = $"{co.Name},{co.Email}"
            }).ToList();

            carViewModel.CarOwners = ownerDropdown;
            return View(carViewModel);
        }

        [HttpPost]
        public IActionResult EditExistingCar(CarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var ownerDropdown = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
                {
                    Id = co.Id,
                    Value = $"{co.Name},{co.Email}"
                }).ToList();

                model.CarOwners = ownerDropdown;
                return View("EditCar", model);
            }

            var checkCar = _applicationDbContext.Cars.FirstOrDefault(cm => cm.PlateNumber == model.PlateNumber && cm.Id!=model.Id) ;

            if (checkCar != null)
            {
                ModelState.AddModelError("PlateNumber", "There is already a car with this plate number!");
                var ownerDropdown = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
                {
                    Id = co.Id,
                    Value = $"{co.Name},{co.Email}"
                }).ToList();

                model.CarOwners = ownerDropdown;

                return View("EditCar", model);
            }
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(c => c.Id == model.Id);
            existingCar.Id = model.Id;
            existingCar.PlateNumber = model.PlateNumber;
            existingCar.OwnerId = model.OwnerId.Value;

            _applicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult CarDetails(int id)
        {
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(c => c.Id == id);

            var availableBookings = _applicationDbContext.Bookings.Include(b => b.ChargeMachine)
                .Where(b => b.CarId == existingCar.Id)
                .OrderByDescending(b => b.StartTime)
                .ToList();


            if (existingCar == null)
            {
                return RedirectToAction("Index");
            }
            var carViewModel = new CarViewModel()
            {
                Id = existingCar.Id,
                PlateNumber = existingCar.PlateNumber,
                OwnerId=existingCar.OwnerId
               

            };
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == carViewModel.OwnerId);

         
            var carOwnerViewModel = new CarOwnersViewModel()
            {
                Name = existingCarOwner.Name,
                Email = existingCarOwner.Email,
                Cars = existingCarOwner.Cars
            };

            carViewModel.Owner = carOwnerViewModel;

            var bookings = _applicationDbContext.Bookings.Select(b => new BookingViewModel
            {
                Id=b.Id,
                CarId = b.CarId,
                ChargeMachineId = b.ChargeMachineId,
                Date = b.StartTime,
                IntervalHour = b.StartTime.Hour,
                Code = b.Code,
                canCancel = b.StartTime > DateTime.Now ? true : false
            }).Where(b=> b.CarId==carViewModel.Id).ToList();

            var carBookings = new CarBookingsViewModel()
            {
                Car = carViewModel,
                Bookings = bookings,
                Owner=carOwnerViewModel
                
            };
      

            return View(carBookings);
        }

    }
}
