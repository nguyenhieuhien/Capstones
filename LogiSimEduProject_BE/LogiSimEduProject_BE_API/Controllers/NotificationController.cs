// File: Controllers/NotificationController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Notification;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet("get_all_notification")]
        [SwaggerOperation(Summary = "Get all notifications", Description = "Returns a list of all notifications.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("get_notification/{id}")]
        [SwaggerOperation(Summary = "Get notification by ID", Description = "Returns a specific notification based on the provided ID.")]
        public async Task<IActionResult> Get(string id)
        {
            var notification = await _service.GetById(id);
            if (notification == null) return NotFound("Notification not found");
            return Ok(notification);
        }

        [HttpPost("create_notification")]
        [SwaggerOperation(Summary = "Create a new notification", Description = "Creates a new notification and saves it to the database.")]
        public async Task<IActionResult> Post([FromBody] NotificationDTOCreate request)
        {
            var model = new Notification
            {
                AccountId = request.AccountId,
                Title = request.Title,
                Description = request.Description,
            };

            var (success, message, id) = await _service.Create(model);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message, Id = id });
        }

        [HttpPut("update_notification/{id}")]
        [SwaggerOperation(Summary = "Update an existing notification", Description = "Updates the title or description of a notification.")]
        public async Task<IActionResult> Put(string id, [FromBody] NotificationDTOUpdate request)
        {
            var notification = await _service.GetById(id);
            if (notification == null) return NotFound("Notification not found");

            notification.AccountId = request.AccountId;
            notification.Title = request.Title;
            notification.Description = request.Description;

            var (success, message) = await _service.Update(notification);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }

        [HttpDelete("delete_notification/{id}")]
        [SwaggerOperation(Summary = "Delete a notification", Description = "Deletes a notification by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success) return NotFound(message);
            return Ok(new { Message = message });
        }
    }
}
