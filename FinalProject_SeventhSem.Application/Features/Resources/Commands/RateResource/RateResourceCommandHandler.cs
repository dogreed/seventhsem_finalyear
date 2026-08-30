using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Resources.Commands.RateResource;

public class RateResourceCommandHandler : IRequestHandler<RateResourceCommand>
{
    private readonly IRepository<Resource> _resourceRepo;
    private readonly IRepository<ResourceRating> _ratingRepo;
    private readonly IUnitOfWork _uow;

    public RateResourceCommandHandler(
        IRepository<Resource> resourceRepo,
        IRepository<ResourceRating> ratingRepo,
        IUnitOfWork uow)
    {
        _resourceRepo = resourceRepo;
        _ratingRepo = ratingRepo;
        _uow = uow;
    }

    public async Task Handle(RateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepo.GetByIdAsync(request.ResourceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resource), request.ResourceId);

        // One rating per student per resource
        var existing = (await _ratingRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(r => r.ResourceId == request.ResourceId && r.StudentId == request.StudentId);

        if (existing is not null)
            throw new ConflictException("You have already rated this resource.");

        await _ratingRepo.AddAsync(new ResourceRating
        {
            ResourceId = request.ResourceId,
            StudentId = request.StudentId,
            Rating = request.Rating,
            Comment = request.Comment,
            RatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
    }
}


