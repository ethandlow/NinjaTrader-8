//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Linear Regression. The Linear Regression is an indicator that 'predicts' the value of a security's price.
	/// </summary>
	public class LinRegSmooth : Indicator
	{
		private double	avg;
		private double	divisor;
		private	double	intercept;
		private double	myPeriod;
		private double	priorSumXY;
		private	double	priorSumY;
		private double	slope;
		private double	sumX2;
		private	double	sumX;
		private double	sumXY;
		private double	sumY;
		private SUM		sum;
		private Series<double> regression;
		private CustomEnumNamespace.MovingAverages maType = CustomEnumNamespace.MovingAverages.SMA;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "";
				Name						= "Lin. reg. smooth";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, "Lin. reg. smooth");
			}
			else if (State == State.Configure)
			{
				avg	= divisor = intercept = myPeriod = priorSumXY
					= priorSumY = slope = sumX = sumX2 = sumY = sumXY = 0;
				regression = new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{
				sum = SUM(Inputs[0], Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				double sumX = (double)Period * (Period - 1) * 0.5;
				double divisor = sumX * sumX - (double)Period * Period * (Period - 1) * (2 * Period - 1) / 6;
				double sumXY = 0;

				for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
					sumXY += count * Input[count];

				double slope = ((double)Period * sumXY - sumX * SUM(Inputs[0], Period)[0]) / divisor;
				double intercept = (SUM(Inputs[0], Period)[0] - slope * sumX) / Period;
								
				regression[0] = intercept + slope * (Period - 1);
				smoothMovingAverage();
			}
			else
			{
				if (IsFirstTickOfBar)
				{
					priorSumY = sumY;
					priorSumXY = sumXY;
					myPeriod = Math.Min(CurrentBar + 1, Period);
					sumX = myPeriod * (myPeriod - 1) * 0.5;
					sumX2 = myPeriod * (myPeriod + 1) * 0.5;
					divisor = myPeriod * (myPeriod + 1) * (2 * myPeriod + 1) / 6 - sumX2 * sumX2 / myPeriod;
				}

				double input0 = Input[0];
				sumXY = priorSumXY - (CurrentBar >= Period ? priorSumY : 0) + myPeriod * input0;
				sumY = priorSumY + input0 - (CurrentBar >= Period ? Input[Period] : 0);
				avg = sumY / myPeriod;
				slope = (sumXY - sumX2 * avg) / divisor;
				intercept = (sum[0] - slope * sumX) / myPeriod;
				regression[0] = CurrentBar == 0 ? input0 : (intercept + slope * (myPeriod - 1));
				smoothMovingAverage();
			}
		}
		
		private void smoothMovingAverage() 
		{
			switch (maType)
			{
				// If the maType is defined as an EMA then...
				case CustomEnumNamespace.MovingAverages.EMA:
				{
					// Sets the plot to be equal to the EMA's plot
					Value[0] = EMA(regression, Smoothing)[0];
					break;
				}
								
				// If the maType is defined as a SMA then...
				case CustomEnumNamespace.MovingAverages.SMA:
				{
					// Sets the plot to be equal to the SMA's plot
					Value[0] = SMA(regression, Smoothing)[0];
					break;
				}
			}	
		}

		#region Properties
		
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Linear Regression Length", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal Smoothing", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Smoothing
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Moving Average Type", GroupName = "Parameters", Description="Choose a Moving Average Type.", Order = 2)]
		public CustomEnumNamespace.MovingAverages MAType
		{
			get { return maType; }
			set { maType = value; }
		}
		
		#endregion
	}
}

namespace CustomEnumNamespace
{
	public enum MovingAverages
	{
		EMA,
		SMA,
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinRegSmooth[] cacheLinRegSmooth;
		public LinRegSmooth LinRegSmooth(int period, int smoothing, CustomEnumNamespace.MovingAverages mAType)
		{
			return LinRegSmooth(Input, period, smoothing, mAType);
		}

		public LinRegSmooth LinRegSmooth(ISeries<double> input, int period, int smoothing, CustomEnumNamespace.MovingAverages mAType)
		{
			if (cacheLinRegSmooth != null)
				for (int idx = 0; idx < cacheLinRegSmooth.Length; idx++)
					if (cacheLinRegSmooth[idx] != null && cacheLinRegSmooth[idx].Period == period && cacheLinRegSmooth[idx].Smoothing == smoothing && cacheLinRegSmooth[idx].MAType == mAType && cacheLinRegSmooth[idx].EqualsInput(input))
						return cacheLinRegSmooth[idx];
			return CacheIndicator<LinRegSmooth>(new LinRegSmooth(){ Period = period, Smoothing = smoothing, MAType = mAType }, input, ref cacheLinRegSmooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegSmooth LinRegSmooth(int period, int smoothing, CustomEnumNamespace.MovingAverages mAType)
		{
			return indicator.LinRegSmooth(Input, period, smoothing, mAType);
		}

		public Indicators.LinRegSmooth LinRegSmooth(ISeries<double> input , int period, int smoothing, CustomEnumNamespace.MovingAverages mAType)
		{
			return indicator.LinRegSmooth(input, period, smoothing, mAType);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegSmooth LinRegSmooth(int period, int smoothing, CustomEnumNamespace.MovingAverages mAType)
		{
			return indicator.LinRegSmooth(Input, period, smoothing, mAType);
		}

		public Indicators.LinRegSmooth LinRegSmooth(ISeries<double> input , int period, int smoothing, CustomEnumNamespace.MovingAverages mAType)
		{
			return indicator.LinRegSmooth(input, period, smoothing, mAType);
		}
	}
}

#endregion
