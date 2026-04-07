using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public class DocumentAuthorizationService
{
    private readonly ApplicationDbContext _context;

    public DocumentAuthorizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        return user?.Role == UserRole.Administrator;
    }

    public async Task<bool> CanUploadToProjectAsync(int userId, int? projectId)
    {
        if (!projectId.HasValue)
        {
            return true;
        }

        if (await IsAdminAsync(userId))
        {
            return true;
        }

        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId.Value);

        if (project == null)
        {
            return false;
        }

        return project.ProjectManagerId == userId || project.ProjectMembers.Any(pm => pm.UserId == userId);
    }

    public async Task<bool> CanViewDocumentAsync(Document document, int userId)
    {
        if (await IsAdminAsync(userId))
        {
            return true;
        }

        if (document.UploadedByUserId == userId)
        {
            return true;
        }

        if (document.ProjectId.HasValue)
        {
            var isProjectMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == document.ProjectId.Value && pm.UserId == userId);

            if (isProjectMember)
            {
                return true;
            }

            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == document.ProjectId.Value);
            if (project?.ProjectManagerId == userId)
            {
                return true;
            }
        }

        return await _context.DocumentShares.AnyAsync(s =>
            s.DocumentId == document.DocumentId &&
            s.SharedWithUserId == userId &&
            s.RevokedDateUtc == null);
    }
}
