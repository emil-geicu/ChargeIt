namespace ChargeIt.Data.DbModels
{
    public class CarDbModel
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }

        public CarOwnerDbModel Owner { get; set; }
        public int? OwnerId { get; set; }
    }
}
