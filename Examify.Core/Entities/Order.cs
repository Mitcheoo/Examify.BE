namespace Examify.Core.Entities
{
    public  class Order : BaseEntity
    {
        public string OrderCode { get; set; }
        public string Amount { get; set; }
        public Guid UserId { get; set; }
        public Guid ExerciseId { get; set; }
        public string Status { get; set; } = "Unpaid";
        public virtual Exercise? Exercise { get; set; }
        public virtual User? User { get; set; }
    }
}
