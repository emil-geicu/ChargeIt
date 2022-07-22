namespace ChargeIt.Models
{
    public class ChargeMachineBookingsViewModel
    {
        public ChargeMachineViewModel ChargeMachine { get; set; }
        public List<BookingViewModel> Bookings { get; set; }
    }
}
