using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apptScheduler
{
    //db connection object
    internal class AppointmentContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
        public string DbPath { get; }
        public AppointmentContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "appt.db");
        }

        public void TruncateAppointments ()
        {
            Database.ExecuteSqlRaw("DELETE FROM Appointments");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");
  
    }


    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }    
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime ApptTime { get; set; }
        public bool isNewPatientAppointment { get; set; }

    }

    public class AppointmentRequestInfo
    {
        public int personId { get; set; }
        public List<int> preferredDocs { get; set; } = new List<int>();
        public List<string> preferredDays { get; set; } = new List<string>();
        public bool isNew {  get; set; }
        public int requestId { get; set; }

        //validate an appt request and post it to the server and db
        public void Schedule(DbContext db, APIService api)
        {
            var newAppt = Validate();
            db.Add(newAppt);
            //api.PostAppointment();
            
        }

        //return an appointment object that can posted and add to our db instance
        private Appointment Validate()
        {
            List<DateTime> prefDates = new List<DateTime>();
            foreach (var item in preferredDays)
            {
                prefDates.Add(DateTime.Parse(item)); 
            }

            //add all validation methods to be run and return the appointment object to be added to the db and sent to the api
            //filter invalid dates from list of preferred dates
                //Dec Nov 2021 Weekend - return the list of preferred time without invalid dates
                //isValidNew - return alist of valid preferred times if a new patient
                //check if appts are spaced properly

            //of the valid dates take the first one that is available
                //isAvailable - check across all listed doctors for availability

            return new Appointment { PatientId = personId, DoctorId = preferredDocs[0], ApptTime = prefDates[0], isNewPatientAppointment = isNew,   };
        }

        //Check if A Given preferred time is available for a list of doctors - prioritize time over doctor
        private bool isAvailable(DateTime date, List<int> docs, AppointmentContext db)
        {
            int exAppt = db.Appointments.Where(appointment => appointment.ApptTime == date &&
                                    docs.Contains(appointment.DoctorId)).Count();

            if ( exAppt < docs.Count())
            {
                return true;
            }

            else
            {
                return false;
            }
        }

    }

}
