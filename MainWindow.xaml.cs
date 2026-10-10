using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CampusCafe.UI
{
    public class IngredientItem : INotifyPropertyChanged
    {
        private int _ingredientID;
        private string _name = string.Empty;
        private int _quantity;

        public int IngredientID
        {
            get => _ingredientID;
            set { _ingredientID = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = Math.Max(0, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string StatusText => Quantity > 0 ? "Available" : "Out of Stock";
        public string StatusColor => Quantity > 0 ? "#10B981" : "#EF4444";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool AvailabilityStatus { get; set; }
    }

    public class EmployeeUI
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string WorkSchedule { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }

    public class SupplierUI
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string TelephoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string BusinessAddress { get; set; } = string.Empty;
    }

    public class DeliveryOverviewUI
    {
        public int DeliveryID { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public double QuantityReceived { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5248/") };
        private readonly List<IngredientItem> _masterIngredientsList = new List<IngredientItem>();

        public ObservableCollection<IngredientItem> IngredientsList { get; set; } = new ObservableCollection<IngredientItem>();

        public MainWindow()
        {
            InitializeComponent();
            IngredientsItemsControl.ItemsSource = IngredientsList;
            
            LoadProducts();
            LoadIngredientsFromDatabase();
            LoadEmployees();
            LoadSuppliers();
            LoadDeliveries();
        }

        // ==========================================
        // EMPLOYEES & SUPPLIERS HANDLERS
        // ==========================================

        private async void LoadEmployees()
        {
            try
            {
                var emps = await _httpClient.GetFromJsonAsync<List<EmployeeUI>>("api/employee");
                DgEmployees.ItemsSource = emps;
            }
            catch { }
        }

        private async void BtnSaveEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEmpName.Text))
            {
                MessageBox.Show("Please enter an employee name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var emp = new EmployeeUI
            {
                Name = TxtEmpName.Text.Trim(),
                ContactNumber = TxtEmpContact.Text.Trim(),
                WorkSchedule = TxtEmpSchedule.Text.Trim(),
                Position = ((ComboBoxItem)CmbEmpPosition.SelectedItem)?.Content?.ToString() ?? "Cashier"
            };

            try
            {
                var res = await _httpClient.PostAsJsonAsync("api/employee", emp);
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Employee saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtEmpName.Clear();
                    TxtEmpContact.Clear();
                    LoadEmployees();
                }
                else
                {
                    string err = await res.Content.ReadAsStringAsync();
                    MessageBox.Show($"Server Error ({res.StatusCode}): {err}", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadSuppliers()
        {
            try
            {
                var sups = await _httpClient.GetFromJsonAsync<List<SupplierUI>>("api/suppliers");
                DgSuppliers.ItemsSource = sups;
            }
            catch { }
        }

        private async void BtnSaveSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCompanyName.Text))
            {
                MessageBox.Show("Please enter a company name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sup = new SupplierUI
            {
                CompanyName = TxtCompanyName.Text.Trim(),
                ContactPerson = TxtContactPerson.Text.Trim(),
                TelephoneNumber = TxtTelNumber.Text.Trim(),
                EmailAddress = TxtEmail.Text.Trim(),
                BusinessAddress = TxtAddress.Text.Trim()
            };

            try
            {
                var res = await _httpClient.PostAsJsonAsync("api/suppliers", sup);
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Supplier saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtCompanyName.Clear();
                    TxtContactPerson.Clear();
                    TxtTelNumber.Clear();
                    TxtEmail.Clear();
                    TxtAddress.Clear();
                    LoadSuppliers();
                }
                else
                {
                    string err = await res.Content.ReadAsStringAsync();
                    MessageBox.Show($"Server Error ({res.StatusCode}): {err}", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadDeliveries()
        {
            try
            {
                var delivs = await _httpClient.GetFromJsonAsync<List<DeliveryOverviewUI>>("api/suppliers/deliveries");
                DgDeliveries.ItemsSource = delivs;
            }
            catch { }
        }

        // ==========================================
        // INGREDIENTS DATABASE HANDLERS
        // ==========================================

        private async void LoadIngredientsFromDatabase()
        {
            try
            {
                var ingredients = await _httpClient.GetFromJsonAsync<List<IngredientItem>>("api/ingredients");
                if (ingredients != null)
                {
                    _masterIngredientsList.Clear();
                    _masterIngredientsList.AddRange(ingredients);
                    ApplyIngredientFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load ingredients: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyIngredientFilter()
        {
            string query = TxtSearchIngredient?.Text.Trim() ?? "";

            IngredientsList.Clear();
            var filtered = string.IsNullOrEmpty(query)
                ? _masterIngredientsList
                : _masterIngredientsList.Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
            {
                IngredientsList.Add(item);
            }
        }

        private void TxtSearchIngredient_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyIngredientFilter();
        }

        private async void BtnAddIngredient_Click(object sender, RoutedEventArgs e)
        {
            string newIng = TxtNewIngredient.Text.Trim();
            if (string.IsNullOrWhiteSpace(newIng))
            {
                MessageBox.Show("Please enter an ingredient name.", "Input Empty", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_masterIngredientsList.Any(i => i.Name.Equals(newIng, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("This ingredient already exists.", "Duplicate Ingredient", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int initQty = 0;
            int.TryParse(TxtInitialQuantity.Text.Trim(), out initQty);

            var dto = new IngredientItem { Name = newIng, Quantity = initQty };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ingredients", dto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Ingredient saved to database!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtNewIngredient.Clear();
                    TxtInitialQuantity.Text = "5";
                    LoadIngredientsFromDatabase();
                }
                else
                {
                    MessageBox.Show("Failed to save ingredient to database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnIncreaseStock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IngredientItem item)
            {
                item.Quantity++;
                await SyncStockToDatabase(item);
            }
        }

        private async void BtnDecreaseStock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IngredientItem item)
            {
                if (item.Quantity > 0)
                {
                    item.Quantity--;
                    await SyncStockToDatabase(item);
                }
            }
        }

        private async Task SyncStockToDatabase(IngredientItem item)
        {
            try
            {
                await _httpClient.PutAsJsonAsync($"api/ingredients/{item.IngredientID}/stock", item.Quantity);
            }
            catch { }
        }

        private async void BtnRemoveIngredient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IngredientItem item)
            {
                var confirm = MessageBox.Show($"Delete '{item.Name}' from database?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await _httpClient.DeleteAsync($"api/ingredients/{item.IngredientID}");
                        if (response.IsSuccessStatusCode)
                        {
                            LoadIngredientsFromDatabase();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Delete Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // ==========================================
        // PRODUCT MANAGEMENT HANDLERS
        // ==========================================

        private async void LoadProducts()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<Product>>("api/products");

                if (products != null)
                {
                    CoffeeItemsControl.ItemsSource = products.Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Coffee", StringComparison.OrdinalIgnoreCase)).ToList();
                    NonCoffeeItemsControl.ItemsSource = products.Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Non-Coffee", StringComparison.OrdinalIgnoreCase)).ToList();
                    TeaItemsControl.ItemsSource = products.Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.IndexOf("Tea", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    ChocolateItemsControl.ItemsSource = products.Where(p => !string.IsNullOrEmpty(p.Category) && p.Category.Equals("Chocolate", StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
            catch { }
        }

        private void ProductItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Product product)
            {
                string desc = string.IsNullOrWhiteSpace(product.Description) ? "No description provided." : product.Description;
                string details = $"Product Name: {product.ProductName}\nCategory: {product.Category}\nPrice: ₱{product.Price:N2}\n\nDescription:\n{desc}";
                MessageBox.Show(details, $"{product.ProductName} - Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtProductName.Text) || string.IsNullOrWhiteSpace(TxtPrice.Text))
            {
                MessageBox.Show("Please enter product name and price.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid price.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string category = ((ComboBoxItem)CmbCategory.SelectedItem)?.Content?.ToString() ?? "Coffee";

            var newProduct = new Product
            {
                ProductName = TxtProductName.Text.Trim(),
                Category = category,
                Price = price,
                Description = TxtDescription.Text.Trim(),
                AvailabilityStatus = ChkAvailable.IsChecked ?? true
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/products", newProduct);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Product saved successfully to database!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtProductName.Clear();
                    TxtPrice.Clear();
                    TxtDescription.Clear();
                    LoadProducts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Save Error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && int.TryParse(btn.Tag.ToString(), out int productId))
            {
                var result = MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await _httpClient.DeleteAsync($"api/products/{productId}");
                        if (response.IsSuccessStatusCode) LoadProducts();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"API Delete Error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadIngredientsFromDatabase();
            LoadEmployees();
            LoadSuppliers();
            LoadDeliveries();
        }
    }
}