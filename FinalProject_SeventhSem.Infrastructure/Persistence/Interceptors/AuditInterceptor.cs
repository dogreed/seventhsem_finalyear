using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Domain.Common;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditInterceptor(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        AuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditEntries(DbContext? context)
    {
        if (context is null) return;

        var auditEntries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added
                              or EntityState.Modified
                              or EntityState.Deleted)
            .ToList();

        foreach (var entry in auditEntries)
        {
            if (entry.Entity is AuditLog) continue;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted => AuditAction.Deleted,
                _ => (AuditAction?)null
            };

            if (action is null) continue;

            string? oldValues = null;
            string? newValues = null;

            if (action == AuditAction.Updated || action == AuditAction.Deleted)
            {
                var original = entry.Properties
                    .Where(p => p.IsModified || action == AuditAction.Deleted)
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                oldValues = JsonSerializer.Serialize(original);
            }

            if (action == AuditAction.Updated || action == AuditAction.Created)
            {
                var current = entry.Properties
                    .Where(p => p.IsModified || action == AuditAction.Created)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValues = JsonSerializer.Serialize(current);
            }

            int? userId = _currentUser.IsAuthenticated ? _currentUser.UserId : null;

            context.Set<AuditLog>().Add(new AuditLog
            {
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = entry.Entity.Id,
                Action = action.Value,
                ChangedByUserId = userId,
                OldValuesJson = oldValues,
                NewValuesJson = newValues,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

