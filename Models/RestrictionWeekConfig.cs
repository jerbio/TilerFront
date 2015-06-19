using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront.Models
{
    public class RestrictionWeekConfig
    {
        public IEnumerable<WeekDays> WeekDayOption { get; set; }
        public string isEnabled { get; set; }

        public RestrictionProfile getRestriction(TimeSpan TimeDiff)
        {
            RestrictionProfile RetValue= null;
            bool Enabled =false;
            if(Boolean.TryParse(isEnabled,out Enabled ))
            {
                if(Enabled)
                {
                    List<DayOfWeek> myDays = new List<DayOfWeek>();
                    List<RestrictionTimeLine> RestrictingTimeLines = new List<RestrictionTimeLine>();
                    foreach (WeekDays eachWeekDays in WeekDayOption)
                    { 
                        DateTimeOffset Start ;
                        bool parseCheck = DateTimeOffset.TryParse(eachWeekDays.Start, out Start);
                        if(!parseCheck )
                        {
                            throw new Exception("Error parsing one of your Start times in restrictive week data");
                        }


                        Start=Start.Add(TimeDiff);
                        DateTimeOffset End ;
                        parseCheck = DateTimeOffset.TryParse(eachWeekDays.End, out End);
                        if(!parseCheck )
                        {
                            throw new Exception("Error parsing one of your End times in restrictive week data");
                        }

                        End=End.Add(TimeDiff);
                        int DayIndex;
                        parseCheck = int.TryParse(eachWeekDays.Index, out DayIndex);
                        if(!parseCheck )
                        {
                            throw new Exception("Invalid day index provided in restrictive week data");
                        }



                        DayOfWeek myDay;
                        try
                        {
                            myDay=(DayOfWeek)DayIndex;
                        }
                        catch(Exception e)
                        {
                            throw new Exception("Invalid day index provided in restrictive week data");
                        }

                        RestrictionTimeLine restrictingFrame = new RestrictionTimeLine(Start, End);

                        myDays.Add(myDay);
                        RestrictingTimeLines.Add(restrictingFrame);
                    }

                    RetValue = new RestrictionProfile(myDays, RestrictingTimeLines);
                }
            }
            return RetValue;;
        }
    }

    public class WeekDays
    {
        public string Start{get;set;}
        public string Index{get;set;}
        public string End{get;set;}
    }
}