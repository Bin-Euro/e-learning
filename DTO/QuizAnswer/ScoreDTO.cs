namespace Cursus.DTO.QuizAnswer
{
    public class ScoreDTO
    {
        public Guid quizID { get; set; }
        public Guid userID { get; set; }
        public double score { get; set; }
    }
}
