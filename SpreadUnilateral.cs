#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SpreadUnilateral : Indicator
	{

		NinjaTrader.Gui.Tools.SimpleFont font = new NinjaTrader.Gui.Tools.SimpleFont("DejaVu Sans Mono", 16) { Size = 12, Bold = true };
		
		private string firstInstrument = @"";	
		private string secondInstrument = @"";
		
		private Series<double> instrumentRatio;
		private Series<double> deltaSeries;
		private Series<double> deltaDiff;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SpreadUnilateral";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(new Stroke(Brushes.DeepSkyBlue, 2), PlotStyle.Line, "Spread");
				AddLine(new Stroke(Brushes.LimeGreen),		-1.5,		"Buy");
				AddLine(new Stroke(Brushes.Red),		1.5,		"Sell");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(FirstInstrument, Data.BarsPeriodType.Minute, BarsPeriod.Value, Data.MarketDataType.Last);
				AddDataSeries(SecondInstrument, Data.BarsPeriodType.Minute, BarsPeriod.Value, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded) {
				instrumentRatio = new Series<double>(this);
				deltaSeries = new Series<double>(this);
				deltaDiff = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 20 || BarsInProgress != 0) return;
			
			instrumentRatio[0] = Closes[1][0] / Closes[2][0];	
			deltaSeries[0] = instrumentRatio[0] - SMA(instrumentRatio, 20).Value[0];
			deltaDiff[0] = deltaSeries[0] - SMA(deltaSeries, 20).Value[0];			
			Spread[0] = deltaDiff[0] / StdDev(deltaSeries, 20).Value[0];
			
			Draw.TextFixed(this, "spread", "Spread Deviation: " + Spread[0].ToString() + (Spread[0] < -1.5 ? " Long" : Spread[0] > 1.5 ? " Short" : " Wait"), TextPosition.BottomRight, Spread[0] < -1.5 ? Brushes.LimeGreen : Spread[0] > 1.5 ? Brushes.Red : Brushes.Yellow, font, null, null, 100);
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="First Instrument", Description="Enter first instrument contract symbol", Order=1, GroupName="Parameters")]
		public string FirstInstrument
		{ 
			get {return firstInstrument;}
			set {firstInstrument = value;}
		}

		[NinjaScriptProperty]
		[Display(Name="Second Instrument", Description="Enter the second instrument contract symbol", Order=2, GroupName="Parameters")]
		public string SecondInstrument
		{ 
			get {return secondInstrument;}
			set {secondInstrument = value;}
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Spread
		{
			get { return Values[0]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SpreadUnilateral[] cacheSpreadUnilateral;
		public SpreadUnilateral SpreadUnilateral(string firstInstrument, string secondInstrument)
		{
			return SpreadUnilateral(Input, firstInstrument, secondInstrument);
		}

		public SpreadUnilateral SpreadUnilateral(ISeries<double> input, string firstInstrument, string secondInstrument)
		{
			if (cacheSpreadUnilateral != null)
				for (int idx = 0; idx < cacheSpreadUnilateral.Length; idx++)
					if (cacheSpreadUnilateral[idx] != null && cacheSpreadUnilateral[idx].FirstInstrument == firstInstrument && cacheSpreadUnilateral[idx].SecondInstrument == secondInstrument && cacheSpreadUnilateral[idx].EqualsInput(input))
						return cacheSpreadUnilateral[idx];
			return CacheIndicator<SpreadUnilateral>(new SpreadUnilateral(){ FirstInstrument = firstInstrument, SecondInstrument = secondInstrument }, input, ref cacheSpreadUnilateral);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SpreadUnilateral SpreadUnilateral(string firstInstrument, string secondInstrument)
		{
			return indicator.SpreadUnilateral(Input, firstInstrument, secondInstrument);
		}

		public Indicators.SpreadUnilateral SpreadUnilateral(ISeries<double> input , string firstInstrument, string secondInstrument)
		{
			return indicator.SpreadUnilateral(input, firstInstrument, secondInstrument);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SpreadUnilateral SpreadUnilateral(string firstInstrument, string secondInstrument)
		{
			return indicator.SpreadUnilateral(Input, firstInstrument, secondInstrument);
		}

		public Indicators.SpreadUnilateral SpreadUnilateral(ISeries<double> input , string firstInstrument, string secondInstrument)
		{
			return indicator.SpreadUnilateral(input, firstInstrument, secondInstrument);
		}
	}
}

#endregion
