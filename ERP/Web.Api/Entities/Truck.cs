namespace Web.Api.Entities;

public class Truck : Entity<int>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public TruckStatusEnum Status { get; set; }
    public string Description { get; set; }

    #region consts

    public const int CodeLength = 10;

    #endregion
}