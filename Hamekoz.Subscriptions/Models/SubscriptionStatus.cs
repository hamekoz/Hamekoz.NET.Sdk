namespace Hamekoz.Subscriptions.Models;

public enum SubscriptionStatus
{
    PendingActivation = 0,
    Active = 1,
    Expired = 2,
    Cancelled = 3,
    Suspended = 4
}