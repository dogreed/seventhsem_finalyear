using FinalProject_SeventhSem.Application.Models.Vacancies;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancy;

public record GetVacancyQuery() : IRequest<IReadOnlyList<VacancyResponse>>;
