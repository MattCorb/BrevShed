using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using apptScheduler;
using Newtonsoft.Json;

public class APIService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _token;

    public APIService(string baseUrl, string token)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
        _token = token;
    }

    public async Task<bool> StartSystem()
    {
        Console.WriteLine(_baseUrl);
        HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/Scheduling/Start?token={_token}", null);
        if (response.IsSuccessStatusCode){
            return true;
        } else {
            return false;
        }
    }

    public async Task<string> StopSystem()
    {
        HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/Scheduling/Stop?token={_token}", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<HttpResponseMessage> GetNextAppointmentRequest()
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/api/Scheduling/AppointmentRequest?token={_token}");
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<List<AppointmentRequestInfo>> CreateNewRequestQueue()
    {
        int max_failed_attempts = 5;
        var new_appointment_infos = new List<AppointmentRequestInfo>();

        while (max_failed_attempts > 0)
        {
            var response = await GetNextAppointmentRequest();
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new_appointment_infos;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                new_appointment_infos.Add(JsonConvert.DeserializeObject<AppointmentRequestInfo>(jsonResponse));
            }
            else
            {
                max_failed_attempts -= 1;
            }
        }
        return new_appointment_infos;
    }

    public async Task<List<Appointment>> GetInitialSchedule()
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/api/Scheduling/Schedule?token={_token}");
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Appointment>>(jsonResponse);

    }

    //post new appointment


}