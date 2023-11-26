using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cursova
{
    public class SirSimulation: ISirSimulation
    {
        public double Susceptible { get; private set; }
        public double Infectious { get; private set; }
        public double Recovered { get; private set; }
        private double totalPopulation;
        public double Beta { get; private set; }
        public double Gamma { get; private set; }
        private double immunityDuration;
        private List<double> recentlyRecovered;
        public double TreatmentEfficiency { get; private set; }
        public double InfectivityVariability { get; private set; }
        public double ImmunityDuration
        {
            get { return immunityDuration; }
            private set { immunityDuration = value; }
        }
        private Random random = new Random();
        public SirSimulation(double s0, double i0, double r0, double beta, double gamma, double immunityDuration, double treatmentEfficiency, double infectivityVariability)
        {
            Susceptible = s0;
            Infectious = i0;
            Recovered = r0;
            Beta = beta;
            Gamma = gamma;
            totalPopulation = s0 + i0 + r0;

            this.immunityDuration = immunityDuration;
            recentlyRecovered = new List<double>(new double[(int)Math.Ceiling(immunityDuration)]);
            TreatmentEfficiency = treatmentEfficiency;
            InfectivityVariability = infectivityVariability;
        }

        public void UpdateParameters(double beta, double gamma, double immunityDuration, double treatmentEfficiency, double infectivityVariability)
        {
            Beta = beta;
            Gamma = gamma;
            this.immunityDuration = immunityDuration;
            TreatmentEfficiency = treatmentEfficiency;
            InfectivityVariability = infectivityVariability;
        }

        public void SimulateStep(IFileHandler fileHandler, int day)
        {
            // Врахування змінності заразності вірусу
            double variabilityEffect = (random.NextDouble() - 0.5) * 2 * InfectivityVariability;
            double variableBeta = Beta * (1 + variabilityEffect);
            variableBeta = Math.Max(variableBeta, 0);  // Уникнення негативного variableBeta

            // Переконайтесь, що variableBeta не є негативним
            variableBeta = Math.Max(variableBeta, 0);

            double newInfections = variableBeta * Susceptible * Infectious / totalPopulation;
            double newRecoveries = Gamma * Infectious * TreatmentEfficiency;

            // Обмеження нових інфекцій та одужань до максимально можливих значень
            newInfections = Math.Min(newInfections, Susceptible);
            newRecoveries = Math.Min(newRecoveries, Infectious);

            // Оновлення стану популяції
            Susceptible -= newInfections;
            Infectious = Infectious + newInfections - newRecoveries;
            Recovered += newRecoveries;

            // Обробка втрати імунітету
            for (int i = 0; i < recentlyRecovered.Count; i++)
            {
                // Поступова втрата імунітету
                double losingImmunity = recentlyRecovered[i] / ImmunityDuration;
                recentlyRecovered[i] -= losingImmunity;
                Susceptible += losingImmunity;
                Recovered -= losingImmunity;
            }

            // Додаємо кількість нових одужань до списку нещодавно одужавших
            recentlyRecovered.Add(newRecoveries);

            // Перевірка, що загальна кількість населення не змінилася
            Debug.Assert(Math.Abs((Susceptible + Infectious + Recovered) - totalPopulation) < 1.0E-5, "The total population should remain constant.");

            // Записуємо дані симуляції у файл
            string simulationData = $"{day},{Susceptible},{Infectious},{Recovered},{totalPopulation}";
            fileHandler.AppendToFile("simulation_log.txt", simulationData);
        }
    }
}