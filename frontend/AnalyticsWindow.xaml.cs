using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace nesa
{
    public partial class AnalyticsWindow : Window
    {
        private readonly HttpClient client = new HttpClient();

        public AnalyticsWindow()
        {
            InitializeComponent();

            LoadAnalytics();
        }

        private async void LoadAnalytics()
        {
            try
            {
                string url = "http://127.0.0.1:8000/analytics";

                HttpResponseMessage response =
                    await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Failed to load analytics.\n\n" +
                        "Status: " + response.StatusCode
                    );

                    return;
                }

                string json =
                    await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                AnalyticsData? data =
                    JsonSerializer.Deserialize<AnalyticsData>(
                        json,
                        options
                    );

                if (data == null)
                {
                    MessageBox.Show("Invalid analytics data.");
                    return;
                }

                DisplayAnalytics(data);
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "Could not connect to the backend.\n\n" +
                    "Make sure FastAPI is running."
                );
            }
            catch (JsonException)
            {
                MessageBox.Show(
                    "Could not read analytics data from the backend."
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unexpected error:\n\n" +
                    ex.Message
                );
            }
        }


        private void DisplayAnalytics(AnalyticsData data)
        {

            totalPatientsText.Text =
                data.TotalPatients.ToString();

            averageAgeText.Text =
                data.AverageAge.ToString("0.00");

            minimumAgeText.Text =
                data.MinimumAge.ToString();

            maximumAgeText.Text =
                data.MaximumAge.ToString();

            commonDiagnosisText.Text =
                string.IsNullOrEmpty(data.MostCommonDiagnosis)
                    ? "-"
                    : data.MostCommonDiagnosis;


            if (data.DiagnosisCounts.Count > 0)
            {
                diagnosisChart.Series =
                    data.DiagnosisCounts
                        .Select(item =>
                            new PieSeries<int>
                            {
                                Name = item.Key,
                                Values = new[] { item.Value }
                            }
                        )
                        .ToArray();
            }
            else
            {
                diagnosisChart.Series =
                    Array.Empty<ISeries>();
            }
            int under18 =
                data.AgeGroups.GetValueOrDefault("Under 18", 0);

            int age18To30 =
                data.AgeGroups.GetValueOrDefault("18-30", 0);

            int age31To50 =
                data.AgeGroups.GetValueOrDefault("31-50", 0);

            int age51Plus =
                data.AgeGroups.GetValueOrDefault("51+", 0);


            ageChart.Series = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Patients",

                    Values = new[]
                    {
                        under18,
                        age18To30,
                        age31To50,
                        age51Plus
                    }
                }
            };
            ageChart.XAxes =
            [
                new Axis
                {
                    Labels =
                    [
                        "Under 18",
                        "18-30",
                        "31-50",
                        "51+"
                    ]
                }
            ];
            ageChart.YAxes =
            [
                new Axis
                {
                    MinLimit = 0
                }
            ];
        }
    }

    public class AnalyticsData
    {
        [JsonPropertyName("total_patients")]
        public int TotalPatients { get; set; }

        [JsonPropertyName("average_age")]
        public double AverageAge { get; set; }

        [JsonPropertyName("minimum_age")]
        public int MinimumAge { get; set; }

        [JsonPropertyName("maximum_age")]
        public int MaximumAge { get; set; }

        [JsonPropertyName("most_common_diagnosis")]
        public string? MostCommonDiagnosis { get; set; }

        [JsonPropertyName("diagnosis_counts")]
        public Dictionary<string, int> DiagnosisCounts { get; set; }
            = new Dictionary<string, int>();

        [JsonPropertyName("age_groups")]
        public Dictionary<string, int> AgeGroups { get; set; }
            = new Dictionary<string, int>();
    }

}