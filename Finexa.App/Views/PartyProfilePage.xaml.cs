using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Domain.Entities;
using Finexa_App.ViewModels;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Microsoft.UI.Xaml.Media;

namespace Finexa_App.Views
{
    public sealed partial class PartyProfilePage : Page
    {
        public PartyDetailsViewModel ViewModel { get; }

        public PartyProfilePage()
        {
            ViewModel = App.Services.GetRequiredService<PartyDetailsViewModel>();
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter is int partyId)
            {
                await ViewModel.LoadPartyDetailsAsync(partyId);
                this.Bindings.Update();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PartiesPage));
        }

        private async void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Party == null) return;

            var dialog = new PartyEditorDialog(ViewModel.Party, ViewModel.Party.Type)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LoadPartyDetailsAsync(ViewModel.Party.Id);
                this.Bindings.Update();
            }
        }

        // Addresses CRUD
        private async void AddAddress_Click(object sender, RoutedEventArgs e)
        {
            DlgAddr1.Text = string.Empty;
            DlgAddr2.Text = string.Empty;
            DlgCity.Text = string.Empty;
            DlgState.Text = string.Empty;
            DlgPin.Text = string.Empty;
            DlgPrimary.IsChecked = false;

            await AddressDialog.ShowAsync();
        }

        private async void AddressDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(DlgAddr1.Text) || string.IsNullOrWhiteSpace(DlgCity.Text) || string.IsNullOrWhiteSpace(DlgState.Text) || string.IsNullOrWhiteSpace(DlgPin.Text))
            {
                args.Cancel = true;
                return;
            }

            var address = new PartyAddress
            {
                AddressLine1 = DlgAddr1.Text.Trim(),
                AddressLine2 = DlgAddr2.Text.Trim(),
                City = DlgCity.Text.Trim(),
                State = DlgState.Text.Trim(),
                Country = "India",
                Pincode = DlgPin.Text.Trim(),
                IsPrimary = DlgPrimary.IsChecked ?? false
            };

            await ViewModel.AddAddressCommand.ExecuteAsync(address);
        }

        private async void DeleteAddress_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PartyAddress address)
            {
                var confirm = new ContentDialog
                {
                    Title = "Confirm Delete",
                    Content = "Are you sure you want to delete this address?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };
                
                if (await confirm.ShowAsync() == ContentDialogResult.Primary)
                {
                    await ViewModel.RemoveAddressCommand.ExecuteAsync(address);
                }
            }
        }

        // Contacts CRUD
        private async void AddContact_Click(object sender, RoutedEventArgs e)
        {
            DlgContactName.Text = string.Empty;
            DlgContactPhone.Text = string.Empty;
            DlgContactEmail.Text = string.Empty;
            DlgContactDesig.Text = string.Empty;

            await ContactDialog.ShowAsync();
        }

        private async void ContactDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(DlgContactName.Text) || string.IsNullOrWhiteSpace(DlgContactPhone.Text))
            {
                args.Cancel = true;
                return;
            }

            var contact = new PartyContact
            {
                Name = DlgContactName.Text.Trim(),
                Phone = DlgContactPhone.Text.Trim(),
                Email = DlgContactEmail.Text.Trim(),
                Designation = DlgContactDesig.Text.Trim()
            };

            await ViewModel.AddContactCommand.ExecuteAsync(contact);
        }

        private async void DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PartyContact contact)
            {
                var confirm = new ContentDialog
                {
                    Title = "Confirm Delete",
                    Content = "Are you sure you want to delete this contact person?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                if (await confirm.ShowAsync() == ContentDialogResult.Primary)
                {
                    await ViewModel.RemoveContactCommand.ExecuteAsync(contact);
                }
            }
        }

        // Adjustments / Transactions CRUD
        private async void AddAdjustment_Click(object sender, RoutedEventArgs e)
        {
            DlgAdjAmount.Value = 0.00;
            DlgAdjRef.Text = string.Empty;
            DlgAdjNotes.Text = string.Empty;
            DlgAdjType.SelectedIndex = 0;

            await AdjustmentDialog.ShowAsync();
        }

        private async void AdjustmentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            decimal amt = (decimal)DlgAdjAmount.Value;
            string refNum = DlgAdjRef.Text.Trim();
            string notes = DlgAdjNotes.Text.Trim();

            if (amt <= 0 || string.IsNullOrWhiteSpace(refNum))
            {
                args.Cancel = true;
                return;
            }

            string isDebit = DlgAdjType.SelectedIndex == 0 ? "Debit" : "Credit";

            var transaction = new PartyTransaction
            {
                Date = DateTime.Today,
                ReferenceType = "Adjustment",
                ReferenceNumber = refNum,
                Amount = amt,
                TransactionType = isDebit
            };

            await ViewModel.AddTransactionCommand.ExecuteAsync(transaction);
            
            // Reload details to update outstanding metrics
            if (ViewModel.Party != null)
            {
                await ViewModel.LoadPartyDetailsAsync(ViewModel.Party.Id);
                this.Bindings.Update();
            }
        }

        // Attachments Upload CRUD
        private async void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker();
                InitializeFilePicker(picker);
                picker.ViewMode = PickerViewMode.List;
                picker.FileTypeFilter.Add(".pdf");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".docx");
                picker.FileTypeFilter.Add(".xlsx");

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    await ViewModel.AddDocumentAsync(file.Name, file.Path);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to upload attachment file.");
            }
        }

        private async void DeleteAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PartyDocument doc)
            {
                var confirm = new ContentDialog
                {
                    Title = "Confirm Delete",
                    Content = "Are you sure you want to delete this attachment?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                if (await confirm.ShowAsync() == ContentDialogResult.Primary)
                {
                    await ViewModel.RemoveDocumentCommand.ExecuteAsync(doc);
                }
            }
        }

        // Bindings helper methods
        public static string GetInitials(Party? p)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.Name)) return "P";
            return p.Name.Substring(0, 1).ToUpper();
        }

        public static string FormatCurrency(decimal amount)
        {
            return $"₹{amount:N2}";
        }

        public static string FormatCreditLimit(decimal limit)
        {
            if (limit <= 0) return "Unlimited";
            return $"₹{limit:N2}";
        }

        public static Brush GetBalanceColor(decimal bal)
        {
            if (bal > 0) return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54)); // Red (Receivable)
            if (bal < 0) return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80));  // Green (Payable)
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 158, 158, 158));             // Grey (Settled)
        }

        public static Brush GetStatusColor(string status)
        {
            return status switch
            {
                "Active" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80)),
                "Blocked" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 152, 0)) // Inactive - Orange
            };
        }

        public static Brush GetStatusBg(string status)
        {
            return status switch
            {
                "Active" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 76, 175, 80)),
                "Blocked" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 244, 67, 54)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 255, 152, 0))
            };
        }

        public static Visibility GetNotesVisibility(string notes)
        {
            return string.IsNullOrEmpty(notes) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InitializeFilePicker(object picker)
        {
            var activeWindow = App.Current.Resources["ActiveWindow"] as Microsoft.UI.Xaml.Window;
            if (activeWindow != null)
            {
                var hwnd = WindowNative.GetWindowHandle(activeWindow);
                InitializeWithWindow.Initialize(picker, hwnd);
            }
        }
    }
}
