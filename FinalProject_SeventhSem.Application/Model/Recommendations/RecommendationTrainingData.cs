
namespace FinalProject_SeventhSem.Application.Model.Recommendations
{
	public record RecommendationTrainingData(
	int ResourceId,
	int MatchingSkills,
	int ResourceSkillCount,
	double MatchPercentage,
	int Recommended
);
}
