using Pegelalarm.Core.Network.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Pegelalarm.Core.Utils;
using System.Globalization;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pegelalarm.Controls
{
    public sealed partial class Chart : UserControl
    {
        private double warnValue;
        private double alarmValue;
        private string minDate;
        private string maxDate;
        private List<Sample> samples;
        private readonly LinearGradientBrush gradient;
        public event EventHandler<double> AlarmValueChanged;

        public Chart()
        {
            this.InitializeComponent();
            this.DrawingArea.SizeChanged += Chart_SizeChanged;
            this.Slider.ValueChanged += Slider_ValueChanged;

            gradient = new LinearGradientBrush();
            gradient.GradientStops = new GradientStopCollection();
            gradient.StartPoint = new Point(0, 0);
            gradient.EndPoint = new Point(0, 1);
            gradient.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 0, 74, 127), Offset = 0 });
            gradient.GradientStops.Add(new GradientStop() { Color = Color.FromArgb(255, 0, 148, 255), Offset = 1 });
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (samples == null || samples.Count == 0) return;
            AlarmValueChanged?.Invoke(this, e.NewValue);
        }

        private void Chart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        public void SetValues(double warnValue, double alarmValue, List<Sample> samples)
        {
            this.warnValue = warnValue;
            this.alarmValue = alarmValue;
            this.samples = samples;

            if (samples != null && samples.Count != 0)
            {
                var format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern + " " + DateTimeFormatInfo.CurrentInfo.ShortTimePattern;
                this.minDate = samples.First().sourceDate.DateFromString().ToString(format);
                this.maxDate = samples.Last().sourceDate.DateFromString().ToString(format);
            }

            Draw();
        }

        public void Draw()
        {
            DrawingArea.Children.Clear();
            if (samples == null || samples.Count == 0)
            {
                NoDataText.Visibility = Visibility.Visible;
                MinYLabel.Text = "";
                CenterYLabel.Text = "";
                MaxYLabel.Text = "";

                MinXLabel.Text = "";
                MaxXLabel.Text = "";
                return;
            }

            NoDataText.Visibility = Visibility.Collapsed;

            var min = 0;//Math.Min(warnValue, Math.Min(alarmValue, points.Min(d => d.Y)));
            var max = Math.Max(warnValue, Math.Max(alarmValue, samples.Max(d => d.value)));
            max = Math.Ceiling((max / 100)) * 100 + 100;
            
            // set axis value

            MinYLabel.Text = $"{min}";
            CenterYLabel.Text = $"{ (max / 2)}";
            MaxYLabel.Text = $"{max}";

            MinXLabel.Text = minDate;
            MaxXLabel.Text = maxDate;

            Slider.Minimum = 1;
            Slider.Maximum = max;
            
            //draw chart
            
            var points = TranslateSamplesToPoints(samples, max);
            
            var pg = new Polygon();
            pg.Points = new PointCollection();
            foreach (var p in points)
            {
                pg.Points.Add(p);
            }

            pg.Points.Add(new Point(DrawingArea.ActualWidth, DrawingArea.ActualHeight));
            pg.Points.Add(new Point(0, DrawingArea.ActualHeight));
            
            
            pg.Fill = gradient;

            DrawingArea.Children.Add(pg);

            //show markers

            if (alarmValue != 0)
            {
                DrawingArea.Children.Add(alarmGrid);
                Canvas.SetTop(alarmGrid, YforValue(alarmValue,max));
            }
            
            if (warnValue != 0)
            {
                DrawingArea.Children.Add(warnGrid);
                Canvas.SetTop(warnGrid, YforValue(warnValue,max));
            }
        }

        public List<Point> TranslateSamplesToPoints(List<Sample> samples, double maxY)
        {
            var ptTemp = samples.Select(s => new Point(s.sourceDate.DateFromString().ToUnixTimeSeconds(), s.value));

            var minXVal = ptTemp.Min(d => d.X);
            var maxXVal = ptTemp.Max(d => d.X);

            var width = DrawingArea.ActualWidth;
            var height = DrawingArea.ActualHeight;

            var pxPVal = width / (maxXVal - minXVal);
            var pyPVal = height / maxY;

            var pt = ptTemp.Select(d =>
            {
                var x = (d.X - minXVal) * pxPVal;
                var y = height - d.Y * pyPVal;
                return new Point(x, y);
            });
            
            return pt.ToList();
        }

        private double YforValue(double value, double maxY)
        {
            var height = DrawingArea.ActualHeight;
            var pyPVal = height / maxY;
            var y = height - value * pyPVal;
            return y;
        }

        public void ShowSlider(bool show, double value)
        {
            Slider.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            if (show)
            {
                var val = warnValue > 0 ? warnValue : Slider.Maximum - 50;

                if (value != 0)
                {
                    val = value;
                }
                else if (warnValue == 0)
                {
                    val = Slider.Maximum - 50;
                }

                Slider.Value = val;
            }

        }

    }
}
