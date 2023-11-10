// See https://aka.ms/new-console-template for more information
using apptScheduler;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

// Construct the full path to your .env file
DotNetEnv.Env.Load("C:\\Users\\matth\\Documents\\PesonalCode\\Brevium_interview\\apptScheduler\\.env");
string apiKey = DotNetEnv.Env.GetString("API_KEY");
string baseURL = DotNetEnv.Env.GetString("BASE_URL");

//connect to db and truncate the table
using var db = new AppointmentContext();
db.TruncateAppointments();

//Hit start
var api = new APIService(token: apiKey , baseUrl: baseURL);
await api.StartSystem();

//Get intiail Schedule and Load contents to db
List<Appointment> schedule = await api.GetInitialSchedule();
db.Appointments.AddRange(schedule);
db.SaveChanges();

//Hit appointment request endpoint until status code 204
var newApptInfos = await api.CreateNewRequestQueue();

//Iterate through the new AppointmentRequestInfo to be validated and scheduled.
foreach (AppointmentRequestInfo apptInfo in newApptInfos)
{
    apptInfo.Schedule(db, api);
}

//hit stop
api.StopSystem();
