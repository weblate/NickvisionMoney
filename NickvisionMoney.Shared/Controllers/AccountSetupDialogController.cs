using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System.Collections.Generic;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when settings are validated
/// </summary>
public enum SetupCheckStatus
{
    Valid = 0,
    EmptyName,
    EmptyCurrency
}

/// <summary>
/// A controller for a AccountSetupDialog
/// </summary>
public class AccountSetupDialogController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The account represented by the controller
    /// </summary>
    public Account Account { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }

    /// <summary>
    /// Creates an AccountSetupDialogController
    /// </summary>
    /// <param name="account">The Account object represented by the controller</param>
    /// <param name="localizer">The Localizer of the app</param>
    public AccountSetupDialogController(Account account, Localizer localizer)
    {
        Localizer = localizer;
        Account = account;
        Accepted = true;
    }

    /// <summary>
    /// Updates the Account object
    /// </summary>
    /// <returns>SetupCheckStatus</returns>
    public SetupCheckStatus UpdateAccount(string name, string currency, bool useCustomCurrency, string currencySymbol, string currencyCode, int transactionType, int accountType, bool saveHideGroups, bool hideGroups, bool showReceiptMark)
    {
        if(string.IsNullOrEmpty(name))
        {
            return SetupCheckStatus.EmptyName;
        }
        if(string.IsNullOrEmpty(currencySymbol))
        {
            return SetupCheckStatus.EmptyCurrency;
        }
        Account.Name = name;
        Account.Currency = currency;
        Account.UseCustomCurrency = useCustomCurrency;
        Account.CustomCurrencySymbol = currencySymbol;
        Account.CustomCurrencyCode = currencyCode;
        Account.DefaultTransactionType = transactionType;
        Account.Type = accountType;
        Account.SaveHideGroups = saveHideGroups;
        Account.HideGroups = hideGroups;
        Account.ShowReceiptMark = showReceiptMark;
        return SetupCheckStatus.Valid;
    }
}