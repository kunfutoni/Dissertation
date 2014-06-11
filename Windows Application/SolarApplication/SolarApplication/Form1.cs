using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MySql.Data.Entity;

namespace SolarApplication
{
    public partial class Form1 : Form
    {


        List<reading> readings = new ReadingRepository().getGeneratedReadings();
        public Form1()
        {
            InitializeComponent();
            double consumed = Math.Round(new ReadingRepository().getConsumedPower() / 1000 / 60, 2);
            label3.ForeColor = Color.Green;
            label3.Text = "Consumed power from generated electricity: " + consumed + "KW";
            double battery = new ReadingRepository().getBatteryHealth();
            string batteryString = "";
            if (battery >= 12)
            {
                batteryString = "Battery is Good";
                label4.ForeColor = Color.Green;
            }
            else
            {
                batteryString = "Check your battery, it needs charging, or else replaced";
                label4.ForeColor = Color.Red;
            }
            label4.Text = batteryString;

            //---------------------- Bar Graph ---------------------------------------
            populateBarchart(DateTime.Now, DateTime.Now);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void barGraph(DateTime startDate, DateTime endDate)
        {
            double meter1 = new ReadingRepository().getLowestMeterReadingValue(1, startDate, endDate);
            double meter2 = new ReadingRepository().getLowestMeterReadingValue(2, startDate, endDate);
            double meter3 = new ReadingRepository().getLowestMeterReadingValue(3, startDate, endDate);


            double tmeter1 = new ReadingRepository().getMetersSum(1, startDate, endDate);
            double tmeter2 = new ReadingRepository().getMetersSum(2, startDate, endDate);
            double tmeter3 = new ReadingRepository().getMetersSum(3, startDate, endDate);

            List<double> cmeter1 = new List<double>(), cmeter2 = new List<double>(), cmeter3 = new List<double>();

            cmeter1.Add(tmeter1);
            cmeter2.Add(tmeter2);
            cmeter3.Add(tmeter3);
            //Vertical bar chart
            //create another area and add it to the chart
            ChartArea area2 = new ChartArea();
            chart1.ChartAreas.Add(area2);

            //Create the series using just the y data
            Series barSeries1 = new Series();
            barSeries1.Points.DataBindY(cmeter1);
            barSeries1.LegendText = new ReadingRepository().getMeterByID(1);

            Series barSeries2 = new Series();
            barSeries2.Points.DataBindY(cmeter2);
            barSeries2.LegendText = new ReadingRepository().getMeterByID(2);

            Series barSeries3 = new Series();
            barSeries3.Points.DataBindY(cmeter3);
            barSeries3.LegendText = new ReadingRepository().getMeterByID(3);


            //Set the chart type, column; vertical bars
            barSeries1.ChartType = SeriesChartType.Column;
            barSeries2.ChartType = SeriesChartType.Column;
            barSeries3.ChartType = SeriesChartType.Column;

            // barSeries2.ChartArea = "Second";


            //Add the series to the chart
            chart1.Series.Add(barSeries1);
            chart1.Series.Add(barSeries2);
            chart1.Series.Add(barSeries3);
            label5.Text = checkPanels(meter1, meter2, meter3);
            label6.Text = checkEfficiency(meter1, meter2, meter3);
        }

        private void btnGetDataByDate_Click(object sender, EventArgs e)
        {
            populateBarchart(dtpStart.Value, dtpEnd.Value);
        }

        public void populateBarchart(DateTime sD, DateTime eD)
        {
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            DateTime startDate = sD;
            string sMonth = "", eMonth = "";

            if (startDate.Month.ToString().Length == 1)
                sMonth = "0" + startDate.Month;
            else sMonth = startDate.Month.ToString();

            string sDate = startDate.Day + "/" + sMonth + "/" + startDate.Year + " 09:00:00";
            startDate = Convert.ToDateTime(sDate);
            DateTime endDate = eD;
            if (endDate.Month.ToString().Length == 1)
                eMonth = "0" + endDate.Month;
            else sMonth = endDate.Month.ToString();

            string eDate = endDate.Day + "/" + eMonth + "/" + endDate.Year + " 18:00:00";
            endDate = Convert.ToDateTime(eDate);
            readings = new ReadingRepository().getGeneratedReadingsByDates(startDate, endDate);

            barGraph(startDate, endDate);
        }

        public string checkPanels(double meter1, double meter2, double meter3)
        {
            string third = "";
            double diff1 = 0;
            if (meter1 > meter2 && meter1 > meter3)
            {
                if (meter2 > meter3)
                {
                    third = "Solar Panel 1";
                    diff1 = meter2 - meter3;
                }
                else
                {
                    diff1 = meter3 - meter2;
                    third = "Solar Panel 2";
                }
            }
            else if (meter2 > meter1 && meter2 > meter3)
            {
                if (meter1 > meter3)
                {
                    diff1 = meter1 - meter3;
                    third = "Solar Panel 3";
                }
                else
                {
                    diff1 = meter3 - meter1;
                    third = "Solar Panel 1";
                }
            }
            else
            {
                if (meter2 > meter1)
                {
                    diff1 = meter2 - meter1;
                    third = "Solar Panel 1";
                }
                else
                {
                    diff1 = meter1 - meter2;
                    third = "Solar Panel 2";
                }
            }
            string message;
            if (diff1 > 1)
                message = third + " is not functioning properly.";
            else
                message = "All Panels are functioning properly.";

            return message;
        }

        public string checkEfficiency(double meter1, double meter2, double meter3)
        {
            //get average forecast
            int mostlyCloudy = 0, partlyCloudy = 0, sunny = 0, mostlySunny = 0, cloudy = 0, clear = 0, chanceRain = 0, fog = 0, total = 0;
            DateTime dt = new DateTime();
            double tm = 0;
            double expectedVoltageHr = 0;
            int largest = 0;
            string message = "";
            try
            {
                foreach (reading r in readings)
                {
                    if (r.MeterID == 1)
                        if (dt != r.ForecastID && r.ForecastID != null)
                        {
                            switch (new ReadingRepository().weatherForecast(r.ForecastID.Value))
                            {
                                case 1:
                                    mostlyCloudy++;
                                    expectedVoltageHr += 9;
                                    if (largest < mostlyCloudy)
                                        largest = mostlyCloudy;
                                    break;
                                case 2:
                                    partlyCloudy++;
                                    expectedVoltageHr += 10.5;
                                    if (largest < partlyCloudy)
                                        largest = partlyCloudy;
                                    break;
                                case 3:
                                    sunny++;
                                    expectedVoltageHr += 12;
                                    if (largest < sunny)
                                        largest = sunny;
                                    break;
                                case 4:
                                    mostlySunny++;
                                    expectedVoltageHr += 12;
                                    if (largest < mostlySunny)
                                        largest = mostlySunny;
                                    break;
                                case 5:
                                    cloudy++;
                                    expectedVoltageHr += 10;
                                    if (largest < cloudy)
                                        largest = cloudy;
                                    break;
                                case 6:
                                    clear++;
                                    expectedVoltageHr += 12;
                                    if (largest < clear)
                                        largest = clear;
                                    break;
                                case 7:
                                    chanceRain++;
                                    expectedVoltageHr += 11;

                                    if (largest < chanceRain)
                                        largest = chanceRain;
                                    break;
                                case 8:
                                    fog++;
                                    expectedVoltageHr += 9;
                                    if (largest < fog)
                                        largest = fog;
                                    break;
                            }
                            tm += r.readValue;
                            total++;
                            dt = r.forecast.ForecastDate;
                        }
                }
            }
            catch
            {

            }


            double diff = expectedVoltageHr - tm;

            if (total != 0)
                label7.Text = "Mostly Cloudy: " + (mostlyCloudy * 100) / total + "%\nPartly Cloudy: " + (partlyCloudy * 100) / total +
                    "%\nSunny: " + (sunny * 100) / total + "%\nMostly Sunny: " + (mostlySunny * 100) / total + "%\nCloudy: " +
                        (cloudy * 100) / total +
                    "%\nClear: " + (clear * 100) / total + "%\nChance of Rain: " + (chanceRain * 100) / total + "%\nFog" + (fog * 100) / total +
                    "%\n\nTotal Hourly Voltage: " + tm + "V\nExpected Hourly Voltage: " + expectedVoltageHr + "V";
            if (diff > 1)
                message += " Panels not functioning well according to forecast";



            return message;
        }
    }
}
