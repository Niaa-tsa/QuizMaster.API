using QuizMaster.Domain.Entities;

namespace QuizMaster.Service.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}