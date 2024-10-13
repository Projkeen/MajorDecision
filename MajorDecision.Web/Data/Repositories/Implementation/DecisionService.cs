using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using Newtonsoft.Json;
using System.Security.Claims;

namespace MajorDecision.Web.Data.Repositories.Implementation
{
    public class DecisionService : IDecision
    {
        private readonly ApplicationDbContext _db;

        public DecisionService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Decision> ShowAnswerByFirstMethodAsync(Decision decision)
        {
            //int secretNumber = decision.SecretMethod();
            int secretNumber = DateTime.Now.Second * decision.Question.Length;
            Random random = new Random();
            int rnd = random.Next(100);
            //decision.Answer = str.Length.ToString();                   
            //int ans = int.Parse(decision.Question.Length.ToString());           
            if ((secretNumber % 2 == 0) && (rnd % 2 == 0))
            {
                decision.Answer = "yes";
            }
            else if ((secretNumber % 2 != 0) && (rnd % 2 != 0))
            {
                decision.Answer = "yes";
            }
            else
            {
                decision.Answer = "no";
            }
            decision.DateOfQuestion = DateTime.Now;
            return decision;
        }

        public async Task<Decision> ShowAnswerBySecondMethodAsync(Decision decision)
        {
            //Decision decision = new Decision();
            //string answersPath = $"{Environment.CurrentDirectory}\\Answers.json";
            //string[] Answers;
            //Random random = new Random();

            //var file = System.IO.File.ReadAllText(answersPath);
            //Answers = JsonConvert.DeserializeObject<string[]>(file);
            //int index = random.Next(Answers.Length);
            //decision.Answer = Answers[index];
            //decision.DateOfQuestion = DateTime.Now;
            //return decision;    

            var randomAnswerFromDb = _db.Answers.OrderBy(x => Guid.NewGuid()).Select(x => x.Answer).FirstOrDefault();
            decision.Answer = randomAnswerFromDb.ToString();
            decision.DateOfQuestion = DateTime.Now;
            return decision;

        }
    }
}
