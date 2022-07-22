using ChargeIt.Data.DbModels;
using System.ComponentModel.DataAnnotations;

namespace ChargeIt.Models
{
    public class CarOwnersViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public ICollection<CarDbModel> Cars { get; set; }
    }
}
