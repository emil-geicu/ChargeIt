namespace ChargeIt.Models
{
    public class CarBookingsViewModel
    {
        public CarViewModel Car { get; set; }
        public List<BookingViewModel> Bookings { get; set; }
        public CarOwnersViewModel Owner { get; set; }
    }
}
