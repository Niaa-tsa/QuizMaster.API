namespace QuizMaster.Service.Interfaces;

public interface IProfilePhotoStorage
{
    Task<string> SaveAsync(string fileName, byte[] content);

  
    Task DeleteAsync(string photoUrl);
}