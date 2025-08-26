using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Members.GetMember;

public sealed class GetMemberHandler
{
    private readonly AppDbContext _db;
    public GetMemberHandler(AppDbContext db) => _db = db;

    public async Task<IResult> Handle(GetMemberCommand cmd, CancellationToken ct)
    {
        // ——— Proje + Owner
        var proj = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == cmd.ProjectId)
            .Select(p => new { p.Id, p.ProjectAdminId })
            .FirstOrDefaultAsync(ct);

        if (proj is null) return Results.NotFound("Project not found.");

        // ——— İstek sahibi
        var me = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == cmd.RequestorId)
            .Select(u => new { u.Id, u.Role })
            .FirstOrDefaultAsync(ct);

        if (me is null) return Results.Unauthorized();

        var role = (me.Role ?? string.Empty).Trim().ToLowerInvariant();
        var isAdmin = role == "admin";
        var isSystemAdmin = role == "system admin";
        var isOwner = proj.ProjectAdminId == cmd.RequestorId;

        // Üye mi?
        var isMember = await _db.ProjectMembers
            .AsNoTracking()
            .AnyAsync(m => m.ProjectId == cmd.ProjectId && m.UserId == cmd.RequestorId, ct);

        if (!(isAdmin || isSystemAdmin || isOwner || isMember))
            return Results.Forbid();

        // ——— Owner bilgisi
        var ownerUser = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == proj.ProjectAdminId)
            .Select(u => new OwnerDto(u.Id, u.FirstName, u.LastName, u.Email))
            .FirstOrDefaultAsync(ct);

        // ——— ProjectMembers + Users join
        var members = await (
            from m in _db.ProjectMembers.AsNoTracking()
            join u in _db.Users.AsNoTracking() on m.UserId equals u.Id
            where m.ProjectId == cmd.ProjectId
            select new MemberDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                m.SubRole ?? string.Empty
            )
        ).ToListAsync(ct);

        // Owner da listede yoksa ekleyelim (en başa)
        if (ownerUser is not null && !members.Any(x => x.UserId == ownerUser.Id))
        {
            members.Insert(0, new MemberDto(
                ownerUser.Id,
                ownerUser.FirstName,
                ownerUser.LastName,
                ownerUser.Email,
                "owner"
            ));
        }

        // Owner her zaman en başta kalsın; kalanlar ada göre
        members = members
            .OrderByDescending(m => m.UserId == proj.ProjectAdminId)
            .ThenBy(m => m.FirstName)
            .ThenBy(m => m.LastName)
            .ToList();

        return Results.Ok(new GetMemberResponse(members));
    }

    // Cevap/DTO’lar
    public sealed record OwnerDto(Guid Id, string FirstName, string LastName, string Email);

    public sealed record MemberDto(
        Guid UserId,
        string FirstName,
        string LastName,
        string Email,
        string SubRole
    );

    public sealed record GetMemberResponse(System.Collections.Generic.IReadOnlyList<MemberDto> Items);
}
