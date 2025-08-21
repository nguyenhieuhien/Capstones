// File: Services/QuizService.cs
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.DTO.Answer;
using Services.DTO.Question;
using Services.DTO.Quiz;
using Services.DTO.QuizSubmission;
using Services.IServices;

namespace Services
{
    

    public class QuizService : IQuizService
    {
        private readonly QuizRepository _repository;
        private readonly QuestionRepository _questionRepo;
        private readonly AnswerRepository _answerRepo;
        private readonly LogiSimEduContext _dbContext;

        public QuizService(LogiSimEduContext dbContext)
        {
            _repository = new QuizRepository();
            _questionRepo = new QuestionRepository();
            _answerRepo = new AnswerRepository();
            _dbContext = dbContext;
        }

        public async Task<List<Quiz>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Quiz>();
        }

        public async Task<Quiz?> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Quiz?> GetFullQuizAsync(Guid quizId)
        {
            return await _repository.GetFullQuizAsync(quizId);
        }

        public async Task<List<QuestionWithAnswersDTO>> GetQuestionsWithAnswersByQuizId(Guid quizId)
        {
            var questions = await _questionRepo.GetQuestionsWithAnswersByQuizId(quizId);

            var result = questions.Select(q => new QuestionWithAnswersDTO
            {
                Id = q.Id,
                Description = q.Description,
                Answers = q.Answers.Select(a => new AnswerDTO
                {
                    Id = a.Id,
                    Description = a.Description
                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<List<Quiz>> GetByLessonId(Guid lessonId)
        {
            return await _repository.GetQuizByLessonId(lessonId);
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(Quiz quiz)
        {
            try
            {
                quiz.Id = Guid.NewGuid();
                quiz.CreatedAt = DateTime.UtcNow;
                quiz.IsActive = true;
                var result = await _repository.CreateAsync(quiz);
                if (result > 0)
                    return (true, "Quiz created successfully", quiz.Id);
                return (false, "Failed to create quiz", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> CreateFullQuiz(Quiz dto)
        {
            try
            {
                dto.Id = Guid.NewGuid();
                dto.CreatedAt = DateTime.UtcNow;
                dto.IsActive = true;

                foreach (var question in dto.Questions)
                {
                    question.Id = Guid.NewGuid();
                    question.QuizId = dto.Id;
                    question.IsActive = true;
                    question.CreatedAt = DateTime.UtcNow;

                    int correctCount = question.Answers.Count(a => a.IsCorrect == true);
                    if (correctCount != 1)
                        return (false, "Each question must have exactly one correct answer.");

                    foreach (var answer in question.Answers)
                    {
                        answer.Id = Guid.NewGuid();
                        answer.QuestionId = question.Id;
                        answer.IsActive = true;
                        answer.CreatedAt = DateTime.UtcNow;
                    }
                }

                await _dbContext.Quizzes.AddAsync(dto);
                await _dbContext.SaveChangesAsync();

                return (true, "Quiz with questions and answers created successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateFullQuizAsync(Guid quizId, UpdateFullQuizDTO request)
        {
            var quiz = await _repository.GetFullQuizAsync(quizId);
            if (quiz == null) return (false, "Quiz not found");

            quiz.QuizName = request.QuizName;
            quiz.UpdatedAt = DateTime.UtcNow;

            // Update Questions
            foreach (var qDto in request.Questions)
            {
                var question = qDto.Id.HasValue && qDto.Id.Value != Guid.Empty
                    ? quiz.Questions.FirstOrDefault(x => x.Id == qDto.Id.Value)
                    : null;

                if (question == null) // thêm mới
                {
                    question = new Question
                    {
                        Id = Guid.NewGuid(),
                        Description = qDto.Description,
                        QuizId = quiz.Id,
                        Answers = new List<Answer>()
                    };
                    quiz.Questions.Add(question);
                }
                else // update
                {
                    question.Description = qDto.Description;
                }

                foreach (var aDto in qDto.Answers)
                {
                    var answer = aDto.Id.HasValue && aDto.Id.Value != Guid.Empty
                        ? question.Answers.FirstOrDefault(x => x.Id == aDto.Id.Value)
                        : null;

                    if (answer == null) // thêm mới
                    {
                        question.Answers.Add(new Answer
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = question.Id,
                            Description = aDto.Description,
                            IsCorrect = aDto.IsCorrect
                        });
                    }
                    else // update
                    {
                        answer.Description = aDto.Description;
                        answer.IsCorrect = aDto.IsCorrect;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return (true, "Quiz updated successfully");
        }

        //public async Task<bool> UpdateFullQuizAsync(Quiz quiz)
        //{
        //    var existingQuiz = await _dbContext.Quizzes
        //        .Include(q => q.Questions)
        //            .ThenInclude(q => q.Answers)
        //        .FirstOrDefaultAsync(q => q.Id == quiz.Id);

        //    if (existingQuiz == null) return false;

        //    // Update quiz info
        //    existingQuiz.QuizName = quiz.QuizName;
        //    existingQuiz.UpdatedAt = DateTime.Now;

        //    // Handle questions
        //    foreach (var question in quiz.Questions)
        //    {
        //        var existingQuestion = existingQuiz.Questions.FirstOrDefault(q => q.Id == question.Id);

        //        if (existingQuestion != null) // update question
        //        {
        //            existingQuestion.Description  = question.Description;

        //            // Handle answers
        //            foreach (var answer in question.Answers)
        //            {
        //                var existingAnswer = existingQuestion.Answers.FirstOrDefault(a => a.Id == answer.Id);
        //                if (existingAnswer != null) // update answer
        //                {
        //                    existingAnswer.Description = answer.Description;
        //                    existingAnswer.IsCorrect = answer.IsCorrect;
        //                }
        //                else // add new answer
        //                {
        //                    existingQuestion.Answers.Add(new Answer
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        Description = answer.Description,
        //                        IsCorrect = answer.IsCorrect,
        //                        QuestionId = existingQuestion.Id
        //                    });
        //                }
        //            }
        //        }
        //        else // add new question
        //        {
        //            var newQuestion = new Question
        //            {
        //                Id = Guid.NewGuid(),
        //                Description = question.Description,
        //                QuizId = existingQuiz.Id,
        //                Answers = question.Answers.Select(a => new Answer
        //                {
        //                    Id = Guid.NewGuid(),
        //                    Description = a.Description,
        //                    IsCorrect = a.IsCorrect
        //                }).ToList()
        //            };
        //            existingQuiz.Questions.Add(newQuestion);
        //        }
        //    }

        //    await _dbContext.SaveChangesAsync();
        //    return true;
        //}

        public async Task<List<QuizReviewDTO>> GetQuizReview(Guid accountId, Guid quizId)
        {
            var questions = await _repository.GetQuestionsWithAnswersByQuizId(quizId);
            var submissions = await _repository.GetQuestionSubmissions(accountId, quizId);

            var result = questions.Select(q => new QuizReviewDTO
            {
                QuestionId = q.Id,
                QuestionDescription = q.Description,
                Answers = q.Answers.Select(a => new AnswerReviewDTO
                {
                    AnswerId = a.Id,
                    Description = a.Description,
                    IsCorrect = a.IsCorrect ?? false
                }).ToList(),
                SelectedAnswerId = submissions.FirstOrDefault(s => s.QuestionId == q.Id)?.SelectedAnswerId
            }).ToList();

            return result;
        }

        public async Task<(bool Success, string Message)> Update(Quiz quiz)
        {
            try
            {
                quiz.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(quiz);
                if (result > 0)
                    return (true, "Quiz updated successfully");
                return (false, "Failed to update quiz");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item == null)
                    return (false, "Quiz not found");

                item.IsActive = false;
                item.DeleteAt = DateTime.UtcNow;

                await _repository.UpdateAsync(item);
                return (true, "Deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
