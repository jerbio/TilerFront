﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class DB_Repetition:Repetition
    {
        internal DB_Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents, int DayOfWeek = 7, TimeLine initializingRangeData = null)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            long repetitionIndex = 0;
            ReadFromFileRecurringListOfCalendarEvents=ReadFromFileRecurringListOfCalendarEvents.OrderBy(obj=>obj.Start).ToArray();
            foreach (CalendarEvent MyRepeatCalendarEvent in ReadFromFileRecurringListOfCalendarEvents)
            {
                MyRepeatCalendarEvent.updateRepetitionIndex(repetitionIndex);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.Calendar_EventID.getRepeatCalendarEventID(), MyRepeatCalendarEvent);
                repetitionIndex++;
            }

            if (initializingRangeData!=null)
            {
                initializingRange = initializingRangeData;
            }
            else
            {
                initializingRange = ReadFromFileRecurringListOfCalendarEvents.Last().ActiveSubEvents.Last().ActiveSlot;
            }
            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                RepeatLocation = ReadFromFileRecurringListOfCalendarEvents[0].myLocation;
            }
        }


        internal DB_Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange, int[] WeekDayData)
            : base(EnableFlag, RepetitionRange_Entry, Frequency, EventActualRange, WeekDayData)
        {
            
        }

        internal DB_Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange, int WeekDayData=7 ):base(EnableFlag, RepetitionRange_Entry, Frequency, EventActualRange, WeekDayData )
        {

        }

        internal DB_Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, Repetition[] repetition_Weekday, int DayOfWeek = 7, TimeLine initializingRangeData=null)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            foreach (Repetition eachRepetition in repetition_Weekday)
            {
                DictionaryOfWeekDayToRepetition.Add(eachRepetition.weekDay, eachRepetition);
            }


            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (repetition_Weekday.Length > 0)
            {
                RepeatLocation = repetition_Weekday[0].myLocation;
            }

            if (initializingRangeData != null)
            {
                initializingRange = initializingRangeData;
            }
            else
            {
                initializingRange = DictionaryOfWeekDayToRepetition.First().Value.RecurringCalendarEvents().Last().ActiveSubEvents.Last().ActiveSlot;
            }
        }

        override public void PopulateRepetitionParameters(CalendarEvent MyParentCalEvent)
        {
            DB_CalendarEventFly MyParentEvent = (DB_CalendarEventFly)MyParentCalEvent;
            if (!MyParentEvent.Repeat.Enable)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }
            EnableRepeat = true;
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            if (DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<int, Repetition> eachKeyValuePair in DictionaryOfWeekDayToRepetition)
                {
                    ((DB_Repetition)eachKeyValuePair.Value).PopulateRepetitionParameters(MyParentEvent, eachKeyValuePair.Key);
                    ((DB_Repetition)eachKeyValuePair.Value).RootCalendarEvent = MyParentEvent;
                }
                return;
            }

            this.RootCalendarEvent = MyParentEvent;

            DateTimeOffset EachRepeatCalendarStart = initializingRange.Start;//Start DateTimeOffset Object for each recurring Calendar Event


            StartingIndex = MyParentEvent.RepetitionIndex;
            ++StartingIndex;
            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID, StartingIndex);
            DateTimeOffset EachRepeatCalendarEnd = initializingRange.End > MyParentEvent.End ? MyParentEvent.End : initializingRange.End;//End DateTimeOffset Object for each recurring Calendar Event

            CalendarEvent MyRepeatCalendarEvent = DB_CalendarEventFly.InstantiateRepeatedCandidate(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.Duration, EachRepeatCalendarStart, EachRepeatCalendarEnd, EachRepeatCalendarStart, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, StartingIndex, DictionaryOfStartToCalEvent, MyParentEvent);

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                //++RepetitionIndex;
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                ++StartingIndex;
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID, StartingIndex);
                MyRepeatCalendarEvent = DB_CalendarEventFly.InstantiateRepeatedCandidate(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.Duration, EachRepeatCalendarStart, EachRepeatCalendarEnd, EachRepeatCalendarStart, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, StartingIndex, DictionaryOfStartToCalEvent, MyParentEvent);

                MyRepeatCalendarEvent.myLocation = MyParentEvent.myLocation;
            }
            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
        }


        protected override void PopulateRepetitionParameters(CalendarEvent MyParentCalEvent, int WeekDay)
        {
            DB_CalendarEventFly MyParentEvent=(DB_CalendarEventFly)MyParentCalEvent;
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(initializingRange.TimelineSpan);

            TimeLine repetitionTimeline = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            DB_Repetition repetitionData = new DB_Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.Frequency, ActiveTimeline, 7);


            this.initializingRange = ActiveTimeline;
            this.RepetitionRange = repetitionTimeline;
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.ID, WeekDay, 0);//using the 0 sequence because MyRepeatCalendarEvent is only needed to generate the parameters for repetition
            //CalendarEvent MyRepeatCalendarEvent = CalendarEvent.InstantiateRepeatedCandidate(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, StartTimeLineForActity, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, repetitionData, MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, 0, DictionaryOfStartToCalEvent);
            //CalendarEvent MyRepeatCalendarEvent = new DB_CalendarEventFly(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.Duration, EachRepeatCalendarStart, EachRepeatCalendarEnd, StartTimeLineForActity, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, repetitionData, MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, 0);
            CalendarEvent MyRepeatCalendarEvent = new DB_CalendarEventFly(MyEventCalendarID.ToString(), MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.EventPriority, repetitionData, MyParentEvent.myLocation, MyParentEvent.EachSplitTimeSpan, EachRepeatCalendarStart, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, MyParentEvent.NumberOfSplit, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, 0, MyParentEvent.ProcrastinationInfo, MyParentEvent.NowInfo, 0, 0, MyParentEvent.getAllUserIDs());
            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
        }

        
    }
}