using ERP.Application.Features.Accounts.GetMyProfile;
using ERP.Application.Features.Audit.ListAuditActions;
using ERP.Application.Features.Audit.ListAuditEntityTypes;
using ERP.Application.Features.Audit.ListAuditEntries;
using ERP.Application.Features.Auth.Login;
using ERP.Application.Features.Catalogs.ListCountries;
using ERP.Application.Features.Catalogs.ListCurrencies;
using ERP.Application.Features.Catalogs.ListIgvAffectations;
using ERP.Application.Features.Catalogs.ListInvoicePriceTypes;
using ERP.Application.Features.Catalogs.ListTaxDocumentTypes;
using ERP.Application.Features.Catalogs.ListUnitsOfMeasure;
using ERP.Application.Features.Companies.Activate;
using ERP.Application.Features.Companies.CreateCompany;
using ERP.Application.Features.Companies.Deactivate;
using ERP.Application.Features.Companies.GetCompany;
using ERP.Application.Features.Companies.ListCompanies;
using ERP.Application.Features.Companies.UpdateCompany;
using ERP.Application.Features.Partners.ActivateBusinessPartner;
using ERP.Application.Features.Partners.CreateBusinessPartner;
using ERP.Application.Features.Partners.DeactivateBusinessPartner;
using ERP.Application.Features.Partners.GetBusinessPartner;
using ERP.Application.Features.Partners.ListBusinessPartners;
using ERP.Application.Features.Partners.ListIdentityDocumentTypes;
using ERP.Application.Features.Partners.LookupRuc;
using ERP.Application.Features.Partners.AddBusinessPartnerRole;
using ERP.Application.Features.Partners.FindBusinessPartnerByDocument;
using ERP.Application.Features.Partners.UpdateBusinessPartner;
using ERP.Application.Features.Products.ActivateProduct;
using ERP.Application.Features.Products.CreateProduct;
using ERP.Application.Features.Products.DeactivateProduct;
using ERP.Application.Features.Products.ExportProducts;
using ERP.Application.Features.Products.GetProductImportTemplate;
using ERP.Application.Features.Products.ImportProducts;
using ERP.Application.Features.Products.PreviewProductImport;
using ERP.Application.Features.Products.ProductImport;
using ERP.Application.Features.Products.GetProduct;
using ERP.Application.Features.Products.ListProducts;
using ERP.Application.Features.Products.UpdateProduct;
using ERP.Application.Features.Purchases.CancelPurchase;
using ERP.Application.Features.Purchases.CreatePurchase;
using ERP.Application.Features.Purchases.GetPurchase;
using ERP.Application.Features.Purchases.ListPurchases;
using ERP.Application.Features.Purchases.PreviewPurchase;
using ERP.Application.Features.SavedViews.CreateSavedView;
using ERP.Application.Features.SavedViews.DeleteSavedView;
using ERP.Application.Features.SavedViews.ListSavedViews;
using ERP.Application.Features.SavedViews.UpdateSavedView;
using ERP.Application.Features.Users.ActivateUser;
using ERP.Application.Features.Users.ChangeUserRole;
using ERP.Application.Features.Users.CreateUser;
using ERP.Application.Features.Users.DeactivateUser;
using ERP.Application.Features.Users.EnsureSuperAdmin;
using ERP.Application.Features.Users.GetUser;
using ERP.Application.Features.Users.ListAssignableRoles;
using ERP.Application.Features.Users.ListUsers;
using ERP.Application.Features.Users.ResetUserPassword;
using ERP.Application.Features.Users.UpdateUserProfile;
using ERP.Application.Features.UnitsOfMeasure.ListAllUnitsOfMeasure;
using ERP.Application.Features.UnitsOfMeasure.ToggleUnitOfMeasure;
using ERP.Application.Features.UnitsOfMeasure.UpdateUnitOfMeasure;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Application
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddApplication()
            {
                services
                    .AddScoped<IListCountriesUseCase, ListCountriesUseCase>()
                    .AddScoped<IListCurrenciesUseCase, ListCurrenciesUseCase>()
                    .AddScoped<IListIgvAffectationsUseCase, ListIgvAffectationsUseCase>()
                    .AddScoped<IListTaxDocumentTypesUseCase, ListTaxDocumentTypesUseCase>()
                    .AddScoped<IListUnitsOfMeasureUseCase, ListUnitsOfMeasureUseCase>()
                    .AddScoped<IListAllUnitsOfMeasureUseCase, ListAllUnitsOfMeasureUseCase>()
                    .AddScoped<IUpdateUnitOfMeasureUseCase, UpdateUnitOfMeasureUseCase>()
                    .AddScoped<IActivateUnitOfMeasureUseCase, ActivateUnitOfMeasureUseCase>()
                    .AddScoped<IDeactivateUnitOfMeasureUseCase, DeactivateUnitOfMeasureUseCase>()
                    .AddScoped<IListInvoicePriceTypesUseCase, ListInvoicePriceTypesUseCase>()

                    .AddScoped<ICreateUserUseCase, CreateUserUseCase>()
                    .AddScoped<IListAssignableRolesUseCase, ListAssignableRolesUseCase>()
                    .AddScoped<IListUsersUseCase, ListUsersUseCase>()
                    .AddScoped<IActivateUserUseCase, ActivateUserUseCase>()
                    .AddScoped<IDeactivateUserUseCase, DeactivateUserUseCase>()
                    .AddScoped<IUpdateUserProfileUseCase, UpdateUserProfileUseCase>()
                    .AddScoped<IChangeUserRoleUseCase, ChangeUserRoleUseCase>()
                    .AddScoped<IResetUserPasswordUseCase, ResetUserPasswordUseCase>()
                    .AddScoped<ILoginUseCase, LoginUseCase>()
                    .AddScoped<IGetUserUseCase, GetUserUseCase>()
                    .AddScoped<IEnsureSuperAdminUseCase, EnsureSuperAdminUseCase>()

                    .AddScoped<IGetMyProfileUseCase, GetMyProfileUseCase>()

                    .AddScoped<ICreateCompanyUseCase, CreateCompanyUseCase>()
                    .AddScoped<IListCompaniesUseCase, ListCompaniesUseCase>()
                    .AddScoped<IActivateCompanyUseCase, ActivateCompanyUseCase>()
                    .AddScoped<IDeactivateCompanyUseCase, DeactivateCompanyUseCase>()
                    .AddScoped<IGetCompanyUseCase, GetCompanyUseCase>()
                    .AddScoped<IUpdateCompanyUseCase, UpdateCompanyUseCase>()

                    .AddScoped<ICreateProductUseCase, CreateProductUseCase>()
                    .AddScoped<IListProductsUseCase, ListProductsUseCase>()
                    .AddScoped<IActivateProductUseCase, ActivateProductUseCase>()
                    .AddScoped<IDeactivateProductUseCase, DeactivateProductUseCase>()
                    .AddScoped<IUpdateProductUseCase, UpdateProductUseCase>()
                    .AddScoped<IGetProductUseCase, GetProductUseCase>()
                    .AddScoped<ProductImportPlanner>()
                    .AddScoped<IGetProductImportTemplateUseCase, GetProductImportTemplateUseCase>()
                    .AddScoped<IExportProductsUseCase, ExportProductsUseCase>()
                    .AddScoped<IPreviewProductImportUseCase, PreviewProductImportUseCase>()
                    .AddScoped<IImportProductsUseCase, ImportProductsUseCase>()

                    .AddScoped<ICreateBusinessPartnerUseCase, CreateBusinessPartnerUseCase>()
                    .AddScoped<IListIdentityDocumentTypesUseCase, ListIdentityDocumentTypesUseCase>()
                    .AddScoped<IListBusinessPartnersUseCase, ListBusinessPartnersUseCase>()
                    .AddScoped<IActivateBusinessPartnerUseCase, ActivateBusinessPartnerUseCase>()
                    .AddScoped<IDeactivateBusinessPartnerUseCase, DeactivateBusinessPartnerUseCase>()
                    .AddScoped<IUpdateBusinessPartnerUseCase, UpdateBusinessPartnerUseCase>()
                    .AddScoped<IGetBusinessPartnerUseCase, GetBusinessPartnerUseCase>()
                    .AddScoped<ILookupRucUseCase, LookupRucUseCase>()
                    .AddScoped<IAddBusinessPartnerRoleUseCase, AddBusinessPartnerRoleUseCase>()
                    .AddScoped<IFindBusinessPartnerByDocumentUseCase, FindBusinessPartnerByDocumentUseCase>()

                    .AddScoped<ICreatePurchaseUseCase, CreatePurchaseUseCase>()
                    .AddScoped<IListPurchasesUseCase, ListPurchasesUseCase>()
                    .AddScoped<IGetPurchaseUseCase, GetPurchaseUseCase>()
                    .AddScoped<ICancelPurchaseUseCase, CancelPurchaseUseCase>()
                    .AddScoped<IPreviewPurchaseUseCase, PreviewPurchaseUseCase>()

                    .AddScoped<IListAuditEntriesUseCase, ListAuditEntriesUseCase>()
                    .AddScoped<IListAuditEntityTypesUseCase, ListAuditEntityTypesUseCase>()
                    .AddScoped<IListAuditActionsUseCase, ListAuditActionsUseCase>()

                    .AddScoped<IListSavedViewsUseCase, ListSavedViewsUseCase>()
                    .AddScoped<ICreateSavedViewUseCase, CreateSavedViewUseCase>()
                    .AddScoped<IUpdateSavedViewUseCase, UpdateSavedViewUseCase>()
                    .AddScoped<IDeleteSavedViewUseCase, DeleteSavedViewUseCase>();

                return services;
            }
        }
    }
}
