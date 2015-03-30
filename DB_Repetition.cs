using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class DB_Repetition:Repetition
    {
        internal DB_Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents, int DayOfWeek = 7)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            long repetitionIndex = 0;
            ReadFromFileRecurringListOfCalendarEvents=ReadFromFileRecurringListOfCalendarEvents.OrderBy(obj=>obj.Start).ToArray();
            foreach (CalendarEvent MyRepeatCalendarEvent in ReadFromFileRecurringListOfCalendarEvents)
            {
                MyRepeatCalendarEvent.updateRepetitionIndex(repetitionIndex);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.Calendar_EventID.getIDUpToRepeatCalendarEvent(), MyRepeatCalendarEvent);
                repetitionIndex++;
            }

            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                RepeatLocation = ReadFromFileRecurringListOfCalendarEvents[0].myLocation;
            }
        }


        internal DB_Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, Repetition[] repetition_Weekday, int DayOfWeek = 7)
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
        }


    }
}