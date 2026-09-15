using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Model;
using System.Net.Http.Json;

namespace FinalProject_SeventhSem.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
	private readonly HttpClient _httpClient;

	public RecommendationService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<double> PredictScoreAsync(
		RecommendationFeatures features,
		CancellationToken cancellationToken = default)
	{
		var request = new RecommendationRequest(
			features.MatchingSkills,
			features.ResourceSkillCount,
			features.MatchPercentage);

		var response = await _httpClient.PostAsJsonAsync(
			"predict",
			request,
			cancellationToken);

		response.EnsureSuccessStatusCode();

		var result = await response.Content
			.ReadFromJsonAsync<RecommendationResponse>(cancellationToken);

		return result?.score ?? 0.0;
	}

	private record RecommendationRequest(
		int matchingSkills,
		int resourceSkillCount,
		double matchPercentage);

	private record RecommendationResponse(
		bool recommended,
		double score);
}