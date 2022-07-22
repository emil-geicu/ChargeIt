using ChargeIt.Data;
using ChargeIt.Data.DbModels;
using ChargeIt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargeIt.Controllers
{
    [Authorize]
    public class CarOwnersController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public CarOwnersController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public IActionResult Index()
        {
            var carOwnersViewModel = _applicationDbContext.CarOwners.Select(co => new CarOwnersViewModel
            {
                Id = co.Id,
                Name = co.Name,
                Email = co.Email,
                Cars = co.Cars,
            }).ToList();

            return View(carOwnersViewModel);
        }
        public IActionResult AddCarOwner()
        {
            var carOwnersViewModel = new CarOwnersViewModel();
            return View();
        }

        [HttpPost]
        public IActionResult AddNewCarOwner(CarOwnersViewModel carOwnerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddCarOwner", carOwnerViewModel);
            }

            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(cm => cm.Email == carOwnerViewModel.Email);

            if (existingCarOwner != null)
            {
                ModelState.AddModelError("Email", "There is already a car owner with this email!");

                return View("AddCarOwner", carOwnerViewModel);
            }



            _applicationDbContext.CarOwners.Add(new CarOwnerDbModel
            {
                Name=carOwnerViewModel.Name,
                Email=carOwnerViewModel.Email,
                Cars=carOwnerViewModel.Cars
            });

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index", "CarOwners");
        }


        public IActionResult DeleteCarOwner(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(c => c.Id == id);
            if (existingCarOwner != null)
            {
                _applicationDbContext.CarOwners.Remove(existingCarOwner);
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public IActionResult EditCarOwner(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == id);
            if (existingCarOwner == null)
            {
                return RedirectToAction("Index");
            }
            var carOwnerViewModel = new CarOwnersViewModel()
            {
                Id = existingCarOwner.Id,
                Name = existingCarOwner.Name,
                Email=existingCarOwner.Email,
                Cars=existingCarOwner.Cars
            };
            return View(carOwnerViewModel);
        }

        [HttpPost]
        public IActionResult EditExistingCar(CarOwnersViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCarOwner", model);
            }

            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == model.Id);
            existingCarOwner.Id = model.Id;
            existingCarOwner.Name = model.Name;
            existingCarOwner.Email = model.Email;
            existingCarOwner.Cars = model.Cars;
            _applicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult CarOwnerDetails(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(c => c.Id == id);

            

            if (existingCarOwner == null)
            {
                return RedirectToAction("Index");
            }
            var carOwnersViewModel = new CarOwnersViewModel()
            {
                Id = existingCarOwner.Id,
                Name = existingCarOwner.Name,
                Email=existingCarOwner.Email,
                Cars=existingCarOwner.Cars

            };

            var cars = _applicationDbContext.Cars.Select(c => new CarViewModel
            {
                PlateNumber=c.PlateNumber,
                OwnerId=c.OwnerId.Value
            }).Where(c => c.OwnerId == carOwnersViewModel.Id).ToList();


            var ownerWithCars = new OwnerWithCarsViewModel()
            {
                CarOwner=carOwnersViewModel,
                Cars=cars
            };

            return View(ownerWithCars);
        }
    }
}
