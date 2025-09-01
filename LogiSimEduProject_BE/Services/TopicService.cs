using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.DTO.Topic;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

    public class TopicService : ITopicService
    {
        private TopicRepository _repository;
        private readonly LogiSimEduContext _dbContext;

        public TopicService()
        {
            _repository = new TopicRepository();
            _dbContext = new LogiSimEduContext();
        }
        public async Task<int> Create(Topic topic)
        {
            topic.Id = Guid.NewGuid();
            return await _repository.CreateAsync(topic);
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                item.IsActive = false;
                item.DeleteAt = DateTime.UtcNow;

                await _repository.UpdateAsync(item);
                return (true, "Deleted successfully");
            }

            return (false, "Topic not found");
        }

        public async Task<List<Topic>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Topic> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Topic>> GetTopicsByCourseId(Guid courseId)
        {
            return await _repository.GetTopicsByCourseIdAsync(courseId);
        }

        public async Task<List<TopicWithFinishDTO>> GetTopicsByCourseIdAsync(
    Guid courseId,
    int completedStatus = 2 // giá trị Status thể hiện Completed trong LessonProgress
)
        {
            // 1) Lấy các lesson active theo topic trong course
            var activeLessons = await _dbContext.Lessons
                .Where(l =>
                    (l.IsActive ?? false) &&
                    l.Topic != null &&
                    l.Topic.CourseId == courseId &&
                    (l.Topic.IsActive ?? false))
                .Select(l => new
                {
                    LessonId = l.Id,
                    TopicId = l.TopicId!.Value
                })
                .ToListAsync();

            // Số lesson active mỗi topic
            var lessonCountByTopic = activeLessons
                .GroupBy(x => x.TopicId)
                .ToDictionary(g => g.Key, g => g.Count());

            // 2) Lấy các progress Completed, gắn với lesson và topic
            //    Distinct theo (AccountId, LessonId) để tránh đếm trùng
            var completedByStudentAndTopic = await _dbContext.LessonProgresses
                .Where(lp =>
                    (lp.IsActive ?? false) &&
                    lp.Status == completedStatus &&
                    lp.Lesson != null &&
                    (lp.Lesson.IsActive ?? false) &&
                    lp.Lesson.Topic != null &&
                    (lp.Lesson.Topic.IsActive ?? false) &&
                    lp.Lesson.Topic.CourseId == courseId)
                .Select(lp => new
                {
                    AccountId = lp.AccountId!.Value,
                    TopicId = lp.Lesson.TopicId!.Value,
                    LessonId = lp.LessonId!.Value
                })
                .Distinct()
                .ToListAsync();

            // 3) Với mỗi (TopicId, AccountId), đếm số lesson Completed
            //    => nếu bằng tổng lesson active của topic đó => student đã hoàn thành toàn bộ
            var studentFinishByTopic = completedByStudentAndTopic
                .GroupBy(x => new { x.TopicId, x.AccountId })
                .Where(g =>
                {
                    var topicId = g.Key.TopicId;
                    return lessonCountByTopic.TryGetValue(topicId, out var totalLessons)
                           && totalLessons > 0
                           && g.Select(x => x.LessonId).Distinct().Count() == totalLessons;
                })
                .GroupBy(g => g.Key.TopicId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Key.AccountId).Distinct().ToList()
                );

            // 4) Trả danh sách topic + field studentFinish
            var topics = await _dbContext.Topics
                .AsNoTracking()
                .Where(t => t.CourseId == courseId && t.IsActive == true)
                .OrderBy(t => t.OrderIndex)
                .Select(t => new TopicWithFinishDTO
                {
                    Id = t.Id,
                    CourseId = t.CourseId,
                    TopicName = t.TopicName,
                    OrderIndex = t.OrderIndex,
                    Description = t.Description,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    DeleteAt = t.DeleteAt,
                    StudentFinish = new List<Guid>() // set ở dưới
                })
                .ToListAsync();

            foreach (var topic in topics)
            {
                if (studentFinishByTopic.TryGetValue(topic.Id, out var finList))
                {
                    topic.StudentFinish = finList;
                }
                else
                {
                    topic.StudentFinish = new List<Guid>();
                }
            }

            return topics;
        }

        public async Task<int> Update(Topic topic)
        {
            return await _repository.UpdateAsync(topic);
        }

    }
}
