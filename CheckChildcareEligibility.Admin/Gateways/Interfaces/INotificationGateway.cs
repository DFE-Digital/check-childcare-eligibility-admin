using CheckChildcareEligibility.Admin.Boundary.Requests;
using CheckChildcareEligibility.Admin.Boundary.Responses;

namespace CheckChildcareEligibility.Admin.Gateways.Interfaces;

public interface INotificationGateway
{
    Task<NotificationItemResponse> SendNotification(NotificationRequest data);
}
