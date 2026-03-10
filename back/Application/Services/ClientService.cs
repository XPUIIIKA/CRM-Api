using Application.Abstractions.Persistence;
using Application.Abstractions.Services.Entities;
using Application.Abstractions.Services.Utils;
using Application.DTOs.Client;
using Application.Services.Helpers;
using Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class ClientService(ICrmDbContext context, ICurrentUserContext userContext) : IClientService
{
    public async Task<ErrorOr<Guid>> CreateAsync(
        CreateClientRequest request,
        CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        
        if (guard.IsError)
            return guard.Errors;
        
        var (userId, companyId) = guard.Value;
        
        var companyExists = await context.Companies
            .AnyAsync(x => x.Id == companyId, ct);

        if (!companyExists)
        {
            return Error.NotFound(
                code: "Company.NotFound",
                description: "Company does not exist"
            );
        }

        var normalizedData = ValidateAndNormalizeClientData(
            request.FirstName,
            request.Surname,
            request.Patronymic,
            request.Phone,
            request.Email,
            request.Address);

        if (normalizedData.IsError)
        {
            return normalizedData.Errors;
        }

        var data = normalizedData.Value;

        if (!string.IsNullOrWhiteSpace(data.Email))
        {
            var emailExists = await context.Clients.AnyAsync(
                x => x.CompanyId == companyId &&
                     x.Email.ToLower() == data.Email.ToLower(),
                ct);

            if (emailExists)
            {
                return Error.Conflict(
                    code: "Client.EmailExists",
                    description: "Client with this email already exists"
                );
            }
        }
        
        var client = new Client(
            companyId: companyId,
            createdBy: userId
        );

        client.UpdatePersonalData(
            data.FirstName,
            data.Surname,
            data.Patronymic,
            data.Phone,
            data.Email,
            data.Address);
        
        context.Clients.Add(client);
        await context.SaveChangesAsync(ct);

        return client.Id;
    }

    public async Task<ErrorOr<List<ClientResponse>>> GetAllAsync(CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError)
        {
            return guard.Errors;
        }

        var (_, companyId) = guard.Value;

        var clients = await context.Clients
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ClientResponse
            {
                Id = x.Id,
                FirstName = x.FirstName,
                Surname = x.Surname,
                Patronymic = x.Patronymic,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return clients;
    }

    public async Task<ErrorOr<ClientResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError)
        {
            return guard.Errors;
        }

        var (_, companyId) = guard.Value;

        var client = await context.Clients
            .AsNoTracking()
            .Where(x => x.Id == id && x.CompanyId == companyId)
            .Select(x => new ClientResponse
            {
                Id = x.Id,
                FirstName = x.FirstName,
                Surname = x.Surname,
                Patronymic = x.Patronymic,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (client is null)
        {
            return Error.NotFound("Client.NotFound", "Client was not found.");
        }

        return client;
    }

    public async Task<ErrorOr<Updated>> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError)
        {
            return guard.Errors;
        }

        var (_, companyId) = guard.Value;

        var client = await context.Clients
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);

        if (client is null)
        {
            return Error.NotFound("Client.NotFound", "Client was not found.");
        }

        var normalizedData = ValidateAndNormalizeClientData(
            request.FirstName,
            request.Surname,
            request.Patronymic,
            request.Phone,
            request.Email,
            request.Address);

        if (normalizedData.IsError)
        {
            return normalizedData.Errors;
        }

        var data = normalizedData.Value;

        if (!string.IsNullOrWhiteSpace(data.Email))
        {
            var emailExists = await context.Clients.AnyAsync(
                x => x.CompanyId == companyId &&
                     x.Id != id &&
                     x.Email.ToLower() == data.Email.ToLower(),
                ct);

            if (emailExists)
            {
                return Error.Conflict("Client.EmailExists", "Client with this email already exists.");
            }
        }

        client.UpdatePersonalData(
            data.FirstName,
            data.Surname,
            data.Patronymic,
            data.Phone,
            data.Email,
            data.Address);

        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var guard = CurrentUserGuard.EnsureUserAndCompany(userContext);
        if (guard.IsError)
        {
            return guard.Errors;
        }

        var (_, companyId) = guard.Value;

        var client = await context.Clients
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);

        if (client is null)
        {
            return Error.NotFound("Client.NotFound", "Client was not found.");
        }

        var ordersWithClient = await context.Orders
            .Where(x => x.CompanyId == companyId && x.ClientId == id)
            .ToListAsync(ct);

        foreach (var order in ordersWithClient)
        {
            order.RemoveClient();
        }

        context.Clients.Remove(client);
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }

    private static ErrorOr<(string FirstName, string Surname, string Patronymic, string Phone, string Email, string Address)> ValidateAndNormalizeClientData(
        string firstName,
        string surname,
        string patronymic,
        string phone,
        string email,
        string address)
    {
        var normalizedFirstName = firstName?.Trim() ?? string.Empty;
        var normalizedSurname = surname?.Trim() ?? string.Empty;
        var normalizedPatronymic = patronymic?.Trim() ?? string.Empty;
        var normalizedPhone = phone?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedAddress = address?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedFirstName))
        {
            return Error.Validation("Client.FirstName.Empty", "Client first name cannot be empty.");
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail) && !InputValidation.IsValidEmail(normalizedEmail))
        {
            return Error.Validation("Client.Email.Invalid", "Invalid email format.");
        }

        if (!InputValidation.IsValidPhone(normalizedPhone))
        {
            return Error.Validation("Client.Phone.Invalid", "Invalid phone format.");
        }

        return (normalizedFirstName, normalizedSurname, normalizedPatronymic, normalizedPhone, normalizedEmail, normalizedAddress);
    }

}
