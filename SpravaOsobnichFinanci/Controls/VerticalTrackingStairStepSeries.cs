using System;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SpravaOsobnichFinanci.Controls
{
    /// <summary>
    /// Vlastní grafová série odvozená od schodkového grafu (StairStepSeries) knihovny OxyPlot.
    /// Řeší problém s výchozím chováním: běžně se Tracker (informační bublina) objevuje 
    /// pouze tehdy, když uživatel najede fyzicky myší přímo na vykreslenou křivku. 
    /// Tato implementace zachycuje kurzor kdekoliv na vertikální ose, díky čemuž stačí pohybovat myší zleva doprava.
    /// </summary>
    public class VerticalTrackingStairStepSeries : StairStepSeries
    {
        /// <summary>
        /// Přepsaná metoda pro získání nejbližšího datového bodu k pozici kurzoru.
        /// </summary>
        /// <param name="point"> Fyzická pozice kurzoru v pixelech (ScreenPoint) </param>
        /// <param name="interpolate"> Určuje, zda interpolovat mezi datovými body (není relevantní pro schodkový graf, ale je součástí podpisu metody) </param>
        /// <returns> Objekt TrackerHitResult obsahující informace o nejbližším datovém bodu, nebo null pokud není žádný relevantní bod nalezen </returns>
        public override TrackerHitResult? GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            // Elementární ochrana proti pádu v případě nevykresleného grafu bez dat
            if (this.XAxis == null || this.YAxis == null || this.Points == null || this.Points.Count == 0 || !this.IsVisible) 
                return null;

            // Převedení fyzické pozice kurzoru (pixel) na datovou souřadnici X
            double dataX = this.XAxis.InverseTransform(point.X);

            if (dataX < this.XAxis.ActualMinimum || dataX > this.XAxis.ActualMaximum)
                return null;

            // Vyhledání nejbližšího datového bodu na ose X k pozici kurzoru.
            // Osa Y (hodnota) je záměrně ignorována, vzniká tak efekt snazšího zachycení dat
            var nearestPoint = this.Points.OrderBy(p => Math.Abs(p.X - dataX)).First();
            var index = this.Points.IndexOf(nearestPoint);

            // Zpětný převod nalezené číselné hodnoty na pochopitelný formát data
            DateTime date = DateTime.MinValue;
            if (this.XAxis is DateTimeAxis dateTimeAxis)
            {
                date = dateTimeAxis.ConvertToDateTime(nearestPoint.X);
            }
            else
            {
                // Potlačení kompilátorového varování kvůli pádu na záložní statickou metodu
#pragma warning disable CS0618
                date = DateTimeAxis.ToDateTime(nearestPoint.X);
#pragma warning restore CS0618
            }

            // Sestavení finálního výsledku s vlastním zobrazením textu bubliny (Datum, Částka)
            return new TrackerHitResult
            {
                Series = this,
                DataPoint = nearestPoint,
                Position = point, 
                Item = nearestPoint,
                Index = index, // index v poli datových bodů
                Text = $"Datum: {date:dd.MM.yyyy}\nČástka: {nearestPoint.Y:N2} Kč"
            };
        }
    }
}
