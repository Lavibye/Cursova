using Cursova.FileHandling;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace Cursova
{
    public partial class MainWindow : Window
    {
        private IFileHandler fileHandler = new FileHandler();

        private SirSimulation simulation;
        private DispatcherTimer timer;
        private int days = 0;
        private int simulationDays;
        private double treatmentEfficiency = 0.0;
        public ObservableCollection<SimulationData> SimulationDataCollection { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries { Title = "Сприйнятливі", Values = new ChartValues<double>() },
                new LineSeries { Title = "Інфекційні", Values = new ChartValues<double>() },
                new LineSeries { Title = "Одужавші", Values = new ChartValues<double>() }
            };
            SimulationDataCollection = new ObservableCollection<SimulationData>();
            Labels = new string[] { };
            DataContext = this;

            SeriesCollection = new SeriesCollection
{
            new LineSeries
            {
                Title = "Сприйнятливі",
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5
            },
            new LineSeries
            {
                Title = "Інфекційні",
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Cross,
                PointGeometrySize = 5
            },
            new LineSeries
            {
                Title = "Одужавші",
                Values = new ChartValues<double>(),
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 5
            }
            };
            ResetSimulation();
        }
        private void StartSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSimulation();

            double immunityDuration;
            double infectivityVariability;
            double treatmentEfficiency;
            if (int.TryParse(SusceptibleTextBox.Text, out int s0) &&
                int.TryParse(InfectiousTextBox.Text, out int i0) &&
                int.TryParse(RecoveredTextBox.Text, out int r0) &&
                double.TryParse(BetaTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double beta) &&
                double.TryParse(GammaTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double gamma) &&
                double.TryParse(ImmunityDurationTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out immunityDuration) &&
                double.TryParse(InfectivityVariabilityTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out infectivityVariability) &&
                double.TryParse(TreatmentEfficiencyTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out treatmentEfficiency) &&
                int.TryParse(DaysTextBox.Text, out simulationDays))
            {
                string userInputDataHeader = "Parameter,Value";
                string userInputData =
                    $"S0,{s0}{Environment.NewLine}" +
                    $"I0,{i0}{Environment.NewLine}" +
                    $"R0,{r0}{Environment.NewLine}" +
                    $"Beta,{beta}{Environment.NewLine}" +
                    $"Gamma,{gamma}{Environment.NewLine}" +
                    $"ImmunityDuration,{immunityDuration}{Environment.NewLine}" +
                    $"InfectivityVariability,{infectivityVariability}{Environment.NewLine}" +
                    $"TreatmentEfficiency,{treatmentEfficiency}{Environment.NewLine}" +
                    $"SimulationDays,{simulationDays}";

                fileHandler.WriteToFile("user_input_data.csv", userInputDataHeader + Environment.NewLine + userInputData);


                simulation = new SirSimulation(s0, i0, r0, beta, gamma, immunityDuration, treatmentEfficiency, infectivityVariability); 
                timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else
            {
                MessageBox.Show("Check the input parameters.");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (days <= simulationDays)
            {
                simulation.SimulateStep(fileHandler, days);

                var newData = new SimulationData
                {
                    Day = days,
                    Susceptible = simulation.Susceptible,
                    Infectious = simulation.Infectious,
                    Recovered = simulation.Recovered
                };

                SimulationDataCollection.Add(newData);
                UpdateChart(newData);
                Labels = Labels.Concat(new[] { days.ToString() }).ToArray();
                UpdateUI();
                days++;
            }
            else
            {
                timer.Stop();
            }
        }
        private void UpdateChart(SimulationData data)
        {
            Dispatcher.Invoke(() =>
            {
                SeriesCollection[0].Values.Add(data.Susceptible);
                SeriesCollection[1].Values.Add(data.Infectious);
                SeriesCollection[2].Values.Add(data.Recovered);
            });
        }
        private void UpdateUI()
        {
            SusceptibleTextBlock.Text = $"Сприйнятливі: {simulation.Susceptible:F2}";
            InfectiousTextBlock.Text = $"Інфекційні: {simulation.Infectious:F2}";
            RecoveredTextBlock.Text = $"Одужавші: {simulation.Recovered:F2}";
        }
        private void ResetSimulation()
        {
            days = 1;
            SimulationDataCollection.Clear();
            foreach (var series in SeriesCollection)
            {
                series.Values.Clear();
            }

            SusceptibleTextBlock.Text = "SСприйнятливі: ";
            InfectiousTextBlock.Text = "Інфекційні: ";
            RecoveredTextBlock.Text = "Одужавші: ";

            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
            }
        }
        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Збереження загального результату симуляції
            SaveSimulationParameters();

            // Збереження детальних даних з DataGrid
            SaveDataGridContent();
        }
        private void SaveSimulationParameters()
        {
            string simulationDataHeader = "Susceptible,Infectious,Recovered";
            string simulationData =
                $"{simulation.Susceptible}," +
                $"{simulation.Infectious}," +
                $"{simulation.Recovered},";

            try
            {
                File.WriteAllText("simulation_data.csv", simulationDataHeader + Environment.NewLine + simulationData);
                MessageBox.Show("Simulation parameters saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving simulation parameters: {ex.Message}");
            }
        }

        private void SaveDataGridContent()
        {
            string directoryPath = "simulation_log";
            string filePath = System.IO.Path.Combine(directoryPath, "simulation_log.csv");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Day,Susceptible,Infectious,Recovered");

            foreach (var data in SimulationDataCollection)
            {
                sb.AppendLine($"{data.Day},{data.Susceptible},{data.Infectious},{data.Recovered}");
            }

            try
            {
                File.WriteAllText(filePath, sb.ToString());
                MessageBox.Show("DataGrid content saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving DataGrid content: {ex.Message}");
            }
        }

        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            string data = fileHandler.LoadFromFile("user_input_data.csv");

            if (data != null)
            {
                string[] lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Перевірка, чи є в файлі достатня кількість рядків
                if (lines.Length < 2)
                {
                    MessageBox.Show("Invalid data format in the file.");
                    return;
                }

                Dictionary<string, string> parameterDictionary = new Dictionary<string, string>();

                // Починаємо обробку з другого рядка (індекс 1), оскільки перший рядок - це заголовки
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string[] parts = line.Split(',');

                    if (parts.Length == 2)
                    {
                        string parameter = parts[0].Trim();
                        string value = parts[1].Trim();

                        parameterDictionary[parameter] = value;
                    }
                }

                if (parameterDictionary.Count == 9 &&
                    int.TryParse(parameterDictionary["S0"], out int s0) &&
                    int.TryParse(parameterDictionary["I0"], out int i0) &&
                    int.TryParse(parameterDictionary["R0"], out int r0) &&
                    double.TryParse(parameterDictionary["Beta"], NumberStyles.Any, CultureInfo.InvariantCulture, out double beta) &&
                    double.TryParse(parameterDictionary["Gamma"], NumberStyles.Any, CultureInfo.InvariantCulture, out double gamma) &&
                    double.TryParse(parameterDictionary["ImmunityDuration"], NumberStyles.Any, CultureInfo.InvariantCulture, out double immunityDuration) &&
                    double.TryParse(parameterDictionary["InfectivityVariability"], NumberStyles.Any, CultureInfo.InvariantCulture, out double infectivityVariability) &&
                    double.TryParse(parameterDictionary["TreatmentEfficiency"], NumberStyles.Any, CultureInfo.InvariantCulture, out double treatmentEfficiency) &&
                    int.TryParse(parameterDictionary["SimulationDays"], out int simulationDays))
                {
                    SusceptibleTextBox.Text = s0.ToString();
                    InfectiousTextBox.Text = i0.ToString();
                    RecoveredTextBox.Text = r0.ToString();
                    BetaTextBox.Text = beta.ToString(CultureInfo.InvariantCulture);
                    GammaTextBox.Text = gamma.ToString(CultureInfo.InvariantCulture);
                    ImmunityDurationTextBox.Text = infectivityVariability.ToString(CultureInfo.InvariantCulture);
                    InfectivityVariabilityTextBox.Text = immunityDuration.ToString(CultureInfo.InvariantCulture);
                    TreatmentEfficiencyTextBox.Text = treatmentEfficiency.ToString(CultureInfo.InvariantCulture);
                    DaysTextBox.Text = simulationDays.ToString();

                    MessageBox.Show("Data loaded successfully.");
                }
                else
                {
                    MessageBox.Show("Invalid data format in the file.");
                }
            }
            else
            {
                MessageBox.Show("File not found or error reading the file.");
            }
        }
    }
}