using MediatR;

namespace PentaBoard.Api.Features.Files.DownloadFiles;

public record DownloadProjectFileQuery(Guid ProjectId, Guid Id) : IRequest<DownloadFileResult>;

public record DownloadFileResult(string AbsPath, string ContentType, string FileName);
