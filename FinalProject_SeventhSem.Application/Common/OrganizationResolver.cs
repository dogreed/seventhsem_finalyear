using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Common;


public static class OrganizationResolver
{
    public static async Task<Organization> ResolveAsync(
        int userId,
        IRepository<Organization> orgRepo,
        CancellationToken ct = default)
    {
        var orgs = await orgRepo.GetAllAsync(ct);
        var org = orgs.FirstOrDefault(o => o.UserId == userId);
        return org ?? throw new NotFoundException(
            $"No organization profile found for UserId {userId}.");
    }
}
