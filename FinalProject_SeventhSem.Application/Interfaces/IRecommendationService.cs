using FinalProject_SeventhSem.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces
{
	public interface IRecommendationService
	{
		Task<double> PredictScoreAsync(
			RecommendationFeatures features,
			CancellationToken cancellationToken = default);
	}
}
