using MediatR;
using Microsoft.AspNetCore.Http;

namespace PentaBoard.Api.Features.Files.UploadFiles;

public record UploadProjectFileCommand(Guid ProjectId, IFormFile File, Guid UploadedById)
    : IRequest<ProjectFileCreatedDto>;

public record ProjectFileCreatedDto(Guid Id, string FileName, string ContentType, long SizeBytes, DateTime CreatedAt);
