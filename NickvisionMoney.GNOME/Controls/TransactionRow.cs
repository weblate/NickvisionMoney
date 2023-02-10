using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a transaction
/// </summary>
public partial class TransactionRow : Adw.PreferencesGroup, IModelRowControl<Transaction>
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string gdk_rgba_to_string(ref Color rgba);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

    private const uint GTK_STYLE_PROVIDER_PRIORITY_USER = 800;

    private CultureInfo _cultureAmount;
    private CultureInfo _cultureDate;
    private Localizer _localizer;
    private bool _isSmall;
    private readonly Adw.ActionRow _row;
    private readonly Gtk.Button _btnId;
    private readonly Gtk.Image _iconCompact;
    private readonly Gtk.Button _btnAmount;
    private readonly Gtk.Label _lblAmount;
    private readonly Gtk.Button _btnEdit;
    private readonly Gtk.Button _btnDelete;
    private readonly Gtk.Box _boxButtons;
    private readonly Gtk.Box _boxSuffix;

    /// <summary>
    /// The id of the Transaction
    /// </summary>
    public uint Id { get; private set; }

    public Gtk.FlowBoxChild? Container { get; set; }

    /// <summary>
    /// Occurs when the edit button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    /// <summary>
    /// Constructs a TransactionRow
    /// </summary>
    /// <param name="transaction">The Transaction to display</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    /// <param name="localizer">The Localizer for the app</param>
    public TransactionRow(Transaction transaction, CultureInfo cultureAmount, CultureInfo cultureDate, Localizer localizer)
    {
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        _localizer = localizer;
        _isSmall = false;
        //Row Settings
        _row = Adw.ActionRow.New();
        _row.SetUseMarkup(false);
        _row.SetTitleLines(1);
        _row.SetSizeRequest(300, 78);
        //Button ID
        _btnId = Gtk.Button.New();
        _btnId.SetName("btnId");
        _btnId.AddCssClass("circular");
        _btnId.SetValign(Gtk.Align.Center);
        _iconCompact = Gtk.Image.NewFromIconName("big-dot-symbolic");
        _iconCompact.SetName("iconCompact");
        _row.AddPrefix(_btnId);
        _row.AddPrefix(_iconCompact);
        //Amount
        _btnAmount = Gtk.Button.New();
        _btnAmount.AddCssClass("circular");
        _btnAmount.SetHalign(Gtk.Align.End);
        _btnAmount.SetValign(Gtk.Align.Center);
        _btnAmount.SetMarginEnd(4);
        _btnAmount.OnClicked += (sender, e) => _row.Activate();
        _lblAmount = Gtk.Label.New(null);
        _lblAmount.SetMarginStart(12);
        _lblAmount.SetMarginEnd(12);
        _btnAmount.SetChild(_lblAmount);
        //Edit Button
        _btnEdit = Gtk.Button.NewFromIconName("document-edit-symbolic");
        _btnEdit.SetValign(Gtk.Align.Center);
        _btnEdit.AddCssClass("flat");
        _btnEdit.SetTooltipText(_localizer["Edit", "TransactionRow"]);
        _btnEdit.OnClicked += Edit;
        _row.SetActivatableWidget(_btnEdit);
        //DeleteButton
        _btnDelete = Gtk.Button.NewFromIconName("user-trash-symbolic");
        _btnDelete.SetValign(Gtk.Align.Center);
        _btnDelete.AddCssClass("flat");
        _btnDelete.SetTooltipText(_localizer["Delete", "TransactionRow"]);
        _btnDelete.OnClicked += Delete;
        //Buttons Box
        _boxButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _boxButtons.SetHalign(Gtk.Align.End);
        _boxButtons.SetMarginEnd(4);
        _boxButtons.Append(_btnEdit);
        _boxButtons.Append(_btnDelete);
        //Suffix Box
        _boxSuffix = Gtk.Box.New(Gtk.Orientation.Horizontal, 2);
        _boxSuffix.SetValign(Gtk.Align.Center);
        _boxSuffix.Append(_btnAmount);
        _boxSuffix.Append(_boxButtons);
        _row.AddSuffix(_boxSuffix);
        //Group Settings
        Add(_row);
        UpdateRow(transaction, cultureAmount, cultureDate);
    }

    /// <summary>
    /// Whether or not the row uses a small style
    /// </summary>
    public bool IsSmall
    {
        get => _isSmall;

        set
        {
            _isSmall = value;
            if (_isSmall)
            {
                _boxSuffix.SetOrientation(Gtk.Orientation.Vertical);
                _boxSuffix.SetMarginTop(4);
                _btnId.SetVisible(false);
                _iconCompact.SetVisible(true);
            }
            else
            {
                _boxSuffix.SetOrientation(Gtk.Orientation.Horizontal);
                _boxSuffix.SetMarginTop(0);
                _btnId.SetVisible(true);
                _iconCompact.SetVisible(false);
            }
        }
    }

    /// <summary>
    /// Updates the row with the new model
    /// </summary>
    /// <param name="transaction">The new Transaction model</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="cultureDate">The culture to use for displaying date strings</param>
    public void UpdateRow(Transaction transaction, CultureInfo cultureAmount, CultureInfo cultureDate)
    {
        Id = transaction.Id;
        _cultureAmount = cultureAmount;
        _cultureDate = cultureDate;
        //Color
        var color = new Color();
        if (!gdk_rgba_parse(ref color, transaction.RGBA))
        {
            gdk_rgba_parse(ref color, "#3584e4");
        }
        //Row Settings
        _row.SetTitle(transaction.Description);
        _row.SetSubtitle($"{transaction.Date.ToString("d", _cultureDate)}{(transaction.RepeatInterval != TransactionRepeatInterval.Never ? $"\n{_localizer["TransactionRepeatInterval", "Field"]}: {_localizer["RepeatInterval", transaction.RepeatInterval.ToString()]}" : "")}");
        //Button Id
        _btnId.SetLabel(transaction.Id.ToString());
        var btnCssProvider = Gtk.CssProvider.New();
        var btnCss = "#btnId, #iconCompact { font-size: 14px; color: " + gdk_rgba_to_string(ref color) + "; }";
        gtk_css_provider_load_from_data(btnCssProvider.Handle, btnCss, btnCss.Length);
        _btnId.GetStyleContext().AddProvider(btnCssProvider, GTK_STYLE_PROVIDER_PRIORITY_USER);
        _iconCompact.GetStyleContext().AddProvider(btnCssProvider, GTK_STYLE_PROVIDER_PRIORITY_USER);
        //Amount Label
        _lblAmount.SetLabel($"{(transaction.Type == TransactionType.Income ? "+  " : "-  ")}{transaction.Amount.ToString("C", _cultureAmount)}");
        _lblAmount.AddCssClass(transaction.Type == TransactionType.Income ? "success" : "error");
        _lblAmount.AddCssClass(transaction.Type == TransactionType.Income ? "denaro-income" : "denaro-expense");
        //Buttons Box
        _btnEdit.SetVisible(transaction.RepeatFrom <= 0);
        _btnEdit.SetSensitive(transaction.RepeatFrom <= 0);
        _btnDelete.SetVisible(transaction.RepeatFrom <= 0);
        _btnDelete.SetSensitive(transaction.RepeatFrom <= 0);
    }

    public void Show() => Container!.Show();

    public void Hide() => Container!.Hide();

    /// <summary>
    /// Occurs when the edit button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Edit(Gtk.Button sender, EventArgs e) => EditTriggered?.Invoke(this, Id);

    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Delete(Gtk.Button sender, EventArgs e) => DeleteTriggered?.Invoke(this, Id);
}