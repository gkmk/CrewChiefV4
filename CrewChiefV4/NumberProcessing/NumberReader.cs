﻿using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using System;
using System.Collections.Generic;
namespace CrewChiefV4
{
    public abstract class NumberReader
    {
        /**
         * Language specific implementation to speak an integer, using whatever rules and words this language requires.
         * Note this char array may contain only '0'. This will typically include words for "seconds", "tenths", "hundreds", etc
         * as well as the number sounds.
         */
        protected abstract List<String> GetIntegerSounds(char[] digits);

        /**
         * Language specific implementation to speak a number of hours, using whatever rules and words this language requires.
         * This might need to take the numbers of minutes, seconds and tenths into consideration.
         */
        protected abstract List<String> GetHoursSounds(int hours, int minutes, int seconds, int tenths);

        /**
         * Language specific implementation to speak a number of minutes, using whatever rules and words this language requires.
         * This might need to take the numbers of hours, seconds and tenths into consideration.
         */
        protected abstract List<String> GetMinutesSounds(int hours, int minutes, int seconds, int tenths);

        /**
         * Language specific implementation to speak a number of seconds, using whatever rules and words this language requires.
         * This might need to take the numbers of hours, minutes and tenths into consideration.
         */
        protected abstract List<String> GetSecondsSounds(int hours, int minutes, int seconds, int tenths);

        /**
         * Language specific implementation to speak a number of tenths, using whatever rules and words this language requires.
         * This might need to take the numbers of hours, minutes and seconds into consideration.
         * The useMoreInflection tells the implementation to select a different tenths sound with a rising (or hanging) inflection. This
         * is needed for Italian numbers.
         */
        protected abstract List<String> GetTenthsSounds(int hours, int minutes, int seconds, int tenths, Boolean useMoreInflection);

        /**
         * Separate recordings for when we just want a number of seconds with tenths. This is only used when we have no minutes part,
         * or we have a minutes part *and* the number of seconds is 10 or more (because these sounds have no "zero.." or "oh.." part.
         * This is (currently) only applicable to English numbers.
         *
         */
        protected abstract String GetSecondsWithTenths(int seconds, int tenths);

        /**
         * Separate recordings for when we just want a number of seconds with tenths with 1 or 2 minutes. 
         * This is (currently) only applicable to English numbers.
         *
         */
        protected abstract List<String> GetMinutesAndSecondsWithTenths(int minutes, int seconds, int tenths);

        protected abstract String getLocale();

        protected Random random = new Random();

        /**
         * Convert a timeSpan to some sound files, using the current language's implementation.
         */
        public List<String> ConvertTimeToSounds(TimeSpan timeSpan, Boolean useMoreInflection)
        {
            // Console.WriteLine(new DateTime(timeSpan.Ticks).ToString("HH:mm:ss.F"));
            List<String> messageFolders = new List<String>();
            if (timeSpan != null)
            {
                // if the milliseconds in this timeSpan is > 949, when we turn this into tenths it'll get rounded up to 
                // ten tenths, which we can't have. So move the timespan on so this rounding doesn't happen
                if (timeSpan.Milliseconds > 949)
                {
                    timeSpan = timeSpan.Add(TimeSpan.FromMilliseconds(1000 - timeSpan.Milliseconds));
                }
                int tenths = (int)Math.Round((float)timeSpan.Milliseconds / 100f);

                // now call the language-specific implementations
                Boolean useNewENMinutes = AudioPlayer.soundPackVersion > 106 && getLocale() == "en" && timeSpan.Hours == 0 && 
                    timeSpan.Minutes > 0 && timeSpan.Minutes < 3 && timeSpan.Seconds > 0 && timeSpan.Seconds < 60;
                Boolean useNewENSeconds = AudioPlayer.soundPackVersion > 106 && getLocale() == "en" && timeSpan.Hours == 0 && 
                    timeSpan.Minutes == 0 && (timeSpan.Seconds > 0 || tenths > 0) && timeSpan.Seconds < 60;

                if (useNewENSeconds)
                {
                    messageFolders.Add(AbstractEvent.Pause(50));
                    messageFolders.Add(GetSecondsWithTenths(timeSpan.Seconds, tenths));
                }
                else if (useNewENMinutes)
                {
                    messageFolders.Add(AbstractEvent.Pause(50));
                    messageFolders.AddRange(GetMinutesAndSecondsWithTenths(timeSpan.Minutes, timeSpan.Seconds, tenths));
                }
                else
                {
                    messageFolders.AddRange(GetHoursSounds(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, tenths));
                    messageFolders.AddRange(GetMinutesSounds(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, tenths));
                    messageFolders.AddRange(GetSecondsSounds(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, tenths));
                    messageFolders.AddRange(GetTenthsSounds(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, tenths, useMoreInflection));
                }

                /*if (messageFolders.Count > 0)
                {
                    Console.WriteLine(String.Join(", ", messageFolders));
                }*/
            }
            return messageFolders;
        }

        /**
         * Convert an integer to some sound files, using the current language's implementation.
         */
        public List<String> GetIntegerSounds(int integer)
        {
            if (integer >= 0 && integer <= 99999)
            {
                return GetIntegerSounds(integer.ToString().ToCharArray());
            }
            else
            {
                Console.WriteLine("Cannot convert integer " + integer + " valid range is 0 - 99999");
                return new List<String>();
            }
        }
    }
}
