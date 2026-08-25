using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;
namespace nesa ///project name
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window ///access modifier-allows .xaml and .xaml.cs to connect into 1-inheritance
    {
        HttpClient client=new HttpClient();
        public MainWindow()///obj constructor executes
        {
            InitializeComponent();
            load_patients();

        }
            private async Task load_patients()
        {
            string url = "http://127.0.0.1:8000/patients";
            HttpResponseMessage response= await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<Patient> loadedPatients =
                    JsonSerializer.Deserialize<List<Patient>>(json, options)
                    ?? new List<Patient>();

                patientdata.ItemsSource = loadedPatients;



            } else
            {
                MessageBox.Show("Error: " + response.StatusCode);
            }




        }
        

            private async void submit_click(object sender, RoutedEventArgs e)
            {
            Patient p = new Patient();
            
            if (!int.TryParse(idbox.Text, out int id))
            {
                MessageBox.Show("ID must be a number.");
                return;
            }

            if (!int.TryParse(agebox.Text, out int age))
            {
                MessageBox.Show("Age must be a number.");
                return;
                
            }
            mybutton.Content = "Submitted";


            p.Id = id;
            p.Name = namebox.Text;
            p.Age = age;
            p.Diagnosis = diabox.Text; ;
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string json=JsonSerializer.Serialize(p,options);
            StringContent content = new StringContent(json,Encoding.UTF8,"application/json");
            //MessageBox.Show("ID:" + p.Id + "\n Name: " + p.Name + "\n Age: " + p.Age + "\n Diagnosis:" + p.Diagnosis+"\n added successfully to the list.");
            string url = "http://127.0.0.1:8000/patients";
            HttpResponseMessage response=await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Patient saved successfully!");
                await load_patients();
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    "Failed to save patient.\n\n" +
                    "Status: " + response.StatusCode + "\n\n" +
                    "Response: " + error
                );
            }
            idbox.Clear();
            namebox.Clear();
            agebox.Clear();
            diabox.Clear();
            mybutton.Content = "Submit";
        }
        private void selection(object sender, SelectionChangedEventArgs e)
        {
            
            if (patientdata.SelectedItem==null)
            {
                 return;
                    
            }
            Patient selectedpatient = (Patient)patientdata.SelectedItem;
            //MessageBox.Show("ID:" + selectedpatient.Id + "\n Name: " + selectedpatient.Name + "\n Age: " + selectedpatient.Age + "\n Diagnosis:" + selectedpatient.Diagnosis);
            idbox.Text = selectedpatient.Id.ToString();
            namebox.Text = selectedpatient.Name;
            agebox.Text = selectedpatient.Age.ToString();
            diabox.Text = selectedpatient.Diagnosis;

        }
        private async void update_click(object sender, RoutedEventArgs e)
        {
            if (patientdata.SelectedItem == null)
            {
                MessageBox.Show("Please select a Patient to update their Info!");
                return;
            }

            // Validate age
            if (!int.TryParse(agebox.Text, out int age))
            {
                MessageBox.Show("Age must be a number.");
                return;
            }

            Patient selectedpatient = (Patient)patientdata.SelectedItem;

            selectedpatient.Name = namebox.Text;
            selectedpatient.Age = age;
            selectedpatient.Diagnosis = diabox.Text;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(selectedpatient, options);

            StringContent content =
                new StringContent(json, Encoding.UTF8, "application/json");

            string url = "http://127.0.0.1:8000/patients/" + selectedpatient.Id;

            HttpResponseMessage response = await client.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Patient updated successfully!");

                await load_patients();

                idbox.Clear();
                namebox.Clear();
                agebox.Clear();
                diabox.Clear();

                patientdata.SelectedItem = null;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    "Failed to update patient.\n\n" +
                    "Status: " + response.StatusCode + "\n\n" +
                    "Response: " + error
                );
            }
        }
        private async void delete_click(object sender, RoutedEventArgs e)
        {
            if (patientdata.SelectedItem == null)
            {
                MessageBox.Show("Please select a Patient to delete their Info!"); return;
            }
            Patient selectedpatient = (Patient)patientdata.SelectedItem;
            
            string url = "http://127.0.0.1:8000/patients/" + selectedpatient.Id;

            HttpResponseMessage response = await client.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Patient deleted successfully!");

                await load_patients();

                idbox.Clear();
                namebox.Clear();
                agebox.Clear();
                diabox.Clear();

                patientdata.SelectedItem = null;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    "Failed to delete patient.\n\n" +
                    "Status: " + response.StatusCode + "\n\n" +
                    "Response: " + error
                );
            }
        }
        private async void search_click(object sender, RoutedEventArgs e)
        {
            string search = searchbox.Text.Trim();

            string url = "http://127.0.0.1:8000/patients";

            if (!string.IsNullOrEmpty(search))
            {
                url += "?search=" + Uri.EscapeDataString(search);
            }

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<Patient> loadedPatients =
                    JsonSerializer.Deserialize<List<Patient>>(json, options)
                    ?? new List<Patient>();

                patientdata.ItemsSource = loadedPatients;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    "Search failed.\n\n" +
                    "Status: " + response.StatusCode +
                    "\n\nResponse: " + error
                );
            }
        }
        private async void clear_click(object sender, RoutedEventArgs e)
        {
            searchbox.Clear();
            minagebox.Clear();
            maxagebox.Clear();
            sortbox.SelectedItem = null;

            await load_patients();
        }
        private void analytics_click(object sender, RoutedEventArgs e)
        {
            AnalyticsWindow window = new AnalyticsWindow();

            window.Show();
        }
        private async void filter_click(object sender, RoutedEventArgs e)
        {
            string minText = minagebox.Text.Trim();
            string maxText = maxagebox.Text.Trim();

            if (!int.TryParse(minText, out int minAge))
            {
                MessageBox.Show("Enter a valid minimum age.");
                return;
            }

            if (!int.TryParse(maxText, out int maxAge))
            {
                MessageBox.Show("Enter a valid maximum age.");
                return;
            }

            if (minAge > maxAge)
            {
                MessageBox.Show("Minimum age cannot be greater than maximum age.");
                return;
            }

            string url =
                $"http://127.0.0.1:8000/patients?min_age={minAge}&max_age={maxAge}";

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<Patient> filteredPatients =
                    JsonSerializer.Deserialize<List<Patient>>(json, options)
                    ?? new List<Patient>();

                patientdata.ItemsSource = filteredPatients;
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    "Failed to filter patients.\n\n" +
                    "Status: " + response.StatusCode + "\n\n" +
                    "Response: " + error
                );
            }
        }
        private async Task sort_patients(string order)
        {
            if (sortbox.SelectedItem == null)
            {
                MessageBox.Show("Please select a field to sort by.");
                return;
            }

            ComboBoxItem selected =
                (ComboBoxItem)sortbox.SelectedItem;

            string sortBy = selected.Content.ToString();

            string url =
                $"http://127.0.0.1:8000/patients" +
                $"?sort_by={sortBy}&sort_order={order}";

            HttpResponseMessage response =
                await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json =
                    await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<Patient> sortedPatients =
                    JsonSerializer.Deserialize<List<Patient>>(json, options)
                    ?? new List<Patient>();

                patientdata.ItemsSource = sortedPatients;
            }
            else
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    "Failed to sort patients.\n\n" +
                    "Status: " + response.StatusCode +
                    "\n\nResponse: " + error
                );
            }
        }
        private async void sort_asc_click(object sender, RoutedEventArgs e)
        {
            await sort_patients("asc");
        }

        private async void sort_desc_click(object sender, RoutedEventArgs e)
        {
            await sort_patients("desc");
        }
    }
    }
