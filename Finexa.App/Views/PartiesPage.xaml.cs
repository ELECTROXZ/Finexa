using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Domain.Entities;
using Finexa_App.ViewModels;
using WinRT.Interop;
using Windows.Storage.Pickers;
using Finexa.Core.Services;
using CommunityToolkit.WinUI.UI.Controls;

namespace Finexa_App.Views
{
    public sealed partial class PartiesPage : Page
    {
        public PartiesViewModel ViewModel { get; }

        public PartiesPage()
        {
            ViewModel = App.Services.GetRequiredService<PartiesViewModel>();
            this.InitializeComponent();

            this.Loaded += PartiesPage_Loaded;
        }

        private async void PartiesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadPartiesAsync();
            this.Bindings.Update();
        }

        public static bool IsPartySelected(Party? selected)
        {
            return selected != null;
        }

        public static string FormatCurrency(decimal amount)
        {
            return $"₹{amount:N2}";
        }

        private void SearchTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                _ = ViewModel.LoadPartiesAsync();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PageIndex = 0;
            _ = ViewModel.LoadPartiesAsync();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PageIndex = 0;
            _ = ViewModel.LoadPartiesAsync();
        }

        private void ShowTrash_Changed(object sender, RoutedEventArgs e)
        {
            ViewModel.PageIndex = 0;
            _ = ViewModel.LoadPartiesAsync();
        }

        private void PartiesGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            string sortCol = e.Column.Tag?.ToString() ?? "Name";
            if (ViewModel.SortBy == sortCol)
            {
                ViewModel.SortDescending = !ViewModel.SortDescending;
            }
            else
            {
                ViewModel.SortBy = sortCol;
                ViewModel.SortDescending = false;
            }

            // Update column sort indicators
            foreach (var col in PartiesGrid.Columns)
            {
                col.SortDirection = null;
            }
            e.Column.SortDirection = ViewModel.SortDescending ? DataGridSortDirection.Descending : DataGridSortDirection.Ascending;

            _ = ViewModel.LoadPartiesAsync();
        }

        private void PartiesGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            NavigateToProfile();
        }

        private void NavigateToProfile()
        {
            if (ViewModel.SelectedParty == null) return;
            this.Frame.Navigate(typeof(PartyProfilePage), ViewModel.SelectedParty.Id);
        }

        private void NewCustomer_Click(object sender, RoutedEventArgs e)
        {
            _ = ShowEditorDialogAsync(null, "Customer");
        }

        private void NewSupplier_Click(object sender, RoutedEventArgs e)
        {
            _ = ShowEditorDialogAsync(null, "Supplier");
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedParty != null)
            {
                _ = ShowEditorDialogAsync(ViewModel.SelectedParty, ViewModel.SelectedParty.Type);
            }
        }

        private void ViewLedger_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProfile();
        }

        private async Task ShowEditorDialogAsync(Party? party, string defaultType)
        {
            var dialog = new PartyEditorDialog(party, defaultType)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LoadPartiesAsync();
            }
        }

        // Context menu handlers
        private void ContextView_Click(object sender, RoutedEventArgs e) => NavigateToProfile();
        private void ContextEdit_Click(object sender, RoutedEventArgs e) => Edit_Click(sender, e);
        private void ContextLedger_Click(object sender, RoutedEventArgs e) => NavigateToProfile();
        private void ContextDuplicate_Click(object sender, RoutedEventArgs e) => ViewModel.DuplicatePartyCommand.Execute(ViewModel.SelectedParty);
        private void ContextDelete_Click(object sender, RoutedEventArgs e) => ViewModel.DeletePartyCommand.Execute(ViewModel.SelectedParty);

        // Export handlers
        private async void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileSavePicker();
                InitializeFilePicker(picker);
                picker.FileTypeChoices.Add("PDF Document", new List<string> { ".pdf" });
                picker.SuggestedFileName = $"Parties_Export_{DateTime.Today:yyyyMMdd}";

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    var bytes = await ViewModel.ExportToPdfAsync();
                    await System.IO.File.WriteAllBytesAsync(file.Path, bytes);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to export PDF.");
            }
        }

        private async void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileSavePicker();
                InitializeFilePicker(picker);
                picker.FileTypeChoices.Add("Excel Workbook", new List<string> { ".xlsx" });
                picker.SuggestedFileName = $"Parties_Export_{DateTime.Today:yyyyMMdd}";

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    var bytes = await ViewModel.ExportToExcelAsync();
                    await System.IO.File.WriteAllBytesAsync(file.Path, bytes);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to export Excel.");
            }
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileSavePicker();
                InitializeFilePicker(picker);
                picker.FileTypeChoices.Add("CSV Comma Separated", new List<string> { ".csv" });
                picker.SuggestedFileName = $"Parties_Export_{DateTime.Today:yyyyMMdd}";

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    var bytes = await ViewModel.ExportToCsvAsync();
                    await System.IO.File.WriteAllBytesAsync(file.Path, bytes);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to export CSV.");
            }
        }

        // Import Handlers
        private void ImportExcel_Click(object sender, RoutedEventArgs e) => _ = ImportFileAsync(true);
        private void ImportCsv_Click(object sender, RoutedEventArgs e) => _ = ImportFileAsync(false);

        private async Task ImportFileAsync(bool isExcel)
        {
            try
            {
                var picker = new FileOpenPicker();
                InitializeFilePicker(picker);
                picker.ViewMode = PickerViewMode.List;
                picker.FileTypeFilter.Add(isExcel ? ".xlsx" : ".csv");

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(file.Path);
                    var importService = App.Services.GetRequiredService<IPartyImportExportService>();
                    
                    List<Party> imported;
                    List<string> errors;

                    if (isExcel)
                    {
                        (imported, errors) = await importService.ImportPartiesFromExcelAsync(fileBytes, "admin");
                    }
                    else
                    {
                        (imported, errors) = await importService.ImportPartiesFromCsvAsync(fileBytes, "admin");
                    }

                    // Open PartyImportDialog to preview mapping and save records
                    var importDialog = new PartyImportDialog(imported, errors)
                    {
                        XamlRoot = this.XamlRoot
                    };

                    var result = await importDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        await ViewModel.LoadPartiesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to import file.");
            }
        }

        // Helper methods for binds
        public static Visibility GetEmptyStateVisibility(int count, bool isLoading)
        {
            return (count == 0 && !isLoading) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static int GetOneBasedPage(int page) => page + 1;
        public static bool CanGoPrevious(int page) => page > 0;
        public static bool CanGoNext(int page, int total) => page < total - 1;

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
