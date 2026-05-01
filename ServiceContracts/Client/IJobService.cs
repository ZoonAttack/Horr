using ServiceContracts.DTOs.JobManagement;
using ServiceContracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Client
{
    public interface IJobService
    {
        public Task<Result<List<JobSummaryDto>>> GetAllJobsAsync();

        public Task<Result<JobDetailsDto>> GetJobDetailsAsync(string jobId);

        public Task<Result<JobDetailsDto>> CreateJobAsync(string clientId, JobDetailsDto jobDetails);
    }
}
