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
	[Gui.CategoryOrder("Gann Bars", 1)]
	[Gui.CategoryOrder("Swings", 2)]
	[Gui.CategoryOrder("Hoagies", 3)]
	[Gui.CategoryOrder("Prior Day OHLC", 4)]
	
	public class GannBarCounting : Indicator
	{
		#region Gann Bar Variables
		
		private int UP = -1;
		private int DOWN = -2;
		private class Bar {
			public int Type { get; set; }
			public int Num { get; set; }
			public double Open { get; set; }
			public double Close { get; set; }
			public double High { get; set; }
			public double Low { get; set; }
		}
		private Series<Bar> bars;
		private List<Bar> swings;
		
		#endregion
		
		#region Hoagie Variables
				
		private bool hoagie = false;
		private int hoagieBar = 0;
		private double hoagieHigh = 0;
		private double hoagieLow = 0;
		
		#endregion
		
		#region Swing variables
		
		List<double> upSwings;
		List<double> downSwings;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "GannBarCounting";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				UpBarBrush									= Brushes.Magenta;
				DownBarBrush								= Brushes.DeepSkyBlue;
				SwingBrush									= Brushes.Blue;
				PriorHighBrush								= Brushes.LimeGreen;
				PriorLowBrush								= Brushes.Red;
				PriorCloseBrush								= Brushes.Orchid;
				confirmBars									= 2;
			}
			else if (State == State.Configure)
			{
				bars = new Series<Bar>(this);
				swings = new List<Bar>();
				upSwings = new List<Double>();
				downSwings = new List<Double>();
			}
		}

		#region Bar Update
		
		protected override void OnBarUpdate()
		{						
			if (CurrentBar < 1) return;

			if (IsFirstTickOfBar) 
			{
				gannBar();
			
				if (drawGannBars) drawBar();
				if (drawHoagies) drawHoagie();				
			}
			
			if (Bars.IsFirstBarOfSession) 
			{	
				if (drawPriorHighLow) {
					keyLevels();
				}	
			}
		}
		
		#endregion
		
		#region Gann Bars
		
		private void gannBar()
		{	
			bars[0] = new Bar { Type = 0, Num = CurrentBar - 1, Open = Open[1], Close = Close[1], High = High[1], Low = Low[1] };
			
			if (CurrentBar < 2) return;
			
			if (High[1] > High[2] && Low[1] > Low[2]) bars[0].Type = UP;
			
			else if (High[1] < High[2] && Low[1] < Low[2]) bars[0].Type = DOWN;
			
			else bars[0].Type = bars[1].Type;
			
			if (!hoagie && High[1] <= High[2] && Low[1] >= Low[2]) 
			{
				hoagie = true;
				hoagieBar = CurrentBar - 1;
				hoagieHigh = High[2];
				hoagieLow = Low[2];
			}
			
			else if (hoagie) 
			{	
				if (High[1] <= hoagieHigh && Low[1] >= hoagieLow) 
				{
					bars[0].Type = bars[1].Type;
				}
				
				else {
					hoagie = false;	
				}
			}
			
			if (bars.Count > 1) 
			{
				if (bars[0].Type != bars[1].Type || swings.Count == 0) swings.Add(bars[0]);
				
				Bar lastPivot = swings[swings.Count - 1];
				
				if (bars[0].Type == lastPivot.Type) 
				{	
					if ((lastPivot.Type == UP && bars[0].High > lastPivot.High) || (lastPivot.Type == DOWN && bars[0].Low < lastPivot.Low)) 
					{
						swings.RemoveAt(swings.Count - 1);
						swings.Add(bars[0]);
					}
				}
				
				if (drawSwings && swings.Count > 1) drawSwing();				
			}
		}
		
		#endregion
		
		#region Drawings
		
		private void drawBar() 
		{									
			if (bars[0].Type == UP) Draw.Line(this, "bar" + CurrentBar, 1, High[1] + TickSize * 2, 1, High[1] + TickSize * 3, UpBarBrush);
			
			else if (bars[0].Type == DOWN) Draw.Line(this, "bar" + CurrentBar, 1, Low[1] - TickSize * 2, 1, Low[1] - TickSize * 3, DownBarBrush);
		}
		
		private void drawHoagie() {
		
			if (!hoagie || CurrentBar - hoagieBar < confirmBars) return;
				
			Draw.Line(this, "hoagie high" + hoagieBar, CurrentBar - hoagieBar + 1, hoagieHigh, 1, hoagieHigh, Brushes.LimeGreen);
			Draw.Line(this, "hoagie low" + hoagieBar, CurrentBar - hoagieBar + 1, hoagieLow, 1, hoagieLow, Brushes.Red);
		}
			
		private void drawSwing() 
		{	
			Bar pivot1 = swings[swings.Count - 2];
			Bar pivot2 = swings[swings.Count - 1];
			
			Draw.Line(this, "swing" + pivot1.Num, CurrentBar - pivot1.Num, pivot1.Type == UP ? pivot1.High : pivot1.Low, CurrentBar - pivot2.Num, pivot2.Type == UP ? pivot2.High : pivot2.Low, SwingBrush);
		}
		
		private void keyLevels() 
		{	
			Draw.Line(this, "high of session" + CurrentBar, false, Time[0].AddDays(1), PriorDayOHLC().PriorHigh[0], Time[0], PriorDayOHLC().PriorHigh[0], PriorHighBrush, DashStyleHelper.Dash, 1);
			Draw.Line(this, "low of session" + CurrentBar, false, Time[0].AddDays(1), PriorDayOHLC().PriorLow[0], Time[0], PriorDayOHLC().PriorLow[0], PriorLowBrush, DashStyleHelper.Dash, 1);
			Draw.Line(this, "close of session" + CurrentBar, false, Time[0].AddDays(1), PriorDayOHLC().PriorClose[0], Time[0], PriorDayOHLC().PriorClose[0], PriorCloseBrush, DashStyleHelper.Dash, 1);		
		}
		
		#endregion
		
		#region Properties
				
		[NinjaScriptProperty]
		[Display(Name="Draw Gann Bars", Order=1, GroupName="Gann Bars")]
		public bool drawGannBars
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Up Bar Color", Order=2, GroupName="Gann Bars")]
		public Brush UpBarBrush { get; set; }
		
		[Browsable(false)]
		public string UpBarBrushSerialize
		{
			get { return Serialize.BrushToString(UpBarBrush); }
			set { UpBarBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Down Bar Color", Order=3, GroupName="Gann Bars")]
		public Brush DownBarBrush { get; set; }
		
		[Browsable(false)]
		public string DownBarBrushSerialize
		{
			get { return Serialize.BrushToString(DownBarBrush); }
			set { DownBarBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Draw Swings", Order=1, GroupName="Swings")]
		public bool drawSwings
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Color", Order=2, GroupName="Swings")]
		public Brush SwingBrush { get; set; }
		
		[Browsable(false)]
		public string SwingBrushSerialize
		{
			get { return Serialize.BrushToString(SwingBrush); }
			set { SwingBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Draw Hoagies", Order=1, GroupName="Hoagies")]
		public bool drawHoagies
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, Int32.MaxValue)]
		[Display(Name="Confirm Bars", Order=2, GroupName="Hoagies")]
		public int confirmBars
		{ get; set; }
				
				
		[NinjaScriptProperty]
		[Display(Name="Draw Prior Day High/Low/Close", Order=1, GroupName="Prior Day OHLC")]
		public bool drawPriorHighLow
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Prior High Color", Order=2, GroupName="Prior Day OHLC")]
		public Brush PriorHighBrush { get; set; }
		
		[Browsable(false)]
		public string PriorHighBrushSerialize
		{
			get { return Serialize.BrushToString(PriorHighBrush); }
			set { PriorHighBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Prior Low Color", Order=3, GroupName="Prior Day OHLC")]
		public Brush PriorLowBrush { get; set; }
		
		[Browsable(false)]
		public string PriorLowBrushSerialize
		{
			get { return Serialize.BrushToString(PriorLowBrush); }
			set { PriorLowBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Prior Close Color", Order=4, GroupName="Prior Day OHLC")]
		public Brush PriorCloseBrush { get; set; }
		
		[Browsable(false)]
		public string PriorCloseBrushSerialize
		{
			get { return Serialize.BrushToString(PriorCloseBrush); }
			set { PriorCloseBrush = Serialize.StringToBrush(value); }
		}
				
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GannBarCounting[] cacheGannBarCounting;
		public GannBarCounting GannBarCounting(bool drawGannBars, bool drawSwings, bool drawHoagies, int confirmBars, bool drawPriorHighLow)
		{
			return GannBarCounting(Input, drawGannBars, drawSwings, drawHoagies, confirmBars, drawPriorHighLow);
		}

		public GannBarCounting GannBarCounting(ISeries<double> input, bool drawGannBars, bool drawSwings, bool drawHoagies, int confirmBars, bool drawPriorHighLow)
		{
			if (cacheGannBarCounting != null)
				for (int idx = 0; idx < cacheGannBarCounting.Length; idx++)
					if (cacheGannBarCounting[idx] != null && cacheGannBarCounting[idx].drawGannBars == drawGannBars && cacheGannBarCounting[idx].drawSwings == drawSwings && cacheGannBarCounting[idx].drawHoagies == drawHoagies && cacheGannBarCounting[idx].confirmBars == confirmBars && cacheGannBarCounting[idx].drawPriorHighLow == drawPriorHighLow && cacheGannBarCounting[idx].EqualsInput(input))
						return cacheGannBarCounting[idx];
			return CacheIndicator<GannBarCounting>(new GannBarCounting(){ drawGannBars = drawGannBars, drawSwings = drawSwings, drawHoagies = drawHoagies, confirmBars = confirmBars, drawPriorHighLow = drawPriorHighLow }, input, ref cacheGannBarCounting);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GannBarCounting GannBarCounting(bool drawGannBars, bool drawSwings, bool drawHoagies, int confirmBars, bool drawPriorHighLow)
		{
			return indicator.GannBarCounting(Input, drawGannBars, drawSwings, drawHoagies, confirmBars, drawPriorHighLow);
		}

		public Indicators.GannBarCounting GannBarCounting(ISeries<double> input , bool drawGannBars, bool drawSwings, bool drawHoagies, int confirmBars, bool drawPriorHighLow)
		{
			return indicator.GannBarCounting(input, drawGannBars, drawSwings, drawHoagies, confirmBars, drawPriorHighLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GannBarCounting GannBarCounting(bool drawGannBars, bool drawSwings, bool drawHoagies, int confirmBars, bool drawPriorHighLow)
		{
			return indicator.GannBarCounting(Input, drawGannBars, drawSwings, drawHoagies, confirmBars, drawPriorHighLow);
		}

		public Indicators.GannBarCounting GannBarCounting(ISeries<double> input , bool drawGannBars, bool drawSwings, bool drawHoagies, int confirmBars, bool drawPriorHighLow)
		{
			return indicator.GannBarCounting(input, drawGannBars, drawSwings, drawHoagies, confirmBars, drawPriorHighLow);
		}
	}
}

#endregion
