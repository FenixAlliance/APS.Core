using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FenixAlliance.ABM.Data;
using FenixAlliance.ABM.Data.Access.Interfaces.DataAccess;
using FenixAlliance.ABM.Models.Holders;
using FenixAlliance.ABM.Models.Security.BusinessPermissions;
using FenixAlliance.ABM.Models.Security.BusinessSecurityLogs;
using FenixAlliance.ABM.Models.Tenants;
using FenixAlliance.ABM.Models.Tenants.BusinessProfileRecords;
using FenixAlliance.Data.Access.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FenixAlliance.Data.Access.DataAccess
{
    public partial class BusinessDataAccessClient : IBusinessDataAccess
    {
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly ABMContext _context;
        private BlobStorageDataAccessClient dataTools { get; set; }
        private AccountUsersHelpers AccountTools { get; set; }
        private AccountGraphHelpers AccountGraphTools { get; set; }

        public BusinessDataAccessClient(ABMContext context, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _env = hostingEnvironment;
            AccountTools = new AccountUsersHelpers(context);
            AccountGraphTools = new AccountGraphHelpers(_context, _configuration);
            dataTools = new BlobStorageDataAccessClient();
        }

        public BusinessDataAccessClient(DbContextOptions<ABMContext> dbContextOptions, IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            _context = new ABMContext(dbContextOptions);
            _configuration = configuration;
            _env = hostingEnvironment;
            AccountTools = new AccountUsersHelpers(_context);
            AccountGraphTools = new AccountGraphHelpers(_context, _configuration);
            dataTools = new BlobStorageDataAccessClient();
        }

        public async Task<bool> ResolveRequestedAccessAsync(string HolderGUID, List<string> RequiredPermissions, List<string> RequiredRoles = null)
        {
            var Holder = await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants).ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c=>c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.SelectedBusiness)
                .FirstOrDefaultAsync(c => c.GUID == HolderGUID);

            return ResolveRequestedAccess(Holder, RequiredPermissions, RequiredRoles);
        }

        public async Task<bool> ResolveRequestedAccessAsync(string HolderGUID,string BusinessTenantID, List<string> RequiredPermissions, List<string> RequiredRoles = null)
        {
            var Holder = await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants).ThenInclude(c => c.BusinessSecurityRole).ThenInclude(c => c.BusinessRolePermissionGrants).ThenInclude(c=>c.BusinessPermission)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.SelectedBusiness)
                .FirstOrDefaultAsync(c => c.GUID == HolderGUID);

            return ResolveRequestedAccess(Holder,BusinessTenantID, RequiredPermissions, RequiredRoles);
        }

        public bool ResolveRequestedAccess(AllianceIDHolder Holder, List<string> RequiredPermissions, List<string> RequiredRoles = null)
        {
            bool Authorized = false;
            if (Holder.SelectedBusiness == null)
                return Authorized;

            try
            {
                var CurrentBPR = GetCurrentBusinessProfileRecord(Holder);
                if (CurrentBPR == null || CurrentBPR.IsDisabled)
                    return Authorized;


                var BusinessSecurityRoles = CurrentBPR.BusinessProfileSecurityRoleGrants.Select(c => c.BusinessSecurityRole);


                if (RequiredRoles != null)
                {
                    // If user is granted with permissions, authowized is true.
                    RequiredRoles.ForEach(requestedRole =>
                    {
                        if (BusinessSecurityRoles.Any(c => c.ID == requestedRole))
                            Authorized = true;
                    });
                    // But if user do not owns one of the requested permissions, let's set this back to false.
                    RequiredRoles.ForEach(requestedRole =>
                    {
                        if (!BusinessSecurityRoles.Any(c => c.ID == requestedRole))
                            Authorized = false;
                    });
                }

                var BusinessPermissionRoleRecords = BusinessSecurityRoles.Select(c => c.BusinessRolePermissionGrants).ToList();
                var BusinessPermissionsFromRoles = BusinessPermissionRoleRecords.SelectMany(c => c.Select(m => m.BusinessPermission)).ToList();
                var DirectPermissions = CurrentBPR.BusinessProfileDirectPermissionGrants.Select(c => c.BusinessPermission).ToList();
                List<BusinessPermission> BusinessPermissions = BusinessPermissionsFromRoles.Union(DirectPermissions).ToList();


                if (RequiredPermissions != null)
                {

                    // If user is granted with permissions, authowized is true.
                    RequiredPermissions.ForEach(requestedScope =>
                    {
                        if (BusinessPermissions.Any(c => c.ID == requestedScope))
                            Authorized = true;
                    });

                    // But if user do not owns one of the requested permissions, let's set this back to false.
                    RequiredPermissions.ForEach(requestedScope =>
                    {
                        if (!BusinessPermissions.Any(c => c.ID == requestedScope))
                            Authorized = false;
                    });
                }
            }
            catch
            {
                return false;
            }

            return Authorized;
        }

        public bool ResolveRequestedAccess(AllianceIDHolder Holder, string BusinessTenantID, List<string> RequiredPermissions, List<string> RequiredRoles = null)
        {
            bool Authorized = false;

            try
            {
                var RequestedBPR = GetSpecificBusinessProfileRecord(Holder, BusinessTenantID);
                if (RequestedBPR == null)
                    return Authorized;

                var BusinessSecurityRoles = RequestedBPR.BusinessProfileSecurityRoleGrants.Select(c => c.BusinessSecurityRole);

                if (RequiredRoles != null)
                {
                    // If user is granted with permissions, authowized is true.
                    RequiredRoles.ForEach(requestedRole =>
                    {
                        if (BusinessSecurityRoles.Any(c => c.ID == requestedRole))
                            Authorized = true;
                    });
                    // But if user do not owns one of the requested permissions, let's set this back to false.
                    RequiredRoles.ForEach(requestedRole =>
                    {
                        if (!BusinessSecurityRoles.Any(c => c.ID == requestedRole))
                            Authorized = false;
                    });
                }

                var BusinessPermissionRoleRecords = BusinessSecurityRoles.Select(c => c.BusinessRolePermissionGrants).ToList();
                var BusinessPermissionsFromRoles = BusinessPermissionRoleRecords.SelectMany(c => c.Select(m => m.BusinessPermission))?.ToList() ?? new List<BusinessPermission>();
                var DirectPermissions = RequestedBPR.BusinessProfileDirectPermissionGrants.Select(c => c.BusinessPermission)?.ToList() ?? new List<BusinessPermission>();

                List<BusinessPermission> BusinessPermissions = BusinessPermissionsFromRoles.Union(DirectPermissions).ToList();
                if (RequiredPermissions != null)
                {
                    // If user is granted with permissions, authowized is true.
                    RequiredPermissions.ForEach(requestedScope =>
                    {
                        if (BusinessPermissions.Any(c => c.ID == requestedScope))
                            Authorized = true;
                    });

                    // But if user do not owns one of the requested permissions, let's set this back to false.
                    RequiredPermissions.ForEach(requestedScope =>
                    {
                        if (!BusinessPermissions.Any(c => c.ID == requestedScope))
                            Authorized = false;
                    });
                }

            }
            catch
            {
                // TODO: OnCatch, try async (with DB) or give up.
                return false;
            }

            return Authorized;
        }

        public async Task<List<string>> GetPermissionsListAsync(string HolderID, string BusinessTenantID)
        {
            List<string> GrantedPermissions = new List<string>();
            try
            {
                var RequestedBPR = await GetSpecificBusinessProfileRecordAsync(HolderID, BusinessTenantID);

                if (RequestedBPR == null)
                    return GrantedPermissions;

                var BusinessSecurityRoles = RequestedBPR.BusinessProfileSecurityRoleGrants.Select(c => c.BusinessSecurityRole);



                var BusinessPermissionRoleRecords = BusinessSecurityRoles.Select(c => c.BusinessRolePermissionGrants).ToList();

                var BusinessPermissionsFromRoles = BusinessPermissionRoleRecords.SelectMany(c => c.Select(m => m.BusinessPermission))?.ToList() ?? new List<BusinessPermission>();
                var DirectPermissions = RequestedBPR.BusinessProfileDirectPermissionGrants.Select(c => c.BusinessPermission)?.ToList() ?? new List<BusinessPermission>();

                List<BusinessPermission> BusinessPermissions = BusinessPermissionsFromRoles.Union(DirectPermissions).ToList();

                if (BusinessPermissions != null)
                {
                    // If user is granted with permissions, authowized is true.
                    BusinessPermissions.ForEach(BusinessPermission =>
                    {
                        GrantedPermissions.Add(BusinessPermission.ID);
                    });
                }

            }
            catch
            {
                // TODO: OnCatch, try async (with DB) or give up.
                return GrantedPermissions;
            }

            return GrantedPermissions;
        }

        public async Task<BusinessProfileRecord> GetCurrentBusinessProfileRecordAsync(string HolderGUID)
        {
            var User = await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileSecurityRoleGrants).ThenInclude(c => c.BusinessSecurityRole)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.BusinessProfileDirectPermissionGrants).ThenInclude(c => c.BusinessPermission)
                .Include(c => c.SelectedBusiness)
                .FirstOrDefaultAsync(c => c.GUID == HolderGUID);

            if (User.SelectedBusiness == null)
                return null;

            return User.BusinessProfileRecords.First(c => c.BusinessID == User.SelectedBusiness.ID);
        }

        public async Task<List<BusinessProfileRecord>> GetHolderBusinessProfileRecordsAsync(string HolderGUID)
        {
            return (await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.Country)
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.ActiveUsers)
            .FirstOrDefaultAsync(c => c.GUID == HolderGUID)).BusinessProfileRecords;
        }

        public async Task<List<Business>> GetHolderBusinessesAsync(string HolderGUID)
        {
            return (await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.BusinessProfileRecords).ThenInclude(c => c.Business).ThenInclude(c => c.Country)
            .FirstOrDefaultAsync(c => c.GUID == HolderGUID)).BusinessProfileRecords.Select(c => c.Business).ToList();
        }

        public List<Business> GetHolderBusinesses(AllianceIDHolder Holder)
        {
            return Holder.BusinessProfileRecords.Select(c => c.Business)?.ToList();
        }

        public async Task<Business> GetHolderCurrentBusinessAsync(string HolderGUID)
        {
            return (await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.SelectedBusiness)
            .FirstOrDefaultAsync(c => c.GUID == HolderGUID)).SelectedBusiness;
        }

        public Business GetHolderCurrentBusiness(AllianceIDHolder Holder)
        {
            return Holder.SelectedBusiness;
        }

        public async Task<BusinessProfileRecord> GetSpecificBusinessProfileRecordAsync(string HolderGUID, string BusinessID)
        {
            return (await _context.AllianceIDHolder.AsNoTracking()
                .Include(c => c.BusinessProfileRecords)
            .FirstOrDefaultAsync(c => c.GUID == HolderGUID)).BusinessProfileRecords.FirstOrDefault(c => c.BusinessID == BusinessID);
        }

        public BusinessProfileRecord GetSpecificBusinessProfileRecord(AllianceIDHolder Holder, string BusinessID)
        {
            return Holder.BusinessProfileRecords.First(c => c.BusinessID == BusinessID);
        }

        public BusinessProfileRecord GetCurrentBusinessProfileRecord(AllianceIDHolder Holder)
        {
            if (Holder.SelectedBusiness == null)
                return null;

            return Holder.BusinessProfileRecords.First(c => c.BusinessID == Holder.SelectedBusiness.ID);
        }

        public async Task<List<BusinessSecurityLog>> GetBusinessSecurityLogs(string HolderGUID, string BusinessProfileRecordID, string BusinessID)
        {
            var Response = new List<BusinessSecurityLog>();
            if (!String.IsNullOrEmpty(BusinessID))
            {
                if (!String.IsNullOrEmpty(BusinessProfileRecordID))
                {
                    if (!String.IsNullOrEmpty(HolderGUID))
                    {
                        Response = (await _context.Business
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c=>c.BusinessSecurityLogs)
                            .FirstAsync(c => c.ID == BusinessID))
                            .BusinessProfileRecords.Where(c=>c.ID == BusinessProfileRecordID && c.AllianceIDHolderGUID == HolderGUID)
                            .SelectMany(c=>c.BusinessSecurityLogs).ToList();
                    }
                    else
                    {
                        Response= (await _context.Business
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c=>c.BusinessSecurityLogs)
                            .FirstAsync(c => c.ID == BusinessID))
                            .BusinessProfileRecords.Where(c=>c.ID == BusinessProfileRecordID)
                            .SelectMany(c=>c.BusinessSecurityLogs).ToList();
                    }
                }
                else
                {

                    if (!String.IsNullOrEmpty(HolderGUID))
                    {
                        Response = (await _context.Business
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c=>c.BusinessSecurityLogs)
                            .FirstAsync(c => c.ID == BusinessID))
                            .BusinessProfileRecords.Where(c=> c.AllianceIDHolderGUID == HolderGUID)
                            .SelectMany(c=>c.BusinessSecurityLogs).ToList();
                    }
                    else
                    {
                        Response= (await _context.Business
                            .Include(c => c.BusinessProfileRecords)
                            .ThenInclude(c=>c.BusinessSecurityLogs)
                            .FirstAsync(c => c.ID == BusinessID))
                            .BusinessProfileRecords
                            .SelectMany(c=>c.BusinessSecurityLogs).ToList();
                    }
                }
            }
            return Response;
        }

    }
}
