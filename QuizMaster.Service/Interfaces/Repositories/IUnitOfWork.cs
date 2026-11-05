using System;
using System.Collections.Generic;
using System.Text;

namespace QuizMaster.Service.Interfaces.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}