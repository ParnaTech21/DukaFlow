namespace DukaFlow.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Preparing = 2,
    Ready = 3,
    Completed = 4,
    Cancelled = 5,
    Rejected = 6
}
