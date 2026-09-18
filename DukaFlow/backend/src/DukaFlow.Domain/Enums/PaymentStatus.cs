namespace DukaFlow.Domain.Enums;

// Order-level payment status. Phase 5 will wire this to real Mobile Money
// providers; for Phase 3 every order simply starts Unpaid.
public enum PaymentStatus
{
    Unpaid = 0,
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}
