using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

// Redoing the code puzzle in 15 minutes.
// Defining classes for JSON deserialization. They should  be in their own files under a 'models' folder but for the sake of simplicity, they are in the same file.
// Change the appsettings localhost to your localhost or the url you are using.
public class Employee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int JobId { get; set; }
}

public class Job
{
    public int JobId { get; set; }
    public string JobName { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration - insteaed of splashing the url here we can use a configuration file
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Get base URL from configuration
        string baseUrl = config["BaseUrl"];

        // Create HttpClient instance, sing statement ensures the HttpClient is disposed after use. No need to have methods such as "GetEmployees" and "GetJobs" as they are only used once.. 
        using (var client = new HttpClient())
        {
            try
            {
                // Get employees
                HttpResponseMessage response = await client.GetAsync($"{baseUrl}/Data/employees");
                response.EnsureSuccessStatusCode(); 
                //doing the same thing here as I did in the office. Using the 'read as string async'
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize JSON array into a list of Employee objects
                // this could also be with newtonsoft json but I am using the built in json serializer. could use JObject.Parse(responseBody) and then use the SelectToken method to get the values 
                List<Employee> employees = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                // Get jobs. Same logic as employees
                response = await client.GetAsync($"{baseUrl}/Data/jobs");
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize JSON array into a list of Job objects, why? to get the job name from the job id and then use linq to match the job id with the employee job id.
                List<Job> jobs = JsonConvert.DeserializeObject<List<Job>>(responseBody);

                // Match employee's jobId with job's jobId to get jobName. This is basically what I was missing because I was overcomplicating the solution.
                foreach (Employee employee in employees)
                {
                    // Find the job with the matching jobId. This is what I mentioned in the office. I could have used a dictionary to store the jobs and then get the job name from the dictionary.
                    Job job = jobs.FirstOrDefault(j => j.JobId == employee.JobId);

                    if (job != null)
                    {
                        Console.WriteLine($"Employee: {employee.FirstName} {employee.LastName}, Job: {job.JobName}");
                    }
                }
            }
            // your standard catches for http errors and other errors.
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HTTP Error: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
