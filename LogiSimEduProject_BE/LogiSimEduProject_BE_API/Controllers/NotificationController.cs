using LogiSimEduProject_BE_API.Controllers.DTO.Notification;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        public NotificationController(INotificationService service) => _service = service;
        [HttpGet("get_all_notification")]
        [SwaggerOperation(Summary = "Get all notifications", Description = "Returns a list of all notifications.")]
        public async Task<IEnumerable<Notification>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_notification/{id}")]
        [SwaggerOperation(Summary = "Get notification by ID", Description = "Returns a specific notification based on the provided ID.")]
        public async Task<Notification> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("create_notification")]
        [SwaggerOperation(Summary = "Create a new notification", Description = "Creates a new notification and saves it to the database.")]
        public async Task<IActionResult> Post(NotificationDTOCreate request)
        {
            var question = new Notification
            {
                AccountId = request.AccountId,
                Title = request.Title,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(question);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("update_notification/{id}")]
        [SwaggerOperation(Summary = "Update an existing notification", Description = "Updates the title or description of a notification.")]
        public async Task<IActionResult> Put(string id, NotificationDTOUpdate request)
        {
            var existingNotification = await _service.GetById(id);
            if (existingNotification == null)
            {
                return NotFound(new { Message = $"Notification with ID {id} was not found." });
            }

            existingNotification.AccountId = request.AccountId;
            existingNotification.Title = request.Title;
            existingNotification.Description = request.Description;
            existingNotification.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingNotification);

            return Ok(new
            {
                Message = "Notification updated successfully.",
                Data = new
                {
                    AccountId = existingNotification.AccountId,
                    Title = existingNotification.Title,
                    Description = existingNotification.Description,
                    IsActive = existingNotification.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("delete_notification/{id}")]
        [SwaggerOperation(Summary = "Delete a notification", Description = "Deletes a notification by its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
