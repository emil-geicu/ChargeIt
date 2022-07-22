using System.ComponentModel.DataAnnotations;

namespace ChargeIt.Models
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public List<DropDownViewModel> ChargeMachines { get; set; }
        public List<DropDownViewModel> Cars { get; set; }

        [Display(Name = "Charge Machine")]
        [Required(ErrorMessage ="Please select aa valid charge machine")]
        public int? ChargeMachineId { get; set; }

        [Display(Name = "Car")]
        [Required(ErrorMessage = "Please select aa valid car")]
        public int? CarId { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        [Display(Name = "Available intervals")]
        [Required(ErrorMessage = "Please select aa valid interval")]
        public int? IntervalHour { get; set; }
        public Guid Code { get; set; }

        public bool canCancel;

        
   

    }
}
