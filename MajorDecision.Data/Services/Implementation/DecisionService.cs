using MajorDecision.Data;
using MajorDecision.Data.Dto;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Security.Claims;

namespace MajorDecision.Data.Services.Implementation
{
    public class DecisionService : IDecisionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFriendshipService _friendshipService;

        public DecisionService(ApplicationDbContext db, IFriendshipService friendshipService)
        {
            _db = db;
            _friendshipService = friendshipService;
        }

        public async Task<Decision> ShowAnswerAsync(string request, string lucky, ApplicationUser? currentUser)
        {
            if (string.IsNullOrWhiteSpace(request))
                throw new ArgumentException("Question is required", nameof(request));

            var decision = new Decision
            {
                Question = request,
                DateOfQuestion = DateTime.UtcNow
            };
            if (lucky == "answer")
            {
                decision.Answer = await ShowAnswerFromDbAsync();
            }
            else
            {
                decision.Answer = ShowRandomAnswer(decision.Question);
            }
            
            if (currentUser != null)
            {
                decision.ApplicationUserId = currentUser.Id;
                var answersWithAppUserId = _db.Decisions.Where(x => x.ApplicationUserId != null & x.ApplicationUserId != currentUser.Id).ToList();
                var checkSameQuestions = answersWithAppUserId.Where(i => i.Question == decision.Question).ToList();
                var questionsFromDiscussion = _db.DiscussionPages.Where(x => x.Question == decision.Question).ToList();
                var findReceiverIds = checkSameQuestions.Select(i => i.ApplicationUserId).Distinct().ToList();
                if (questionsFromDiscussion != null & questionsFromDiscussion.ToString().ToLower() == decision.Question.ToLower())
                {
                    //decision.ApplicationUserId = currentUser.Id;
                    _db.Decisions.Add(decision);
                    await _db.SaveChangesAsync();
                }
                if (findReceiverIds.Count > 0)
                {
                    await _friendshipService.UndergroundMethod(currentUser.Id, findReceiverIds);
                }
                _db.Decisions.Add(decision);
                await _db.SaveChangesAsync();
            }
            else
            {
                _db.Decisions.Add(decision);
                await _db.SaveChangesAsync();
            }
            return decision;            

        }
        private static string ShowRandomAnswer(string question)
        {
            int secretNumber = DateTime.Now.Second * question.Length;
            Random random = new Random();
            int rnd = random.Next(100);
            string ans = null;
            //decision.Answer = str.Length.ToString();                   
            //int ans = int.Parse(decision.Question.Length.ToString());           
            if (secretNumber % 2 == 0 && rnd % 2 == 0)
            {
                return ans = "yes";
            }
            else if (secretNumber % 2 != 0 && rnd % 2 != 0)
            {
                return ans = "yes";
            }
            else
            {
                return ans = "no";
            }
        }

        private async Task<string> ShowAnswerFromDbAsync()
        {
            var randomAnswerFromDb = await _db.Answers.OrderBy(x => Guid.NewGuid()).Select(x => x.Answer).FirstOrDefaultAsync();
            return randomAnswerFromDb;
        }

        public IQueryable<DecisionVM> GetAllAsync()
        {
            var decisions = _db.Decisions.Select(d => new DecisionVM
            {
                Id = d.Id,
                Question = d.Question,
                Answer = d.Answer,
                DateOfQuestion = d.DateOfQuestion,
                ApplicationUser = d.ApplicationUser,
                ApplicationUserId = d.ApplicationUserId,
            });

            return decisions;
        }

        public async Task<int> DeleteAsync(IEnumerable<int> decisionIds, string userId)
        {
            var toDelete = await _db.Decisions.Where(d => decisionIds.Contains(d.Id) && d.ApplicationUserId == userId).ToListAsync();

            if (toDelete.Count == 0)
                return 0;

            _db.Decisions.RemoveRange(toDelete);
            await _db.SaveChangesAsync();
            return toDelete.Count;
        }

        public async Task DeleteAllAsync(string userId)
        {
            var decisions = _db.Decisions.Where(d => d.ApplicationUserId == userId);
            if(decisions.Count() != 0)
            {
                _db.Decisions.RemoveRange(decisions);
                await _db.SaveChangesAsync();
            }   
        }

        public async Task<PaginatedList<DecisionVM>> GetHistoryAsync(string userId, int pageNumber, string? searchString, int pageSize = 13)
        {
            if (pageNumber < 1) pageNumber = 1;

            var query = _db.Decisions.Where(d => d.ApplicationUserId == userId).OrderByDescending(d => d.DateOfQuestion).Select(d => new DecisionVM
            {
                Id = d.Id,
                Question = d.Question,
                Answer = d.Answer,
                DateOfQuestion = d.DateOfQuestion,
                ApplicationUserId = d.ApplicationUserId
            });

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.Answer.Contains(searchString) || d.Question.Contains(searchString) || d.DateOfQuestion.ToString().Contains(searchString));
            }

            return await PaginatedList<DecisionVM>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<List<Decision>> GetForDownloadAsync(string userId)
        {
            return await _db.Decisions.Where(d => d.ApplicationUserId == userId).OrderByDescending(d => d.DateOfQuestion).ToListAsync();
        }

        public async Task<List<string>> GetLastQuestionsAsync(int count = 20)
        {
            return await _db.Decisions.OrderByDescending(d => d.DateOfQuestion).Take(count).Select(d => d.Question).ToListAsync();
        }
    }
}
