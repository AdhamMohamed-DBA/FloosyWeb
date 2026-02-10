using Blazored.LocalStorage;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using FloosyWeb.Models;
using Microsoft.Extensions.Configuration;

namespace FloosyWeb;

public class FirebaseService
{
    private readonly string ApiKey;
    private readonly string DbUrl;
    private readonly string AuthDomain;

    private readonly FirebaseAuthClient auth;
    private readonly FirebaseClient db;
    private readonly ISyncLocalStorageService _localStorage;

    public string? CurrentUserId { get; private set; }
    public string? UserEmail { get; private set; }

    public FirebaseService(ISyncLocalStorageService localStorage, IConfiguration config)
    {
        _localStorage = localStorage;

        // قراءة الإعدادات من appsettings.json للأمان
        ApiKey = config["Firebase:ApiKey"] ?? "";
        DbUrl = config["Firebase:DbUrl"] ?? "";
        AuthDomain = config["Firebase:AuthDomain"] ?? "";

        var authConfig = new FirebaseAuthConfig
        {
            ApiKey = ApiKey,
            AuthDomain = AuthDomain,
            Providers = [new EmailProvider()]
        };

        auth = new FirebaseAuthClient(authConfig);

        // ربط الـ Database بالـ Token لضمان عمل الـ Rules الجديدة
        db = new FirebaseClient(DbUrl, new FirebaseOptions
        {
            AuthTokenAsyncFactory = async () =>
            {
                if (auth.User != null) return await auth.User.GetIdTokenAsync();
                return null;
            }
        });
    }

    public void InitUser()
    {
        try
        {
            var savedId = _localStorage.GetItem<string>("UserId");
            var savedEmail = _localStorage.GetItem<string>("UserEmail");
            if (!string.IsNullOrEmpty(savedId)) { CurrentUserId = savedId; UserEmail = savedEmail; }
        }
        catch { }
    }

    public async Task<string> Login(string email, string pass)
    {
        try
        {
            var userCred = await auth.SignInWithEmailAndPasswordAsync(email, pass);
            SetUser(userCred.User.Uid, email);

            if (!string.IsNullOrEmpty(userCred.User.Info.DisplayName))
            {
                _localStorage.SetItem("UserName", userCred.User.Info.DisplayName);
            }
            return "Success";
        }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    public async Task<string> Register(string name, string email, string pass)
    {
        try
        {
            var userCred = await auth.CreateUserWithEmailAndPasswordAsync(email, pass);
            await userCred.User.ChangeDisplayNameAsync(name);
            SetUser(userCred.User.Uid, email);
            _localStorage.SetItem("UserName", name);

            await Save(new AppData { Accounts = [new Account { Name = "Cash", Balance = 0 }] });
            return "Success";
        }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    // ✅ تصحيح خطأ GetUserName المفقود
    public string GetUserName()
    {
        var localName = _localStorage.GetItem<string>("UserName");
        if (!string.IsNullOrEmpty(localName)) return localName;
        return auth?.User?.Info?.DisplayName ?? "User";
    }

    // ✅ تصحيح خطأ ResetPassword المفقود
    public async Task<string> ResetPassword(string email)
    {
        try { await auth.ResetEmailPasswordAsync(email); return "Success"; }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    // ✅ تصحيح خطأ UpdateEmail المفقود
    public async Task<string> UpdateEmail(string newEmail)
    {
        if (auth?.User == null) return "Not Logged In";
        try
        {
            UserEmail = newEmail;
            _localStorage.SetItem("UserEmail", newEmail);
            return "Success";
        }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    private void SetUser(string uid, string email)
    {
        CurrentUserId = uid;
        UserEmail = email;
        _localStorage.SetItem("UserId", uid);
        _localStorage.SetItem("UserEmail", email);
    }

    public void SignOut()
    {
        auth?.SignOut();
        CurrentUserId = null;
        UserEmail = null;
        _localStorage.RemoveItem("UserId");
        _localStorage.RemoveItem("UserEmail");
        _localStorage.RemoveItem("UserName");
    }

    public async Task Save(AppData data)
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return;
        // الحفظ تحت مسار Users/{uid} لتوافق الـ Rules الجديدة
        await db.Child("Users").Child(CurrentUserId).PutAsync(data);
    }

    public async Task<AppData> Load()
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return new AppData();
        var data = await db.Child("Users").Child(CurrentUserId).OnceSingleAsync<AppData>();
        return data ?? new AppData { Accounts = [new Account { Name = "Cash", Balance = 0 }] };
    }

    public async Task UpdateUserName(string newName)
    {
        if (auth?.User != null)
        {
            await auth.User.ChangeDisplayNameAsync(newName);
            _localStorage.SetItem("UserName", newName);
        }
    }

    public async Task<string> ChangePassword(string newPass)
    {
        if (auth?.User == null) return "Not Logged In";
        try { await auth.User.ChangePasswordAsync(newPass); return "Success"; }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    public async Task<string> DeleteUser()
    {
        if (auth?.User == null) return "Not Logged In";
        try
        {
            if (!string.IsNullOrEmpty(CurrentUserId)) await db.Child("Users").Child(CurrentUserId).DeleteAsync();
            // تفعيل حذف المستخدم من Auth بعد حل مشكلة الـ Build
            await auth.User.DeleteAsync();
            SignOut();
            return "Success";
        }
        catch (Exception ex) { return GetFriendlyError(ex); }
    }

    private static string GetFriendlyError(Exception ex)
    {
        string msg = ex.Message;
        if (msg.Contains("EMAIL_NOT_FOUND") || msg.Contains("INVALID_EMAIL")) return "Invalid Email.";
        if (msg.Contains("INVALID_PASSWORD") || msg.Contains("INVALID_LOGIN_CREDENTIALS")) return "Wrong Password.";
        if (msg.Contains("EMAIL_EXISTS")) return "Email already exists.";
        if (msg.Contains("RequiresRecentLogin")) return "Please login again to verify.";
        return "Check credentials or internet.";
    }
}