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

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await context.Clients.AnyAsync(
                x => x.CompanyId == companyId &&
                     x.Email == request.Email,
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

        client.UpdatePersonalData(request.FirstName, request.Surname, request.Patronymic,  request.Phone, request.Email);
        
        context.Clients.Add(client);
        await context.SaveChangesAsync(ct);

        return client.Id;
    }
}
