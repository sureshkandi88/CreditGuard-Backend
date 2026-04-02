using System.Threading.Tasks;
using CreditGuard.Core.Entities;
using CreditGuard.Core.Interfaces;
using CreditGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CreditGuard.Core.Utilities;

namespace CreditGuard.Infrastructure.Services;

public class BlobService : IBlobService
{
    private readonly BlobDbContext _context;

    public BlobService(BlobDbContext context)
    {
        _context = context;
    }

    public async Task<string> SaveBlobAsync(byte[] data, string contentType)
    {
        var blob = new Blob
        {
            Data = data,
            ContentType = contentType,
            CreatedAt = DateHelper.GetCurrentUnixTimeSeconds()
        };

        _context.Blobs.Add(blob);
        await _context.SaveChangesAsync();
        return blob.Id;
    }

    public async Task<Blob?> GetBlobAsync(string blobId)
    {
        return await _context.Blobs.FirstOrDefaultAsync(b => b.Id == blobId);
    }
}
