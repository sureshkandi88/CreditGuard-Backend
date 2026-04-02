using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreditGuard.Core.Entities;

namespace CreditGuard.Core.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(string username, string phone, string firstName, string lastName, string address, string password);
    Task<string> LoginAsync(string username, string password);
}

public interface IWalletService
{
    Task<long> GetBalanceAsync(string creditorId);
    Task AddMoneyAsync(string creditorId, long amount, string? reason);
    Task<bool> DebitMoneyAsync(string creditorId, long amount, string? reason);
    Task CreditMoneyAsync(string creditorId, long amount, string? reason);
    Task<IEnumerable<WalletTransaction>> GetTransactionsAsync(string creditorId);
}

public interface ICustomerService
{
    Task<Customer> CreateCustomerAsync(string aadhaar, string firstName, string lastName, string phone, string address, string? photoId);
    Task<Customer?> GetCustomerAsync(string customerId);
    Task<IEnumerable<Customer>> SearchCustomersAsync(string query);
    Task<bool> IsCustomerInActiveGroupAsync(string customerId);
}

public interface IGroupService
{
    Task<CustomerGroup> CreateGroupAsync(string name, string location, string groupLeaderId, string? groupPhotoId);
    Task<CustomerGroup?> GetGroupAsync(string groupId);
    Task<IEnumerable<CustomerGroup>> GetAllGroupsAsync();
    Task AddMemberToGroupAsync(string groupId, string customerId, double ratio);
    Task RemoveMemberFromGroupAsync(string groupId, string customerId);
    Task DeactivateGroupAsync(string groupId);
    Task ActivateGroupAsync(string groupId);
}

public interface ICreditService
{
    Task<Credit> CreateCreditAsync(string groupId, long totalPrincipal, string creditorId, Dictionary<string, double>? memberRatios);
    Task<Credit?> GetCreditAsync(string creditId);
    Task<IEnumerable<Credit>> GetActiveCreditsAsync();
    Task CloseCreditEarlyAsync(string creditId);
}

public interface ICollectionService
{
    Task<DailyCollection> RecordCollectionAsync(string creditId, long amountPaid, string collectedBy, string? notes, bool isAdvancePayment);
    Task<IEnumerable<DailyCollection>> GetTodaysCollectionsAsync();
    Task<IEnumerable<DailyCollection>> GetCollectionsByCreditAsync(string creditId);
    Task<(long TotalCollectedToday, long TotalExpectedToday)> GetDashboardCollectionStatsAsync();
}

public interface IBlobService
{
    Task<string> SaveBlobAsync(byte[] data, string contentType);
    Task<Blob?> GetBlobAsync(string blobId);
}
