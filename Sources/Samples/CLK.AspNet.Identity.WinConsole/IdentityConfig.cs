﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace CLK.AspNet.Identity.WinConsole
{
    // Context
    public partial class ApplicationDbContext
    {
        // Constructors
        static ApplicationDbContext()
        {
            //Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }


        // Methods
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext("DefaultConnection");
        }
    }

    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        // Methods
        protected override void Seed(ApplicationDbContext context)
        {
            // Initialize
            this.InitializeIdentity(context);

            // Base
            base.Seed(context);
        }

        private void InitializeIdentity(ApplicationDbContext context)
        {
            #region Contracts

            if (context == null) throw new ArgumentNullException();

            #endregion

            // Default - User
            const string adminUserName = "admin@hotmail.com";
            const string adminUserPassword = "admin";

            // Default - Role
            const string adminRoleName = "Admin";

            // Default - Permission
            const string aboutPermissionName = "About";
            const string contactPermissionName = "Contact";


            // Manager
            var userManager = HttpContext.Current.GetOwinContext().Get<ApplicationUserManager>();
            var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            var permissionManager = HttpContext.Current.GetOwinContext().Get<ApplicationPermissionManager>();

            // User
            var user = userManager.FindByName(adminUserName);
            if (user == null)
            {
                user = new ApplicationUser { UserName = adminUserName, Email = adminUserName };
                userManager.Create(user, adminUserPassword);
                userManager.SetLockoutEnabled(user.Id, false);
            }

            // Role
            var role = roleManager.FindByName(adminRoleName);
            if (role == null)
            {
                role = new ApplicationRole(adminRoleName);
                roleManager.Create(role);
            }

            // Permission
            var aboutPermission = permissionManager.FindByName(aboutPermissionName);
            if (aboutPermission == null)
            {
                aboutPermission = new ApplicationPermission(aboutPermissionName);
                permissionManager.Create(aboutPermission);
            }

            var contactPermission = permissionManager.FindByName(contactPermissionName);
            if (contactPermission == null)
            {
                contactPermission = new ApplicationPermission(contactPermissionName);
                permissionManager.Create(contactPermission);
            }


            // UserAddToRole 
            var rolesForUser = userManager.GetRoles(user.Id);
            if (rolesForUser.Contains(role.Name) == false)
            {
                userManager.AddToRole(user.Id, role.Name);
            }

            // PermissionAddToRole 
            var rolesForAboutPermission = permissionManager.GetRolesById(aboutPermission.Id);
            if (rolesForAboutPermission.Contains(role.Name) == false)
            {
                permissionManager.AddToRole(aboutPermission.Id, role.Name);
            }

            var rolesForContactPermission = permissionManager.GetRolesById(contactPermission.Id);
            if (rolesForContactPermission.Contains(role.Name) == false)
            {
                permissionManager.AddToRole(contactPermission.Id, role.Name);
            }
        }
    }


    // Manager
    public partial class ApplicationUserManager
    {
        // Methods
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            #region Contracts

            if (options == null) throw new ArgumentNullException();
            if (context == null) throw new ArgumentNullException();

            #endregion

            // 建立使用者管理員
            var userManager = new ApplicationUserManager(context.Get<ApplicationDbContext>());
            if (userManager == null) throw new InvalidOperationException();

            // 設定使用者名稱的驗證邏輯
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // 設定密碼的驗證邏輯
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 5,              // 最小長度
                RequireNonLetterOrDigit = false, // 是否需要一個非字母或是數字
                RequireDigit = false,            // 是否需要一個數字
                RequireLowercase = false,        // 是否需要一個小寫字母
                RequireUppercase = false,        // 是否需要一個大寫字母
            };

            // 設定使用者鎖定詳細資料
            userManager.UserLockoutEnabledByDefault = true;
            userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            userManager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // 註冊雙因素驗證提供者。此應用程式使用手機和電子郵件接收驗證碼以驗證使用者
            // 您可以撰寫專屬提供者，並將它外掛到這裡。
            userManager.RegisterTwoFactorProvider("電話代碼", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "您的安全碼為 {0}"
            });
            userManager.RegisterTwoFactorProvider("電子郵件代碼", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "安全碼",
                BodyFormat = "您的安全碼為 {0}"
            });
            userManager.EmailService = new EmailService();
            userManager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            // 回傳
            return userManager;
        }
    }

    public partial class ApplicationRoleManager
    {
        // Methods
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(context.Get<ApplicationDbContext>());
        }
    }

    public partial class ApplicationPermissionManager
    {
        // Methods
        public static ApplicationPermissionManager Create(IdentityFactoryOptions<ApplicationPermissionManager> options, IOwinContext context)
        {
            return new ApplicationPermissionManager(context.Get<ApplicationDbContext>());
        }
    }

    public partial class ApplicationSignInManager
    {
        // Methods
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync(this.UserManager as ApplicationUserManager);
        }
    }


    // Identity
    public partial class ApplicationUser
    {
        // Methods
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            // 注意 authenticationType 必須符合 CookieAuthenticationOptions.AuthenticationType 中定義的項目
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在這裡新增自訂使用者宣告
            return userIdentity;
        }
    }

    public partial class ApplicationRole
    {
        // Methods

    }

    public partial class ApplicationPermission
    {
        // Methods

    }


    // Service
    public class EmailService : IIdentityMessageService
    {
        // Methods
        public Task SendAsync(IdentityMessage message)
        {
            // 將您的電子郵件服務外掛到這裡以傳送電子郵件。
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        // Methods
        public Task SendAsync(IdentityMessage message)
        {
            // 將您的 SMS 服務外掛到這裡以傳送簡訊。
            return Task.FromResult(0);
        }
    }
}
