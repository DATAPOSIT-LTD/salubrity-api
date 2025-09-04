using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salubrity.Api.Controllers.Common;
using Salubrity.Application.DTOs.Notifications;
using Salubrity.Application.Interfaces.Services.Notifications;
using Salubrity.Shared.Responses;

namespace Salubrity.Api.Controllers.Notifications
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/notifications")]
    [Produces("application/json")]
    [Tags("Notifications Management")]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserNotifications(Guid userId, CancellationToken ct = default)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, ct);
            return notifications.Count > 0 ? Success(notifications) : NotFound();
        }

        [HttpPut("{notificationId:guid}/mark-as-read")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken ct = default)
        {
            var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, ct);
            return result ? Success("Notification marked as read") : NotFound();
        }
    }
}
