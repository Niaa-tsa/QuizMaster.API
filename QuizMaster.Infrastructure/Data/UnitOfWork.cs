using System;
using System.Collections.Generic;
using System.Text;

namespace QuizMaster.Infrastructure.Data;

using QuizMaster.Service.Interfaces.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly QuizMasterDbContext _context;

    public UnitOfWork(QuizMasterDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}