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
namespace NinjaTrader.NinjaScript.Indicators.ICT
{
	
	public class FairValueGap 
	{
		public int type { get; set; }
		public double high { get; set; }
		public double low { get; set; }
		public int startBar { get; set; }
		public int endBar { get; set; }
		public bool taken { get; set; }
	}
	
	public class FVG : Indicator
	{
		private int BULLISH = -1;
		private int BEARISH = -2;
		private List<FairValueGap> fvgs;
		
		public override string DisplayName { get { return "FVG"; } }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "FVG";
				Calculate									= Calculate.OnBarClose;
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
			}
			else if (State == State.Configure)
			{
				fvgs = new List<FairValueGap>();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2) return;
			
			if (Low[0] > High[2] && (Low[0] - High[2]) > MinimumTicks * TickSize)
			{
				fvgs.Add(new FairValueGap { type = BULLISH, high = Low[0], low = High[2], startBar = CurrentBar - 1, endBar = CurrentBar, taken = false });
			}
			
			if (High[0] < Low[2] && (Low[2] - High[0]) > MinimumTicks * TickSize)
			{
				fvgs.Add(new FairValueGap { type = BEARISH, high = Low[2], low = High[0], startBar = CurrentBar - 1, endBar = CurrentBar, taken = false });
			}
			
			foreach (FairValueGap fvg in fvgs)
			{				
				if (!fvg.taken && CurrentBar > fvg.startBar + 1)
				{
					fvg.endBar = CurrentBar;
					
					if (fvg.type == BULLISH)
					{
						Draw.Rectangle(this, "Bullish FVG" + fvg.startBar, false, CurrentBar - fvg.startBar, fvg.high, CurrentBar - fvg.endBar, fvg.low, Brushes.Transparent, Brushes.LimeGreen, 30);	
						
						if (Low[0] < fvg.high)
						{						
							fvg.taken = true;
							
							if (fvg.endBar < fvg.startBar + 2 + MinimumBars)
							{
								RemoveDrawObject("Bullish FVG" + fvg.startBar);
							}
						}
					}
					
					else if (fvg.type == BEARISH)
					{
						Draw.Rectangle(this, "Bearish FVG" + fvg.startBar, false, CurrentBar - fvg.startBar, fvg.high, CurrentBar - fvg.endBar, fvg.low, Brushes.Transparent, Brushes.Red, 30);	
						
						if (High[0] > fvg.low)
						{
							fvg.taken = true;
							
							if (fvg.endBar < fvg.startBar + 2 + MinimumBars)
							{
								RemoveDrawObject("Bearish FVG" + fvg.startBar);
							}
						}
					}
				}
			}
		}
		
		[NinjaScriptProperty]
		[Range(0, Int32.MaxValue)]
		[Display(Name="Minimum Threshold (Ticks)", Order=2, GroupName="Parameters")]
		public int MinimumTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, Int32.MaxValue)]
		[Display(Name="Minimum Bars", Order=2, GroupName="Parameters")]
		public int MinimumBars
		{ get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ICT.FVG[] cacheFVG;
		public ICT.FVG FVG(int minimumTicks, int minimumBars)
		{
			return FVG(Input, minimumTicks, minimumBars);
		}

		public ICT.FVG FVG(ISeries<double> input, int minimumTicks, int minimumBars)
		{
			if (cacheFVG != null)
				for (int idx = 0; idx < cacheFVG.Length; idx++)
					if (cacheFVG[idx] != null && cacheFVG[idx].MinimumTicks == minimumTicks && cacheFVG[idx].MinimumBars == minimumBars && cacheFVG[idx].EqualsInput(input))
						return cacheFVG[idx];
			return CacheIndicator<ICT.FVG>(new ICT.FVG(){ MinimumTicks = minimumTicks, MinimumBars = minimumBars }, input, ref cacheFVG);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ICT.FVG FVG(int minimumTicks, int minimumBars)
		{
			return indicator.FVG(Input, minimumTicks, minimumBars);
		}

		public Indicators.ICT.FVG FVG(ISeries<double> input , int minimumTicks, int minimumBars)
		{
			return indicator.FVG(input, minimumTicks, minimumBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ICT.FVG FVG(int minimumTicks, int minimumBars)
		{
			return indicator.FVG(Input, minimumTicks, minimumBars);
		}

		public Indicators.ICT.FVG FVG(ISeries<double> input , int minimumTicks, int minimumBars)
		{
			return indicator.FVG(input, minimumTicks, minimumBars);
		}
	}
}

#endregion
