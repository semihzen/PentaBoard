using MediatR;

namespace PentaBoard.Api.Features.Files.ListFiles;

public record ListProjectFilesQuery(Guid ProjectId) : IRequest<IReadOnlyList<ProjectFileDto>>;

public record ProjectFileDto(Guid Id, string FileName, string ContentType, long SizeBytes, DateTime CreatedAt);
