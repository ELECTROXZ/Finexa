using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Domain.Entities;
using Finexa.Core.Services;

namespace Finexa_App.Views
{
    public sealed partial class PartyImportDialog : ContentDialog
    {
        private readonly List<Party> _importedParties;
        private readonly List<string> _validationErrors;
        private readonly IPartyService _partyService;

        public PartyImportDialog(List<Party> imported, List<string> errors)
        {
            _importedParties = imported;
            _validationErrors = errors;
            _partyService = App.Services.GetRequiredService<IPartyService>();

            this.InitializeComponent();

            LoadImportPreview();
        }

        private void LoadImportPreview()
        {
            int validCount = _importedParties.Count;
            int errorCount = _validationErrors.Count;

            TotalParsedText.Text = (validCount + errorCount).ToString();
            ValidRecordsText.Text = validCount.ToString();
            InvalidRecordsText.Text = errorCount.ToString();

            ImportGrid.ItemsSource = _importedParties;

            if (errorCount > 0)
            {
                ErrorsList.ItemsSource = _validationErrors;
                ErrorSection.Visibility = Visibility.Visible;
            }

            if (validCount == 0)
            {
                // Disable primary import button if no valid records exist
                this.IsPrimaryButtonEnabled = false;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                // Bulk create valid parties
                foreach (var party in _importedParties)
                {
                    await _partyService.CreateAsync(party, "admin");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to save imported parties.");
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
