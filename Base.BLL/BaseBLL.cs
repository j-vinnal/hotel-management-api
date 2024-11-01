﻿using Base.Contracts.BLL;
using Base.Contracts.DAL;

namespace Base.BLL;

public abstract class BaseBLL<TUOW> : IBaseBLL
    where TUOW : IUnitOfWork
{
    protected readonly TUOW Uow;

    protected BaseBLL(TUOW uow)
    {
        Uow = uow;
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await Uow.SaveChangesAsync();
    }
}