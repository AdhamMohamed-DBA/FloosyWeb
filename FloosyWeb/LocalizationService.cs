using Blazored.LocalStorage;
using System.Text.RegularExpressions;

namespace FloosyWeb;

public class LocalizationService
{
    private const string LanguageStorageKey = "floosy_app_language";
    private readonly ISyncLocalStorageService _localStorage;

    public string CurrentLanguage { get; private set; } = "en";
    public bool IsArabic => CurrentLanguage == "ar";

    public event Action? OnChange;

    public LocalizationService(ISyncLocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public void Init()
    {
        try
        {
            var stored = _localStorage.GetItem<string>(LanguageStorageKey);
            if (!string.IsNullOrWhiteSpace(stored) && (stored == "en" || stored == "ar"))
            {
                CurrentLanguage = stored;
            }
        }
        catch
        {
            CurrentLanguage = "en";
        }
    }

    public Task SetLanguageAsync(string language)
    {
        var normalized = (language ?? "en").Trim().ToLowerInvariant();
        if (normalized != "ar" && normalized != "en") normalized = "en";

        CurrentLanguage = normalized;
        _localStorage.SetItem(LanguageStorageKey, CurrentLanguage);
        OnChange?.Invoke();

        return Task.CompletedTask;
    }

    public string T(string key)
    {
        if (!IsArabic)
        {
            return key switch
            {
                // Update flow / admin update settings
                "Ok" => "OK",
                "AppUpdated" => "App Updated",
                "UpdateRequired" => "Update Required",
                "UpdateRequiredBlockingHint" => "You must update to continue using the app.",
                "CurrentVersionText" => "Your Version",
                "LatestVersionText" => "Latest Version",
                "InstalledAppVersion" => "Installed App Version",
                "UpdateNow" => "Update Now",
                "RequiredVersion" => "Required Version",
                "RequiredVersionPlaceholder" => "Required version (e.g. 2.6.0)",
                "PleaseAddRequiredVersion" => "Please add required version.",
                "ForceUpdateForOlderVersions" => "Force update for users below required version",
                "InvalidVersionFormat" => "Version format must be x.y.z (example: 2.7.0).",
                "RequiredVersionCannotExceedBroadcastVersion" => "Required version cannot be higher than broadcast version.",
                "UpdateLoadFailed" => "Could not load update settings. Please check your internet or permissions.",
                "UpdateSaveFailed" => "Could not save update settings. Please check your internet or permissions.",
                "UpdateRequiredFallbackMessage" => "A required update is available. Please update the app to continue.",
                // Legacy key kept for backward compatibility
                "ShowUpdatePopupOnce" => "Force update for users below required version",
                "BillsTitle" => "Bills 🧾",
                "BillEndDateOptional" => "End date (optional)",
                "EndsOn" => "Ends on",
                "ResetFrequency" => "Reset frequency",
                "EndDateBeforeDueDate" => "End date cannot be before due date.",
                "DebtsTitle" => "Debts 💵",
                "HistoryTitle" => "History 📜",
                "Analytics" => "Analytics 📊",
                "VsPrevMonth" => "vs Prev Month",
                "AddContact" => "+ Add Contact",
                "DontChangeBalanceTextOnly" => "Don't Change Balance (Text Only)",
                "JustDeleteNoRefund" => "Just Delete (No Refund)",
                "DeleteOnlyNoAccountChange" => "Just delete (no account change)",
                "FilterByAccount" => "Filter by account",
                "AllAccounts" => "All accounts",
                "UnknownAccount" => "Unknown account",
                "SourceAccount" => "Source account",
                "SpentThisPeriod" => "Spent this period",
                "ExpensesByAccount" => "Expenses by account",
                "NoData" => "No Data",
                "NoTransactionsFound" => "No transactions found.",
                "NoTransactionsThisMonth" => "No transactions found for this month.",
                "NoRecentTransactions" => "No recent transactions.",
                "NoSavedNames" => "No saved names yet.",
                "PleaseFillAllFields" => "Please fill in all fields.",
                "DeleteAccountAction" => "🗑️ Delete Account",
                "ContactWhatsapp" => "💬 Contact via WhatsApp",
                _ => HumanizeKey(key)
            };
        }

        return key switch
        {
            // Shared / nav
            "Home" => "الرئيسية",
            "Bills" => "الفواتير",
            "Charts" => "الرسوم",
            "Debts" => "الديون",
            "History" => "السجل",
            "Settings" => "الإعدادات",
            "Version" => "الإصدار",
            "Ok" => "موافق",
            "Save" => "حفظ",
            "Cancel" => "إلغاء",
            "Delete" => "حذف",
            "Close" => "إغلاق",
            "Confirm" => "تأكيد",
            "Error" => "خطأ",
            "Success" => "نجاح",
            "Loading" => "جاري التحميل...",

            // Update popup / localization controls
            "AppUpdated" => "تم تحديث التطبيق",
            "UpdateRequired" => "تحديث مطلوب",
            "UpdateRequiredBlockingHint" => "يجب تحديث التطبيق للمتابعة.",
            "CurrentVersionText" => "نسختك الحالية",
            "LatestVersionText" => "أحدث نسخة",
            "InstalledAppVersion" => "نسخة التطبيق المثبتة",
            "UpdateNow" => "حدّث الآن",
            "RequiredVersion" => "النسخة المطلوبة",
            "RequiredVersionPlaceholder" => "النسخة المطلوبة (مثال 2.6.0)",
            "PleaseAddRequiredVersion" => "من فضلك أدخل النسخة المطلوبة.",
            "Language" => "اللغة",
            "English" => "English",
            "Arabic" => "العربية",
            "AlignArabic" => "محاذاة عربي",
            "AlignEnglish" => "محاذاة إنجليزي",
            "UpdateBroadcast" => "إشعار التحديث",
            "UpdateMessagePlaceholder" => "اكتب رسالة التحديث...",
            "ForceUpdateForOlderVersions" => "إلزام المستخدمين الأقل من النسخة المطلوبة بالتحديث",
            "InvalidVersionFormat" => "صيغة الإصدار يجب أن تكون x.y.z (مثال: 2.7.0).",
            "RequiredVersionCannotExceedBroadcastVersion" => "لا يمكن أن تكون النسخة المطلوبة أعلى من نسخة الإشعار.",
            "UpdateLoadFailed" => "تعذر تحميل إعدادات التحديث. تحقق من الإنترنت أو الصلاحيات.",
            "UpdateSaveFailed" => "تعذر حفظ إعدادات التحديث. تحقق من الإنترنت أو الصلاحيات.",
            "UpdateRequiredFallbackMessage" => "يوجد تحديث إلزامي جديد. من فضلك حدّث التطبيق للمتابعة.",

            // Home
            "Welcome" => "أهلًا",
            "TotalBalance" => "إجمالي الرصيد",
            "In" => "دخل",
            "Out" => "مصروف",
            "Transfer" => "تحويل",
            "MyAccounts" => "حساباتي",
            "Add" => "إضافة",
            "RecentActivity" => "آخر النشاطات",
            "NoRecentTransactions" => "لا توجد معاملات حديثة.",
            "NewAccount" => "حساب جديد",
            "AccountName" => "اسم الحساب",
            "InitialBalance" => "الرصيد الابتدائي",
            "PickColor" => "اختيار اللون",
            "Create" => "إنشاء",
            "Up" => "أعلى",
            "Down" => "أسفل",
            "EditBalance" => "تعديل الرصيد",
            "Rename" => "إعادة تسمية",
            "ChangeColor" => "تغيير اللون:",
            "DeleteAccount" => "حذف الحساب",
            "RenameAccount" => "إعادة تسمية الحساب",
            "NewName" => "الاسم الجديد",
            "DeleteAccountQuestion" => "حذف الحساب؟",
            "DeleteAccountConfirm" => "هل أنت متأكد أنك تريد حذف",
            "YesDelete" => "نعم، احذف",
            "NewIncome" => "دخل جديد",
            "NewExpense" => "مصروف جديد",
            "Date" => "التاريخ",
            "Amount" => "المبلغ",
            "DescriptionOptional" => "الوصف (اختياري)",
            "ManageCategories" => "إدارة التصنيفات",
            "NewCategory" => "تصنيف جديد",
            "DeleteTransactionQuestion" => "حذف المعاملة؟",
            "DontChangeBalance" => "لا تغيّر الرصيد",
            "DeductFrom" => "خصم من:",
            "RefundTo" => "استرجاع إلى:",
            "From" => "من:",
            "To" => "إلى:",

            // Bills
            "BillsTitle" => "الفواتير 🧾",
            "NewMonth" => "شهر جديد",
            "NewMonthAria" => "بدء شهر جديد وإعادة تعيين الفواتير المتكررة",
            "EditBill" => "تعديل فاتورة",
            "NewBill" => "فاتورة جديدة",
            "BillName" => "اسم الفاتورة",
            "SharedBill" => "فاتورة مشتركة؟",
            "PersonName" => "اسم الشخص",
            "Paid" => "مدفوع",
            "Remaining" => "متبقي",
            "UpdateBill" => "تحديث الفاتورة",
            "AddBill" => "إضافة فاتورة",
            "CancelEdit" => "إلغاء التعديل",
            "Shared" => "مشتركة",
            "With" => "مع:",
            "Status" => "الحالة:",
            "Pay" => "دفع",
            "DeleteBillQuestion" => "حذف الفاتورة؟",
            "NewMonthQuestion" => "شهر جديد؟",
            "NewMonthConfirm" => "بدء دورة جديدة؟ سيتم تصفير الفواتير حسب النوع المختار وتحريك تاريخ الاستحقاق تلقائيًا.",
            "BillEndDateOptional" => "تاريخ الانتهاء (اختياري)",
            "EndsOn" => "ينتهي في",
            "ResetFrequency" => "نوع التصفير",
            "EndDateBeforeDueDate" => "تاريخ الانتهاء لا يمكن أن يكون قبل تاريخ الاستحقاق.",
            "Yes" => "نعم",
            "PayBill" => "دفع فاتورة",
            "FromAccount" => "من الحساب:",
            "Processing" => "جاري المعالجة...",
            "InsufficientBalance" => "الرصيد غير كافٍ!",

            // Debts
            "DebtsTitle" => "الديون 💵",
            "AddContact" => "+ إضافة جهة اتصال",
            "NoDebtContacts" => "لا توجد جهات اتصال بها ديون بعد.",
            "NoDebtContactsHint" => "استخدم \"إضافة جهة اتصال\" لبدء تتبع من عليك له أو من له عليك.",
            "TheyPaidYou" => "هم قاموا بالدفع لك",
            "YouPaidThem" => "أنت قمت بالدفع لهم",
            "EditDebtMove" => "تعديل حركة دين",
            "DeleteDebtMove" => "حذف حركة دين",
            "EditContact" => "تعديل جهة الاتصال",
            "ContactName" => "اسم جهة الاتصال",
            "SavedContactNames" => "أسماء محفوظة",
            "InitialDebtOptional" => "الدين الابتدائي (اختياري)",
            "TheyOweMe" => "عليهم لي",
            "IOweThem" => "عليّ لهم",
            "NoteOptional" => "ملاحظة (اختيارية)",
            "DeleteContact" => "حذف جهة الاتصال",
            "NoSavedNames" => "لا توجد أسماء محفوظة بعد.",
            "Current" => "الحالي",
            "Direction" => "الاتجاه",
            "Account" => "الحساب",
            "DeletePersonQuestion" => "حذف الشخص؟",
            "DeletePersonConfirm" => "هل أنت متأكد أنك تريد حذف",
            "CurrentBalance" => "الرصيد الحالي",
            "UpdateAccountOptional" => "تحديث الحساب (اختياري)",
            "DontChangeAccount" => "لا تغير الحساب",
            "DeleteDebtMoveQuestion" => "حذف حركة الدين؟",
            "RefundOnAccountOptional" => "الاسترجاع على الحساب (اختياري)",
            "DeleteOnlyNoAccountChange" => "حذف فقط (بدون تغيير حساب)",
            "FromPerson" => "من",
            "ToPerson" => "إلى",
            "RemainingBalance" => "متبقي",
            "Overpaid" => "مدفوع زيادة",
            "Settled" => "تمت التسوية",
            "DebtRemaining" => "دين متبقٍ",
            "NoBalance" => "لا يوجد رصيد",

            // History
            "HistoryTitle" => "السجل 📜",
            "FilterByMonth" => "تصفية حسب الشهر",
            "FilterByAccount" => "تصفية حسب الحساب",
            "AllAccounts" => "كل الحسابات",
            "UnknownAccount" => "حساب غير معروف",
            "SourceAccount" => "الحساب المصدر",
            "SpentThisPeriod" => "المصروف في هذه الفترة",
            "NoTransactionsThisMonth" => "لا توجد معاملات لهذا الشهر.",
            "EditTransaction" => "تعديل المعاملة",
            "Description" => "الوصف",
            "UpdateBalanceOnAccount" => "تحديث الرصيد على الحساب:",
            "DontChangeBalanceTextOnly" => "لا تغيّر الرصيد (تعديل النص فقط)",
            "DeleteTransactionTitle" => "حذف المعاملة؟",
            "RefundToOptional" => "الاسترجاع إلى (اختياري):",
            "JustDeleteNoRefund" => "حذف فقط (بدون استرجاع)",
            "Deleting" => "جاري الحذف...",

            // Charts
            "Analytics" => "التحليلات 📊",
            "Income" => "دخل",
            "Expense" => "مصروف",
            "Net" => "الصافي",
            "VsPrevMonth" => "مقارنة بالشهر السابق",
            "Monthly" => "شهري",
            "Lifetime" => "مدى الحياة",
            "MonthlyAnalyticsAria" => "التبديل إلى تحليلات شهرية",
            "LifetimeAnalyticsAria" => "التبديل إلى تحليلات شاملة",
            "SelectMonth" => "اختر الشهر",
            "LifetimeTotal" => "إجمالي شامل",
            "SelectedMonth" => "الشهر المحدد",
            "NoData" => "لا توجد بيانات",
            "ShowIncomeChart" => "عرض مخطط الدخل",
            "ShowExpenseChart" => "عرض مخطط المصروف",
            "ExpensesByAccount" => "المصروف حسب الحساب",
            "TransactionsHistory" => "السجل",
            "NoTransactionsFound" => "لا توجد معاملات.",
            "Other" => "أخرى",

            // Login
            "WelcomeBack" => "مرحبًا بعودتك!",
            "Email" => "البريد الإلكتروني",
            "Password" => "كلمة المرور",
            "RememberEmail" => "تذكر البريد الإلكتروني",
            "ShowPassword" => "إظهار كلمة المرور",
            "Login" => "تسجيل الدخول",
            "LoginWithBiometric" => "تسجيل الدخول بالبصمة",
            "CreateAccount" => "إنشاء حساب",
            "ForgotPassword" => "نسيت كلمة المرور؟",
            "FullName" => "الاسم الكامل",
            "PasswordMin6" => "كلمة المرور (6 أحرف على الأقل)",
            "SignUp" => "إنشاء الحساب",
            "BackToLogin" => "العودة لتسجيل الدخول",
            "ResetPassword" => "إعادة تعيين كلمة المرور",
            "SendLink" => "إرسال الرابط",
            "MissingInfo" => "بيانات ناقصة",
            "PleaseFillAllFields" => "من فضلك املأ جميع الحقول.",
            "EnterEmailAndPassword" => "من فضلك أدخل البريد وكلمة المرور.",
            "LoginFailed" => "فشل تسجيل الدخول",
            "InvalidEmail" => "بريد غير صالح",
            "InvalidEmailHint" => "أدخل بريدًا صحيحًا مثل name@gmail.com أو name@outlook.com.",
            "WeakPassword" => "كلمة مرور ضعيفة",
            "WeakPasswordHint" => "كلمة المرور يجب أن تكون 6 أحرف على الأقل.",
            "RegistrationFailed" => "فشل إنشاء الحساب",
            "NotSupported" => "غير مدعوم",
            "BiometricNotAvailable" => "تسجيل الدخول بالبصمة غير متاح على هذا الجهاز.",
            "NotEnabled" => "غير مُفعّل",
            "BiometricEnableHint" => "اذهب إلى الإعدادات ← الأمان وفعّل البصمة على هذا الجهاز أولاً.",
            "Cancelled" => "تم الإلغاء",
            "BiometricCancelled" => "تم إلغاء أو فشل التحقق بالبصمة.",
            "LoginRequired" => "تسجيل الدخول مطلوب",
            "BiometricNeedFirstLogin" => "سجل الدخول مرة واحدة بالبريد وكلمة المرور بعد تفعيل البصمة لحفظ بيانات الدخول.",
            "StoredCredentialsInvalid" => "بيانات الدخول المحفوظة لم تعد صالحة. من فضلك سجّل الدخول مرة أخرى.",
            "BiometricUnavailableNow" => "تسجيل الدخول بالبصمة غير متاح حاليًا.",
            "MissingEmail" => "بريد إلكتروني مفقود",
            "EnterEmailFirst" => "أدخل البريد الإلكتروني أولًا.",
            "EmailSent" => "تم إرسال البريد",
            "ResetEmailSentHint" => "تحقق من بريدك الإلكتروني لرسالة إعادة التعيين!",

            // Settings
            "AccountManagement" => "إدارة الحساب",
            "ChangeEmail" => "تغيير البريد الإلكتروني",
            "NewEmailAddress" => "بريد إلكتروني جديد",
            "CurrentPassword" => "كلمة المرور الحالية",
            "UpdateEmail" => "تحديث البريد الإلكتروني",
            "Updating" => "جاري التحديث...",
            "DangerZone" => "منطقة الخطر",
            "DeleteAccountAction" => "🗑️ حذف الحساب",
            "Security" => "الأمان",
            "BiometricUnlock" => "فتح بالبصمة (هذا الجهاز)",
            "BiometricHint" => "يستخدم Face ID / البصمة عند الدعم. الأجهزة بدون بصمة ستظل تعمل بكلمة المرور فقط.",
            "StartOfMonth" => "بداية الشهر (يوم الراتب)",
            "SalaryDayExample" => "مثال: إذا كان 25، فمعاملات 25 فبراير تظهر في رسوم مارس.",
            "DisplayName" => "اسم العرض",
            "YourName" => "اسمك",
            "ChangePassword" => "تغيير كلمة المرور",
            "OldPassword" => "كلمة المرور القديمة",
            "NewPassword" => "كلمة المرور الجديدة",
            "UpdatePassword" => "تحديث كلمة المرور",
            "Support" => "الدعم",
            "ContactWhatsapp" => "💬 تواصل عبر واتساب",
            "VersionPlaceholder" => "الإصدار (مثال 2.4.0)",
            // Legacy key kept for backward compatibility
            "ShowUpdatePopupOnce" => "إلزام المستخدمين الأقل من النسخة المطلوبة بالتحديث",
            "SaveUpdateMessage" => "حفظ رسالة التحديث",
            "SignOut" => "تسجيل الخروج",
            "DeleteAccountPrompt" => "حذف الحساب؟",
            "DeleteAccountWarning" => "لا يمكن التراجع عن هذا الإجراء. ستفقد كل بياناتك.",
            "ConfirmPassword" => "تأكيد كلمة المرور",
            "UserNotFound" => "المستخدم غير موجود.",
            "Unauthorized" => "غير مصرح.",
            "PleaseAddVersion" => "من فضلك أدخل الإصدار.",
            "PleaseAddUpdateMessage" => "من فضلك أدخل رسالة التحديث.",
            "UpdateSaved" => "تم حفظ إشعار التحديث.",
            "FinancialDayRange" => "اليوم يجب أن يكون بين 1 و 28",
            "FinancialDayUpdated" => "تم تحديث يوم بداية الفترة المالية!",
            "NameUpdated" => "تم تحديث الاسم!",
            "EnterOldNewPassword" => "أدخل كلمة المرور القديمة والجديدة.",
            "UserEmailNotFound" => "لم يتم العثور على بريد المستخدم.",
            "OldPasswordIncorrect" => "كلمة المرور القديمة غير صحيحة!",
            "EnterNewEmailCurrentPassword" => "أدخل البريد الجديد وكلمة المرور الحالية.",
            "PasswordIncorrect" => "كلمة المرور غير صحيحة!",
            "EmailUpdatedSuccessfully" => "تم تحديث البريد الإلكتروني بنجاح!",
            "PasswordIncorrectCannotDelete" => "كلمة المرور غير صحيحة! لا يمكن الحذف.",
            "BiometricNotSupportedDevice" => "فتح البصمة غير متاح على هذا الجهاز.",
            "CouldNotEnableBiometric" => "تعذر تفعيل فتح البصمة على هذا الجهاز.",

            _ => key
        };
    }

    private static string HumanizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var text = Regex.Replace(key, "([a-z])([A-Z])", "$1 $2");
        text = text.Replace("Aria", "").Trim();

        return text;
    }
}
