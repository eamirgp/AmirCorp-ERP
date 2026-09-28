using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Partners.GetBusinessPartner;
using ERP.Application.Features.Partners.ListBusinessPartners;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class BusinessPartnerQueries : IBusinessPartnerQueries
    {
        private readonly ErpDbContext _context;

        public BusinessPartnerQueries(ErpDbContext context) => _context = context;

        public async Task<PagedResult<ListBusinessPartnersResponseDto>> ListBusinessPartnersAsync(ListBusinessPartnersDto listBusinessPartnersDto)
        {
            var query = _context.BusinessPartners.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(listBusinessPartnersDto.SearchTerm))
            {
                var nameTerm = listBusinessPartnersDto.SearchTerm.ToLower();

                query = query.Where(bp =>
                    bp.DocumentNumber.StartsWith(listBusinessPartnersDto.SearchTerm) ||
                    bp.Name.ToLower().Contains(nameTerm)
                    );
            }

            if (listBusinessPartnersDto.IsActive is not null)
                query = query.Where(bp => bp.IsActive == listBusinessPartnersDto.IsActive);

            if (listBusinessPartnersDto.PartnerRoleFilter == PartnerRoleFilter.Client)
                query = query.Where(bp => bp.IsClient);
            else if (listBusinessPartnersDto.PartnerRoleFilter == PartnerRoleFilter.Supplier)
                query = query.Where(bp => bp.IsSupplier);

            if (listBusinessPartnersDto.IdentityDocumentType is not null)
                query = query.Where(bp => bp.IdentityDocumentType == listBusinessPartnersDto.IdentityDocumentType);

            var totalCount = await query.CountAsync();

            query = (listBusinessPartnersDto.SortBy, listBusinessPartnersDto.SortDescending) switch
            {
                (BusinessPartnerSortBy.Name, false) => query.OrderBy(bp => bp.Name).ThenBy(bp => bp.Id),
                (BusinessPartnerSortBy.Name, true) => query.OrderByDescending(bp => bp.Name).ThenByDescending(bp => bp.Id),
                (BusinessPartnerSortBy.CreatedAt, false) => query.OrderBy(bp => bp.CreatedAt).ThenBy(bp => bp.Id),
                _ => query.OrderByDescending(bp => bp.CreatedAt).ThenByDescending(bp => bp.Id)
            };

            var items = await query
                .Skip((listBusinessPartnersDto.Page - 1) * listBusinessPartnersDto.PageSize)
                .Take(listBusinessPartnersDto.PageSize)
                .Select(bp => new ListBusinessPartnersResponseDto(
                    bp.Id,
                    bp.IdentityDocumentType,
                    bp.DocumentNumber,
                    bp.Country,
                    bp.Name,
                    bp.IsClient,
                    bp.IsSupplier,
                    bp.IsActive,
                    bp.CreatedAt,
                    _context.Users.Where(u => u.Id == bp.CreatedBy).Select(u => u.Name).FirstOrDefault()
                    ))
                .ToArrayAsync();

            return new PagedResult<ListBusinessPartnersResponseDto>(
                items,
                listBusinessPartnersDto.Page,
                listBusinessPartnersDto.PageSize,
                totalCount
                );
        }

        public async Task<GetBusinessPartnerResponseDto?> GetBusinessPartnerAsync(GetBusinessPartnerDto getBusinessPartnerDto) =>
            await _context.BusinessPartners
            .AsNoTracking()
            .Where(bp => bp.Id == getBusinessPartnerDto.Id)
            .Select(bp => new GetBusinessPartnerResponseDto(
                bp.Id,
                bp.IdentityDocumentType,
                bp.DocumentNumber,
                bp.Country,
                bp.Name,
                bp.IsClient,
                bp.IsSupplier,
                bp.IsActive,
                bp.CreatedAt,
                _context.Users.Where(u => u.Id == bp.CreatedBy).Select(u => u.Name).FirstOrDefault(),
                bp.UpdatedAt,
                _context.Users.Where(u => u.Id == bp.UpdatedBy).Select(u => u.Name).FirstOrDefault()
                ))
            .FirstOrDefaultAsync();
    }
}
