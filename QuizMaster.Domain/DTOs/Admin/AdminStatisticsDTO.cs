
namespace QuizMaster.Domain.DTOs.Admin;

public class AdminStatisticsDTO
{
    public int TotalUsers { get; set; }

    public int TotalStudents { get; set; }

    public int TotalLecturers { get; set; }

    public int PendingLecturers { get; set; }

    public int TotalQuizzes { get; set; }
}
