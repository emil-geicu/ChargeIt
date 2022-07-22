using System.ComponentModel.DataAnnotations;

namespace ChargeIt.Models
{
    public class CarViewModel
    {
        
        public List<DropDownViewModel> CarOwners { get; set; }
        public int Id { get; set; }

        [Required]
        public string PlateNumber { get; set; }

        [Required(ErrorMessage ="Please select an Owner")]
        public int? OwnerId { get; set; }
        
        public CarOwnersViewModel Owner { get; set; }
        
        
    }
}
