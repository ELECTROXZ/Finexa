using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Domain.Entities;
using Finexa.Core.Services;

namespace Finexa_App.Views
{
    public sealed partial class PartyEditorDialog : ContentDialog
    {
        private readonly Party? _originalParty;
        private readonly IPartyService _partyService;
        private readonly string _defaultType;

        public PartyEditorDialog(Party? party, string defaultType)
        {
            _originalParty = party;
            _defaultType = defaultType;
            _partyService = App.Services.GetRequiredService<IPartyService>();
            
            this.InitializeComponent();
            
            LoadPartyDetails();
        }

        private void LoadPartyDetails()
        {
            if (_originalParty == null)
            {
                // New Party Defaults
                SetComboBoxSelectedValue(TypeCombo, _defaultType);
                StatusCombo.SelectedIndex = 0;
                PaymentTermsCombo.SelectedIndex = 0;
                PaymentMethodCombo.SelectedIndex = 0;
                return;
            }

            // Edit Party: Pre-fill forms
            BusinessNameBox.Text = _originalParty.Name;
            DisplayNameBox.Text = _originalParty.DisplayName;
            SetComboBoxSelectedValue(TypeCombo, _originalParty.Type);
            ContactPersonBox.Text = _originalParty.ContactPerson;
            PhoneBox.Text = _originalParty.Phone;
            AltPhoneBox.Text = _originalParty.AlternativePhone;
            EmailBox.Text = _originalParty.Email;
            WebsiteBox.Text = _originalParty.Website;
            
            GstinBox.Text = _originalParty.GSTIN;
            PanBox.Text = _originalParty.PAN;
            AadhaarBox.Text = _originalParty.Aadhaar;
            BrnBox.Text = _originalParty.BusinessRegistrationNumber;
            
            Address1Box.Text = _originalParty.AddressLine1;
            Address2Box.Text = _originalParty.AddressLine2;
            CityBox.Text = _originalParty.City;
            StateBox.Text = _originalParty.State;
            CountryBox.Text = _originalParty.Country;
            PincodeBox.Text = _originalParty.Pincode;
            
            OpeningBalanceBox.Value = (double)_originalParty.OpeningBalance;
            CreditLimitBox.Value = (double)_originalParty.CreditLimit;
            SetComboBoxSelectedValue(PaymentTermsCombo, _originalParty.PaymentTerms);
            SetComboBoxSelectedValue(PaymentMethodCombo, _originalParty.PreferredPaymentMethod);
            SetComboBoxSelectedValue(StatusCombo, _originalParty.Status);
            NotesBox.Text = _originalParty.Notes;

            // Opening balance is not editable when modifying a party to prevent ledger discrepancies
            OpeningBalanceBox.IsEnabled = false;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Defer dialog closing to allow async database validation checks
            var deferral = args.GetDeferral();
            
            try
            {
                ErrorBorder.Visibility = Visibility.Collapsed;

                string typeValue = (TypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Customer";
                string statusValue = (StatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Active";
                string termValue = (PaymentTermsCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "COD";
                string methodValue = (PaymentMethodCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Cash";

                var tempParty = new Party
                {
                    Id = _originalParty?.Id ?? 0,
                    Name = BusinessNameBox.Text.Trim(),
                    DisplayName = string.IsNullOrWhiteSpace(DisplayNameBox.Text) ? BusinessNameBox.Text.Trim() : DisplayNameBox.Text.Trim(),
                    Type = typeValue,
                    ContactPerson = ContactPersonBox.Text.Trim(),
                    Phone = PhoneBox.Text.Trim(),
                    AlternativePhone = AltPhoneBox.Text.Trim(),
                    Email = EmailBox.Text.Trim(),
                    Website = WebsiteBox.Text.Trim(),
                    GSTIN = GstinBox.Text.Trim().ToUpper(),
                    PAN = PanBox.Text.Trim().ToUpper(),
                    Aadhaar = AadhaarBox.Text.Trim(),
                    BusinessRegistrationNumber = BrnBox.Text.Trim(),
                    AddressLine1 = Address1Box.Text.Trim(),
                    AddressLine2 = Address2Box.Text.Trim(),
                    City = CityBox.Text.Trim(),
                    State = StateBox.Text.Trim(),
                    Country = CountryBox.Text.Trim(),
                    Pincode = PincodeBox.Text.Trim(),
                    OpeningBalance = (decimal)OpeningBalanceBox.Value,
                    CreditLimit = (decimal)CreditLimitBox.Value,
                    PaymentTerms = termValue,
                    PreferredPaymentMethod = methodValue,
                    Status = statusValue,
                    IsActive = statusValue == "Active",
                    Notes = NotesBox.Text.Trim()
                };

                // Perform full database validation checks
                var errors = await ValidateFieldsAsync(tempParty);
                if (errors.Any())
                {
                    args.Cancel = true;
                    ErrorText.Text = string.Join("\n", errors);
                    ErrorBorder.Visibility = Visibility.Visible;
                    deferral.Complete();
                    return;
                }

                // If valid, write to original or create new
                if (_originalParty == null)
                {
                    await _partyService.CreateAsync(tempParty, "admin");
                }
                else
                {
                    _originalParty.Name = tempParty.Name;
                    _originalParty.DisplayName = tempParty.DisplayName;
                    _originalParty.Type = tempParty.Type;
                    _originalParty.ContactPerson = tempParty.ContactPerson;
                    _originalParty.Phone = tempParty.Phone;
                    _originalParty.AlternativePhone = tempParty.AlternativePhone;
                    _originalParty.Email = tempParty.Email;
                    _originalParty.Website = tempParty.Website;
                    _originalParty.GSTIN = tempParty.GSTIN;
                    _originalParty.PAN = tempParty.PAN;
                    _originalParty.Aadhaar = tempParty.Aadhaar;
                    _originalParty.BusinessRegistrationNumber = tempParty.BusinessRegistrationNumber;
                    _originalParty.AddressLine1 = tempParty.AddressLine1;
                    _originalParty.AddressLine2 = tempParty.AddressLine2;
                    _originalParty.Address = $"{tempParty.AddressLine1} {tempParty.AddressLine2}".Trim();
                    _originalParty.City = tempParty.City;
                    _originalParty.State = tempParty.State;
                    _originalParty.Country = tempParty.Country;
                    _originalParty.Pincode = tempParty.Pincode;
                    _originalParty.CreditLimit = tempParty.CreditLimit;
                    _originalParty.PaymentTerms = tempParty.PaymentTerms;
                    _originalParty.PreferredPaymentMethod = tempParty.PreferredPaymentMethod;
                    _originalParty.Status = tempParty.Status;
                    _originalParty.IsActive = tempParty.IsActive;
                    _originalParty.Notes = tempParty.Notes;

                    await _partyService.UpdateAsync(_originalParty, "admin");
                }
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                ErrorText.Text = $"Database Error: {ex.Message}";
                ErrorBorder.Visibility = Visibility.Visible;
                Serilog.Log.Error(ex, "Failed to save party from editor dialog.");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async Task<List<string>> ValidateFieldsAsync(Party p)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(p.Name))
                errors.Add("- Business name / company is required.");

            if (!string.IsNullOrEmpty(p.Email) && !p.Email.Contains("@"))
                errors.Add("- Email address format is invalid.");

            if (!string.IsNullOrEmpty(p.GSTIN))
            {
                if (p.GSTIN.Length != 15)
                {
                    errors.Add("- GSTIN must be exactly 15 alphanumeric characters.");
                }
                
                // Duplicate check
                var allParties = await _partyService.GetAllAsync(false);
                bool dupGst = allParties.Any(x => x.Id != p.Id && string.Equals(x.GSTIN, p.GSTIN, StringComparison.OrdinalIgnoreCase));
                if (dupGst)
                    errors.Add($"- GSTIN '{p.GSTIN}' is already registered to another party.");
            }

            if (!string.IsNullOrEmpty(p.PAN))
            {
                if (p.PAN.Length != 10)
                {
                    errors.Add("- PAN must be exactly 10 alphanumeric characters.");
                }
                
                // Duplicate check
                var allParties = await _partyService.GetAllAsync(false);
                bool dupPan = allParties.Any(x => x.Id != p.Id && string.Equals(x.PAN, p.PAN, StringComparison.OrdinalIgnoreCase));
                if (dupPan)
                    errors.Add($"- PAN '{p.PAN}' is already registered to another party.");
            }

            if (!string.IsNullOrEmpty(p.Phone))
            {
                var allParties = await _partyService.GetAllAsync(false);
                bool dupPhone = allParties.Any(x => x.Id != p.Id && string.Equals(x.Phone, p.Phone, StringComparison.OrdinalIgnoreCase));
                if (dupPhone)
                    errors.Add($"- Phone number '{p.Phone}' is already registered to another party.");
            }

            return errors;
        }

        private void SetComboBoxSelectedValue(ComboBox combo, string value)
        {
            if (combo == null || string.IsNullOrEmpty(value)) return;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is ComboBoxItem item && string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
