using System;
using System.IO;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Utility;
using NUnit.Framework;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void LoadAndDisplayCalendar()
        {
            // The following code loads and displays an iCalendar
            // with US Holidays for 2006.
            //
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\USHolidays.ics")[0];
            Assert.IsNotNull(iCal, "iCalendar did not load.  Are you connected to the internet?");

            var occurrences = iCal.GetOccurrences(
                new CalDateTime(2006, 1, 1, "US-Eastern"),
                new CalDateTime(2006, 12, 31, "US-Eastern"));

            //foreach (var o in occurrences)
            //{
            //    var evt = o.Source as IEvent;
            //    if (evt != null)
            //    {
            //        // Display the date of the event
            //        Console.Write(o.Period.StartTime.AsSystemLocal.Date.ToString("MM/dd/yyyy") + " -\t");

            //        // Display the event summary
            //        Console.Write(evt.Summary);

            //        // Display the time the event happens (unless it's an all-day event)
            //        if (evt.Start.HasTime)
            //        {
            //            Console.Write(" (" + evt.Start.AsSystemLocal.ToShortTimeString() + " - " + evt.End.AsSystemLocal.ToShortTimeString());
            //            if (evt.Start.TimeZoneObservance != null && evt.Start.TimeZoneObservance.HasValue)
            //                Console.Write(" " + evt.Start.TimeZoneObservance.Value.TimeZoneInfo.TimeZoneName);
            //            Console.Write(")");
            //        }

            //        Console.Write(Environment.NewLine);
            //    }
            //}
        }

        private const string _tzid = "US-Eastern";

        [SetUp]
        public void Init()
        {}

        public static void TestCal(ICalendar cal)
        {
            Assert.IsNotNull(cal, "The iCalendar was not loaded");
            if (cal.Events.Count > 0)
                Assert.IsTrue(cal.Events.Count == 1, "Calendar should contain 1 event; however, the iCalendar loaded " + cal.Events.Count + " events");
            else if (cal.Todos.Count > 0)
                Assert.IsTrue(cal.Todos.Count == 1, "Calendar should contain 1 todo; however, the iCalendar loaded " + cal.Todos.Count + " todos");
        }

        [Test]
        public void LoadFromFile()
        {
            var path = @"Calendars\Serialization\Calendar1.ics";
            Assert.IsTrue(File.Exists(path), "File '" + path + "' does not exist.");

            var iCal = Calendar.LoadFromFile(path)[0];
            Assert.AreEqual(14, iCal.Events.Count);
        }

        [Test]
        public void LoadFromUri()
        {
            var path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, "Calendars/Serialization/Calendar1.ics").Replace(@"\", "/");
            path = "file:///" + path;
            var uri = new Uri(path);
            var iCal = Calendar.LoadFromUri(uri)[0];
            Assert.AreEqual(14, iCal.Events.Count);
        }        

        /// <summary>
        /// The following test is an aggregate of MonthlyCountByMonthDay3() and MonthlyByDay1() in the
        /// <see cref="Recurrence"/> class.
        /// </summary>
        [Test]
        public void Merge1()
        {
            var iCal1 = Calendar.LoadFromFile(@"Calendars\Recurrence\MonthlyCountByMonthDay3.ics")[0];
            var iCal2 = Calendar.LoadFromFile(@"Calendars\Recurrence\MonthlyByDay1.ics")[0];

            // Change the UID of the 2nd event to make sure it's different
            iCal2.Events[iCal1.Events[0].Uid].Uid = "1234567890";
            iCal1.MergeWith(iCal2);

            var evt1 = iCal1.Events.First();
            var evt2 = iCal1.Events.Skip(1).First();

            // Get occurrences for the first event
            var occurrences = evt1.GetOccurrences(
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2000, 1, 1, _tzid)).OrderBy(o => o.Period.StartTime).ToList();

            var dateTimes = new[]
            {
                new CalDateTime(1997, 9, 10, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 11, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 13, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 14, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid),
                new CalDateTime(1999, 3, 10, 9, 0, 0, _tzid),
                new CalDateTime(1999, 3, 11, 9, 0, 0, _tzid),
                new CalDateTime(1999, 3, 12, 9, 0, 0, _tzid),
                new CalDateTime(1999, 3, 13, 9, 0, 0, _tzid)
            };

            var timeZones = new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            };

            for (var i = 0; i < dateTimes.Length; i++)
            {
                IDateTime dt = dateTimes[i];
                var start = occurrences[i].Period.StartTime;
                Assert.AreEqual(dt, start);

                var expectedZone = DateUtil.GetZone(dt.TimeZoneName);
                var actualZone = DateUtil.GetZone(timeZones[i]);

                //Assert.AreEqual();

                //Normalize the time zones and then compare equality
                Assert.AreEqual(expectedZone, actualZone);

                //Assert.IsTrue(dt.TimeZoneName == TimeZones[i], "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.IsTrue(occurrences.Count == dateTimes.Length, "There should be exactly " + dateTimes.Length + " occurrences; there were " + occurrences.Count);

            // Get occurrences for the 2nd event
            occurrences = evt2.GetOccurrences(
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 4, 1, _tzid)).OrderBy(o => o.Period.StartTime).ToList();

            var dateTimes1 = new[]
            {
                new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid),
                new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid),
                new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid),
                new CalDateTime(1997, 11, 18, 9, 0, 0, _tzid),
                new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid),
                new CalDateTime(1998, 1, 6, 9, 0, 0, _tzid),
                new CalDateTime(1998, 1, 13, 9, 0, 0, _tzid),
                new CalDateTime(1998, 1, 20, 9, 0, 0, _tzid),
                new CalDateTime(1998, 1, 27, 9, 0, 0, _tzid),
                new CalDateTime(1998, 3, 3, 9, 0, 0, _tzid),
                new CalDateTime(1998, 3, 10, 9, 0, 0, _tzid),
                new CalDateTime(1998, 3, 17, 9, 0, 0, _tzid),
                new CalDateTime(1998, 3, 24, 9, 0, 0, _tzid),
                new CalDateTime(1998, 3, 31, 9, 0, 0, _tzid)
            };

            var timeZones1 = new[]
            {
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",                
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern",
                "US-Eastern"
            };

            for (var i = 0; i < dateTimes1.Length; i++)
            {
                IDateTime dt = dateTimes1[i];
                var start = occurrences[i].Period.StartTime;
                Assert.AreEqual(dt, start);
                Assert.IsTrue(dt.TimeZoneName == timeZones1[i], "Event " + dt + " should occur in the " + timeZones1[i] + " timezone");
            }

            Assert.AreEqual(dateTimes1.Length, occurrences.Count, "There should be exactly " + dateTimes1.Length + " occurrences; there were " + occurrences.Count);
        }

        [Test]
        public void SystemTimeZone3()
        {
            // Per Jon Udell's test, we should be able to get all 
            // system time zones on the machine and ensure they
            // are properly translated.
            var zones = TimeZoneInfo.GetSystemTimeZones();
            foreach (var zone in zones)
            {
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(zone.Id);                    
                }
                catch (Exception e)
                {
                    Assert.Fail("Not found: " + zone.StandardName);                    
                }
            }
        }
    }
}
