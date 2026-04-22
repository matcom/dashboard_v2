namespace Dashboard_v2.Application.Authors;

public record AuthorSearchDto(string Id, string Name);

public record CoauthorSearchDto(string Id, string Name, string Type);

public record PotentialAuthorMatchDto(string Id, string Name);

public record PotentialAuthorMatchesDto(
	IReadOnlyList<PotentialAuthorMatchDto> ExactMatches,
	IReadOnlyList<PotentialAuthorMatchDto> FuzzyMatches);
