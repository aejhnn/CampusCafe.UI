using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace CampusCafe.UI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5248/");
        }

        public async Task<List<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"API Get Error ({response.StatusCode}):\n\n{errorDetails}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<T>();
                }

                var data = await response.Content.ReadFromJsonAsync<List<T>>();
                return data ?? new List<T>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Get Connection Error:\n\n{ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<T>();
            }
        }

        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);

                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Database Save Failed ({response.StatusCode}):\n\n{errorDetails}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Post Connection Error:\n\n{ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Delete Error:\n\n{ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}