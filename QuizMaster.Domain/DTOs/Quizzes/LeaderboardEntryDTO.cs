using System;
using System.Collections.Generic;
using System.Text;

namespace QuizMaster.Domain.DTOs.Quizzes;

public class LeaderboardEntryDTO
{
    public int Rank { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public DateTime FinishedAt { get; set; }
}
