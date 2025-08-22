using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Users.ListUsers;

public class ListUsersHandler : IRequestHandler<ListUsersQuery, IReadOnlyList<ListUsersResponse>>
{
    private readonly AppDbContext _db;
    public ListUsersHandler(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ListUsersResponse>> Handle(ListUsersQuery request, CancellationToken ct)
    {
        var q = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var text = request.Q.Trim().ToLower();
            q = q.Where(u =>
                (u.FirstName ?? "").ToLower().Contains(text) ||
                (u.LastName  ?? "").ToLower().Contains(text) ||
                u.Email.ToLower().Contains(text));
        }

        if (!string.IsNullOrWhiteSpace(request.Role) && request.Role != "all")
        {
            q = q.Where(u => (u.Role ?? "") == request.Role);
        }

        var page     = request.Page     <= 0 ? 1   : request.Page;
        var pageSize = request.PageSize <= 0 ? 100 : Math.Min(request.PageSize, 200);

        var items = await q
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ThenBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new ListUsersResponse(u.Id, u.FirstName, u.LastName, u.Email, u.Role))
            .ToListAsync(ct);

        return items;
    }
}
